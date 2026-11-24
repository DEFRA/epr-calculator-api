using EPR.Calculator.API.BackgroundService.Builder.CommsCost;
using EPR.Calculator.API.BackgroundService.Builder.LaDisposalCost;
using EPR.Calculator.API.BackgroundService.Builder.Lapcap;
using EPR.Calculator.API.BackgroundService.Builder.LateReportingTonnages;
using EPR.Calculator.API.BackgroundService.Builder.OnePlusFourApportionment;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.DataTypes;

namespace EPR.Calculator.API.BackgroundService.Models
{
    public record CalcResult
    {
        public required CalcResultDetail CalcResultDetail { get; set; }

        public required CalcResultLapcapData CalcResultLapcapData { get; set; } =
            CalcResultLapcapDataBuilder.Create(new Dictionary<string, ByCountryCost>());

        public CalcResultCommsCost CalcResultCommsCostReportDetail { get; set; } =
            CalcResultCommsCostBuilder.Create(
                onePlusFourApportionment: ByCountryApportionment.Empty,
                byMaterial:               new Dictionary<string, CalcResultCommsCostCommsCostByMaterial>(),
                commsCostUkWide:          ByCountryCost.Empty,
                commsCostByCountry:       ByCountryCost.Empty);

        public required CalcResultLateReportingTonnage CalcResultLateReportingTonnageData { get; set; } =
            CalcResultLateReportingBuilder.Create(new Dictionary<string, CalcResultLateReportingTonnageDetail>());


        public required CalcResultParameterOtherCost CalcResultParameterOtherCost { get; set; } =
            new()
            {
                SchemeSetupCost  = ByCountryCost.Empty
            };

        public CalcResultOnePlusFourApportionment CalcResultOnePlusFourApportionment { get; set; }
            = CalcResultOnePlusFourApportionmentBuilder.Create(ByCountryCost.Empty, ByCountryCost.Empty);

        public CalcResultLaDisposalCostData CalcResultLaDisposalCostData { get; set; }
            = CalcRunLaDisposalCostBuilder.Create(new Dictionary<string, CalcResultLaDisposalCostDataDetail>());

        public required CalcResultPartialObligations CalcResultPartialObligations { get; set; }

        public required CalcResultProjectedProducers CalcResultProjectedProducers { get; set; }

        public required CalcResultScaledupProducers CalcResultScaledupProducers { get; set; }

        public IReadOnlyList<CalcResultCancelledProducer> CalcResultCancelledProducers { get; set; } = [];

        public IEnumerable<CalcResultRejectedProducer> CalcResultRejectedProducers { get; set; } = [];

        public FeeDetail ProducerFeesTotal { get; set; } = new() { ProducerId = 0, SubsidiaryId = string.Empty, ProducerName = string.Empty };

        public IEnumerable<CalcResultErrorReport> CalcResultErrorReports { get; set; } = [];

        public ModulationResult? CalcResultModulation { get; set; }

        public static CalcResult Empty =>
            new CalcResult
            {
                CalcResultDetail = new CalcResultDetail { RunId = 0, RelativeYear = new RelativeYear() } ,
                CalcResultLapcapData = CalcResultLapcapDataBuilder.Create(new Dictionary<string, ByCountryCost>()),
                CalcResultLateReportingTonnageData = CalcResultLateReportingBuilder.Create(new Dictionary<string, CalcResultLateReportingTonnageDetail>()),
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
