using System.ComponentModel.DataAnnotations.Schema;
using EPR.Calculator.API.Data.Models;

namespace EPR.Calculator.API.Data.DataModels;

public class CalculatorRun
{
    public int Id { get; set; }
    public int CalculatorRunClassificationId { get; set; }
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
    public bool? IsBillingFileGenerating { get; set; } = null;

    #region EF navigational properties

    public virtual CalculatorRunPomDataMaster? CalculatorRunPomDataMaster { get; set; }
    public virtual CalculatorRunOrganisationDataMaster? CalculatorRunOrganisationDataMaster { get; set; }
    public virtual LapcapDataMaster? LapcapDataMaster { get; set; }
    public virtual DefaultParameterSettingMaster? DefaultParameterSettingMaster { get; set; }
    public virtual ICollection<ProducerDetail> ProducerDetails { get; } = [];
    public virtual ICollection<CountryApportionment> CountryApportionments { get; } = [];
    public virtual ICollection<CalculatorRunBillingFileMetadata> CalculatorRunBillingFileMetadata { get; set; } = [];
    public virtual ICollection<CalculatorRunCsvFileMetadata> CsvFileMetadata { get; } = [];
    public virtual ICollection<ProducerInvoicedMaterialNetTonnage> ProducerInvoicedMaterialNetTonnage { get; } = [];
    public virtual ICollection<ProducerDesignatedRunInvoiceInstruction> ProducerDesignatedRunInvoiceInstruction { get; } = [];
    public virtual ICollection<ProducerResultFileSuggestedBillingInstruction> ProducerResultFileSuggestedBillingInstruction { get; } = [];
    public virtual ICollection<ErrorReport> ErrorReports { get; } = [];

    #endregion
}
