using EPR.Calculator.API.Data;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Enums;
using EPR.Calculator.API.Mappers;
using EPR.Calculator.API.Services;
using EPR.Calculator.API.Services.Abstractions;
using EPR.Calculator.API.Utils;
using EPR.Calculator.API.Validators;
using EPR.Calculator.API.Wrapper;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.Controllers
{
    [Route("V2")]
    [ApiController]
    public class CalculatorNewController : ControllerBase
    {
        private readonly ApplicationDBContext context;
        private readonly ICalculatorRunStatusDataValidator calculatorRunStatusDataValidator;
        private readonly IBillingFileService billingFileService;
        private readonly TelemetryClient telemetryClient;
        private readonly ICalculationRunService calculationRunService;

        private IOrgAndPomWrapper Wrapper { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CalculatorNewController"/> class.
        /// </summary>
        /// <param name="context">Db Context.</param>
        /// <param name="calculatorRunStatusDataValidator">Db Validator.</param>
        /// <param name="billingFileService">Service for handling billing file operations.</param>
        /// <param name="wrapper">Wrapper for organization and POM data.</param>
        /// <param name="telemetryClient">Telemetry client for logging and tracking.</param>
        /// <param name="calculationRunService">Service for managing calculation runs.</param>
        public CalculatorNewController(
            ApplicationDBContext context,
            ICalculatorRunStatusDataValidator calculatorRunStatusDataValidator,
            IBillingFileService billingFileService,
            IOrgAndPomWrapper wrapper,
            TelemetryClient telemetryClient,
            ICalculationRunService calculationRunService)
        {

            this.context = context;

            this.calculatorRunStatusDataValidator = calculatorRunStatusDataValidator;

            this.billingFileService = billingFileService;

            this.Wrapper = wrapper;

            this.telemetryClient = telemetryClient;

            this.calculationRunService = calculationRunService;
        }
        [HttpPut]
        [Route("calculatorRuns")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PutCalculatorRunStatus([FromBody] CalculatorRunStatusUpdateDto runStatusUpdateDto)
        {
            try
            {
                var claim = this.User.Claims.FirstOrDefault(x => x.Type == "name");
                if (claim == null)
                {
                    return new ObjectResult(CommonResources.NoClaimInRequest) { StatusCode = StatusCodes.Status401Unauthorized };
                }

                var userName = claim.Value;

                var classification = await this.context.CalculatorRunClassifications.SingleOrDefaultAsync(x =>
                    x.Id == runStatusUpdateDto.ClassificationId);

                if (classification == null)
                {
                    return new ObjectResult(string.Format(CommonResources.UnableToFindClassificationId, runStatusUpdateDto.ClassificationId))
                    { StatusCode = StatusCodes.Status422UnprocessableEntity };
                }

                var calculatorRun = await this.context.CalculatorRuns.SingleOrDefaultAsync(
                            x => x.Id == runStatusUpdateDto.RunId);
                if (calculatorRun == null)
                {
                    return new ObjectResult(string.Format(CommonResources.UnableToFindRunId, runStatusUpdateDto.RunId))
                    { StatusCode = StatusCodes.Status422UnprocessableEntity };
                }

                // Perform basic validation on classification status
                GenericValidationResultDto genericValidationResultDto = this.calculatorRunStatusDataValidator.Validate(calculatorRun, runStatusUpdateDto);

                if (genericValidationResultDto.IsInvalid)
                {
                    return new ObjectResult(genericValidationResultDto.Errors)
                    { StatusCode = StatusCodes.Status422UnprocessableEntity };
                }

                // Perform validation to check other designated runs are not in progress and not already completed for the same financial year
                List<ClassifiedCalculatorRunDto> designatedRuns = await this.calculationRunService.GetDesignatedRunsByFinanialYear(
                    calculatorRun.FinancialYearId);

                genericValidationResultDto = this.calculatorRunStatusDataValidator.Validate(designatedRuns, calculatorRun, runStatusUpdateDto);

                if (genericValidationResultDto.IsInvalid)
                {
                    return new ObjectResult(genericValidationResultDto.Errors)
                    { StatusCode = StatusCodes.Status422UnprocessableEntity };
                }

                calculatorRun.CalculatorRunClassificationId = runStatusUpdateDto.ClassificationId;
                calculatorRun.UpdatedAt = DateTime.UtcNow;
                calculatorRun.UpdatedBy = userName;

                this.context.CalculatorRuns.Update(calculatorRun);
                await this.context.SaveChangesAsync();

                return this.StatusCode(201);
            }
            catch (Exception exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, exception);
            }
        }

        [HttpGet]
        [Route("calculatorRuns/{runId}")]
        [ProducesResponseType(typeof(CalculatorRunBillingDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCalculatorRun(int runId)
        {
            if (runId <= 0)
            {
                return this.StatusCode(StatusCodes.Status400BadRequest, string.Format(CommonResources.InvalidForRunId, runId));
            }

            try
            {
                var calculatorRunDetail =
                    await (from run in this.context.CalculatorRuns
                           join classification in this.context.CalculatorRunClassifications
                               on run.CalculatorRunClassificationId equals classification.Id
                           join billingfilemetadata in this.context.CalculatorRunBillingFileMetadata
                               on run.Id equals billingfilemetadata.CalculatorRunId into calculatorrunbillingFile
                           from billingfilemetadata in calculatorrunbillingFile.DefaultIfEmpty()
                           where run.Id == runId
                           select new
                           {
                               Run = run,
                               Classification = classification,
                               BillingFileMetadata = billingfilemetadata,
                           }).OrderByDescending(x => x.BillingFileMetadata.BillingFileCreatedDate)
                           .FirstOrDefaultAsync();

                if (calculatorRunDetail == null)
                {
                    return new NotFoundObjectResult(string.Format(CommonResources.UnableToFindRunId, runId));
                }

                var calcRun = calculatorRunDetail.Run;
                var runClassification = calculatorRunDetail.Classification;
                var calculatorRunbillingFile = calculatorRunDetail.BillingFileMetadata;
                var runDto = CalcRunBillingFileMapper.Map(calcRun, runClassification, calculatorRunbillingFile);
                return new ObjectResult(runDto);
            }
            catch (Exception exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, exception);
            }
        }

        [HttpPost]
        [Route("prepareBillingFileSendToFSS/{runId}")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> PrepareBillingFileSendToFSS(int runId, CancellationToken cancellationToken = default)
        {
            var claim = this.User.Claims.FirstOrDefault(x => x.Type == "name");
            if (claim == null)
            {
                return new ObjectResult(CommonResources.NoClaimInRequest) { StatusCode = StatusCodes.Status401Unauthorized };
            }

            var userName = claim.Value;
            try
            {
                if (runId <= 0)
                {
                    return this.StatusCode(StatusCodes.Status400BadRequest, string.Format(CommonResources.InvalidForRunId, runId));
                }

                var calculatorRun = await this.context.CalculatorRuns.SingleOrDefaultAsync(x => x.Id == runId);
                if (calculatorRun == null)
                {
                    return new ObjectResult(string.Format(CommonResources.UnableToFindRunId, runId))
                    { StatusCode = StatusCodes.Status422UnprocessableEntity };
                }

                // Update calculation run classification status: Initial run completed
                calculatorRun.CalculatorRunClassificationId = (int)RunClassification.INITIAL_RUN_COMPLETED;
                var metadata = await this.context.CalculatorRunBillingFileMetadata.
                    Where(x => x.CalculatorRunId == runId).OrderByDescending(x => x.BillingFileCreatedDate).
                    FirstOrDefaultAsync();

                if (metadata == null)
                {
                    return new ObjectResult(string.Format(CommonResources.UnableToFindBillingFileMetadata, runId))
                    { StatusCode = StatusCodes.Status422UnprocessableEntity };
                }

                metadata.BillingFileAuthorisedBy = userName;
                metadata.BillingFileAuthorisedDate = DateTime.UtcNow;

                using (var transaction = await this.context.Database.BeginTransactionAsync(cancellationToken))
                {
                    try
                    {
                        var createRunInvoiceDetailsCommand = Util.GetFormattedSqlString(
                            CommonResources.InsertInvoiceDetailsAtProducerLevel,
                            metadata.BillingFileAuthorisedBy,
                            metadata.BillingFileAuthorisedDate,
                            runId);

                        var affectedRows = await this.Wrapper.ExecuteSqlAsync(createRunInvoiceDetailsCommand, cancellationToken);

                        this.telemetryClient.TrackEvent(CommonResources.InsertInvoiceDetailsAtProducerLevel, new Dictionary<string, string>
                        {
                            { "Procedure", CommonResources.InsertInvoiceDetailsAtProducerLevel },
                            { "RunId", runId.ToString() },
                            { "RowsAffected", affectedRows.ToString() },
                        });

                        this.context.CalculatorRuns.Update(calculatorRun);
                        await this.context.SaveChangesAsync(cancellationToken);
                        var result = await this.billingFileService.MoveBillingJsonFile(runId, cancellationToken);
                        if (!result)
                        {
                            return this.StatusCode(StatusCodes.Status422UnprocessableEntity, string.Format(CommonResources.UnableToMoveBillingFile, runId));
                        }

                        // All good, commit transaction
                        await transaction.CommitAsync(cancellationToken);
                    }

                    // All good, commit transaction
                    catch (Exception exception)
                    {
                        // Error, rollback transaction
                        await transaction.RollbackAsync(cancellationToken);

                        this.telemetryClient.TrackException(exception, new Dictionary<string, string>
                        {
                            { "RunId", runId.ToString() },
                        });

                        // Return error status code: Internal Server Error
                        return this.StatusCode(StatusCodes.Status500InternalServerError, exception);
                    }
                }

                // Return accepted status code
                return this.StatusCode(StatusCodes.Status202Accepted);
            }
            catch (Exception exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, exception);
            }
        }
    }
}