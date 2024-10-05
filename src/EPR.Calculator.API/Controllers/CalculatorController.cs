using EPR.Calculator.API.Common.Models;
using EPR.Calculator.API.Common.ServiceBus;
using EPR.Calculator.API.Data;
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
        public async Task<IActionResult> CreateCalculatorRun([FromBody] CalculatorRunMessage message)
        {
            try
            {
                // There will not be any request body for this API call as the calculator run id should be created in this API controller

                // TO DO: Create calculator run record, get the calculator run id and send it in the message

                var serviceBusConnectionString = this._configuration.GetSection("ServiceBus").GetSection("ConnectionString").Value;
                var serviceBusQueueName = this._configuration.GetSection("ServiceBus").GetSection("QueueName").Value;

                if (string.IsNullOrWhiteSpace(serviceBusConnectionString) || string.IsNullOrWhiteSpace(serviceBusQueueName))
                {
                    return StatusCode(StatusCodes.Status500InternalServerError);
                }

                await ServiceBus.SendMessage(serviceBusConnectionString, serviceBusQueueName, message);

                return new OkResult();
            }
            catch (Exception exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, exception);
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
