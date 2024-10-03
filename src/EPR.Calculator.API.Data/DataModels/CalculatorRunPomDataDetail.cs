using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.API.Data.DataModels
{
    [ExcludeFromCodeCoverage]
    [Table("calculator_run_pom_data_detail")]
    public class CalculatorRunPomDataDetail
    {
        public required string OrganisationId { get; set; }

        public required string SubsidaryId { get; set; }

        public required string SubmissionPeriod { get; set; }

        public string PackagingActivity { get; set; }

        public string PackagingType { get; set; }
    }
}
