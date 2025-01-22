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
using Microsoft.AspNetCore.Http.Timeouts;
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
        private readonly IRpdStatusDataValidator rpdStatusDataValidator;
        private readonly IOrgAndPomWrapper wrapper;
        private readonly ICalcResultBuilder builder;
        private readonly ICalcResultsExporter<CalcResult> exporter;
        private readonly ITransposePomAndOrgDataService transposePomAndOrgDataService;
        private readonly IStorageService storageService;
        private readonly IConfiguration configuration;

        public CalculatorInternalController(ApplicationDBContext context,
                                            IRpdStatusDataValidator rpdStatusDataValidator,
                                            IOrgAndPomWrapper wrapper,
                                            ICalcResultBuilder builder,
                                            ICalcResultsExporter<CalcResult> exporter,
                                            ITransposePomAndOrgDataService transposePomAndOrgDataService,
                                            IStorageService storageService,
                                            IConfiguration configuration)
        {
            this.context = context;
            this.rpdStatusDataValidator = rpdStatusDataValidator;
            this.wrapper = wrapper;
            this.builder = builder;
            this.exporter = exporter;
            this.transposePomAndOrgDataService = transposePomAndOrgDataService;
            this.storageService = storageService;
            this.configuration = configuration;
        }

        [HttpPost]
        [Route("rpdStatus")]
        [RequestTimeout("RpdStatus")]
        public async Task<IActionResult> UpdateRpdStatus([FromBody] UpdateRpdStatus request)
        {
            SetCommandTimeout("RpdStatusCommand");

            try
            {
                var runId = request.RunId;
                var calcRun = await this.context.CalculatorRuns.SingleOrDefaultAsync(
                    run => run.Id == runId,
                    HttpContext.RequestAborted);
                var runClassifications = await this.context.CalculatorRunClassifications
                    .ToListAsync(HttpContext.RequestAborted);

                var validationResult = this.rpdStatusDataValidator.IsValidRun(calcRun, runId, runClassifications);
                if (!validationResult.isValid)
                {
                    return StatusCode(validationResult.StatusCode, validationResult.ErrorMessage);
                }

                if (!request.isSuccessful && calcRun != null)
                {
                    calcRun.CalculatorRunClassificationId = runClassifications.Single(x => x.Status == RunClassification.ERROR.ToString()).Id;
                    await this.context.SaveChangesAsync(HttpContext.RequestAborted);
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
                using (var transaction = await this.context.Database.BeginTransactionAsync(HttpContext.RequestAborted))
                {
                    try
                    {
                        var createRunOrgCommand = Util.GetFormattedSqlString("dbo.CreateRunOrganization", runId, calendarYear, createdBy);
                        await this.wrapper.ExecuteSqlAsync(createRunOrgCommand, HttpContext.RequestAborted);
                        var createRunPomCommand = Util.GetFormattedSqlString("dbo.CreateRunPom", runId, calendarYear, createdBy);
                        await this.wrapper.ExecuteSqlAsync(createRunPomCommand, HttpContext.RequestAborted);

                        calcRun!.CalculatorRunClassificationId = runClassifications.Single(x => x.Status == RunClassification.RUNNING.ToString()).Id;
                        await this.context.SaveChangesAsync(HttpContext.RequestAborted);
                        await transaction.CommitAsync(HttpContext.RequestAborted);
                        return new ObjectResult(null) { StatusCode = StatusCodes.Status201Created };
                    }
                    catch (OperationCanceledException ex)
                    {
                        await transaction.RollbackAsync();
                        return StatusCode(StatusCodes.Status408RequestTimeout, ex.ToString());
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        return StatusCode(StatusCodes.Status500InternalServerError, ex);
                    }
                }
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(StatusCodes.Status408RequestTimeout, ex.ToString());
            }
        }

        [HttpPost]
        [Route("transposeBeforeCalcResults")]
        [RequestTimeout("Transpose")]
        public async Task<IActionResult> TransposeBeforeCalcResults([FromBody] CalcResultsRequestDto resultsRequestDto)
        {
            var startTime = DateTime.Now; 
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ModelState.Values.SelectMany(x => x.Errors));
            }

            SetCommandTimeout("TransposeCommand");

            CalculatorRun? calculatorRun = null;
            try
            {
                calculatorRun = await this.context.CalculatorRuns.SingleOrDefaultAsync(
                run => run.Id == resultsRequestDto.RunId,
                HttpContext.RequestAborted);
                if (calculatorRun == null)
                {
                    return new ObjectResult($"Unable to find Run Id {resultsRequestDto.RunId}")
                    { StatusCode = StatusCodes.Status404NotFound };
                }


                var isTransposeSuccessful = await this.transposePomAndOrgDataService.Transpose(
                    resultsRequestDto,
                    HttpContext.RequestAborted);
                var endTime = DateTime.Now;
                var timeDiff = startTime - endTime;
                return new ObjectResult(timeDiff.TotalMinutes) { StatusCode = StatusCodes.Status201Created };
            }
            catch (OperationCanceledException exception)
            {
                if (calculatorRun != null)
                {
                    calculatorRun.CalculatorRunClassificationId = (int)RunClassification.ERROR;
                    this.context.CalculatorRuns.Update(calculatorRun);
                    await this.context.SaveChangesAsync();
                }
                return StatusCode(StatusCodes.Status408RequestTimeout, exception.ToString());
            }
            catch (Exception exception)
            {
                if (calculatorRun != null)
                {
                    calculatorRun.CalculatorRunClassificationId = (int)RunClassification.ERROR;
                    this.context.CalculatorRuns.Update(calculatorRun);
                    await this.context.SaveChangesAsync();
                }
                return StatusCode(StatusCodes.Status500InternalServerError, exception.ToString());
            }
        }

        [HttpPost]
        [Route("prepareCalcResults")]
        [RequestTimeout("PrepareCalcResults")]
        public async Task<IActionResult> PrepareCalcResults([FromBody] CalcResultsRequestDto resultsRequestDto)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ModelState.Values.SelectMany(x => x.Errors));
            }

            SetCommandTimeout("PrepareCalcResultsCommand");

            CalculatorRun? calculatorRun = null;
            try
            {
                calculatorRun = await this.context.CalculatorRuns.SingleOrDefaultAsync(
                    run => run.Id == resultsRequestDto.RunId,
                    HttpContext.RequestAborted);
                if (calculatorRun == null)
                {
                    return new ObjectResult($"Unable to find Run Id {resultsRequestDto.RunId}")
                    { StatusCode = StatusCodes.Status404NotFound };
                }

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
                    await this.context.SaveChangesAsync(HttpContext.RequestAborted);
                    return new ObjectResult(null) { StatusCode = StatusCodes.Status201Created };
                }
            }
            catch (OperationCanceledException exception)
            {
                if (calculatorRun != null)
                {
                    calculatorRun.CalculatorRunClassificationId = (int)RunClassification.ERROR;
                    this.context.CalculatorRuns.Update(calculatorRun);
                    await this.context.SaveChangesAsync();
                }
                return StatusCode(StatusCodes.Status408RequestTimeout, exception.ToString());
            }
            catch (Exception exception)
            {
                if (calculatorRun != null)
                {
                    calculatorRun.CalculatorRunClassificationId = (int)RunClassification.ERROR;
                    this.context.CalculatorRuns.Update(calculatorRun);
                    await this.context.SaveChangesAsync();
                }
                return StatusCode(StatusCodes.Status500InternalServerError, exception.ToString());
            }
            calculatorRun.CalculatorRunClassificationId = (int)RunClassification.ERROR;
            this.context.CalculatorRuns.Update(calculatorRun);
            await this.context.SaveChangesAsync();
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        private void SetCommandTimeout(string key)
        {
            var commandTimeout = this.configuration
                .GetSection("Timeouts")
                .GetValue<double>(key);
            if (commandTimeout > 0)
            {
                context.Database.SetCommandTimeout(TimeSpan.FromMinutes(commandTimeout));
            }
        }
    }
}
