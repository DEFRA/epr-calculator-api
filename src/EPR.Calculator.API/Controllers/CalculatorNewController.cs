using System.Configuration;
using EPR.Calculator.API.Constants;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Enums;
using EPR.Calculator.API.Mappers;
using EPR.Calculator.API.Validators;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.Controllers
{
    [Route("V2")]
    [ApiController]
    public class CalculatorNewController : ControllerBase
    {
        private readonly ApplicationDBContext context;
        private readonly IConfiguration configuration;
        private readonly ICalculatorRunStatusDataValidator calculatorRunStatusDataValidator;

        /// <summary>
        /// Initializes a new instance of the <see cref="CalculatorNewController"/> class.
        /// </summary>
        /// <param name="context">Db Context</param>
        /// <param name="calculatorRunStatusDataValidator">Db Validator</param>
        public CalculatorNewController(
            ApplicationDBContext context,
            IConfiguration configuration,
            ICalculatorRunStatusDataValidator calculatorRunStatusDataValidator)
        {
            this.context = context;
            this.configuration = configuration;
            this.calculatorRunStatusDataValidator = calculatorRunStatusDataValidator;
        }

        [HttpPut]
        [Route("calculatorRuns")]
        public async Task<IActionResult> PutCalculatorRunStatus([FromBody] CalculatorRunStatusUpdateDto runStatusUpdateDto)
        {
            try
            {
                var claim = this.User.Claims.FirstOrDefault(x => x.Type == "name");
                if (claim == null)
                {
                    return new ObjectResult("No claims in the request") { StatusCode = StatusCodes.Status401Unauthorized };
                }

                var userName = claim.Value;

                var classification = await this.context.CalculatorRunClassifications.SingleOrDefaultAsync(x =>
                    x.Id == runStatusUpdateDto.ClassificationId);

                if (classification == null)
                {
                    return new ObjectResult($"Unable to find Classification Id {runStatusUpdateDto.ClassificationId}")
                    { StatusCode = StatusCodes.Status422UnprocessableEntity };
                }

                var calculatorRun = await this.context.CalculatorRuns.SingleOrDefaultAsync(
                            x => x.Id == runStatusUpdateDto.RunId);
                if (calculatorRun == null)
                {
                    return new ObjectResult($"Unable to find Run Id {runStatusUpdateDto.RunId}")
                    { StatusCode = StatusCodes.Status422UnprocessableEntity };
                }

                var validationResult = this.calculatorRunStatusDataValidator.Validate(calculatorRun, runStatusUpdateDto);

                if (validationResult.IsInvalid)
                {
                    return new ObjectResult(validationResult.Errors)
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
        public async Task<IActionResult> GetCalculatorRun(int runId)
        {
            if (runId <= 0)
            {
                return this.StatusCode(StatusCodes.Status400BadRequest, "Invalid Run Id.");
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
                           }).SingleOrDefaultAsync();

                if (calculatorRunDetail == null)
                {
                    return new NotFoundObjectResult($"Unable to find Run Id {runId}");
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
        public async Task<ActionResult> PrepareBillingFileSendToFSS(int runId)
        {
            var claim = this.User.Claims.FirstOrDefault(x => x.Type == "name");
            if (claim == null)
            {
                return new ObjectResult("No claims in the request") { StatusCode = StatusCodes.Status401Unauthorized };
            }

            var userName = claim.Value;
            try
            {
                if (runId <= 0)
                {
                    return this.StatusCode(StatusCodes.Status400BadRequest, "Invalid Run Id.");
                }

                var calculatorRun = await this.context.CalculatorRuns.SingleOrDefaultAsync(x => x.Id == runId);
                if (calculatorRun == null)
                {
                    return new ObjectResult($"Unable to find Run Id {runId}")
                    { StatusCode = StatusCodes.Status422UnprocessableEntity };
                }

                if (calculatorRun.CalculatorRunClassificationId != (int)RunClassification.INITIAL_RUN || !calculatorRun.HasBillingFileGenerated)
                {
                    return new ObjectResult($"Run Id {runId} classification status is not an {RunClassification.INITIAL_RUN} or {nameof(calculatorRun.HasBillingFileGenerated)} column is not set to true")
                    { StatusCode = StatusCodes.Status422UnprocessableEntity };
                }

                var billingJsonFileName = this.configuration.GetSection(CommonConstants.BillingJsonFileName).Value;
                if (string.IsNullOrWhiteSpace(billingJsonFileName))
                {
                    throw new ConfigurationErrorsException($"Configuration item not found: {CommonConstants.BillingJsonFileName}");
                }

                using (var transaction = await this.context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // Add entry to calculator run billing file metadata
                        var calculatorRunBillingFileMetadata = new CalculatorRunBillingFileMetadata
                        {
                            BillingCsvFileName = null,
                            BillingJsonFileName = billingJsonFileName,
                            BillingFileCreatedDate = DateTime.UtcNow,
                            BillingFileCreatedBy = userName,
                            BillingFileAuthorisedDate = DateTime.UtcNow,
                            BillingFileAuthorisedBy = userName,
                            CalculatorRunId = runId,
                        };
                        await this.context.CalculatorRunBillingFileMetadata.AddAsync(calculatorRunBillingFileMetadata);

                        // Update calculation run classification status: Initial run completed
                        calculatorRun.CalculatorRunClassificationId = (int)RunClassification.INITIAL_RUN_COMPLETED;
                        this.context.CalculatorRuns.Update(calculatorRun);

                        await this.context.SaveChangesAsync();

                        // All good, commit transaction
                        await transaction.CommitAsync();
                    }
                    catch (Exception exception)
                    {
                        // Error, rollback transaction
                        await transaction.RollbackAsync();

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
