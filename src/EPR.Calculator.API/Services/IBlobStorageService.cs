using System.Text;

namespace EPR.Calculator.API.Services
{
    public interface IBlobStorageService
    {
        Task UploadResultFileContentAsync(string fileName, StringBuilder content);
        Task<string?> GetResultFileContentAsync();
    }
}
