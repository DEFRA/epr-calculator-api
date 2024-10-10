﻿using Azure.Core;
using Azure.Messaging.ServiceBus;
using EPR.Calculator.API.Common.Models;
using EPR.Calculator.API.Common.ServiceBus;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Azure;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using Newtonsoft.Json;

namespace EPR.Calculator.API.Controllers
{
    [Route("v1")]
    public class CalculatorController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly IAzureClientFactory<ServiceBusClient> _factory;
        private readonly IServiceBusClientFactory _serviceBusClientFactory;

        public CalculatorController(ApplicationDBContext context, IConfiguration configuration, IServiceBusClientFactory serviceBusClientFactory, IAzureClientFactory<ServiceBusClient> factory)
        {
            _context = context;
            _configuration = configuration;
            _factory = factory;
            _serviceBusClientFactory = serviceBusClientFactory;
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

            var message = DataPreChecksBeforeInitialisingCalculatorRun(request.FinancialYear);

            if (!string.IsNullOrWhiteSpace(message))
            {
                return new ObjectResult(message) { StatusCode = StatusCodes.Status424FailedDependency };
            }

            // Read configuration items: service bus connection string and queue name 
            var serviceBusConnectionString = this._configuration.GetSection("ServiceBus").GetSection("ConnectionString").Value;
            var serviceBusQueueName = this._configuration.GetSection("ServiceBus").GetSection("QueueName").Value;

            if (string.IsNullOrWhiteSpace(serviceBusConnectionString))
            {
                return new ObjectResult("Configuration item not found: ServiceBus__ConnectionString") { StatusCode = StatusCodes.Status500InternalServerError };
            }

            if (string.IsNullOrWhiteSpace(serviceBusQueueName))
            {
                return new ObjectResult("Configuration item not found: ServiceBus__QueueName") { StatusCode = StatusCodes.Status500InternalServerError };
            }

            // Read configuration items: message retry count and period
            var messageRetryCountFound = int.TryParse(this._configuration.GetSection("ServiceBus").GetSection("PostMessageRetryCount").Value, out int messageRetryCount);
            var messageRetryPeriodFound = int.TryParse(this._configuration.GetSection("ServiceBus").GetSection("PostMessageRetryPeriod").Value, out int messageRetryPeriod);

            if (!messageRetryCountFound)
            {
                return new ObjectResult("Configuration item not found: ServiceBus__PostMessageRetryCount") { StatusCode = StatusCodes.Status500InternalServerError };
            }

            if (!messageRetryPeriodFound)
            {
                return new ObjectResult("Configuration item not found: ServiceBus__PostMessageRetryPeriod") { StatusCode = StatusCodes.Status500InternalServerError };
            }

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // Get active default parameter settings master id
                    var activeDefaultParameterSettingsMasterId = _context.DefaultParameterSettings
                        .SingleOrDefault(x => x.EffectiveTo == null && x.ParameterYear == request.FinancialYear)?.Id;

                    // Get active lapcap data master id
                    var activeLapcapDataMasterId = _context.LapcapDataMaster
                        .SingleOrDefault(data => data.ProjectionYear == request.FinancialYear && data.EffectiveTo == null)?.Id;

                    // Setup calculator run details
                    var calculatorRun = new CalculatorRun
                    {
                        Name = request.CalculatorRunName,
                        Financial_Year = request.FinancialYear,
                        CreatedBy = request.CreatedBy,
                        CreatedAt = DateTime.Now,
                        CalculatorRunClassificationId = 1,
                        DefaultParameterSettingMasterId = activeDefaultParameterSettingsMasterId,
                        LapcapDataMasterId = activeLapcapDataMasterId
                    };

                    // Save calculator run details to the database
                    _context.CalculatorRuns.Add(calculatorRun);
                    _context.SaveChanges();

                    // Setup message
                    var calculatorRunMessage = new CalculatorRunMessage
                    {
                        CalculatorRunId = calculatorRun.Id,
                        FinancialYear = calculatorRun.Financial_Year,
                        CreatedBy = User?.Identity?.Name ?? request.CreatedBy
                    };

                    var client = _factory.CreateClient("new Client");

                    ServiceBusSender serviceBusSender = client.CreateSender(serviceBusQueueName);
                    var messageString = JsonConvert.SerializeObject(message);
                    ServiceBusMessage serviceBusMessage = new ServiceBusMessage(messageString);
                    await serviceBusSender.SendMessageAsync(serviceBusMessage);

                    await ServiceBus.SendMessage(serviceBusConnectionString, serviceBusQueueName, calculatorRunMessage, messageRetryCount, messageRetryPeriod);

                    // await SendMessage(serviceBusConnectionString, serviceBusQueueName, messageRetryCount, messageRetryPeriod, calculatorRunMessage);

                    //await using (var serviceBusClient = this._serviceBusClientFactory.GetServiceBusClient(serviceBusConnectionString, messageRetryCount, messageRetryPeriod))
                    //{
                    //    var messageString = JsonConvert.SerializeObject(calculatorRunMessage);
                    //    ServiceBusMessage serviceBusMessage = new ServiceBusMessage(messageString);

                    //    ServiceBusSender serviceBusSender = serviceBusClient.CreateSender(serviceBusQueueName);

                    //    await serviceBusSender.SendMessageAsync(serviceBusMessage);
                    //}

                    // All good, commit transaction
                    transaction.Commit();
                }
                catch (Exception exception)
                {
                    // Error, rollback transaction
                    transaction.Rollback();
                    return StatusCode(StatusCodes.Status500InternalServerError, exception);
                }
            }

            // Return ccepted status code: Accepted
            return new ObjectResult(null) { StatusCode = StatusCodes.Status202Accepted };
        }

        //private async Task SendMessage(string serviceBusConnectionString, string serviceBusQueueName, int messageRetryCount, int messageRetryPeriod, CalculatorRunMessage message)
        //{
        //    await using (var serviceBusClient = this._serviceBusClientFactory.GetServiceBusClient(serviceBusConnectionString, messageRetryCount, messageRetryPeriod))
        //    {
        //        var messageString = JsonConvert.SerializeObject(message);
        //        ServiceBusMessage serviceBusMessage = new ServiceBusMessage(messageString);

        //        ServiceBusSender serviceBusSender = serviceBusClient.CreateSender(serviceBusQueueName);

        //        await serviceBusSender.SendMessageAsync(serviceBusMessage);
        //    }
        //}

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
                return new ObjectResult("Invalid financial year provided") { StatusCode  = StatusCodes.Status400BadRequest };
            }

            try
            {
                var calculatorRuns = _context.CalculatorRuns.Where(run => run.Financial_Year == request.FinancialYear).OrderByDescending(run => run.CreatedAt).ToList();

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
        [Route("calculatorRuns/{id}")]
        public IActionResult GetCalculatorRun()
        {
            // TODO: Return the details of a particular run
            return new OkResult();
        }

        private string DataPreChecksBeforeInitialisingCalculatorRun(string financialYear)
        {
            // Get active default parameter settings for the given financial year
            var activeDefaultParameterSettings = _context.DefaultParameterSettings
                        .SingleOrDefault(x => x.EffectiveTo == null && x.ParameterYear == financialYear);

            // Get active Lapcap data for the given financial year
            var activeLapcapData = _context.LapcapDataMaster
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
    }
}
