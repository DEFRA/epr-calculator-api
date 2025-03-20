namespace EPR.Calculator.API.Services
{
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    namespace EPR.Calculator.API.Services
    {
        public class LocalFileStorageService : IStorageService
        {
            public Task<string?> GetResultFileContentAsync()
            {
                throw new NotImplementedException();
            }

            public Task<string> UploadResultFileContentAsync(string fileName, string content)
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), fileName);
                File.WriteAllText(path, content, Encoding.UTF8);
                return Task.FromResult(path);
            }

            public Task<IResult> DownloadFile(string fileName, string blobUri)
            {
                throw new NotImplementedException();
            }
        }
    }
}
