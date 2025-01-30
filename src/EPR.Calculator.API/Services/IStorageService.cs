using System.Text;

namespace EPR.Calculator.API.Services
{
    public interface IStorageService
    {
        Task<string> UploadResultFileContentAsync(string fileName, string content);
        Task<IResult> DownloadFile(string fileName, string blobUri);
    }
}
