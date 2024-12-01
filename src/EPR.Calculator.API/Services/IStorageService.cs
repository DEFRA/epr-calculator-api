using System.Text;

namespace EPR.Calculator.API.Services
{
    public interface IStorageService
    {
        Task UploadResultFileContentAsync(string fileName, StringBuilder content);
    }
}
