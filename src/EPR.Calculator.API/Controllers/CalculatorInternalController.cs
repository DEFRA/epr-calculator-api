using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
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

            var stagingOrganisationData = this.context.OrganisationData.ToList();
            foreach ( var organisation in stagingOrganisationData )
            {
                var calcOrganisationMaster = new CalculatorRunOrganisationDataMaster
                {
                    CalendarYear = "2024-25", //Take the financial year from Calc Run table and Derive the Calendar year
                    CreatedAt = DateTime.Now,
                    CreatedBy = request.UpdatedBy,
                    EffectiveFrom = DateTime.Now,
                    EffectiveTo = null,
                };

                var calcOrganisationDataDetail = new CalculatorRunOrganisationDataDetail
                {
                    OrganisationId = organisation.OrganisationId,
                    SubsidaryId = organisation.SubsidaryId,
                    LoadTimeStamp = organisation.LoadTimestamp,
                    OrganisationName = organisation.OrganisationName,
                    CalculatorRunOrganisationDataMaster = calcOrganisationMaster,
                };

                this.context.CalculatorRunOrganisationDataDetails.Add( calcOrganisationDataDetail );
                var calcRun = this.context.CalculatorRuns.Single(run => run.Id == request.RunId);
                calcRun.CalculatorRunOrganisationDataMaster = calcOrganisationMaster;
                this.context.SaveChanges();
            }

            return new ObjectResult(null) { StatusCode = StatusCodes.Status201Created };
        }
    }
}
