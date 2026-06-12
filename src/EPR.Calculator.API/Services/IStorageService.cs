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
    }
}
