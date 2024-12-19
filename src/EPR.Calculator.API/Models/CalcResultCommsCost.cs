namespace EPR.Calculator.API.Models
{
    /// <summary>
    /// The CommsCost report.
    /// </summary>
    public class CalcResultCommsCost
    {
        public string Name { get; set; } = string.Empty;
        public IEnumerable<CalcResultCommsCostOnePlusFourApportionment> CalcResultCommsCostOnePlusFourApportionment { get; set; }
            = [];
        public IEnumerable<CalcResultCommsCostCommsCostByMaterial> CalcResultCommsCostCommsCostByMaterial { get; set; }
            = [];
        public IEnumerable<CalcResultCommsCostOnePlusFourApportionment> CommsCostByCountry { get; set; }
            = [];
    }
}
