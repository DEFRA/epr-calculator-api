using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Exceptions;
using EPR.Calculator.Service.Function.Features.CalculatorRun.Contexts;
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

                if (producerInvoicedNetTonnage.Count == 0)
                    throw new RunProcessingException(runContext, "No invoiced net tonnages generated");

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
                    invoiced = new ProducerInvoicedMaterialNetTonnage
                    {
                        CalculatorRunId = runId,
                        ProducerId = producer.ProducerIdInt,
                        InvoicedNetTonnage = feeSummary.NetReportedTonnage.total,
                        MaterialId = material.Id
                    };
                }

                producerInvoiceNetTonnages.Add(invoiced);
            }
        }

        return producerInvoiceNetTonnages.ToImmutable();
    }
}
