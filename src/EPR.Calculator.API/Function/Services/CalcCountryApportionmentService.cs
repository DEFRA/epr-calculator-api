using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.Service.Function.Services
{

    public class CalcCountryApportionmentService : ICalcCountryApportionmentService
    {
        private readonly ApplicationDBContext context;
        public CalcCountryApportionmentService(ApplicationDBContext context)
        {
            this.context = context; 
        }

        public async Task SaveChangesAsync(CalcCountryApportionmentServiceDto countryApportionmentServiceDto)
        {
            context.CountryApportionment.Add(new CountryApportionment
            {
                CalculatorRunId = countryApportionmentServiceDto.RunId,
                CountryId = countryApportionmentServiceDto.Countries.Single(x => x.Name == "England").Id,
                CostTypeId = countryApportionmentServiceDto.CostTypeId,
                Apportionment = countryApportionmentServiceDto.EnglandCost,
            });

            context.CountryApportionment.Add(new CountryApportionment
            {
                CalculatorRunId = countryApportionmentServiceDto.RunId,
                CountryId = countryApportionmentServiceDto.Countries.Single(x => x.Name == "Wales").Id,
                CostTypeId = countryApportionmentServiceDto.CostTypeId,
                Apportionment = countryApportionmentServiceDto.WalesCost,
            });

            context.CountryApportionment.Add(new CountryApportionment
            {
                CalculatorRunId = countryApportionmentServiceDto.RunId,
                CountryId = countryApportionmentServiceDto.Countries.Single(x => x.Name == "Northern Ireland").Id,
                CostTypeId = countryApportionmentServiceDto.CostTypeId,
                Apportionment = countryApportionmentServiceDto.NorthernIrelandCost,
            });

            context.CountryApportionment.Add(new CountryApportionment
            {
                CalculatorRunId = countryApportionmentServiceDto.RunId,
                CountryId = countryApportionmentServiceDto.Countries.Single(x => x.Name == "Scotland").Id,
                CostTypeId = countryApportionmentServiceDto.CostTypeId,
                Apportionment = countryApportionmentServiceDto.ScotlandCost,
            });

            await context.SaveChangesAsync();
        }   
    }
}
