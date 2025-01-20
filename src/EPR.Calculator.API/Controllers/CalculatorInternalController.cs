using EPR.Calculator.API.Builder;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Enums;
using EPR.Calculator.API.Exporter;
using EPR.Calculator.API.Models;
using EPR.Calculator.API.Services;
using EPR.Calculator.API.Utils;
using EPR.Calculator.API.Validators;
using EPR.Calculator.API.Wrapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        private readonly IStorageService storageService;

        public CalculatorInternalController(ApplicationDBContext context,
                                            IRpdStatusDataValidator rpdStatusDataValidator,
                                            IOrgAndPomWrapper wrapper,
                                            ICalcResultBuilder builder,
                                            ICalcResultsExporter<CalcResult> exporter,
                                            ITransposePomAndOrgDataService transposePomAndOrgDataService,
                                            IStorageService storageService)
        {
            this.context = context;
            this.rpdStatusDataValidator = rpdStatusDataValidator;
            this.wrapper = wrapper;
            this.builder = builder;
            this.exporter = exporter;
            this.transposePomAndOrgDataService = transposePomAndOrgDataService;
            this.storageService = storageService;
        }

        [HttpPost]
        [Route("rpdStatus")]
        public async Task<IActionResult> UpdateRpdStatus([FromBody] UpdateRpdStatus request)
        {
            var runId = request.RunId;
            var calcRun = await this.context.CalculatorRuns.SingleOrDefaultAsync(run => run.Id == runId);
            var runClassifications = await this.context.CalculatorRunClassifications.ToListAsync();

            var validationResult = this.rpdStatusDataValidator.IsValidRun(calcRun, runId, runClassifications);
            if (!validationResult.isValid)
            {
                return StatusCode(validationResult.StatusCode, validationResult.ErrorMessage);
            }

            if (!request.isSuccessful && calcRun != null)
            {
                calcRun.CalculatorRunClassificationId = runClassifications.Single(x => x.Status == RunClassification.ERROR.ToString()).Id;
                await this.context.SaveChangesAsync();
                return new ObjectResult(null) { StatusCode = StatusCodes.Status201Created };
            }

            var vr = this.rpdStatusDataValidator.IsValidSuccessfulRun(runId);
            if (!vr.isValid)
            {
                return StatusCode(vr.StatusCode, vr.ErrorMessage);
            }

            string financialYear = calcRun?.Financial_Year ?? string.Empty;
            var calendarYear = Util.GetCalendarYear(financialYear);
            var createdBy = request.UpdatedBy;
            using (var transaction = await this.context.Database.BeginTransactionAsync())
            {
                try
                {
                    var  createRunOrgCommand = Util.GetFormattedSqlString("dbo.CreateRunOrganization", runId, calendarYear, createdBy);
                    await this.wrapper.ExecuteSqlAsync(createRunOrgCommand);
                    var createRunPomCommand = Util.GetFormattedSqlString("dbo.CreateRunPom", runId, calendarYear, createdBy);
                    await this.wrapper.ExecuteSqlAsync(createRunPomCommand);

                    calcRun!.CalculatorRunClassificationId = runClassifications.Single(x => x.Status == RunClassification.RUNNING.ToString()).Id;
                    await this.context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return new ObjectResult(null) { StatusCode = StatusCodes.Status201Created };
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(StatusCodes.Status500InternalServerError, ex);
                }
            }
        }

        [HttpPost]
        [Route("transposeBeforeCalcResults")]
        public async Task<IActionResult> TransposeBeforeCalcResults([FromBody] CalcResultsRequestDto resultsRequestDto)
        {
            var startTime = DateTime.Now; 
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ModelState.Values.SelectMany(x => x.Errors));
            }

            var calculatorRun = await this.context.CalculatorRuns.SingleOrDefaultAsync(run => run.Id == resultsRequestDto.RunId);
            if (calculatorRun == null)
            {
                return new ObjectResult($"Unable to find Run Id {resultsRequestDto.RunId}")
                { StatusCode = StatusCodes.Status404NotFound };
            }

            try
            {
                var isTransposeSuccessful = await this.transposePomAndOrgDataService.Transpose(resultsRequestDto);
                var endTime = DateTime.Now;
                var timeDiff = startTime - endTime;
                return new ObjectResult(timeDiff.TotalMinutes) { StatusCode = StatusCodes.Status201Created };
            }
            catch (Exception exception)
            {
                calculatorRun.CalculatorRunClassificationId = (int)RunClassification.ERROR;
                this.context.CalculatorRuns.Update(calculatorRun);
                await this.context.SaveChangesAsync();
                return StatusCode(StatusCodes.Status500InternalServerError, exception);
            }
        }

        [HttpPost]
        [Route("prepareCalcResults")]
        public async Task<IActionResult> PrepareCalcResults([FromBody] CalcResultsRequestDto resultsRequestDto)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ModelState.Values.SelectMany(x => x.Errors));
            }

            var calculatorRun = await this.context.CalculatorRuns.SingleOrDefaultAsync(run => run.Id == resultsRequestDto.RunId);
            if (calculatorRun == null)
            {
                return new ObjectResult($"Unable to find Run Id {resultsRequestDto.RunId}")
                    { StatusCode = StatusCodes.Status404NotFound };
            }

            try
            {
                var results = await this.builder.Build(resultsRequestDto);
                var exportedResults = this.exporter.Export(results);

                var fileName = new CalcResultsFileName(
                    results.CalcResultDetail.RunId,
                    results.CalcResultDetail.RunName,
                    results.CalcResultDetail.RunDate);
                var resultsFileWritten = await this.storageService.UploadResultFileContentAsync(fileName, exportedResults);

                if (resultsFileWritten)
                {
                    calculatorRun.CalculatorRunClassificationId = (int)RunClassification.UNCLASSIFIED;
                    this.context.CalculatorRuns.Update(calculatorRun);
                    await this.context.SaveChangesAsync();
                    return new ObjectResult(null) { StatusCode = StatusCodes.Status201Created };
                }
            }
            catch (Exception exception)
            {
                calculatorRun.CalculatorRunClassificationId = (int)RunClassification.ERROR;
                this.context.CalculatorRuns.Update(calculatorRun);
                await this.context.SaveChangesAsync();
                return StatusCode(StatusCodes.Status500InternalServerError, exception);
            }
            calculatorRun.CalculatorRunClassificationId = (int)RunClassification.ERROR;
            this.context.CalculatorRuns.Update(calculatorRun);
            await this.context.SaveChangesAsync();
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
