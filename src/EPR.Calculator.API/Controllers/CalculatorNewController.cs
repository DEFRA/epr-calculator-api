using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.Enums;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Enums;
using EPR.Calculator.API.Mappers;
using EPR.Calculator.API.Services;
using EPR.Calculator.API.Services.Abstractions;
using EPR.Calculator.API.Validators;
using EPR.Calculator.Service.Function.Services;
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
        private readonly IInvoiceDetailsService invoiceDetailsService;

        /// <summary>
        /// Initializes a new instance of the <see cref="CalculatorNewController"/> class.
        /// </summary>
        /// <param name="context">Db Context.</param>
        /// <param name="calculatorRunStatusDataValidator">Db Validator.</param>
        /// <param name="billingFileService">Service for handling billing file operations.</param>
        /// <param name="invoiceDetailsService">Service for inserting invoice details.</param>
        /// <param name="telemetryClient">Telemetry client for logging and tracking.</param>
        /// <param name="calculationRunService">Service for managing calculation runs.</param>
        public CalculatorNewController(
            ApplicationDBContext context,
            ICalculatorRunStatusDataValidator calculatorRunStatusDataValidator,
            IBillingFileService billingFileService,
            IInvoiceDetailsService invoiceDetailsService,
            TelemetryClient telemetryClient,
            ICalculationRunService calculationRunService)
        {
            this.context = context;

            this.calculatorRunStatusDataValidator = calculatorRunStatusDataValidator;

            this.billingFileService = billingFileService;

            this.invoiceDetailsService = invoiceDetailsService;

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

                // Perform validation to check other designated runs are not in progress and not already completed for the same relative year
                List<CalculatorRunDto> designatedRuns = await this.calculationRunService.GetDesignatedRunsByFinanialYear(calculatorRun.RelativeYear);

                genericValidationResultDto = this.calculatorRunStatusDataValidator.Validate(designatedRuns, calculatorRun, runStatusUpdateDto);

                if (genericValidationResultDto.IsInvalid)
                {
                    return new ObjectResult(genericValidationResultDto.Errors)
                    { StatusCode = StatusCodes.Status422UnprocessableEntity };
                }

                calculatorRun.Classification = runStatusUpdateDto.Classification;
                calculatorRun.UpdatedAt = DateTime.UtcNow;
                calculatorRun.UpdatedBy = userName;

                this.context.CalculatorRuns.Update(calculatorRun);
                await this.context.SaveChangesAsync();

                return this.StatusCode(201);
            }
            catch (Exception exception)
            {
                this.telemetryClient.TrackException(exception);

                Console.WriteLine(exception.ToString());

                return this.StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        message = "An unexpected error occurred.",
                        detail = exception.Message,
                        traceId = HttpContext.TraceIdentifier
                    });
            }
        }

        [HttpGet]
        [Route("calculatorRuns/{runId}")]
        [ProducesResponseType(typeof(CalculatorRunDto), StatusCodes.Status200OK)]
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
                var runDto = await this.context.CalculatorRuns
                    .Where(x => x.Id == runId)
                    .Select(CalcRunMapper.ToDto)
                    .SingleOrDefaultAsync();

                if (runDto == null)
                {
                    return new NotFoundObjectResult(string.Format(CommonResources.UnableToFindRunId, runId));
                }

                return new ObjectResult(runDto);
            }
            catch (Exception exception)
            {
                this.telemetryClient.TrackException(exception);

                Console.WriteLine(exception.ToString());

                return this.StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        message = "An unexpected error occurred.",
                        detail = exception.Message,
                        traceId = HttpContext.TraceIdentifier
                    });
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

                var calculatorRun = await this.context.CalculatorRuns
                    .Include(cr => cr.BillingFileMetadata)
                    .SingleOrDefaultAsync(x => x.Id == runId, cancellationToken);

                if (calculatorRun == null)
                {
                    return new ObjectResult(string.Format(CommonResources.UnableToFindRunId, runId))
                        { StatusCode = StatusCodes.Status422UnprocessableEntity };
                }

                if (calculatorRun.BillingRunStatus is not BillingRunStatus.Completed)
                {
                    return new ObjectResult(string.Format(CommonResources.BillingRunStatusInvalidForSend, runId, calculatorRun.BillingRunStatus))
                        { StatusCode = StatusCodes.Status422UnprocessableEntity };
                }

                if (calculatorRun.BillingFileMetadata is null)
                {
                    return new ObjectResult(string.Format(CommonResources.UnableToFindBillingFileMetadata, runId))
                        { StatusCode = StatusCodes.Status422UnprocessableEntity };
                }

                var wasBillingGeneratedAfterLatestInstructions = await billingFileService.WasBillingGeneratedAfterLatestInstructions(runId, cancellationToken);

                if (!wasBillingGeneratedAfterLatestInstructions)
                {
                    return new ObjectResult(string.Format(CommonResources.BillingFileOutdated, runId))
                    {
                        StatusCode = StatusCodes.Status422UnprocessableEntity,
                    };
                }

                var metadata = calculatorRun.BillingFileMetadata!;
                metadata.BillingFileAuthorisedBy = userName;
                metadata.BillingFileAuthorisedDate = DateTime.UtcNow;

                RunClassification newClassificationValue;
                try
                {
                    // Update calculation run classification status: Initial run completed
                    newClassificationValue = calculatorRun.Classification switch
                    {
                        RunClassification.InitialRun => RunClassification.InitialRunCompleted,
                        RunClassification.InterimRecalculationRun => RunClassification.InterimRecalculationRunCompleted,
                        RunClassification.FinalRecalculationRun => RunClassification.FinalRecalculationRunCompleted,
                        RunClassification.FinalRun => RunClassification.FinalRunCompleted,
                        _ => throw new InvalidOperationException(),
                    };
                }
                catch (InvalidOperationException)
                {
                    return new ObjectResult(string.Format(CommonResources.UnableToChangeStatusToCompleted, calculatorRun.Classification))
                        { StatusCode = StatusCodes.Status422UnprocessableEntity };
                }

                calculatorRun.Classification = newClassificationValue;

                using (var transaction = await this.context.Database.BeginTransactionAsync(cancellationToken))
                {
                    try
                    {
                        var affectedRows = await this.invoiceDetailsService.InsertInvoiceDetailsAtProducerLevel(runId, metadata.BillingFileAuthorisedDate.Value, metadata.BillingFileAuthorisedBy, cancellationToken);

                        this.telemetryClient.TrackEvent("InsertInvoiceDetailsAtProducerLevel", new Dictionary<string, string>
                        {
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

                        Console.WriteLine(exception.ToString());

                        return this.StatusCode(
                            StatusCodes.Status500InternalServerError,
                            new
                            {
                                message = "An unexpected error occurred.",
                                detail = exception.Message,
                                traceId = HttpContext.TraceIdentifier
                            });
                    }
                }

                // Return accepted status code
                return this.StatusCode(StatusCodes.Status202Accepted);
            }
            catch (Exception exception)
            {
                this.telemetryClient.TrackException(exception);
                Console.WriteLine(exception.ToString());

                return this.StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        message = "An unexpected error occurred.",
                        detail = exception.Message,
                        traceId = HttpContext.TraceIdentifier
                    });
            }
        }
    }
}
