using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.DataTypes;

namespace EPR.Calculator.Service.Function.Models
{
    public record CalcResult
    {
        public required CalcResultDetail CalcResultDetail { get; set; }

        public required CalcResultLapcapData CalcResultLapcapData { get; set; } =
            new() { ByMaterial = new Dictionary<string, ByCountryCost>() };

        public CalcResultCommsCost CalcResultCommsCostReportDetail { get; set; } =
            new() {
                OnePlusFourApportionment = ByCountryApportionment.Empty,
                ByMaterial               = new Dictionary<string, CalcResultCommsCostCommsCostByMaterial>(),
                CommsCostUkWide          = ByCountryCost.Empty,
                CommsCostByCountry       = ByCountryCost.Empty
            };

        public required CalcResultLateReportingTonnage CalcResultLateReportingTonnageData { get; set; } =
            new() { ByMaterial = new Dictionary<string, CalcResultLateReportingTonnageDetail>() };


        public required CalcResultParameterOtherCost CalcResultParameterOtherCost { get; set; } =
            new()
            {
                SchemeSetupCost  = ByCountryCost.Empty
            };

        public CalcResultOnePlusFourApportionment CalcResultOnePlusFourApportionment { get; set; }
            = new()
            {
                LADataPrepCharge = ByCountryCost.Empty,
                LaDisposalCost   = ByCountryCost.Empty
            };

        public CalcResultLaDisposalCostData CalcResultLaDisposalCostData { get; set; }
            = new() { ByMaterial = new Dictionary<string, CalcResultLaDisposalCostDataDetail>() };

        public required CalcResultPartialObligations CalcResultPartialObligations { get; set; }

        public required CalcResultProjectedProducers CalcResultProjectedProducers { get; set; }

        public required CalcResultScaledupProducers CalcResultScaledupProducers { get; set; }

        public IReadOnlyList<CalcResultCancelledProducer> CalcResultCancelledProducers { get; set; } = [];

        public IEnumerable<CalcResultRejectedProducer> CalcResultRejectedProducers { get; set; } = [];

        public ProducerFees ProducerFees { get; set; } = new() { CalculatorRunId = 0, Total = new() { ProducerId = 0, SubsidiaryId = string.Empty, ProducerName = string.Empty } };

        public IEnumerable<CalcResultErrorReport> CalcResultErrorReports { get; set; } = [];

        public SelfManagedConsumerWaste? Smcw { get; set; }

        public ModulationResult? CalcResultModulation { get; set; }

        public static CalcResult Empty => 
            new CalcResult
            {
                CalcResultDetail = new CalcResultDetail { RunId = 0, RelativeYear = new RelativeYear() } ,
                CalcResultLapcapData = new CalcResultLapcapData
                {
                    ByMaterial = new Dictionary<string, ByCountryCost>()
                },
                CalcResultLateReportingTonnageData = new CalcResultLateReportingTonnage
                {
                    ByMaterial = new Dictionary<string, CalcResultLateReportingTonnageDetail>()
                },
                CalcResultParameterOtherCost = new CalcResultParameterOtherCost(),
                CalcResultPartialObligations = new CalcResultPartialObligations(){
                    PartialObligations = ImmutableList<CalcResultPartialObligation>.Empty,
                },
                CalcResultProjectedProducers = new CalcResultProjectedProducers(){
                    H1ProjectedProducers = ImmutableList<CalcResultH1ProjectedProducer>.Empty,
                    H2ProjectedProducers = ImmutableList<CalcResultH2ProjectedProducer>.Empty
                },
                CalcResultScaledupProducers = new CalcResultScaledupProducers(){
                    ScaledupProducers = ImmutableList<CalcResultScaledupProducer>.Empty,
                },
                CalcResultCancelledProducers = ImmutableList<CalcResultCancelledProducer>.Empty,
                CalcResultRejectedProducers = new List<CalcResultRejectedProducer>()
            };
    }
}
