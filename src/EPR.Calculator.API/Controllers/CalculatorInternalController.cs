using EPR.Calculator.API.Data;
using EPR.Calculator.API.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.API.Controllers
{
    [Route("v1/internal")]
    [ApiController]
    public class CalculatorInternalController : ControllerBase
    {
        private readonly ApplicationDBContext context;

        public CalculatorInternalController(ApplicationDBContext context)
        {
            this.context = context;
        }

        [HttpPost]
        [Route("rpdStatus")]
        public IActionResult UpdateRpdStatus([FromBody] UpdateRpdStatus request)
        {
            var runId = request.RunId;

            var calcRunExist = this.context.CalculatorRuns.Any(run => run.Id == runId);
            if (!calcRunExist)
            {
                return StatusCode(StatusCodes.Status400BadRequest, $"Calculator Run for {runId} is missing");
            }

            var pomDataExists = this.context.PomData.Any();
            if (!pomDataExists)
            {
                return StatusCode(StatusCodes.Status422UnprocessableEntity, $"PomData for {runId} is missing");
            }

            var organisationDataExists = this.context.OrganisationData.Any();
            if (!organisationDataExists)
            {
                return StatusCode(StatusCodes.Status422UnprocessableEntity, $"OrganisationData for {runId} is missing");
            }

            return new ObjectResult(null) { StatusCode = StatusCodes.Status201Created };
        }
    }
}
