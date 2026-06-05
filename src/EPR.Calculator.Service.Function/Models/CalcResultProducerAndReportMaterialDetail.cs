using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.Service.Function.Models
{
    public class CalcResultProducerAndReportMaterialDetail
    {
        required public ProducerDetail ProducerDetail { get; set; }

        required public ProducerReportedMaterialProjected ProducerReportedMaterialProjected { get; set; }
    }
}
