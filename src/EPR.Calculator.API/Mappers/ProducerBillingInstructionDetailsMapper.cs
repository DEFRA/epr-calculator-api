using EPR.Calculator.API.Dtos;

namespace EPR.Calculator.API.Mappers
{
    public static class ProducerBillingInstructionDetailsMapper
    {
        public static List<ProducerBillingInstructionsDto> Map(List<ProducerBillingInstructionDetails> details)
        {
            var producerBillingInstructions = new List<ProducerBillingInstructionsDto>();

            foreach (var detail in details)
            {
                producerBillingInstructions.Add(
                    new ProducerBillingInstructionsDto()
                    {
                        ProducerId = detail.ProducerId,
                        BillingInstructionAcceptReject = detail.BillingInstructionAcceptReject,
                        ProducerName = detail.ProducerName,
                        SuggestedBillingInstruction = detail.SuggestedBillingInstruction,
                        SuggestedInvoiceAmount = detail.SuggestedInvoiceAmount,
                    });
            }

            return producerBillingInstructions;
        }
    }
}
