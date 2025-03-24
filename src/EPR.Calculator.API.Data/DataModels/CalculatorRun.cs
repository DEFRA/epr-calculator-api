using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.API.Data.DataModels
{
    [Table("calculator_run")]
    public class CalculatorRun
    {
        [Column("id")]
        [Required]
        public int Id { get; set; }

        [Column("calculator_run_classification_id")]
        [Required]
        public int CalculatorRunClassificationId { get; set; }

        [Column("name")]
        [StringLength(250)]
        [Required]
        public required string Name { get; set; }

        [Column("financial_year")]
        [Required]
        public string FinancialYearId { get; set; }

        public required CalculatorRunFinancialYear Financial_Year { get; set; }

        [Column("created_by")]
        [StringLength(400)]
        [Required]
        public string CreatedBy { get; set; } = string.Empty;

        [Column("created_at")]
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("updated_by")]
        [StringLength(400)]
        public string? UpdatedBy { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [Column("calculator_run_pom_data_master_id")]
        public int? CalculatorRunPomDataMasterId { get; set; }

        [Column("calculator_run_organization_data_master_id")]
        public int? CalculatorRunOrganisationDataMasterId { get; set; }

        [Column("lapcap_data_master_id")]
        public int? LapcapDataMasterId { get; set; }

        [Column("default_parameter_setting_master_id")]
        public int? DefaultParameterSettingMasterId { get; set; }

        public CalculatorRunPomDataMaster? CalculatorRunPomDataMaster { get; set; }

        public CalculatorRunOrganisationDataMaster? CalculatorRunOrganisationDataMaster { get; set; }

        public LapcapDataMaster? LapcapDataMaster { get; set; }

        public virtual DefaultParameterSettingMaster? DefaultParameterSettingMaster { get; set; }

        public ICollection<ProducerDetail> ProducerDetails { get; } = new List<ProducerDetail>();

        public ICollection<CountryApportionment> CountryApportionments { get; } = new List<CountryApportionment>();
    }
}
