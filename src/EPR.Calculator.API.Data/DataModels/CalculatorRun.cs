using EPR.Calculator.API.Data.DataTypes;

namespace EPR.Calculator.API.Data.DataModels;

public class CalculatorRun
{
    public int Id { get; set; }
    public RunClassification Classification { get; set; }
    public CalculationRunStatus CalculationRunStatus { get; set; }
    public BillingRunStatus BillingRunStatus { get; set; }
    public DateTime? BillingRunStartedAt { get; set; }
    public bool IsBillingFileShared { get; set; }
    public string? BillingFileSharedBy { get; set; }
    public DateTime? BillingFileSharedAt { get; set; }
    public required string Name { get; set; }
    public RelativeYear RelativeYear { get; set; }
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
    public virtual ICollection<CalculatorRunBillingFileMetadata> CalculatorRunBillingFileMetadata { get; set; } = [];
    public virtual ICollection<CalculatorRunCsvFileMetadata> CsvFileMetadata { get; } = [];
    public virtual ICollection<ProducerInvoicedMaterialNetTonnage> ProducerInvoicedMaterialNetTonnage { get; } = [];
    public virtual ICollection<ProducerDesignatedRunInvoiceInstruction> ProducerDesignatedRunInvoiceInstruction { get; } = [];
    public virtual ICollection<ProducerResultFileSuggestedBillingInstruction> ProducerResultFileSuggestedBillingInstruction { get; } = [];
    public virtual ICollection<ErrorReport> ErrorReports { get; } = [];

    #endregion
}

public static class CalculatorRunExtensions
{
    /// <summary>
    ///     How long a billing run can be <see cref="BillingRunStatus.Started" /> before it's considered stuck, e.g. due to an
    ///     unclean shutdown of the processor.
    /// </summary>
    public static readonly TimeSpan BillingRunTimeout = TimeSpan.FromHours(1);

    extension(CalculatorRun run)
    {
        /// <summary>
        ///     Is a billing run underway? i.e. <see cref="BillingRunStatus.Started" /> within the last
        ///     <see cref="BillingRunTimeout" />.
        /// </summary>
        /// <remarks>
        ///     Billing runs that were started longer ago are considered stuck, so may be restarted. Those without a start time
        ///     are never considered stuck.
        /// </remarks>
        public bool IsBillingRunUnderway(DateTimeOffset now) =>
            run.BillingRunStatus == BillingRunStatus.Started
            && (run.BillingRunStartedAt is not { } startedAt || now.UtcDateTime - startedAt <= BillingRunTimeout);
    }
}
