using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Services;

public interface IPrepareProducerDataInsertService
{
    public Task<bool> InsertProducerDataToDatabase(CalcResult calcResult, IImmutableList<MaterialDetail> materials);
}

public class PrepareProducerDataInsertService(
    IBillingInstructionService billingInstructionService,
    IProducerInvoiceNetTonnageService producerInvoiceNetTonnageService,
    ILogger<PrepareProducerDataInsertService> logger)
    : IPrepareProducerDataInsertService
{
    public async Task<bool> InsertProducerDataToDatabase(CalcResult calcResult, IImmutableList<MaterialDetail> materials)
    {
        try
        {
            var isBillingInstructionsInserted = await billingInstructionService.CreateBillingInstructions(calcResult);
            var isProducerInvoiceTonnageInserted = await producerInvoiceNetTonnageService.CreateProducerInvoiceNetTonnage(calcResult, materials);
            return isBillingInstructionsInserted && isProducerInvoiceTonnageInserted;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error occurred while populating the billing instructions");
            return false;
        }
    }
}
