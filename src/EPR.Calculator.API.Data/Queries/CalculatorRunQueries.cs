using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.DataTypes;

namespace EPR.Calculator.API.Data.Queries;

public static class CalculatorRunQueries
{
    extension(IQueryable<CalculatorRun> calculatorRunQuery)
    {
        /// <summary>
        ///     Limits the query to all Official runs for a given year.
        /// </summary>
        public IQueryable<CalculatorRun> WhereOfficialForYear(RelativeYear relativeYear)
        {
            return calculatorRunQuery
                .Where(run =>
                    run.RelativeYear == relativeYear
                    && (run.Classification == RunClassification.Initial
                        || run.Classification == RunClassification.Recalculation));
        }

        /// <summary>
        ///     Limits the query to Official runs with shared billing files for a given year.
        /// </summary>
        public IQueryable<CalculatorRun> WhereCompletedOfficialForYear(RelativeYear relativeYear)
        {
            return calculatorRunQuery
                .WhereOfficialForYear(relativeYear)
                .Where(run => run.IsBillingFileShared);
        }

        /// <summary>
        ///     Limits the query to Official runs without shared billing files for a given year.
        /// </summary>
        public IQueryable<CalculatorRun> WhereIncompleteOfficialForYear(RelativeYear relativeYear)
        {
            return calculatorRunQuery
                .WhereOfficialForYear(relativeYear)
                .Where(run => !run.IsBillingFileShared);
        }
    }
}
