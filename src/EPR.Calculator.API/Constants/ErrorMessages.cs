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
    }
}