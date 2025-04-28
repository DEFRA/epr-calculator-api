using EPR.Calculator.API.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.API.Controllers
{
    [Route("V2")]
    [ApiController]
    public class CalculatorNewController : ControllerBase
    {
        [HttpPut]
        [Route("calculatorRuns")]
        [AllowAnonymous]
        public async Task<IActionResult> PutCalculatorRunStatus(CalculatorRunStatusUpdateDto runStatusUpdateDto)
        {
            throw new NotImplementedException();
        }
    }
}
