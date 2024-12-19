using Azure.Analytics.Synapse.Artifacts;
using Azure.Core;

namespace EPR.Calculator.API.Utils
{
    /// <summary>
    /// Factory for initialising Azure Synapse pipeline clients.
    /// </summary>
    /// <remarks>
    /// Used by <see cref="AzureSynapsePipelineTestController"/> via dependancy injection
    /// so that the clients can be replaced with mocks when unit testing.
    /// </remarks>
    public class PipelineClientFactory
    {
        public virtual PipelineClient GetPipelineClient(Uri pipelineUrl, TokenCredential tokenCredential)
            => new PipelineClient(pipelineUrl, tokenCredential);

        public virtual PipelineRunClient GetPipelineRunClient(Uri pipelineUrl, TokenCredential tokenCredential)
            => new PipelineRunClient(pipelineUrl, tokenCredential);
    }
}
