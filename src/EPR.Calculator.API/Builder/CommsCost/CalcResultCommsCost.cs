using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EPR.Calculator.API.Builder.CommsCost
{
    /// <summary>
    /// The CommsCost report.
    /// </summary>
    public class CalcResultCommsCost
    {
        public IEnumerable<CalcResultCommsCostOnePlusFourApportionment> CalcResultCommsCostOnePlusFourApportionment { get; set; }
        public IEnumerable<CalcResultCommsCostCommsCostByMaterial> CalcResultCommsCostCommsCostByMaterial { get; set; }
        public IEnumerable<CalcResultCommsCostOnePlusFourApportionment> CommsCostByCountry { get; set; }
    }
}
