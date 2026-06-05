using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Logging;
using EPR.Calculator.Service.Function.Mappers;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Services;

public interface IProducerInvoiceNetTonnageService
{
    public Task<bool> CreateProducerInvoiceNetTonnage(CalcResult calcResult, IImmutableList<MaterialDetail> materials);
}

public class ProducerInvoiceNetTonnageService(
    ApplicationDBContext dbContext,
    IBulkOperations bulkOps,
    IProducerInvoiceTonnageMapper producerInvoiceTonnageMapper,
    ILogger<ProducerInvoiceNetTonnageService> logger)
    : IProducerInvoiceNetTonnageService
{
    public Task<bool> CreateProducerInvoiceNetTonnage(CalcResult calcResult, IImmutableList<MaterialDetail> materials) =>
        logger.LogDuration(async () =>

        {
            try
            {
                var producerInvoicedNetTonnage = GetInvoicedMaterialNetTonnage(calcResult, materials);

                if (producerInvoicedNetTonnage.Count == 0)
                {
                    logger.LogError("No producer invoiced net tonnage generated");
                    return false;
                }

                await bulkOps.BulkInsertAsync(dbContext, producerInvoicedNetTonnage);

                logger.LogInformation("Inserted {ProducerInvoicedNetTonnageCount} producer invoiced net tonnages", producerInvoicedNetTonnage.Count);
                return true;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Error occurred while populating the producer invoiced net tonnages");
                return false;
            }
        });

    private ImmutableList<ProducerInvoicedMaterialNetTonnage> GetInvoicedMaterialNetTonnage(CalcResult calcResult, IReadOnlyList<MaterialDetail> materials)
    {
        var producers = calcResult.CalcResultSummary.ProducerDisposalFees
            .Where(producer => producer.Level == CommonConstants.LevelOne.ToString());

        var runId = calcResult.CalcResultDetail.RunId;

        var producerInvoiceNetTonnages = ImmutableList.CreateBuilder<ProducerInvoicedMaterialNetTonnage>();

        foreach (var producer in producers)
        {
            foreach (var material in materials)
            {
                var invoiced = new ProducerInvoicedMaterialNetTonnage();
                var disposalFees = producer.ProducerDisposalFeesByMaterial;

                if (disposalFees is not null && disposalFees.TryGetValue(material.Code, out var feeSummary))
                {
                    invoiced = producerInvoiceTonnageMapper.Map(new ProducerInvoiceTonnage
                    {
                        RunId = runId,
                        ProducerId = producer.ProducerIdInt,
                        NetTonnage = feeSummary.NetReportedTonnage.total,
                        MaterialId = material.Id
                    });
                }

                producerInvoiceNetTonnages.Add(invoiced);
            }
        }

        return producerInvoiceNetTonnages.ToImmutable();
    }
}
