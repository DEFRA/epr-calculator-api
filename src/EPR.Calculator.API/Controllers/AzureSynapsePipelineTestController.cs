using Azure.Analytics.Synapse.Artifacts;
using Azure.Identity;
using EPR.Calculator.API.Utils;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography.Xml;

namespace EPR.Calculator.API.Controllers
{
    
    public class AzureSynapsePipelineTestController(IConfiguration configuration) : ControllerBase
    {
        private PipelineClientFactory? PipelineClientFactory { get; }

        private string PipelineUrl { get; init; } = configuration.GetSection("AzureSynapse")["PipelineUrl"]
            ?? string.Empty;

        private string PipelineName { get; init; } = configuration.GetSection("AzureSynapse")["PipelineName"]
            ?? string.Empty;

        [ActivatorUtilitiesConstructor]
        public AzureSynapsePipelineTestController(IConfiguration configuration, PipelineClientFactory pipelineRunClientFactory)
            : this(configuration)
        {
            ArgumentNullException.ThrowIfNull(pipelineRunClientFactory);

            this.PipelineClientFactory = pipelineRunClientFactory;
        }

        [Route("AzureSynapseTest")]
        [HttpGet]
        public async Task<IActionResult> GetPipeline(Guid runId)
        {
            if (this.PipelineClientFactory == null)
                throw new InvalidOperationException("Pipeline client factory not initialised.");

            #if DEBUG
                var credentials = new DefaultAzureCredential();
            #else
                var credentials = new ManagedIdentityCredential();
            #endif

            var pipelineClient = this.PipelineClientFactory.GetPipelineRunClient(
                new Uri(this.PipelineUrl),
                credentials);
            try
            {
                var result = await pipelineClient.GetPipelineRunAsync(runId.ToString());
                return Ok(result.Value.Status);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [Route("AzureSynapseTest")]
        [HttpPost]
        public async Task<IActionResult> PostPipeline()
        {
            #if DEBUG
                var credentials = new DefaultAzureCredential();
            #else
                var credentials = new ManagedIdentityCredential();
            #endif

            var pipelineClient = this.PipelineClientFactory.GetPipelineClient(
                new Uri(this.PipelineUrl),
                credentials);

            try
            {
                var result = await pipelineClient.CreatePipelineRunAsync(
                this.PipelineName,
                parameters: new Dictionary<string,object> 
                { 
                    { "key", "value"},
                });

                return Ok($"RunId: {result.Value.RunId}");
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
    }
}
