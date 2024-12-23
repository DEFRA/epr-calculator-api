using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.API.Builder.Summary;

public class CalcResultsProducerAndReportMaterialDetail
{
    public required ProducerDetail ProducerDetail { get; set; }
    public required ProducerReportedMaterial ProducerReportedMaterial { get; set; }
}