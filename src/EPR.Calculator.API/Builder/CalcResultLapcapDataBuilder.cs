using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Models;
using Microsoft.Azure.Amqp.Framing;
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

            var materials = context.Material.Select(x => x.Name).ToList();

            foreach (var material in materials)
            {
                var detail = new CalcResultLapcapDataDetails
                {
                    Name = material,
                    EnglandCost = GetMaterialDisposalCostPerCountry("England", material, results),
                    NorthernIrelandCost = GetMaterialDisposalCostPerCountry("NI", material, results),
                    ScotlandCost = GetMaterialDisposalCostPerCountry("Scotland", material, results),
                    WalesCost = GetMaterialDisposalCostPerCountry("Wales", material, results),
                    OrderId = ++orderId,
                    TotalCost = GetTotalMaterialDisposalCost(material, results)
                };

                detail.EnglandDisposalCost = detail.EnglandCost.ToString("C");
                detail.NorthernIrelandDisposalCost = detail.NorthernIrelandCost.ToString("C");
                detail.ScotlandDisposalCost = detail.ScotlandCost.ToString("C");
                detail.WalesDisposalCost = detail.WalesCost.ToString("C");
                detail.TotalDisposalCost = detail.TotalCost.ToString("C");

                data.Add(detail);
            }

            var totalDetail = new CalcResultLapcapDataDetails
            {
                Name = "Total",
                EnglandCost = data.Sum(x => x.EnglandCost),
                NorthernIrelandCost = data.Sum(x => x.NorthernIrelandCost),
                ScotlandCost = data.Sum(x => x.ScotlandCost),
                WalesCost = data.Sum(x => x.WalesCost),
                TotalCost = data.Sum(x => x.TotalCost),
                OrderId = ++orderId
            };
            totalDetail.EnglandDisposalCost = totalDetail.EnglandCost.ToString("C");
            totalDetail.NorthernIrelandDisposalCost = totalDetail.NorthernIrelandCost.ToString("C");
            totalDetail.ScotlandDisposalCost = totalDetail.ScotlandCost.ToString("C");
            totalDetail.WalesDisposalCost = totalDetail.WalesCost.ToString("C");
            totalDetail.TotalDisposalCost = totalDetail.TotalCost.ToString("C");
            data.Add(totalDetail);


            var countryAppPercent = new CalcResultLapcapDataDetails
            {
                Name = "1 Country Apportionment",
                EnglandCost = (totalDetail.EnglandCost / totalDetail.TotalCost) * 100,
                NorthernIrelandCost = (totalDetail.NorthernIrelandCost / totalDetail.TotalCost) * 100,
                ScotlandCost = (totalDetail.ScotlandCost / totalDetail.TotalCost) * 100,
                WalesCost = (totalDetail.WalesCost / totalDetail.TotalCost) * 100,
                TotalCost = 100,
                OrderId = ++orderId
            };
            countryAppPercent.EnglandDisposalCost = $"{countryAppPercent.EnglandCost}%";
            countryAppPercent.NorthernIrelandDisposalCost = $"{countryAppPercent.NorthernIrelandCost}%";
            countryAppPercent.ScotlandDisposalCost = $"{countryAppPercent.ScotlandCost}%";
            countryAppPercent.WalesDisposalCost = $"{countryAppPercent.WalesCost}%";
            countryAppPercent.TotalDisposalCost = $"{countryAppPercent.TotalCost}%";
            data.Add(countryAppPercent);


            return new CalcResultLapcapData { Name = "LAPCAP Data", CalcResultLapcapDataDetails = data };
        }

        internal static decimal GetMaterialDisposalCostPerCountry(string country, string material, IEnumerable<ResultsClass> results)
        {
            var date = DateTime.Today;
            var newDateTime = date.AddYears(-1);

            var totalSum = results.Where(x => x.Country.Equals(country, StringComparison.OrdinalIgnoreCase) &&
                x.Material.Equals(material, StringComparison.OrdinalIgnoreCase))
                .Sum(x => x.TotalCost);
            return totalSum;
        }

        internal static decimal GetTotalMaterialDisposalCost(string material, IEnumerable<ResultsClass> results)
        {
            var totalSum = results.Where(x => x.Material.Equals(material, StringComparison.OrdinalIgnoreCase)).Sum(x => x.TotalCost);
            return totalSum;
        }
    }
}
