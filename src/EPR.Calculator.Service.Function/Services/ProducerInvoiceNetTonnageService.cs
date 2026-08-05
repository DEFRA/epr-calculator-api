using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Exceptions;
using EPR.Calculator.Service.Function.Features.CalculatorRuns.Contexts;
using EPR.Calculator.Service.Function.Logging;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Services;

public interface IProducerInvoiceNetTonnageService
{
    Task CreateProducerInvoiceNetTonnage(CalculatorRunContext runContext, CalcResult calcResult);
}

public class ProducerInvoiceNetTonnageService(
    ApplicationDBContext dbContext,
    IBulkOperations bulkOps,
    IMaterialService materialService,
    ILogger<ProducerInvoiceNetTonnageService> logger)
    : IProducerInvoiceNetTonnageService
{
    public Task CreateProducerInvoiceNetTonnage(CalculatorRunContext runContext, CalcResult calcResult) =>
        logger.LogDuration(async () =>
        {
            try
            {
                var materials = await materialService.GetMaterials();
                var producerInvoicedNetTonnage = GetInvoicedMaterialNetTonnage(calcResult, materials);

                await bulkOps.BulkInsertAsync(dbContext, producerInvoicedNetTonnage);

                logger.LogInformation("Inserted {ProducerInvoicedNetTonnageCount} invoiced net tonnages", producerInvoicedNetTonnage.Count);
            }
            catch (Exception exception)
            {
                throw new RunProcessingException(runContext, "Error occurred while generating invoiced net tonnages, see inner exception for details.", exception);
            }
        });

    private ImmutableList<ProducerInvoicedMaterialNetTonnage> GetInvoicedMaterialNetTonnage(CalcResult calcResult, IReadOnlyList<MaterialDetail> materials)
    {
        var producers = calcResult.ProducerFees.Details
            .Where(producer => producer.FeeDetail.Level == CommonConstants.LevelOne.ToString());

        var runId = calcResult.CalcResultDetail.RunId;

        var producerInvoiceNetTonnages = ImmutableList.CreateBuilder<ProducerInvoicedMaterialNetTonnage>();

        foreach (var producer in producers.Select(fee => fee.FeeDetail))
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
