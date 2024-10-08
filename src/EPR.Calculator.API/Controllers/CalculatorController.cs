using EPR.Calculator.API.Common.Models;
using EPR.Calculator.API.Common.ServiceBus;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.API.Controllers
{
    [Route("v1")]
    public class CalculatorController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        private readonly IConfiguration _configuration;

        public CalculatorController(ApplicationDBContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost]
        [Route("calculatorRun")]
        public async Task<IActionResult> Create([FromBody] CreateCalculatorRunDto request)
        {
            // Return bad request if the model is invalid
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ModelState.Values.SelectMany(x => x.Errors));
            }

            // Get active default parameter settings for the given financial year
            var activeDefaultParameterSettings = _context.DefaultParameterSettings
                        .SingleOrDefault(x => x.EffectiveTo == null && x.ParameterYear == request.FinancialYear);

            // Get active Lapcap data for the given financial year
            var activeLapcapData = _context.LapcapDataMaster
                .SingleOrDefault(data => data.ProjectionYear == request.FinancialYear && data.EffectiveTo == null);

            // Return not found with detailed message if there are no active default paramater settings and lapcap data
            if (activeDefaultParameterSettings == null && activeLapcapData == null)
            {
                return new ObjectResult($"Default parameter settings and Lapcap data not available for the financial year {request.FinancialYear}.") { StatusCode = StatusCodes.Status404NotFound };
            }

            // Return not found with detailed message if no active default parameter settings found
            if (activeDefaultParameterSettings == null)
            {
                return new ObjectResult($"Default parameter settings not available for the financial year {request.FinancialYear}.") { StatusCode = StatusCodes.Status404NotFound };
            }

            // Return not found with detailed message if no active lapcap data found
            if (activeLapcapData == null)
            {
                return new ObjectResult($"Lapcap data not available for the financial year {request.FinancialYear}.") { StatusCode = StatusCodes.Status404NotFound };
            }

            // Read configuration items: service bus connection string and queue name
            var serviceBusConnectionString = this._configuration.GetSection("ServiceBus").GetSection("ConnectionString").Value;
            var serviceBusQueueName = this._configuration.GetSection("ServiceBus").GetSection("QueueName").Value;

            // Return server error if configuration items not found
            if (string.IsNullOrWhiteSpace(serviceBusConnectionString) || string.IsNullOrWhiteSpace(serviceBusQueueName))
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            // Read configuration items: message retry count and period
            var messageRetryTimesFound = int.TryParse(this._configuration.GetSection("MessageRetry").GetSection("PostMessageRetryCount").Value, out int messageRetryTimes);
            var messageRetryPeriodFound = int.TryParse(this._configuration.GetSection("MessageRetry").GetSection("PostMessageRetryPeriod").Value, out int messageRetryPeriod);

            // Return server error if configuration items not found
            if (!messageRetryPeriodFound || !messageRetryTimesFound)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // Setup calculator run details
                    var calculatorRun = new CalculatorRun
                    {
                        Name = request.CalculatorRunName,
                        Financial_Year = request.FinancialYear,
                        CreatedBy = request.CreatedBy,
                        CreatedAt = DateTime.Now,
                        CalculatorRunClassificationId = 1
                    };

                    // Save calculator run details to the database
                    _context.CalculatorRuns.Add(calculatorRun);
                    _context.SaveChanges();

                    // Setup message
                    var calculatorRunMessage = new CalculatorRunMessage
                    {
                        CalculatorRunId = calculatorRun.Id,
                        FinancialYear = calculatorRun.Financial_Year,
                        CreatedBy = User?.Identity?.Name ?? request.CreatedBy
                    };

                    // Send message to service bus
                    await ServiceBus.SendMessage(serviceBusConnectionString, serviceBusQueueName, calculatorRunMessage, messageRetryTimes, messageRetryPeriod);

                    // All good, commit transaction
                    transaction.Commit();
                }
                catch (Exception exception)
                {
                    // Error, rollback transaction
                    transaction.Rollback();
                    return StatusCode(StatusCodes.Status500InternalServerError, exception);
                }
            }

            // Return ccepted status code: Accepted
            return new ObjectResult(null) { StatusCode = StatusCodes.Status202Accepted };
        }

        [HttpPost]
        [Route("calculatorRuns")]
        public IActionResult GetCalculatorRuns([FromBody] CalculatorRunsParamsDto request)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ModelState.Values.SelectMany(x => x.Errors));
            }

            if (string.IsNullOrWhiteSpace(request.FinancialYear))
            {
                return new ObjectResult("Invalid financial year provided") { StatusCode  = StatusCodes.Status400BadRequest };
            }

            try
            {
                var calculatorRuns = _context.CalculatorRuns.Where(run => run.Financial_Year == request.FinancialYear).OrderByDescending(run => run.CreatedAt).ToList();

                if (calculatorRuns.Count == 0)
                {
                    return new ObjectResult("No data available for the specified year. Please check the year and try again.") { StatusCode = StatusCodes.Status404NotFound };
                }

                return new ObjectResult(calculatorRuns) { StatusCode = StatusCodes.Status200OK };
            }
            catch (Exception exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, exception);
            }
        }

        [HttpGet]
        [Route("calculatorRuns/{id}")]
        public IActionResult GetCalculatorRun()
        {
            // TODO: Return the details of a particular run
            return new OkResult();
        }
    }
}
