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

            public Task<bool> UploadResultFileContentAsync(string fileName, string content)
            {
                var path = $"{Directory.GetCurrentDirectory()}\\{fileName}";
                File.WriteAllText(path, content, Encoding.UTF8);
                var result = Task.FromResult(true);
                return result;
            }

            public Task<IResult> DownloadFile(string fileName)
            {
                throw new NotImplementedException();
            }
        }
    }
}
