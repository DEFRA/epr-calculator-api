using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Features.Common;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;

namespace EPR.Calculator.Service.Function.Builder.CancelledProducers;

public interface ICalcResultCancelledProducersBuilder
{
    Task<IReadOnlyList<CalcResultCancelledProducer>> ConstructAsync(RunContext runContext, IReadOnlyCollection<MaterialDetail> materialDetails);
}

[ExcludeFromCodeCoverage(Justification = "Tests to be re-added within ECV-473")]
public class CalcResultCancelledProducersBuilder(IInvoicedProducerService invoicedProducerService)
    : ICalcResultCancelledProducersBuilder
{
    public async Task<IReadOnlyList<CalcResultCancelledProducer>> ConstructAsync(RunContext runContext, IReadOnlyCollection<MaterialDetail> materialDetails)
    {
        var lookup = await GetMissingAcceptedCancelledInvoicedProducersLookup(runContext);
        var materialsByCode = materialDetails.ToImmutableDictionary(m => m.Code);

        var builder = ImmutableList.CreateBuilder<CalcResultCancelledProducer>();

        foreach (var (producerId, recordsByMaterialId) in lookup)
        {
            var latestRecord = recordsByMaterialId.Values.OrderByDescending(r => r.CalculatorRunId).First();

            builder.Add(new CalcResultCancelledProducer
            {
                ProducerId = producerId,
                ProducerOrSubsidiaryName = latestRecord.ProducerName,
                TradingName = latestRecord.TradingName,

                LastTonnage = new LastTonnage
                {
                    Aluminium = recordsByMaterialId.GetValueOrDefault(materialsByCode[MaterialCodes.Aluminium].Id)?.InvoicedNetTonnage,
                    FibreComposite = recordsByMaterialId.GetValueOrDefault(materialsByCode[MaterialCodes.FibreComposite].Id)?.InvoicedNetTonnage,
                    Glass = recordsByMaterialId.GetValueOrDefault(materialsByCode[MaterialCodes.Glass].Id)?.InvoicedNetTonnage,
                    PaperOrCard = recordsByMaterialId.GetValueOrDefault(materialsByCode[MaterialCodes.PaperOrCard].Id)?.InvoicedNetTonnage,
                    Plastic = recordsByMaterialId.GetValueOrDefault(materialsByCode[MaterialCodes.Plastic].Id)?.InvoicedNetTonnage,
                    Wood = recordsByMaterialId.GetValueOrDefault(materialsByCode[MaterialCodes.Wood].Id)?.InvoicedNetTonnage,
                    Steel = recordsByMaterialId.GetValueOrDefault(materialsByCode[MaterialCodes.Steel].Id)?.InvoicedNetTonnage,
                    OtherMaterials = recordsByMaterialId.GetValueOrDefault(materialsByCode[MaterialCodes.OtherMaterials].Id)?.InvoicedNetTonnage
                },

                LatestInvoice = new LatestInvoice
                {
                    BillingInstructionId = latestRecord.BillingInstructionId,
                    RunNumber = latestRecord.CalculatorRunId.ToString(),
                    RunName = latestRecord.CalculatorName,
                    CurrentYearInvoicedTotalToDate = latestRecord.CurrentYearInvoicedTotalAfterThisRun
                }
            });
        }

        return builder.ToImmutable();
    }

    private async Task<ImmutableDictionary<int, ImmutableDictionary<int, InvoicedProducer>>> GetMissingAcceptedCancelledInvoicedProducersLookup(RunContext runContext)
    {
        var producerIdsForRun = await invoicedProducerService.GetProducerIdsForRun(runContext.RunId);
        var invoicedProducerIdsForYear = await invoicedProducerService.GetInvoicedProducerIdsForYear(runContext.RelativeYear);
        var missingProducerIds = invoicedProducerIdsForYear.Except(producerIdsForRun);

        ImmutableHashSet<int> missingAcceptedCancelledProducerIds;

        if (runContext.RunType == RunType.Billing)
        {
            var acceptedCancelledProducers = await invoicedProducerService.GetAcceptedCancelledProducerIdsForRun(runContext.RunId);
            missingAcceptedCancelledProducerIds = acceptedCancelledProducers.Intersect(missingProducerIds);
        }
        else
        {
            var acceptedCancelledProducers = await invoicedProducerService.GetInvoicedThenCancelledProducerIdsForYear(runContext.RelativeYear);
            missingAcceptedCancelledProducerIds = missingProducerIds.Except(acceptedCancelledProducers);
        }

        var missingAcceptedCancelledInvoicedProducers = await invoicedProducerService.GetInvoicedProducers(runContext.RelativeYear, missingAcceptedCancelledProducerIds);

        // The grouping here selects the latest invoice for each producer/material combination
        return missingAcceptedCancelledInvoicedProducers
            .GroupBy(r => new { r.ProducerId, r.MaterialId })
            .Select(group => group.OrderByDescending(t => t.CalculatorRunId).First())
            .GroupBy(r => r.ProducerId)
            .ToImmutableDictionary(g => g.Key, g => g.ToImmutableDictionary(r => r.MaterialId));
    }
}
