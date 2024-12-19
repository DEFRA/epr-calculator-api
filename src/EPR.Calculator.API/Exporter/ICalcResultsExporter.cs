namespace EPR.Calculator.API.Exporter
{
    public interface ICalcResultsExporter<T>
    {
        public string Export(T results);
    }
}
