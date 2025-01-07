namespace EPR.Calculator.API.Exporter
{
    public interface ICalcResultsExporter<T>
    {
        string Export(T results);
    }
}
