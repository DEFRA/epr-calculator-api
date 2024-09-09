using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.API.Constants
{
    [ExcludeFromCodeCoverage]
    public static class ErrorMessages
    {
        public static string YearRequired = "Parameter Year is required";
        public static string SchemeParameterTemplateValuesMissing =
            $"Uploaded Scheme parameter template should have a count of {DefaultParameterUniqueReferences.UniqueReferences.Length}";
    }
}
