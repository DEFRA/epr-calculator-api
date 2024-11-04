using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.API.Services
{
    public static class DataService
    {
        public static IEnumerable<CalculatorRunPomDataDetail> GetPomDataDetails(ApplicationDBContext context, int pomDataMasterId)
        {
            return context.CalculatorRunPomDataDetails.Where(pdd => pdd.CalculatorRunPomDataMasterId == pomDataMasterId).ToList();
        }
    }
}
