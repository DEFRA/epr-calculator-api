using EPR.Calculator.API.Builder;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Enums;
using EPR.Calculator.API.Exporter;
using EPR.Calculator.API.Models;
using EPR.Calculator.API.Services;
using EPR.Calculator.API.Utils;
using EPR.Calculator.API.Validators;
using EPR.Calculator.API.Wrapper;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.API.Controllers
{
    [Route("v1/internal")]
    [ApiController]
    public class CalculatorInternalController : ControllerBase
    {
        private readonly ApplicationDBContext context;
        private readonly IRpdStatusDataValidator rpdStatusDataValidator;
        private readonly IOrgAndPomWrapper wrapper;
        private readonly ICalcResultBuilder builder;
        private readonly ICalcResultsExporter<CalcResult> exporter;
        private readonly ITransposePomAndOrgDataService transposePomAndOrgDataService;

        public CalculatorInternalController(ApplicationDBContext context,
                                            IRpdStatusDataValidator rpdStatusDataValidator,
                                            IOrgAndPomWrapper wrapper,
                                            ICalcResultBuilder builder,
                                            ICalcResultsExporter<CalcResult> exporter,
                                            ITransposePomAndOrgDataService transposePomAndOrgDataService)
        {
            this.context = context;
            this.rpdStatusDataValidator = rpdStatusDataValidator;
            this.wrapper = wrapper;
            this.builder = builder;
            this.exporter = exporter;
            this.transposePomAndOrgDataService = transposePomAndOrgDataService;
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
                calcRun.CalculatorRunClassificationId = runClassifications.Single(x => x.Status == RunClassification.ERROR.ToString()).Id;
                this.context.SaveChanges();
                return new ObjectResult(null) { StatusCode = StatusCodes.Status201Created };
            }

            var vr = this.rpdStatusDataValidator.IsValidSuccessfulRun(runId);
            if (!vr.isValid)
            {
                return StatusCode(vr.StatusCode, vr.ErrorMessage);
            }

            string financialYear = calcRun?.Financial_Year ?? string.Empty;

            using (var transaction = this.context.Database.BeginTransaction())
            {
                try
                {
                    var stagingOrganisationData = this.wrapper.GetOrganisationData();
                    var calcOrganisationMaster = new CalculatorRunOrganisationDataMaster
                    {
                        CalendarYear = Util.GetCalendarYear(financialYear),
                        CreatedAt = DateTime.Now,
                        CreatedBy = request.UpdatedBy,
                        EffectiveFrom = DateTime.Now,
                        EffectiveTo = null,
                    };
                    foreach (var organisation in stagingOrganisationData)
                    {
                        var calcOrganisationDataDetail = new CalculatorRunOrganisationDataDetail
                        {
                            OrganisationId = organisation.OrganisationId,
                            SubsidaryId = organisation.SubsidaryId,
                            LoadTimeStamp = organisation.LoadTimestamp,
                            OrganisationName = organisation.OrganisationName,
                            SubmissionPeriodDesc = organisation.SubmissionPeriodDesc,
                            CalculatorRunOrganisationDataMaster = calcOrganisationMaster,
                        };


                        this.context.CalculatorRunOrganisationDataDetails.Add(calcOrganisationDataDetail);
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                        calcRun.CalculatorRunOrganisationDataMaster = calcOrganisationMaster;
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                    }

                    var stagingPomData = this.wrapper.GetPomData();
                    var calcRunPomMaster = new CalculatorRunPomDataMaster
                    {
                        CalendarYear = Util.GetCalendarYear(financialYear),
                        CreatedAt = DateTime.Now,
                        CreatedBy = request.UpdatedBy,
                        EffectiveFrom = DateTime.Now,
                        EffectiveTo = null,
                    };
                    foreach (var pomData in stagingPomData)
                    {
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
                            SubmissionPeriodDesc = pomData.SubmissionPeriodDesc,
                            CalculatorRunPomDataMaster = calcRunPomMaster,
                        };

                        this.context.CalculatorRunPomDataDetails.Add(calcRuntPomDataDetail);
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                        calcRun.CalculatorRunPomDataMaster = calcRunPomMaster;
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                    }


#pragma warning disable CS8602 // Dereference of a possibly null reference.
                    calcRun.CalculatorRunClassificationId = runClassifications.Single(x => x.Status == RunClassification.RUNNING.ToString()).Id;
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                    this.context.SaveChanges();
                    transaction.Commit();
                    return new ObjectResult(null) { StatusCode = StatusCodes.Status201Created };
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    // return StatusCode(StatusCodes.Status500InternalServerError, ex);
                    throw;
                }
            }
        }

        [HttpPost]
        [Route("prepareCalcResults")]
        public IActionResult PrepareCalcResults([FromBody] CalcResultsRequestDto resultsRequestDto)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ModelState.Values.SelectMany(x => x.Errors));
            }

            this.transposePomAndOrgDataService.Transpose(resultsRequestDto);
            var results = this.builder.Build(resultsRequestDto);
            this.exporter.Export(results);
            return new ObjectResult(null) { StatusCode = StatusCodes.Status201Created };
        }
    }
}
