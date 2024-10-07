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
            this._context = context;
            this._configuration = configuration;
        }

        [HttpPost]
        [Route("calculatorRun")]
        public async Task<IActionResult> Create([FromBody] CreateCalculatorRunDto request)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ModelState.Values.SelectMany(x => x.Errors));
            }

            var serviceBusConnectionString = this._configuration.GetSection("ServiceBus").GetSection("ConnectionString").Value;
            var serviceBusQueueName = this._configuration.GetSection("ServiceBus").GetSection("QueueName").Value;

            if (string.IsNullOrWhiteSpace(serviceBusConnectionString) || string.IsNullOrWhiteSpace(serviceBusQueueName))
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            var messageRetryTimesFound = int.TryParse(this._configuration.GetSection("MessageRetry").GetSection("PostMessageRetryCount").Value, out int messageRetryTimes);
            var messageRetryPeriodFound = int.TryParse(this._configuration.GetSection("MessageRetry").GetSection("PostMessageRetryPeriod").Value, out int messageRetryPeriod);

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
                    this._context.CalculatorRuns.Add(calculatorRun);
                    this._context.SaveChanges();

                    // Setup message
                    var calculatorRunMessage = new CalculatorRunMessage
                    {
                        CalculatorRunId = calculatorRun.Id,
                        FinancialYear = calculatorRun.Financial_Year
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

                return new ObjectResult(null) { StatusCode = StatusCodes.Status202Accepted };
            }
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
