﻿namespace EPR.Calculator.API.Services
{
    using System.IO;
    using System.Text;

    namespace EPR.Calculator.API.Services
    {
        public class LocalFileStorageService : IBlobStorageService
        {
            public Task<string?> GetResultFileContentAsync()
            {
                throw new NotImplementedException();
            }

            public Task UploadResultFileContentAsync(string fileName, StringBuilder content)
            {
                var path = $"C:\\Users\\Uday Denduluri\\OneDrive - Eviden\\Desktop\\Result-Files\\${fileName}";
                File.WriteAllText(path, content.ToString(), Encoding.UTF8);
                return Task.CompletedTask;
            }
        }
    }

}
