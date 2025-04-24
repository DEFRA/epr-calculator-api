using System.Text;

namespace EPR.Calculator.API.Services
{
    public interface IStorageService
    {
        Task<IResult> DownloadFile(string fileName, string blobUri);

        Task<IResult> DownloadFile(string fileName, string blobUri, string downLoadFileName);
    }
}
