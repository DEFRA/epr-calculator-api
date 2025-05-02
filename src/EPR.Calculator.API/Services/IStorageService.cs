namespace EPR.Calculator.API.Services
{
    /// <summary>
    /// Defines the contract for storage service operations.
    /// </summary>
    public interface IStorageService
    {
        /// <summary>
        /// Downloads a file from the specified blob storage.
        /// </summary>
        /// <param name="fileName">The name of the file to download.</param>
        /// <param name="blobUri">The URI of the blob storage.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the download result.</returns>
        Task<IResult> DownloadFile(string fileName, string blobUri);

        /// <summary>
        /// Checks if a blob exists in the specified blob storage.
        /// </summary>
        /// <param name="fileName">The name of the blob to check.</param>
        /// <param name="blobUri">The URI of the blob storage.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the blob exists.</returns>
        Task<bool> IsBlobExistsAsync(string fileName, string blobUri, CancellationToken cancellationToken);
    }
}
