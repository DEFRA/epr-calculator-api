﻿using Azure.Messaging.ServiceBus;
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
            // Return bad request if the model is invalid
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ModelState.Values.SelectMany(x => x.Errors));
            }

            bool isCalcAlreadyRunning = this.context.CalculatorRuns.Any(run => run.CalculatorRunClassificationId == (int)RunClassification.RUNNING);
            if (isCalcAlreadyRunning)
            {
                return new ObjectResult(new { Message = ErrorMessages.CalculationAlreadyRunning })
                {
                    StatusCode = StatusCodes.Status422UnprocessableEntity,
                };
            }

#pragma warning disable S6966 // Awaitable method should be used
            using (var transaction = this.context.Database.BeginTransaction())
            {
                try
                {
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
                        return new ObjectResult("Configuration item not found: ServiceBus__ConnectionString") { StatusCode = StatusCodes.Status500InternalServerError };
                    }

                    if (string.IsNullOrWhiteSpace(serviceBusQueueName))
                    {
                        return new ObjectResult("Configuration item not found: ServiceBus__QueueName") { StatusCode = StatusCodes.Status500InternalServerError };
                    }

                    // Read configuration items: message retry count and period
                    var messageRetryCountFound = int.TryParse(this.configuration.GetSection("ServiceBus").GetSection("PostMessageRetryCount").Value, out int messageRetryCount);
                    var messageRetryPeriodFound = int.TryParse(this.configuration.GetSection("ServiceBus").GetSection("PostMessageRetryPeriod").Value, out int messageRetryPeriod);

                    if (!messageRetryCountFound)
                    {
                        return new ObjectResult("Configuration item not found: ServiceBus__PostMessageRetryCount") { StatusCode = StatusCodes.Status500InternalServerError };
                    }

                    if (!messageRetryPeriodFound)
                    {
                        return new ObjectResult("Configuration item not found: ServiceBus__PostMessageRetryPeriod") { StatusCode = StatusCodes.Status500InternalServerError };
                    }

                    // Get active default parameter settings master id
                    var activeDefaultParameterSettingsMasterId = this.context.DefaultParameterSettings
                        .SingleOrDefault(x => x.EffectiveTo == null && x.ParameterYear == request.FinancialYear)?.Id;

                    // Get active lapcap data master id
                    var activeLapcapDataMasterId = this.context.LapcapDataMaster
                        .SingleOrDefault(data => data.ProjectionYear == request.FinancialYear && data.EffectiveTo == null)?.Id;

                    // Setup calculator run details
                    var calculatorRun = new CalculatorRun
                    {
                        Name = request.CalculatorRunName,
                        Financial_Year = request.FinancialYear,
                        CreatedBy = request.CreatedBy,
                        CreatedAt = DateTime.Now,
                        CalculatorRunClassificationId = (int)RunClassification.RUNNING,
                        DefaultParameterSettingMasterId = activeDefaultParameterSettingsMasterId,
                        LapcapDataMasterId = activeLapcapDataMasterId
                    };

                    // Save calculator run details to the database
                    this.context.CalculatorRuns.Add(calculatorRun);
                    this.context.SaveChanges();

                    // Setup message
                    var calculatorRunMessage = new CalculatorRunMessage
                    {
                        CalculatorRunId = calculatorRun.Id,
                        FinancialYear = calculatorRun.Financial_Year,
                        CreatedBy = User?.Identity?.Name ?? request.CreatedBy
                    };

                    // Send message to service bus
                    var client = serviceBusClientFactory.CreateClient("calculator");
                    ServiceBusSender serviceBusSender = client.CreateSender(serviceBusQueueName);
                    var messageString = JsonConvert.SerializeObject(calculatorRunMessage);
                    ServiceBusMessage serviceBusMessage = new ServiceBusMessage(messageString);
                    await serviceBusSender.SendMessageAsync(serviceBusMessage);

                    // All good, commit transaction
                    transaction.Commit();
                }
                catch (Exception exception)
                {
                    // Error, rollback transaction
                    transaction.Rollback();
                    // Return error status code: Internal Server Error
                    return StatusCode(StatusCodes.Status500InternalServerError, exception);
                }
#pragma warning restore S6966 // Awaitable method should be used
            }
            // Return accepted status code: Accepted
            return new ObjectResult(null) { StatusCode = StatusCodes.Status202Accepted };
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
    }
}