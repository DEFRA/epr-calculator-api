namespace EPR.Calculator.API.Exceptions;

public class DataRetrievalException : Exception
{
    public DataRetrievalException() : base()
    {
    }

    public DataRetrievalException(string message) : base(message)
    {
    }

    public DataRetrievalException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
