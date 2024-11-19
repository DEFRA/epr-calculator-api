namespace EPR.Calculator.API.Constants
{
    public class BlobStorageSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string ContainerName { get; set; } = string.Empty;
        public string CsvFileName { get; set; } = string.Empty;
    }
}