using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.API.Constants
{
    [ExcludeFromCodeCoverage]
    public static class ErrorMessages
    {
        public static readonly string YearRequired = "Parameter Year is required";
        public static readonly string SchemeParameterTemplateValuesMissing =
            $"Uploaded Scheme parameter template should have a count of {DefaultParameterUniqueReferences.UniqueReferences.Length}";

        public static readonly string LapcapDataTemplateValuesMissing =
            $"Uploaded Lapcap parameter template should have a count of {LapcapDataUniqueReferences.UniqueReferences.Length}";

        public static readonly string FileNameRequired = "FileName is required";
        public static readonly string MaxFileNameLength = "File name should be less than 256 characters";
        public static readonly string CalculationAlreadyRunning = "The calculator is currently running. You will be able to run another calculation once the current one has finished.";
        public static readonly string InvalidFinancialYear = "The financial year is invalid.";
        public static readonly string RejectionReason = "Reason for rejection is required.";
        public static readonly string InvalidRunId = "RunId is invalid.";
        public static readonly string InvalidRunStatusForAcceptAll = "Run is not having the right status or Is Billing File Generating is already set to true";
        public static readonly string InvalidOrganisationId = "OrganisationId is invalid.";
        public static readonly string InvalidStatus = "Status is invalid.";
        public static readonly string RunNotFound = "A calculator run with the specified RunId was not found.";
    }
}