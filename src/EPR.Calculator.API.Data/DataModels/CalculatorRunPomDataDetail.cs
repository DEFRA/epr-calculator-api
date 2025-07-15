using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.Data.DataModels
{
    [Index(nameof(OrganisationId), nameof(CalculatorRunPomDataMasterId))]
    public class CalculatorRunPomDataDetail
    {
        public int Id { get; set; }

        public int? OrganisationId { get; set; }

        public string? SubsidaryId { get; set; }

        public required string? SubmissionPeriod { get; set; }

        public string? PackagingActivity { get; set; }

        public string? PackagingType { get; set; }

        public string? PackagingClass { get; set; }

        public string? PackagingMaterial { get; set; }

        public double? PackagingMaterialWeight { get; set; }

        public required string? SubmissionPeriodDesc { get; set; }

        public required DateTime LoadTimeStamp { get; set; }

        public int CalculatorRunPomDataMasterId { get; set; }

        public virtual CalculatorRunPomDataMaster? CalculatorRunPomDataMaster { get; set; }
    }
}