using Azure.Messaging.ServiceBus;
using EPR.Calculator.API.Constants;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Enums;
using EPR.Calculator.API.Mappers;
using EPR.Calculator.API.Exporter;
using EPR.Calculator.API.Models;
using EPR.Calculator.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Newtonsoft.Json;
using System.Configuration;

namespace EPR.Calculator.API.Controllers
{
    [Route("v1")]
    public class CalculatorController : ControllerBase
    {
        private readonly ApplicationDBContext context;
        private readonly IConfiguration configuration;
        private readonly IAzureClientFactory<ServiceBusClient> serviceBusClientFactory;
        private readonly IStorageService storageService;

        public CalculatorController(ApplicationDBContext context, IConfiguration configuration,
            IAzureClientFactory<ServiceBusClient> serviceBusClientFactory, IStorageService storageService)
        {
            this.context = context;
            this.configuration = configuration;
            this.serviceBusClientFactory = serviceBusClientFactory;
            this.storageService = storageService;
        }

        [HttpPost]
        [Route("calculatorRun")]
        public async Task<IActionResult> Create([FromBody] CreateCalculatorRunDto request)
        {
            try
            {
                // Return bad request if the model is invalid
                if (!ModelState.IsValid)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, ModelState.Values.SelectMany(x => x.Errors));
                }

                bool isCalcAlreadyRunning = await context.CalculatorRuns.AnyAsync(run => run.CalculatorRunClassificationId == (int)RunClassification.RUNNING);
                if (isCalcAlreadyRunning)
                {
                    return new ObjectResult(new { Message = ErrorMessages.CalculationAlreadyRunning })
                    {
                        StatusCode = StatusCodes.Status422UnprocessableEntity,
                    };
                }

                // Return failed dependency error if at least one of the dependent data not available for the financial year
                var dataPreCheckMessage = DataPreChecksBeforeInitialisingCalculatorRun(request.FinancialYear);
                if (!string.IsNullOrWhiteSpace(dataPreCheckMessage))
                {
                    return new ObjectResult(dataPreCheckMessage) { StatusCode = StatusCodes.Status424FailedDependency };
                }

                // Return bad gateway error if the calculator run name provided already exists
                var calculatorRunNameExistsMessage = CalculatorRunNameExists(request.CalculatorRunName);
                if (!string.IsNullOrWhiteSpace(calculatorRunNameExistsMessage))
                {
                    return new ObjectResult(calculatorRunNameExistsMessage) { StatusCode = StatusCodes.Status400BadRequest };
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
                    .SingleAsync(x => x.EffectiveTo == null && x.ParameterYear == request.FinancialYear);

                // Get active lapcap data master
                var activeLapcapDataMaster = await this.context.LapcapDataMaster
                    .SingleAsync(data => data.ProjectionYear == request.FinancialYear && data.EffectiveTo == null);

                // Setup calculator run details
                var calculatorRun = new CalculatorRun
                {
                    Name = request.CalculatorRunName,
                    Financial_Year = request.FinancialYear,
                    CreatedBy = request.CreatedBy,
                    CreatedAt = DateTime.Now,
                    CalculatorRunClassificationId = (int)RunClassification.RUNNING,
                    DefaultParameterSettingMasterId = activeDefaultParameterSettingsMaster.Id,
                    LapcapDataMasterId = activeLapcapDataMaster.Id
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
                            FinancialYear = calculatorRun.Financial_Year,
                            CreatedBy = User?.Identity?.Name ?? request.CreatedBy
                        };

                        // Send message
                        // await SendMessage(serviceBusQueueName, calculatorRunMessage);

                        // All good, commit transaction
                        await transaction.CommitAsync();
                    }
                    catch (Exception exception)
                    {
                        // Error, rollback transaction
                        await transaction.RollbackAsync();
                        // Return error status code: Internal Server Error
                        return StatusCode(StatusCodes.Status500InternalServerError, exception);
                    }
                }

