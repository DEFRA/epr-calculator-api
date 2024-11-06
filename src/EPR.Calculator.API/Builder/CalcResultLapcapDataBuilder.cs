using EPR.Calculator.API.Data;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Models;
using Microsoft.Extensions.Hosting;

namespace EPR.Calculator.API.Builder
{
    internal class ResultsClass
    {
        public string Material { get; set; }
        public string Country { get; set; }
        public decimal TotalCost { get; set; }
    }

    public class CalcResultLapcapDataBuilder : ICalcResultLapcapDataBuilder
    {
        private readonly ApplicationDBContext context;
        public CalcResultLapcapDataBuilder(ApplicationDBContext context)
        {
            this.context = context;
        }

        public CalcResultLapcapData Construct(CalcResultsRequestDto resultsRequestDto)
        {
            var orderId = 1;
            var data = new List<CalcResultLapcapDataDetails>();
            data.Add(new CalcResultLapcapDataDetails
            {
                Name = "Material",
                EnglandDisposalCost = "England LA Disposal Cost",
                WalesDisposalCost = "Wales LA Disposal Cost",
                ScotlandDisposalCost = "Scotland LA Disposal Cost",
                NorthernIrelandDisposalCost = "Northern Ireland LA Disposal Cost",
                OrderId = orderId,
                TotalDisposalCost = "1 LA Disposal Cost Total"
            });

            var results = (from run in context.CalculatorRuns
                           join lapcapMaster in context.LapcapDataMaster on run.LapcapDataMasterId equals lapcapMaster.Id
                           join lapcapDetail in context.LapcapDataDetail on lapcapMaster.Id equals lapcapDetail.LapcapDataMasterId
                           join lapcapTemplate in context.LapcapDataTemplateMaster on lapcapDetail.UniqueReference equals lapcapTemplate.UniqueReference
                           where run.Id == resultsRequestDto.RunId
                           select new ResultsClass
                           {
                               Material = lapcapTemplate.Material,
                               Country = lapcapTemplate.Country,
                               TotalCost = lapcapDetail.TotalCost
                           }).ToList();

            var countries = results.Select(x => x.Country).Distinct().ToList();
            var materials = results.Select(x => x.Material).Distinct().OrderBy(x => x).ToList();

            foreach (var material in materials)
            {
                var detail = new CalcResultLapcapDataDetails
                {
                    Name = material,
                    EnglandDisposalCost = GetMaterialDisposalCostPerCountry(countries.Single(x => x.Equals("England", StringComparison.CurrentCultureIgnoreCase)), material, results),
                    NorthernIrelandDisposalCost = GetMaterialDisposalCostPerCountry(countries.Single(x => x.Equals("NI", StringComparison.CurrentCultureIgnoreCase)), material, results),
                    ScotlandDisposalCost = GetMaterialDisposalCostPerCountry(countries.Single(x => x.Equals("Scotland", StringComparison.CurrentCultureIgnoreCase)), material, results),
                    WalesDisposalCost = GetMaterialDisposalCostPerCountry(countries.Single(x => x.Equals("Wales", StringComparison.CurrentCultureIgnoreCase)), material, results),
                    OrderId = ++orderId,
                    TotalDisposalCost = GetTotalMaterialDisposalCost(material, results)
                };
                data.Add(detail);
            }

            return new CalcResultLapcapData { Name = "LAPCAP Data", CalcResultLapcapDataDetails = data };
        }

        internal static string GetMaterialDisposalCostPerCountry(string country, string material, IEnumerable<ResultsClass> results)
        {
            var date = DateTime.Today;
            var newDateTime = date.AddYears(-1);

            var totalSum = results.Where(x => x.Country.Equals(country, StringComparison.OrdinalIgnoreCase) &&
                x.Material.Equals(material, StringComparison.OrdinalIgnoreCase))
                .Sum(x => x.TotalCost);
            return totalSum.ToString("C");
        }

        internal static string GetTotalMaterialDisposalCost(string material, IEnumerable<ResultsClass> results)
        {
            var date = DateTime.Today;
            var newDateTime = date.AddYears(-1);

            var totalSum = results.Where(x => x.Material.Equals(material, StringComparison.OrdinalIgnoreCase)).Sum(x => x.TotalCost);
            return totalSum.ToString("C");
        }
    }
}
