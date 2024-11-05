namespace EPR.Calculator.API.Exporter
{
    public interface ICalcResultsExporter<T>
    {
        public void Export(T results);
    }
}
