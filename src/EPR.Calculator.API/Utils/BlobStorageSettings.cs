namespace EPR.Calculator.API.Utils
{
    public class BlobStorageSettings
    {
        public string ConnectionString { get; set; } = string.Empty;

        public string ResultFileCSVContainerName { get; set; } = string.Empty;

        public string BillingFileCSVContainerName { get; set; } = string.Empty;

        public string BillingFileJsonContainerName { get; set; } = string.Empty;

        public string BillingFileJsonForFssContainerName { get; set; } = string.Empty;

        public string CsvFileName { get; set; } = string.Empty;

        public string AccountName { get; set; } = string.Empty;

        public string AccountKey { get; set; } = string.Empty;

        public void ExtractAccountDetails()
        {
            var connectionStringParts = ConnectionString.Split(';');
            foreach (var part in connectionStringParts)
            {
                if (part.StartsWith("AccountName=", StringComparison.OrdinalIgnoreCase))
                {
                    AccountName = part.Substring("AccountName=".Length);
                }
                else if (part.StartsWith("AccountKey=", StringComparison.OrdinalIgnoreCase))
                {
                    AccountKey = part.Substring("AccountKey=".Length);
                }
            }
        }
    }
}