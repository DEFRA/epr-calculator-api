namespace EPR.Calculator.API.Services
{
    using System.IO;
    using System.Text;

    namespace EPR.Calculator.API.Services
    {
        public class LocalFileStorageService : IStorageService
        {
            public Task<string?> GetResultFileContentAsync()
            {
                throw new NotImplementedException();
            }

            public Task UploadResultFileContentAsync(string fileName, StringBuilder content)
            {
                var path = $"{Directory.GetCurrentDirectory()}\\{fileName}";
                File.WriteAllText(path, content.ToString(), Encoding.UTF8);
                return Task.CompletedTask;
            }

            public Task<string> DownloadFile(string fileName)
            {
                throw new NotImplementedException();
            }
        }
    }
}
