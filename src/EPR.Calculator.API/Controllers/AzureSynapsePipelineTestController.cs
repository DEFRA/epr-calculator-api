using Azure.Analytics.Synapse.Artifacts;
using Azure.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.API.Controllers
{
    
    public class AzureSynapsePipelineTestController(IConfiguration configuration) : ControllerBase
    {
        private string PipelineUrl { get; init; } = configuration.GetSection("AzureSynapse")["PipelineUrl"]
            ?? string.Empty;

        private string PipelineName { get; init; } = configuration.GetSection("AzureSynapse")["PipelineName"]
            ?? string.Empty;

        [Route("AzureSynapseTest")]
        [HttpGet]
        public async Task<IActionResult> GetPipeline()
        {
#if DEBUG
            var credentials = new DefaultAzureCredential();
#else
            var TokenCredential credentials = What do we need to use for credentials in prod? Managed identity maybe?
#endif

            var pipelineClient = new PipelineClient(
                new Uri(this.PipelineUrl),
                credentials);

            try
            {
                var result = await pipelineClient.GetPipelineAsync(
                this.PipelineName);

                return Ok(result.Value);
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
            var TokenCredential credentials = new ManagedIdentityCredential();
#endif

            var pipelineClient = new PipelineClient(
                new Uri(this.PipelineUrl),
                credentials);

            try
            {
                var result = await pipelineClient.CreatePipelineRunAsync(
                this.PipelineName);

                return Ok($"RunId: {result.Value.RunId}");
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
            return Ok();
        }
    }
}
