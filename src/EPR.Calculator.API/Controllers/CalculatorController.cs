using EPR.Calculator.API.Common;
using EPR.Calculator.API.Common.Models;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.API.Controllers
{
    [Route("v1")]
    public class CalculatorController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public CalculatorController(ApplicationDBContext context)
        {
            this._context = context;
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

        [HttpPost]
        [Route("calculatorRun")]
        public async Task<IActionResult> CreateCalculatorRun([FromBody] CalculatorRunMessage message)
        {
            await ServiceBus.SendMessage(message);

            // TODO: Initiate calculator run by sending message to the Azure Service Bus
            return new OkResult();
        }
    }
}
