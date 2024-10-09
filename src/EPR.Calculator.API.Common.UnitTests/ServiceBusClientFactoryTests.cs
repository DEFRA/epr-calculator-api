﻿using Azure.Messaging.ServiceBus;
using EPR.Calculator.API.Common.ServiceBus;
using Moq;
using Moq.Protected;

namespace EPR.Calculator.API.Common.UnitTests
{
    [TestClass]
    public class ServiceBusClientFactoryTests
    {
        [TestMethod]
        public void GetServiceBusClient_ReturnErrorIfNoConnectionString()
        {
            try
            {
                var serviceBusClientFactory = new ServiceBusClientFactory(string.Empty, 1, 1);
                var serviceBusClient = serviceBusClientFactory.GetServiceBusClient();
            }
            catch (ServiceBusException exception)
            {
                Assert.IsTrue(exception.Message.Contains("ServiceBusClient: Connection string not provided."));
                Assert.AreEqual(ServiceBusFailureReason.ServiceCommunicationProblem, exception.Reason);
            }
        }

        //[TestMethod]
        //public void GetServiceBusClient_ReturnErrorWhenInitialising()
        //{
        //    try
        //    {
        //        Mock<ServiceBusClientOptions> serviceBusClientOptionsMock = new Mock<ServiceBusClientOptions>();
        //        serviceBusClientOptionsMock
        //            .Setup(_ => _.RetryOptions)
        //            .Returns(x => throw new ServiceBusException())

        //        var serviceBusClientFactory = new ServiceBusClientFactory("Test connection", 3, 2);
        //        var serviceBusClient = serviceBusClientFactory.GetServiceBusClient();

        //        Mock<ServiceBusClientFactory> serviceBusClientFactoryMock = new Mock<ServiceBusClientFactory>();
        //        serviceBusClientFactoryMock
        //            .Setup(_ => _.GetServiceBusClient())
        //            .Returns(new ServiceBusException("", ServiceBusFailureReason.ServiceCommunicationProblem));

        //        Mock<ServiceBusClient> mockServiceBusClient = new Mock<ServiceBusClient>();
        //        mockServiceBusClient.SetReturnsDefault(new ServiceBusException("", ServiceBusFailureReason.ServiceCommunicationProblem));
        //    }
        //    catch (ServiceBusException exception)
        //    {
        //        // Assert.IsTrue(exception.Message.Contains("ServiceBusClient: Connection string not provided."));
        //        Assert.AreEqual(ServiceBusFailureReason.ServiceCommunicationProblem, exception.Reason);
        //        Assert.IsTrue(exception.Message.Contains("ServiceBusClient: Connection string not provided."));
        //    }
        //}
    }
}
