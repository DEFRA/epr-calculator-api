namespace EPR.Calculator.API.UnitTests.Controllers
{
    using System;
    using System.Threading.Tasks;
    using AutoFixture;
    using Azure;
    using Azure.Analytics.Synapse.Artifacts;
    using Azure.Analytics.Synapse.Artifacts.Models;
    using Azure.Core;
    using EPR.Calculator.API.Controllers;
    using EPR.Calculator.API.Utils;
    using Microsoft.Extensions.Configuration;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class AzureSynapsePipelineTestControllerTests
    {
        private AzureSynapsePipelineTestController TestClass { get; set; }

        private Mock<PipelineClientFactory> Factory { get; set; }

        [TestInitialize]
        public void SetUp()
        {
            this.Factory = new Mock<PipelineClientFactory>();
            var configuration = new Mock<IConfiguration>();
            configuration.Setup(c => c.GetSection("AzureSynapse")["PipelineUrl"]).Returns("http://not.a.real.url");
            this.TestClass = new AzureSynapsePipelineTestController(configuration.Object, this.Factory.Object);
        }

        [TestMethod]
        public async Task CanCallGetPipeline()
        {
            // Arrange
            var runId = new Guid("33a07ce6-c34e-410f-b878-d0c89fd4f893");
            var pipelineResult = new Mock<PipelineRun>();
            var expectedResponse = new Mock<Response<PipelineRun>>();
            expectedResponse.Setup(r => r.Value).Returns(pipelineResult.Object);
            var client = new Mock<PipelineRunClient>(); 
            client.Setup(c => c.GetPipelineRunAsync(runId.ToString(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(expectedResponse.Object));
            this.Factory.Setup(f => f.GetPipelineRunClient(It.IsAny<Uri>(), It.IsAny<TokenCredential>()))
                .Returns(client.Object);
            
            // Act
            var result = await this.TestClass.GetPipeline(runId);

            // Assert
            Assert.Fail("Create or modify test");
        }

        [TestMethod]
        public async Task CanCallPostPipeline()
        {
            // Act
            var result = await this.TestClass.PostPipeline();

            // Assert
            Assert.Fail("Create or modify test");
        }
    }
}