using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Models;
using Microsoft.Azure.Amqp.Framing;
using Microsoft.Extensions.Hosting;
using System.Globalization;

namespace EPR.Calculator.API.Builder.Lapcap
{
    public class CalcResultLapcapDataBuilder : ICalcResultLapcapDataBuilder
    {
        private readonly ApplicationDBContext context;
        public const string LapcapHeader = "LAPCAP Data";
        public const string CountryApportionment = "1 Country Apportionment";
        public const string Total = "Total";
        public const int HundredPercent = 100;
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
                Name = LapcapHeaderConstants.Name,
                EnglandDisposalCost = LapcapHeaderConstants.EnglandDisposalCost,
                WalesDisposalCost = LapcapHeaderConstants.WalesDisposalCost,
                ScotlandDisposalCost = LapcapHeaderConstants.ScotlandDisposalCost,
                NorthernIrelandDisposalCost = LapcapHeaderConstants.NorthernIrelandDisposalCost,
                OrderId = orderId,
                TotalDisposalCost = LapcapHeaderConstants.TotalDisposalCost
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
                    EnglandCost = GetMaterialDisposalCostPerCountry(CountryConstants.England, material, results),
                    NorthernIrelandCost = GetMaterialDisposalCostPerCountry(CountryConstants.NI, material, results),
                    ScotlandCost = GetMaterialDisposalCostPerCountry(CountryConstants.Scotland, material, results),
                    WalesCost = GetMaterialDisposalCostPerCountry(CountryConstants.Wales, material, results),
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
                Name = Total,
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
                Name = CountryApportionment,
                EnglandCost = CalculateApportionment(totalDetail.EnglandCost, totalDetail.TotalCost),
                NorthernIrelandCost = CalculateApportionment(totalDetail.NorthernIrelandCost, totalDetail.TotalCost),
                ScotlandCost = CalculateApportionment(totalDetail.ScotlandCost, totalDetail.TotalCost),
                WalesCost = CalculateApportionment(totalDetail.WalesCost, totalDetail.TotalCost),
                TotalCost = HundredPercent,
                OrderId = ++orderId
            };
            countryAppPercent.EnglandDisposalCost = $"{countryAppPercent.EnglandCost.ToString("N", new NumberFormatInfo { NumberDecimalDigits = 8 })}%";
            countryAppPercent.NorthernIrelandDisposalCost = $"{countryAppPercent.NorthernIrelandCost.ToString("N", new NumberFormatInfo { NumberDecimalDigits = 8 })}%";
            countryAppPercent.ScotlandDisposalCost = $"{countryAppPercent.ScotlandCost.ToString("N", new NumberFormatInfo { NumberDecimalDigits = 8 })}%";
            countryAppPercent.WalesDisposalCost = $"{countryAppPercent.WalesCost.ToString("N", new NumberFormatInfo { NumberDecimalDigits = 8 })}%";
            countryAppPercent.TotalDisposalCost = $"{countryAppPercent.TotalCost.ToString("N", new NumberFormatInfo { NumberDecimalDigits = 8 })}%";
            data.Add(countryAppPercent);


            return new CalcResultLapcapData { Name = LapcapHeader, CalcResultLapcapDataDetails = data };
        }

        internal static decimal CalculateApportionment(decimal countryCost, decimal totalCost)
        {
            if (totalCost != 0)
            {
                var total = (countryCost / totalCost);
                return total * 100;
            }
            return 0;
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
