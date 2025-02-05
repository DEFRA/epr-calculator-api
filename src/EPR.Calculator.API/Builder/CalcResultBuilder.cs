using EPR.Calculator.API.Builder.CommsCost;
using EPR.Calculator.API.Builder.Detail;
using EPR.Calculator.API.Builder.LaDisposalCost;
using EPR.Calculator.API.Builder.Lapcap;
using EPR.Calculator.API.Builder.LateReportingTonnages;
using EPR.Calculator.API.Builder.OnePlusFourApportionment;
using EPR.Calculator.API.Builder.ParametersOther;
using EPR.Calculator.API.Builder.ScaledupProducers;
using EPR.Calculator.API.Builder.Summary;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.Builder
{
    public class CalcResultBuilder : ICalcResultBuilder
    {
        private readonly ApplicationDBContext context;
        private readonly ICalcResultParameterOtherCostBuilder calcResultParameterOtherCostBuilder;
        private readonly ICalcResultDetailBuilder calcResultDetailBuilder;
        private readonly ICalcResultLapcapDataBuilder lapcapBuilder;
        private readonly ICalcResultSummaryBuilder summaryBuilder;
        private readonly ICalcResultOnePlusFourApportionmentBuilder lapcapplusFourApportionmentBuilder;
        private readonly ICalcResultCommsCostBuilder commsCostReportBuilder;
        private readonly ICalcResultLateReportingBuilder lateReportingBuilder;
        private readonly ICalcRunLaDisposalCostBuilder laDisposalCostBuilder;
        private readonly ICalcResultScaledupProducersBuilder calcResultScaledupProducersBuilder;

        public CalcResultBuilder(
            ApplicationDBContext context,
            ICalcResultDetailBuilder calcResultDetailBuilder,
            ICalcResultLapcapDataBuilder lapcapBuilder,
            ICalcResultParameterOtherCostBuilder calcResultParameterOtherCostBuilder,
            ICalcResultOnePlusFourApportionmentBuilder calcResultOnePlusFourApportionmentBuilder,
            ICalcResultCommsCostBuilder commsCostReportBuilder,
            ICalcResultLateReportingBuilder lateReportingBuilder,
            ICalcRunLaDisposalCostBuilder calcRunLaDisposalCostBuilder,
            ICalcResultScaledupProducersBuilder calcResultScaledupProducersBuilder,
            ICalcResultSummaryBuilder summaryBuilder)
        {
            this.context = context;
            this.calcResultDetailBuilder = calcResultDetailBuilder;
            this.lapcapBuilder = lapcapBuilder;
            this.commsCostReportBuilder = commsCostReportBuilder;
            this.lateReportingBuilder = lateReportingBuilder;
            this.calcResultParameterOtherCostBuilder = calcResultParameterOtherCostBuilder;
            this.laDisposalCostBuilder = calcRunLaDisposalCostBuilder;
            this.lapcapplusFourApportionmentBuilder = calcResultOnePlusFourApportionmentBuilder;
            this.calcResultScaledupProducersBuilder = calcResultScaledupProducersBuilder;
            this.summaryBuilder = summaryBuilder;
        }

        public async Task<CalcResult> Build(CalcResultsRequestDto resultsRequestDto)
        {
            var result = new CalcResult
            {
                CalcResultLapcapData =
                    new CalcResultLapcapData
                    {
                        CalcResultLapcapDataDetails = new List<CalcResultLapcapDataDetails>()
                    },
                CalcResultLateReportingTonnageData = new CalcResultLateReportingTonnage 
                    { 
                        CalcResultLateReportingTonnageDetails = new List<CalcResultLateReportingTonnageDetail>()
                    },
                CalcResultParameterOtherCost = new CalcResultParameterOtherCost 
                    {
                        Name = string.Empty
                    }
            };

            var scaledupProducers = await GetScaledupProducers(resultsRequestDto);

            result.CalcResultDetail = await this.calcResultDetailBuilder.Construct(resultsRequestDto);
            result.CalcResultLapcapData = await this.lapcapBuilder.Construct(resultsRequestDto);
            result.CalcResultLateReportingTonnageData = await this.lateReportingBuilder.Construct(resultsRequestDto);
            result.CalcResultParameterOtherCost = await this.calcResultParameterOtherCostBuilder.Construct(resultsRequestDto);
            
            result.CalcResultOnePlusFourApportionment = this.lapcapplusFourApportionmentBuilder.Construct(resultsRequestDto, result);
            result.CalcResultCommsCostReportDetail = await this.commsCostReportBuilder.Construct(
                resultsRequestDto, result.CalcResultOnePlusFourApportionment);
            result.CalcResultLaDisposalCostData = await this.laDisposalCostBuilder.Construct(resultsRequestDto, result);
            result.CalcResultScaledupProducers = await this.calcResultScaledupProducersBuilder.Construct(resultsRequestDto, result, scaledupProducers);
            result.CalcResultSummary = await this.summaryBuilder.Construct(resultsRequestDto, result);

            return result;
        }

        private async Task<IEnumerable<ScaledupProducer>> GetScaledupProducers(CalcResultsRequestDto resultsRequestDto)
        {
            return await(
                        from run in context.CalculatorRuns
                        join pdm in context.CalculatorRunPomDataMaster
                            on run.CalculatorRunPomDataMasterId equals pdm.Id
                        join pdd in context.CalculatorRunPomDataDetails
                            on pdm.Id equals pdd.CalculatorRunPomDataMasterId
                        join spl in context.SubmissionPeriodLookup
                            on pdd.SubmissionPeriod equals spl.SubmissionPeriod
                        where run.Id == resultsRequestDto.RunId
                        select new ScaledupProducer
                        {
                            ProducerId = pdd.OrganisationId,
                            SubmissionPeriod = pdd.SubmissionPeriod,
                            ScaleupFactor = spl.ScaleupFactor,
                            DaysInSubmissionPeriod = spl.DaysInSubmissionPeriod,
                            DaysInWholePeriod = spl.DaysInWholePeriod
                        }).Distinct().ToListAsync();
        }
    }
}