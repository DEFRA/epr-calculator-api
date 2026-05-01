using System.ComponentModel.DataAnnotations.Schema;
using EPR.Calculator.API.Data.Enums;
using EPR.Calculator.API.Data.Models;

namespace EPR.Calculator.API.Data.DataModels;

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

    #region EF navigational properties

    public virtual CalculatorRunPomDataMaster? CalculatorRunPomDataMaster { get; set; }
    public virtual CalculatorRunOrganisationDataMaster? CalculatorRunOrganisationDataMaster { get; set; }
    public virtual LapcapDataMaster? LapcapDataMaster { get; set; }
    public virtual DefaultParameterSettingMaster? DefaultParameterSettingMaster { get; set; }
    public virtual ICollection<ProducerDetail> ProducerDetails { get; } = [];
    public virtual ICollection<CountryApportionment> CountryApportionments { get; } = [];
    public virtual CalculatorRunBillingFileMetadata? BillingFileMetadata { get; set; }
    public virtual ICollection<CalculatorRunCsvFileMetadata> CsvFileMetadata { get; } = [];
    public virtual ICollection<ProducerInvoicedMaterialNetTonnage> ProducerInvoicedMaterialNetTonnage { get; } = [];
    public virtual ICollection<ProducerDesignatedRunInvoiceInstruction> ProducerDesignatedRunInvoiceInstruction { get; } = [];
    public virtual ICollection<ProducerResultFileSuggestedBillingInstruction> ProducerResultFileSuggestedBillingInstruction { get; } = [];
    public virtual ICollection<ErrorReport> ErrorReports { get; } = [];

    #endregion
}
