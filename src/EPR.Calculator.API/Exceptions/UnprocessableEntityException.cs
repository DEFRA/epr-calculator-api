namespace EPR.Calculator.API.Exceptions
{
    public class UnprocessableEntityException : Exception
    {
        public UnprocessableEntityException(string message)
            : base(message)
        {
        }
    }
}
