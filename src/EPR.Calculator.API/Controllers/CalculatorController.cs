using EPR.Calculator.API.Data;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.API.Controllers
{
    public class CalculatorController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public CalculatorController(ApplicationDBContext context)
        {
            this._context = context;
        }

        [HttpPost]
        [Route("api/calculatorRuns")]
        public IActionResult GetCalculatorRuns([FromBody] string financialYear)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ModelState.Values.SelectMany(x => x.Errors));
            }

            try
            {
                var calculatorRuns = _context.CalculatorRuns.Where(run => run.Financial_Year == financialYear).ToList();

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
        [Route("api/calculatorRuns/{id}")]
        public IActionResult GetCalculatorRun()
        {
            // TODO: Return the details of a particular run
            return new OkResult();
        }
    }
}
