using System.Text;

namespace EPR.Calculator.API.Services
{
    public interface IStorageService
    {
        Task<bool> UploadResultFileContentAsync(string fileName, string content);
        Task<IResult> DownloadFile(string fileName);
    }
}
