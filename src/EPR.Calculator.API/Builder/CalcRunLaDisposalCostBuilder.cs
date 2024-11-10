using EPR.Calculator.API.Constants;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace EPR.Calculator.API.Builder
{
    public class CalcRunLaDisposalCostBuilder : ICalcRunLaDisposalCostBuilder
    {
        internal class ProducerData
        {
            public string Material { get; set; }
            public decimal Tonnage { get; set; }            
        }


        private readonly ApplicationDBContext context;
        private List<ProducerData> producerData;
        public CalcRunLaDisposalCostBuilder(ApplicationDBContext context)
        {
            this.context = context;
            producerData = new List<ProducerData>();
        }


        public CalcResultLaDisposalCostData Construct(CalcResultsRequestDto resultsRequestDto, CalcResult calcResult)
        {           

            var laDisposalCostDetails = new List<CalcResultLaDisposalCostDataDetail>();
            var OrderId = 1;           

            producerData = (from run in context.CalculatorRuns
                            join producerDetail in context.ProducerDetail on run.Id equals producerDetail.CalculatorRunId
                           join  producerMaterial in context.ProducerReportedMaterial on producerDetail.Id equals producerMaterial.ProducerDetailId
                           join material in context.Material on producerMaterial.MaterialId equals material.Id
                           where run.Id == resultsRequestDto.RunId                           
                           select new ProducerData
                           {
                               Material = material.Name,
                               Tonnage = producerMaterial.PackagingTonnage
                           }).ToList();

            var lapcapDetails = calcResult.CalcResultLapcapData.CalcResultLapcapDataDetails.Where(t => t.OrderId != 1 && t.Name != "1 Country Apportionment").ToList();


            foreach (var details  in lapcapDetails)
            {
                var laDiposalDetail = new CalcResultLaDisposalCostDataDetail()
                {
                    Name = details.Name,
                    England = details.EnglandDisposalCost,
                    Wales = details.WalesDisposalCost,
                    Scotland = details.ScotlandDisposalCost,
                    NorthernIreland = details.NorthernIrelandDisposalCost,
                    Total = details.TotalDisposalCost,
                    ProducerReportedHouseholdPackagingWasteTonnage = GetTonnageDataByMaterial(details.Name),
                    OrderId = ++OrderId
                };                     
                laDisposalCostDetails.Add(laDiposalDetail);

            }


            foreach (var details in laDisposalCostDetails)
            {
                details.LateReportingTonnage = GetLateReportingTonnageDataByMaterial(details.Name);

                details.ProducerReportedHouseholdTonnagePlusLateReportingTonnage = GetProducerReportedHouseholdTonnagePlusLateReportingTonnage(details);
                if (details.Name == CommonConstants.Total) continue;
                details.DisposalCostPricePerTonne = CalculateDisposalCostPricePerTonne(details);
            }


            var header = new CalcResultLaDisposalCostDataDetail()
            {
                Name = CommonConstants.Material,
                England = CommonConstants.England,
                Wales = CommonConstants.Wales,
                Scotland = CommonConstants.Scotland,
                NorthernIreland = CommonConstants.NorthernIreland,
                Total = CommonConstants.Total,
                ProducerReportedHouseholdPackagingWasteTonnage = CommonConstants.ProducerReportedHouseholdPackagingWasteTonnage,
                LateReportingTonnage = CommonConstants.LateReportingTonnage,
                ProducerReportedHouseholdTonnagePlusLateReportingTonnage = CommonConstants.ProduceLateTonnage,
                DisposalCostPricePerTonne = CommonConstants.DisposalCostPricePerTonne,
                OrderId = 1
            };

            laDisposalCostDetails.Insert(0, header);

            return new CalcResultLaDisposalCostData() { Name = CommonConstants.LADisposalCostData, CalcResultLaDisposalCostDetails = laDisposalCostDetails.AsEnumerable() };             
            
        }


        private string GetTonnageDataByMaterial(string material)
        {
            return producerData.Where(t => t.Material== material).Sum(t => t.Tonnage).ToString();
        }

        private string GetLateReportingTonnageDataByMaterial(string material)
        {
            var details = GetLateReportingTonnage();

            return details.Where(t => t.Name == material).Sum(t => t.TotalLateReportingTonnage).ToString();
        }

        private string GetProducerReportedHouseholdTonnagePlusLateReportingTonnage(CalcResultLaDisposalCostDataDetail detail)
        {
            var value = GetDecimalValue(detail.LateReportingTonnage) + GetDecimalValue(detail.ProducerReportedHouseholdPackagingWasteTonnage);
            return value.ToString();
        }


        private List<CalcResultLateReportingTonnageDetail> GetLateReportingTonnage()
        {
            
           var details = new List<CalcResultLateReportingTonnageDetail>
           {
               new() { Name = "Aluminium", TotalLateReportingTonnage = 8000.00M },
               new() { Name = "Fibre composite", TotalLateReportingTonnage = 7000.00M },
               new() { Name = "Glass", TotalLateReportingTonnage = 6000.00M },
               new() { Name = "Paper or card", TotalLateReportingTonnage = 5000.00M },
               new() { Name = "Plastic", TotalLateReportingTonnage = 4000.00M },
               new() { Name = "Steel", TotalLateReportingTonnage = 3000.00M },
               new() { Name = "Wood", TotalLateReportingTonnage = 2000.00M },
               new() { Name = "Wood", TotalLateReportingTonnage = 2000.00M },
               new() { Name = "Other materials", TotalLateReportingTonnage = 1000.00M }
           };
            return details;
        }


        private string CalculateDisposalCostPricePerTonne(CalcResultLaDisposalCostDataDetail detail)
        {
            var value = Math.Round(ConvertCurrencyToDecimal(detail.Total) / GetDecimalValue(detail.ProducerReportedHouseholdTonnagePlusLateReportingTonnage), 4);
            var culture = CultureInfo.CreateSpecificCulture("en-GB");
            culture.NumberFormat.CurrencySymbol = "£";
            culture.NumberFormat.CurrencyPositivePattern = 0;
            return value.ToString("C", culture);
        }


        private decimal GetDecimalValue(string value)
        {
            return decimal.Parse(value, CultureInfo.InvariantCulture);
        }

        private decimal ConvertCurrencyToDecimal(string currency)
        {
            decimal amount;
            decimal.TryParse(currency, NumberStyles.Currency, CultureInfo.GetCultureInfo("en-GB"), out amount);
            return amount;
        }
    }
}
