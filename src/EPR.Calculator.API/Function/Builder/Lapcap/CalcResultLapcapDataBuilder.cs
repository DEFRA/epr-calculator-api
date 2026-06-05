using EPR.Calculator.API.Data;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.Builder.Lapcap
{
    public interface ICalcResultLapcapDataBuilder
    {
        Task<CalcResultLapcapData> ConstructAsync(
            IEnumerable<MaterialDetail> materialDetails,
            CalcResultsRequestDto resultsRequestDto
        );
    }

    public class CalcResultLapcapDataBuilder : ICalcResultLapcapDataBuilder
    {
        private readonly ICalcCountryApportionmentService calcCountryApportionmentService;
        private readonly ApplicationDBContext dbContext;

        public CalcResultLapcapDataBuilder(ApplicationDBContext dbContext,
            ICalcCountryApportionmentService calcCountryApportionmentService)
        {
            this.dbContext = dbContext;
            this.calcCountryApportionmentService = calcCountryApportionmentService;
        }

#pragma warning disable S1854
        public async Task<CalcResultLapcapData> ConstructAsync(
            IEnumerable<MaterialDetail> materialDetails,
            CalcResultsRequestDto resultsRequestDto
        )
        {
            var results = await (
                from run in dbContext.CalculatorRuns
                join lapcapMaster in dbContext.LapcapDataMaster on run.LapcapDataMasterId equals lapcapMaster.Id
                join lapcapDetail in dbContext.LapcapDataDetail on lapcapMaster.Id equals lapcapDetail.LapcapDataMasterId
                join lapcapTemplate in dbContext.LapcapDataTemplateMaster on lapcapDetail.UniqueReference equals lapcapTemplate.UniqueReference
                where run.Id == resultsRequestDto.RunId
                select new ResultsClass
                {
                    Material  = lapcapTemplate.Material,
                    Country   = lapcapTemplate.Country,
                    TotalCost = lapcapDetail.TotalCost,
                }
            ).ToListAsync();

            var countries = await dbContext.Country.ToListAsync();

            var costType = await dbContext.CostType.SingleAsync(x => x.Name == "Fee for LA Disposal Costs");
            var costTypeId = costType.Id;

            var data = materialDetails.Select(material =>
                (material.Code, new ByCountryCost
                {
                    England         = GetMaterialDisposalCostPerCountry(CountryConstants.England , material, results),
                    Wales           = GetMaterialDisposalCostPerCountry(CountryConstants.Wales   , material, results),
                    Scotland        = GetMaterialDisposalCostPerCountry(CountryConstants.Scotland, material, results),
                    NorthernIreland = GetMaterialDisposalCostPerCountry(CountryConstants.NI      , material, results)
                })
            ).ToDictionary();

            var lapcapData = new CalcResultLapcapData {
                ByMaterial = data
            };

            if (!resultsRequestDto.IsBillingFile)
            {
                await calcCountryApportionmentService.SaveChangesAsync(new CalcCountryApportionmentServiceDto
                {
                    RunId               = resultsRequestDto.RunId,
                    Countries           = countries,
                    CostTypeId          = costTypeId,
                    EnglandCost         = lapcapData.CountryApportionment.England,
                    NorthernIrelandCost = lapcapData.CountryApportionment.NorthernIreland,
                    ScotlandCost        = lapcapData.CountryApportionment.Scotland,
                    WalesCost           = lapcapData.CountryApportionment.Wales
                });
            }

            return lapcapData;
        }
#pragma warning restore

        internal static decimal GetMaterialDisposalCostPerCountry(string country, MaterialDetail material, IEnumerable<ResultsClass> results)
        {
            return
                results.Where(x =>
                    x.Country.Equals(country, StringComparison.OrdinalIgnoreCase)
                    && x.Material.Equals(material.Name, StringComparison.OrdinalIgnoreCase)
                )
                .Sum(x => x.TotalCost);
        }
    }
}
