using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.API.BackgroundService.Models
{
    public class CalcResultProducerAndReportMaterialDetail
    {
        required public ProducerDetail ProducerDetail { get; set; }

        required public ProducerMaterialPackaging ProducerMaterialPackaging { get; set; }
    }
}
