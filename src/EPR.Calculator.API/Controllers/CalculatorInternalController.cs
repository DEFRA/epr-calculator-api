﻿using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Models;
using EPR.Calculator.API.Validators;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.API.Controllers
{
    [Route("v1/internal")]
    [ApiController]
    public class CalculatorInternalController : ControllerBase
    {
        private readonly ApplicationDBContext context;
        private readonly IRpdStatusDataValidator rpdStatusDataValidator;

        public CalculatorInternalController(ApplicationDBContext context, IRpdStatusDataValidator rpdStatusDataValidator)
        {
            this.context = context;
            this.rpdStatusDataValidator = rpdStatusDataValidator;
        }

        [HttpPost]
        [Route("rpdStatus")]
        public IActionResult UpdateRpdStatus([FromBody] UpdateRpdStatus request)
        {
            var runId = request.RunId;
            var calcRun = this.context.CalculatorRuns.SingleOrDefault(run => run.Id == runId);
            var runClassifications = this.context.CalculatorRunClassifications.ToList();

            var validationResult = this.rpdStatusDataValidator.IsValidRun(calcRun, runId, runClassifications);
            if (!validationResult.isValid)
            {
                return StatusCode(validationResult.StatusCode, validationResult.ErrorMessage);
            }

            if (!request.isSuccessful && calcRun != null)
            {
                calcRun.CalculatorRunClassificationId = runClassifications.Single(x => x.Status == RunClassiciations.ERROR.ToString()).Id;
                this.context.SaveChanges();
                return new ObjectResult(null) { StatusCode = StatusCodes.Status201Created };
            }

            var vr = this.rpdStatusDataValidator.IsValidSuccessfulRun(runId);
            if (!vr.isValid)
            {
                return StatusCode(vr.StatusCode, vr.ErrorMessage);
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
                            CalculatorRunOrganisationDataMasterId = 0
                        };


                        this.context.CalculatorRunOrganisationDataDetails.Add(calcOrganisationDataDetail);
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                        calcRun.CalculatorRunOrganisationDataMaster = calcOrganisationMaster;
#pragma warning restore CS8602 // Dereference of a possibly null reference.
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
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                        calcRun.CalculatorRunPomDataMaster = calcRunPomMaster;
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                    }


#pragma warning disable CS8602 // Dereference of a possibly null reference.
                    calcRun.CalculatorRunClassificationId = runClassifications.Single(x => x.Status == RunClassiciations.RUNNING.ToString()).Id;
#pragma warning restore CS8602 // Dereference of a possibly null reference.
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
