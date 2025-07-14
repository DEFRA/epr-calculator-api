using System.Configuration;
using EnumsNET;
using EPR.Calculator.API.Constants;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Enums;
using EPR.Calculator.API.Mappers;
using EPR.Calculator.API.Models;
using EPR.Calculator.API.Services;
using EPR.Calculator.API.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.Controllers
{
    [Route("v1")]
    public class CalculatorController : ControllerBase
    {
        private readonly ApplicationDBContext context;
        private readonly IConfiguration configuration;
        private readonly IStorageService storageService;
        private readonly IServiceBusService serviceBusService;
        private readonly ICalcFinancialYearRequestDtoDataValidator validator;

        public CalculatorController(
            ApplicationDBContext context,
            IConfiguration configuration,
            IStorageService storageService,
            IServiceBusService serviceBusService,
            ICalcFinancialYearRequestDtoDataValidator validator)
        {
            this.context = context;
            this.configuration = configuration;
            this.storageService = storageService;
            this.serviceBusService = serviceBusService;
            this.validator = validator;
        }

        [HttpPost]
        [Route("calculatorRun")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status424FailedDependency)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateCalculatorRunDto request)
        {
            var claim = this.User.Claims.FirstOrDefault(x => x.Type == "name");
            if (claim == null)
            {
                return new ObjectResult("No claims in the request") { StatusCode = StatusCodes.Status401Unauthorized };
            }

            var userName = claim.Value;
            try
            {
                // Return bad request if the model is invalid
                if (!this.ModelState.IsValid)
                {
                    return this.StatusCode(
                        StatusCodes.Status400BadRequest,
                        this.ModelState.Values.SelectMany(x => x.Errors));
                }

                bool isCalcAlreadyRunning = await this.context.CalculatorRuns.AnyAsync(
                    run => run.CalculatorRunClassificationId == (int)RunClassification.RUNNING);
                if (isCalcAlreadyRunning)
                {
                    return new ObjectResult(new { Message = ErrorMessages.CalculationAlreadyRunning })
                    {
                        StatusCode = StatusCodes.Status422UnprocessableEntity,
                    };
                }

                var financialYear = await this.context.FinancialYears.SingleOrDefaultAsync(
                    year => year.Name == request.FinancialYear);
                if (financialYear is null)
                {
                    return new ObjectResult(new { Message = ErrorMessages.InvalidFinancialYear })
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                    };
                }

                // Return failed dependency error if at least one of the dependent data not available for the financial year
                var dataPreCheckMessage = this.DataPreChecksBeforeInitialisingCalculatorRun(financialYear);
                if (!string.IsNullOrWhiteSpace(dataPreCheckMessage))
                {
                    return new ObjectResult(dataPreCheckMessage) { StatusCode = StatusCodes.Status424FailedDependency };
                }

                // Return bad gateway error if the calculator run name provided already exists
                var calculatorRunNameExistsMessage = this.CalculatorRunNameExists(request.CalculatorRunName);
                if (!string.IsNullOrWhiteSpace(calculatorRunNameExistsMessage))
                {
                    return new ObjectResult(calculatorRunNameExistsMessage)
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                    };
                }

                // Read configuration items: service bus connection string and queue name
                var serviceBusConnectionString = this.configuration.GetSection("ServiceBus").GetSection("ConnectionString").Value;
                var serviceBusQueueName = this.configuration.GetSection("ServiceBus").GetSection("QueueName").Value;

                if (string.IsNullOrWhiteSpace(serviceBusConnectionString))
                {
                    throw new ConfigurationErrorsException("Configuration item not found: ServiceBus__ConnectionString");
                }

                if (string.IsNullOrWhiteSpace(serviceBusQueueName))
                {
                    throw new ConfigurationErrorsException("Configuration item not found: ServiceBus__QueueName");
                }

                // Get active default parameter settings master
                var activeDefaultParameterSettingsMaster = await this.context.DefaultParameterSettings
                    .SingleAsync(x => x.EffectiveTo == null && x.ParameterYear == financialYear);

                // Get active lapcap data master
                var activeLapcapDataMaster = await this.context.LapcapDataMaster
                    .SingleAsync(data => data.ProjectionYear == financialYear && data.EffectiveTo == null);

                // Setup calculator run details
                var calculatorRun = new CalculatorRun
                {
                    Name = request.CalculatorRunName,
                    Financial_Year = financialYear,
                    CreatedBy = userName,
                    CreatedAt = DateTime.Now,
                    CalculatorRunClassificationId = (int)RunClassification.RUNNING,
                    DefaultParameterSettingMasterId = activeDefaultParameterSettingsMaster.Id,
                    LapcapDataMasterId = activeLapcapDataMaster.Id,
                };

                using (var transaction = await this.context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // Save calculator run details to the database
                        await this.context.CalculatorRuns.AddAsync(calculatorRun);
                        await this.context.SaveChangesAsync();

                        // Setup message
                        var calculatorRunMessage = new CalculatorRunMessage
                        {
                            CalculatorRunId = calculatorRun.Id,
                            FinancialYear = calculatorRun.Financial_Year.Name,
                            CreatedBy = this.User.Identity?.Name ?? userName,
                            MessageType = CommonConstants.ResultMessageType,
                        };

                        // Send message
                        await this.serviceBusService.SendMessage(serviceBusQueueName, calculatorRunMessage);

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

                // Return accepted status code: Accepted
                return new ObjectResult(null) { StatusCode = StatusCodes.Status202Accepted };
            }
            catch (Exception exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, exception);
            }
        }

        [HttpPost]
        [Route("calculatorRuns")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCalculatorRuns([FromBody] CalculatorRunsParamsDto request)
        {
            if (!this.ModelState.IsValid)
            {
                return this.StatusCode(StatusCodes.Status400BadRequest, this.ModelState.Values.SelectMany(x => x.Errors));
            }

            if (string.IsNullOrWhiteSpace(request.FinancialYear))
            {
                return new ObjectResult("Invalid financial year provided") { StatusCode = StatusCodes.Status400BadRequest };
            }

            try
            {
                var calculatorRuns = await (from run in this.context.CalculatorRuns
                       join bill in this.context.CalculatorRunBillingFileMetadata on run.Id equals bill.CalculatorRunId
                       into billFile
                       where run.Financial_Year.Name == request.FinancialYear
                                    select new
                                    {
                                        run.Id,
                                        run.Name,
                                        Financial_Year = run.FinancialYearId,
                                        run.CreatedAt,
                                        run.CreatedBy,
                                        run.CalculatorRunClassificationId,
                                        HasBillingFileGenerated = billFile.Any(),
                                    })
                       .OrderByDescending(run => run.CreatedAt)
                       .ToListAsync();

                if (calculatorRuns.Count == 0)
                {
                    return new ObjectResult("No data available for the specified year. Please check the year and try again.") { StatusCode = StatusCodes.Status404NotFound };
                }

                return new ObjectResult(calculatorRuns) { StatusCode = StatusCodes.Status200OK };
            }
            catch (Exception exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, exception);
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
            if (!this.ModelState.IsValid)
            {
                return this.StatusCode(StatusCodes.Status400BadRequest, this.ModelState.Values.SelectMany(x => x.Errors));
            }

            try
            {
                var calculatorRunDetail =
                    await (from run in this.context.CalculatorRuns
                     join classification in this.context.CalculatorRunClassifications
                         on run.CalculatorRunClassificationId equals classification.Id
                     where run.Id == runId
                     select new
                     {
                         Run = run,
                         Classification = classification,
                     }).SingleOrDefaultAsync();

                if (calculatorRunDetail == null)
                {
                    return new NotFoundObjectResult($"Unable to find Run Id {runId}");
                }

                var calcRun = calculatorRunDetail.Run;
                var runClassification = calculatorRunDetail.Classification;
                var runDto = CalcRunMapper.Map(calcRun, runClassification);
                return new ObjectResult(runDto);
            }
            catch (Exception exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, exception);
            }
        }

        [HttpPut]
        [Route("calculatorRuns")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PutCalculatorRunStatus([FromBody] CalculatorRunStatusUpdateDto runStatusUpdateDto)
        {
            var claim = this.User.Claims.FirstOrDefault(x => x.Type == "name");
            if (claim == null)
            {
                return new ObjectResult("No claims in the request") { StatusCode = StatusCodes.Status401Unauthorized };
            }

            var userName = claim.Value;
            if (!this.ModelState.IsValid)
            {
                return this.StatusCode(StatusCodes.Status400BadRequest, this.ModelState.Values.SelectMany(x => x.Errors));
            }

            try
            {
                var calculatorRun = await this.context.CalculatorRuns.SingleOrDefaultAsync(x => x.Id == runStatusUpdateDto.RunId);
                if (calculatorRun == null)
                {
                    return new ObjectResult($"Unable to find Run Id {runStatusUpdateDto.RunId}")
                    { StatusCode = StatusCodes.Status422UnprocessableEntity };
                }

                var classification =
                    await this.context.CalculatorRunClassifications.SingleOrDefaultAsync(x =>
                        x.Id == runStatusUpdateDto.ClassificationId);

                if (classification == null)
                {
                    return new ObjectResult($"Unable to find Classification Id {runStatusUpdateDto.ClassificationId}")
                    { StatusCode = StatusCodes.Status422UnprocessableEntity };
                }

                if (runStatusUpdateDto.ClassificationId == calculatorRun.CalculatorRunClassificationId)
                {
                    return new ObjectResult(
                            $"RunId {runStatusUpdateDto.RunId} cannot be changed to classification {runStatusUpdateDto.ClassificationId}")
                    { StatusCode = StatusCodes.Status422UnprocessableEntity };
                }

                calculatorRun.CalculatorRunClassificationId = runStatusUpdateDto.ClassificationId;
                calculatorRun.UpdatedAt = DateTime.Now;
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
        [Route("CheckCalcNameExists/{name}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCalculatorRunByName([FromRoute] string name)
        {
            if (!this.ModelState.IsValid)
            {
                return this.StatusCode(StatusCodes.Status400BadRequest, this.ModelState.Values.SelectMany(x => x.Errors));
            }

            try
            {
                var calculatorRun = await this.context.CalculatorRuns.CountAsync(run => EF.Functions.Like(run.Name, name));

                if (calculatorRun <= 0)
                {
                    return new ObjectResult("No data found for this calculator name") { StatusCode = StatusCodes.Status404NotFound };
                }

                return new ObjectResult(StatusCodes.Status200OK);
            }
            catch (Exception exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, exception);
            }
        }

        [HttpGet]
        [Route("DownloadResult/{runId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IResult> DownloadResultFile(int runId)
        {
            if (!this.ModelState.IsValid)
            {
                var badRequest = Results.BadRequest(this.ModelState.Values.SelectMany(x => x.Errors));
                return badRequest;
            }

            var csvFileMetadata = await this.context.CalculatorRunCsvFileMetadata.
                SingleOrDefaultAsync(metadata => metadata.CalculatorRunId == runId && metadata.FileName != null && metadata.FileName.Contains("_Results"));
            if (csvFileMetadata == null)
            {
                return Results.NotFound($"No CSV file found for Run Id {runId}");
            }

            try
            {
                return await this.storageService.DownloadFile(csvFileMetadata.FileName, csvFileMetadata.BlobUri);
            }
            catch (Exception e)
            {
                return Results.Problem(e.Message);
            }
        }

        [HttpGet]
        [Route("FinancialYears")]
        [ProducesResponseType(typeof(IEnumerable<FinancialYearDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> FinancialYears()
        {
            try
            {
                var financialYears = await this.context.FinancialYears.ToListAsync();
                return new ObjectResult(financialYears.Select(FinancialYearMapper.Map));
            }
            catch (Exception exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, exception);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("ClassificationByFinancialYear")]
        [ProducesResponseType(typeof(FinancialYearClassificationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ClassificationByFinancialYear([FromQuery] CalcFinancialYearRequestDto request)
        {
            try
            {
                if (!this.ModelState.IsValid)
                {
                    return this.StatusCode(StatusCodes.Status400BadRequest, this.ModelState.Values.SelectMany(x => x.Errors));
                }

                var validationResult = this.validator.Validate(request);
                if (validationResult.IsInvalid)
                {
                    return this.BadRequest(validationResult.Errors);
                }

                var validStatuses = new[]
                {
                    RunClassification.INITIAL_RUN.AsString(EnumFormat.Description),
                    RunClassification.TEST_RUN.AsString(EnumFormat.Description),
                };

                var classifications = await this.context.CalculatorRunClassifications
                    .Where(c => validStatuses.Contains(c.Status))
                    .ToListAsync();

                if (!classifications.Any())
                {
                    return this.NotFound("No classifications found.");
                }

                var runDto = FinancialYearClassificationsMapper.Map(request.FinancialYear, classifications);
                return this.Ok(runDto);
            }
            catch (Exception exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        private string DataPreChecksBeforeInitialisingCalculatorRun(CalculatorRunFinancialYear financialYear)
        {
            // Get active default parameter settings for the given financial year
            var activeDefaultParameterSettings = this.context.DefaultParameterSettings
                        .SingleOrDefault(x => x.EffectiveTo == null && x.ParameterYear == financialYear);

            // Get active Lapcap data for the given financial year
            var activeLapcapData = this.context.LapcapDataMaster
                .SingleOrDefault(data => data.ProjectionYear == financialYear && data.EffectiveTo == null);

            // Return no active default paramater settings and lapcap data message
            if (activeDefaultParameterSettings == null && activeLapcapData == null)
            {
                return $"Default parameter settings and Lapcap data not available for the financial year {financialYear}.";
            }

            // Return no active default parameter settings found message
            if (activeDefaultParameterSettings == null)
            {
                return $"Default parameter settings not available for the financial year {financialYear}.";
            }

            // Return no active lapcap data found message
            if (activeLapcapData == null)
            {
                return $"Lapcap data not available for the financial year {financialYear}.";
            }

            // All good, return empty string
            return string.Empty;
        }

        private string CalculatorRunNameExists(string runName)
        {
            var calculatorRun = this.context.CalculatorRuns.Count(run => EF.Functions.Like(run.Name, runName));

            // Return calculator run name already exists
            if (calculatorRun > 0)
            {
                return $"Calculator run name already exists: {runName}";
            }

            // All good, return empty string
            return string.Empty;
        }
    }
}