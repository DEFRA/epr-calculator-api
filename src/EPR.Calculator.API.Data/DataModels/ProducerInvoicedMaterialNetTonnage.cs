namespace EPR.Calculator.API.Data.DataModels
{
    public class ProducerInvoicedMaterialNetTonnage
    {
        public int Id { get; set; }

        public int CalculatorRunId { get; set; }

        public int MaterialId { get; set; }

        public int ProducerId { get; set; }

        public decimal? InvoicedNetTonnage { get; set; }
    }
}
