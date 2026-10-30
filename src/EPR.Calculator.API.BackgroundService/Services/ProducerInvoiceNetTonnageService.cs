using EPR.Calculator.API.BackgroundService.Constants;
using EPR.Calculator.API.BackgroundService.Exceptions;
using EPR.Calculator.API.BackgroundService.Features.CalculatorRuns.Contexts;
using EPR.Calculator.API.BackgroundService.Models;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.API.BackgroundService.Services;

public interface IProducerInvoiceNetTonnageService
{
    Task CreateProducerInvoiceNetTonnage(CalculatorRunContext runContext, CalcResult calcResult, IReadOnlyList<FeeDetail> producerFeeDetails, CancellationToken cancellationToken);
}

public class ProducerInvoiceNetTonnageService(
    ApplicationDBContext dbContext,
    IBulkOperations bulkOps,
    IMaterialService materialService,
    ILogger<ProducerInvoiceNetTonnageService> logger
) : IProducerInvoiceNetTonnageService
{
    [ActivityTrace]
    public async Task CreateProducerInvoiceNetTonnage(CalculatorRunContext runContext, CalcResult calcResult, IReadOnlyList<FeeDetail> producerFeeDetails, CancellationToken cancellationToken)
    {
        try
        {
            var materials = await materialService.GetMaterials();
            var producerInvoicedNetTonnage = GetInvoicedMaterialNetTonnage(calcResult, producerFeeDetails, materials);

            await bulkOps.BulkInsertAsync(dbContext, producerInvoicedNetTonnage, cancellationToken);

            logger.LogInformation("Inserted {ProducerInvoicedNetTonnageCount} invoiced net tonnages", producerInvoicedNetTonnage.Count);
        }
        catch (Exception exception)
        {
            throw new RunProcessingException(runContext, "Error occurred while generating invoiced net tonnages, see inner exception for details.", exception);
        }
    }

    private static ImmutableList<ProducerInvoicedMaterialNetTonnage> GetInvoicedMaterialNetTonnage(CalcResult calcResult, IReadOnlyList<FeeDetail> producerFeeDetails, IReadOnlyList<MaterialDetail> materials)
    {
        var producers = producerFeeDetails.Where(producer => producer.Level == CommonConstants.LevelOne.ToString());

        var runId = calcResult.CalcResultDetail.RunId;

        var producerInvoiceNetTonnages = ImmutableList.CreateBuilder<ProducerInvoicedMaterialNetTonnage>();

        foreach (var producer in producers)
        {
            foreach (var material in materials)
            {
                var invoiced = new ProducerInvoicedMaterialNetTonnage();
                var disposalFees = producer.DisposalFeesByMaterial.ToDictionary(k => k.Key, v => v.Value);

                if (disposalFees.TryGetValue(material.Code, out var feeSummary))
                {
                    invoiced = new ProducerInvoicedMaterialNetTonnage
                    {
                        CalculatorRunId = runId,
                        ProducerId = producer.ProducerId,
                        InvoicedNetTonnage = feeSummary.NetTonnage.Total,
                        MaterialId = material.Id
                    };
                }

                producerInvoiceNetTonnages.Add(invoiced);
            }
        }

        return producerInvoiceNetTonnages.ToImmutable();
    }
}
