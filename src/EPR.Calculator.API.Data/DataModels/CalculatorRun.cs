using System.ComponentModel.DataAnnotations.Schema;
using EPR.Calculator.API.Data.Enums;
using EPR.Calculator.API.Data.Models;

namespace EPR.Calculator.API.Data.DataModels
{
    public class CalculatorRun
    {
        public int Id { get; set; }
        public RunClassification Classification { get; set; } = RunClassification.None;
        public BillingRunStatus BillingRunStatus { get; set; } = BillingRunStatus.None;

        /// <summary>
        ///     Gets or sets the UTC timestamp of when the billing run started.
        ///     Null if the billing run has never been started.
        /// </summary>
        public DateTime? BillingRunStartedAt { get; set; }

        public required string Name { get; set; }

        public int RelativeYearValue { get; private set; }

        [NotMapped]
        public RelativeYear RelativeYear
        {
            get => new(RelativeYearValue);
            set => RelativeYearValue = value.Value;
        }

        public string CreatedBy { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? UpdatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public int? CalculatorRunPomDataMasterId { get; set; }

        public int? CalculatorRunOrganisationDataMasterId { get; set; }

        public int? LapcapDataMasterId { get; set; }

        public int? DefaultParameterSettingMasterId { get; set; }

        public CalculatorRunPomDataMaster? CalculatorRunPomDataMaster { get; set; }

        public CalculatorRunOrganisationDataMaster? CalculatorRunOrganisationDataMaster { get; set; }

        public LapcapDataMaster? LapcapDataMaster { get; set; }

        public virtual DefaultParameterSettingMaster? DefaultParameterSettingMaster { get; set; }

        public ICollection<ProducerDetail> ProducerDetails { get; } = new List<ProducerDetail>();

        public ICollection<CountryApportionment> CountryApportionments { get; } = new List<CountryApportionment>();

        public CalculatorRunBillingFileMetadata? BillingFileMetadata { get; set; }

        public ICollection<ProducerInvoicedMaterialNetTonnage> ProducerInvoicedMaterialNetTonnage { get; } = [];

        public ICollection<ProducerDesignatedRunInvoiceInstruction> ProducerDesignatedRunInvoiceInstruction { get; } = [];

        public ICollection<ProducerResultFileSuggestedBillingInstruction> ProducerResultFileSuggestedBillingInstruction { get; } = [];

        public ICollection<ErrorReport> ErrorReports { get; } = new List<ErrorReport>();
    }
}