                // Return accepted status code: Accepted
                return new ObjectResult(null) { StatusCode = StatusCodes.Status202Accepted };
            }
            catch (Exception exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, exception);
            }
        }

        [HttpPost]
        [Route("calculatorRuns")]
        public IActionResult GetCalculatorRuns([FromBody] CalculatorRunsParamsDto request)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ModelState.Values.SelectMany(x => x.Errors));
            }

            if (string.IsNullOrWhiteSpace(request.FinancialYear))
            {
                return new ObjectResult("Invalid financial year provided") { StatusCode = StatusCodes.Status400BadRequest };
            }

            try
            {
                var calculatorRuns = context.CalculatorRuns.Where(run => run.Financial_Year == request.FinancialYear).OrderByDescending(run => run.CreatedAt).ToList();

                if (calculatorRuns.Count == 0)
                {
                    return new ObjectResult("No data available for the specified year. Please check the year and try again.") { StatusCode = StatusCodes.Status404NotFound };
                }

                return new ObjectResult(calculatorRuns) { StatusCode = StatusCodes.Status200OK };
            }
            catch (Exception exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, exception);
            }
        }

        [HttpGet]
        [Route("calculatorRuns/{runId}")]
        public IActionResult GetCalculatorRun(int runId)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ModelState.Values.SelectMany(x => x.Errors));
            }

            try
            {
                var calculatorRunDetail =
                    (from run in this.context.CalculatorRuns
                     join classification in context.CalculatorRunClassifications
                         on run.CalculatorRunClassificationId equals classification.Id
                     where run.Id == runId
                     select new
                     {
                         Run = run,
                         Classification = classification
                     }).SingleOrDefault();
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
                return StatusCode(StatusCodes.Status500InternalServerError, exception);
            }
        }

        [HttpPut]
        [Route("calculatorRuns")]
        public IActionResult PutCalculatorRunStatus(CalculatorRunStatusUpdateDto runStatusUpdateDto)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ModelState.Values.SelectMany(x => x.Errors));
            }

            try
            {
                var calculatorRun = context.CalculatorRuns.SingleOrDefault(x => x.Id == runStatusUpdateDto.RunId);
                if (calculatorRun == null)
                {
                    return new ObjectResult($"Unable to find Run Id {runStatusUpdateDto.RunId}")
                    { StatusCode = StatusCodes.Status422UnprocessableEntity };
                }

                var classification =
                    this.context.CalculatorRunClassifications.SingleOrDefault(x =>
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

                this.context.CalculatorRuns.Update(calculatorRun);
                this.context.SaveChanges();

                return StatusCode(201);
            }
            catch (Exception exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, exception);
            }
        }

        [HttpGet]
        [Route("CheckCalcNameExists/{name}")]
        public IActionResult GetCalculatorRunByName([FromRoute] string name)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ModelState.Values.SelectMany(x => x.Errors));
            }

            try
            {
                var calculatorRun = context.CalculatorRuns.Count(run => EF.Functions.Like(run.Name, name));

                if (calculatorRun <= 0)
                {
                    return new ObjectResult("No data found for this calculator name") { StatusCode = StatusCodes.Status404NotFound };
                }
                return new ObjectResult(StatusCodes.Status200OK);
            }
            catch (Exception exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, exception);
            }
        }

        [HttpGet]
        [Route("DownloadResult/{runId}")]
        public async Task<IResult> DownloadResultFile(int runId)
        {
            if (!ModelState.IsValid)
            {
                var badRequest = Results.BadRequest(ModelState.Values.SelectMany(x => x.Errors));
                return badRequest;
            }

            var calcRun = await context.CalculatorRuns.SingleOrDefaultAsync(x => x.Id == runId);
            if (calcRun == null)
            {
                var notFound = Results.NotFound(ModelState.Values.SelectMany(x => x.Errors));
                return notFound;
            }

            try
            {
                var fileName = new CalcResultsFileName(
                    calcRun.Id,
                    calcRun.Name ?? string.Empty,
                    calcRun.CreatedAt);
                return await storageService.DownloadFile(fileName);
            }
            catch (Exception e)
            {
                return Results.Problem(e.Message);
            }
        }

        private string DataPreChecksBeforeInitialisingCalculatorRun(string financialYear)
        {
            // Get active default parameter settings for the given financial year
            var activeDefaultParameterSettings = context.DefaultParameterSettings
                        .SingleOrDefault(x => x.EffectiveTo == null && x.ParameterYear == financialYear);

            // Get active Lapcap data for the given financial year
            var activeLapcapData = context.LapcapDataMaster
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
            var calculatorRun = context.CalculatorRuns.Count(run => EF.Functions.Like(run.Name, runName));

            // Return calculator run name already exists
            if (calculatorRun > 0)
            {
                return $"Calculator run name already exists: {runName}";
            }

            // All good, return empty string
            return string.Empty;
        }

        private async Task SendMessage(string serviceBusQueueName, CalculatorRunMessage calculatorRunMessage)
        {
            // Send message to service bus
            var client = serviceBusClientFactory.CreateClient(CommonConstants.ServiceBusClientName);
            ServiceBusSender serviceBusSender = client.CreateSender(serviceBusQueueName);
            var messageString = JsonConvert.SerializeObject(calculatorRunMessage);
            ServiceBusMessage serviceBusMessage = new ServiceBusMessage(messageString);
            await serviceBusSender.SendMessageAsync(serviceBusMessage);
        }
    }
}