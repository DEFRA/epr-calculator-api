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
        public IActionResult GetCalculatorRuns()
        {
            return new OkResult();
        }

        [HttpGet]
        [Route("api/calculatorRuns/:id")]
        public IActionResult GetCalculatorRun()
        {
            // TODO: Return the details of a particular run
            return new OkResult();
        }
    }
}
