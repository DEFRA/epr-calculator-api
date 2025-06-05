using EPR.Calculator.API.Dtos;

namespace EPR.Calculator.API.Services.Abstractions
{
    /// <summary>
    /// Interface for the billing file service.
    /// </summary>
    public interface IBillingFileService
    {
        /// <summary>
        /// Generates a billing file based on the provided request.
        /// </summary>
        /// <param name="generateBillingFileRequestDto">The request containing the details for generating the billing file.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="ServiceProcessResponseDto"/> containing the status and message of the operation.</returns>
        Task<ServiceProcessResponseDto> GenerateBillingFileAsync(
            GenerateBillingFileRequestDto generateBillingFileRequestDto,
            CancellationToken cancellationToken);

        Task<ServiceProcessResponseDto> UpdateProducerBillingInstructionsAsync(
            int runId,
            string userName,
            ProduceBillingInstuctionRequestDto produceBillingInstuctionRequestDto,
            CancellationToken cancellationToken);

        Task<ServiceProcessResponseDto> UpdateProducerBillingInstructionsAcceptAllAsync(
            int runId,
            string userName,
            CancellationToken cancellationToken);

        /// <summary>
        /// Get list of Producer Billing Instructions.
        /// </summary>
        /// <param name="runId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<ProducersInstructionResponse> GetProducersInstructionResponseAsync(
            int runId,
            CancellationToken cancellationToken);
    }
}
