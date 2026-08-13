namespace EPR.Calculator.API.BackgroundService.Services
{
    public interface ICalcCountryApportionmentService
    {
        Task SaveChangesAsync(CalcCountryApportionmentServiceDto countryApportionmentServiceDto);
    }
}
