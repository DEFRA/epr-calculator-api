using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

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

            var calcRun = this.context.CalculatorRuns.SingleOrDefault(run => run.Id == runId);
            if (calcRun == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, $"Calculator Run for {runId} is missing");
            }

            if (calcRun.CalculatorRunOrganisationDataMasterId != null)
            {
                return StatusCode(StatusCodes.Status422UnprocessableEntity,
                    $"Calculator Run for {runId} already has OrganisationDataMasterId associated with it");
            }

            if (calcRun.CalculatorRunPomDataMasterId != null)
            {
                return StatusCode(StatusCodes.Status422UnprocessableEntity,
                    $"Calculator Run for {runId} already has PomDataMasterId associated with it");
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

            using (var transaction = this.context.Database.BeginTransaction())
            {
                try
                {
                    var stagingOrganisationData = this.context.OrganisationData.ToList();
                    foreach (var organisation in stagingOrganisationData)
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


                        this.context.CalculatorRunOrganisationDataDetails.Add(calcOrganisationDataDetail);
                        calcRun.CalculatorRunOrganisationDataMaster = calcOrganisationMaster;
                    }

                    var stagingPomData = this.context.PomData.ToList();
                    foreach (var pomData in stagingPomData)
                    {
                        var calcRunPomMaster = new CalculatorRunPomDataMaster
                        {
                            CalendarYear = "2024-25", //Take the financial year from Calc Run table and Derive the Calendar year
                            CreatedAt = DateTime.Now,
                            CreatedBy = request.UpdatedBy,
                            EffectiveFrom = DateTime.Now,
                            EffectiveTo = null,
                        };

                        var calcRuntPomDataDetail = new CalculatorRunPomDataDetail
                        {
                            OrganisationId = pomData.OrganisationId,
                            SubsidaryId = pomData.SubsidaryId,
                            LoadTimeStamp = pomData.LoadTimeStamp,
                            SubmissionPeriod = pomData.SubmissionPeriod,
                            PackagingActivity = pomData.PackagingActivity,
                            PackagingType = pomData.PackagingType,
                            PackagingClass = pomData.PackagingClass,
                            PackagingMaterial = pomData.PackagingMaterial,
                            PackagingMaterialWeight = pomData.PackagingMaterialWeight,
                            CalculatorRunPomDataMaster = calcRunPomMaster,
                            CalculatorRunPomDataMasterId = 0
                        };


                        this.context.CalculatorRunPomDataDetails.Add(calcRuntPomDataDetail);
                        calcRun.CalculatorRunPomDataMaster = calcRunPomMaster;
                    }
                    this.context.SaveChanges();
                    transaction.Commit();
                    return new ObjectResult(null) { StatusCode = StatusCodes.Status201Created };
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return StatusCode(StatusCodes.Status500InternalServerError, ex);
                }
            }
        }
    }
}
