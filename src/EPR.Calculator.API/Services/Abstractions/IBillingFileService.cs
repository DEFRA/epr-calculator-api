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

        /// <summary>
        /// Gets the billing file name from the calculator_run_billing_file_metadata based on the run ID.
        /// and then calls the blob storage service 2 to move the json file to the FSS container.
        /// </summary>
        /// <param name="runId">The calculator run id.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="string"/> containing the name of the json billing file.</returns>
        Task<bool> MoveBillingJsonFile(int runId, CancellationToken cancellationToken);

        /// <summary>
        /// Gets producer billing instructions for a given calculator run.
        /// </summary>
        /// <param name="runId">The calculator run id.</param>
        /// <param name="requestDto">The request payload.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="ProducerBillingInstructionsResponseDto"/> response containing records and pagination data.</returns>
        Task<ProducerBillingInstructionsResponseDto> GetProducerBillingInstructionsAsync(int runId, ProducerBillingInstructionsRequestDto requestDto, CancellationToken cancellationToken);
    }
}
