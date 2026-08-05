using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.Service.Function.Services
{
    public class CalcCountryApportionmentServiceDto
    {
        public int RunId { get; set; }
        public int CostTypeId { get; set; }
        public decimal EnglandCost { get; set; }
        public decimal WalesCost { get; set; }
        public decimal NorthernIrelandCost { get; set; }
        public decimal ScotlandCost { get; set; }
        public IEnumerable<Country> Countries { get; set; } = new List<Country>();
    }
}
