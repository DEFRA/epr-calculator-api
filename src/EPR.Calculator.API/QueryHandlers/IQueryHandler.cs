namespace EPR.Calculator.API.QueryHandlers
{
    public interface IQueryHandler<TQuery, TResult>
    {
        TResult Query(TQuery query);
    }
}
