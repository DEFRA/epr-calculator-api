using EPR.Calculator.API.Constants;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Models;
using System.Globalization;

namespace EPR.Calculator.API.Builder.LaDisposalCost
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
                            join producerMaterial in context.ProducerReportedMaterial on producerDetail.Id equals producerMaterial.ProducerDetailId
                            join material in context.Material on producerMaterial.MaterialId equals material.Id
                            where run.Id == resultsRequestDto.RunId && producerMaterial.PackagingType != null && producerMaterial.PackagingType.Equals(CommonConstants.Household, StringComparison.OrdinalIgnoreCase)
                            select new ProducerData
                            {
                                Material = material.Name,
                                Tonnage = producerMaterial.PackagingTonnage
                            }).ToList();

            var lapcapDetails = calcResult?.CalcResultLapcapData?.CalcResultLapcapDataDetails?.Where(t => t.OrderId != 1 && t.Name != "1 Country Apportionment").ToList();


            foreach (var details in lapcapDetails)
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
                details.LateReportingTonnage = GetLateReportingTonnageDataByMaterial(details.Name, calcResult?.CalcResultLateReportingTonnageData?.CalcResultLateReportingTonnageDetails?.ToList());

                details.ProducerReportedHouseholdTonnagePlusLateReportingTonnage = GetProducerReportedHouseholdTonnagePlusLateReportingTonnage(details);
                if (details.Name == CommonConstants.Total) continue;
                details.DisposalCostPricePerTonne = CalculateDisposalCostPricePerTonne(details);
            }


            var header = GetHeader();
            laDisposalCostDetails.Insert(0, header);

            return new CalcResultLaDisposalCostData() { Name = CommonConstants.LADisposalCostData, CalcResultLaDisposalCostDetails = laDisposalCostDetails.AsEnumerable() };

        }


        private string GetTonnageDataByMaterial(string material)
        {
            return material == "Total"? producerData.Sum(t=>t.Tonnage).ToString()  : producerData.Where(t => t.Material == material).Sum(t => t.Tonnage).ToString();
        }

        private string GetLateReportingTonnageDataByMaterial(string material, List<CalcResultLateReportingTonnageDetail> details)
        {
            return details.Where(t => t.Name == material).Sum(t => t.TotalLateReportingTonnage).ToString();
        }

        private string GetProducerReportedHouseholdTonnagePlusLateReportingTonnage(CalcResultLaDisposalCostDataDetail detail)
        {
            var value = GetDecimalValue(detail.LateReportingTonnage) + GetDecimalValue(detail.ProducerReportedHouseholdPackagingWasteTonnage);
            return value.ToString();
        }

        private string CalculateDisposalCostPricePerTonne(CalcResultLaDisposalCostDataDetail detail)
        {
            var HouseholdTonnagePlusLateReportingTonnage = GetDecimalValue(detail.ProducerReportedHouseholdTonnagePlusLateReportingTonnage);
            if (HouseholdTonnagePlusLateReportingTonnage == 0) return "0";
            var value = Math.Round(ConvertCurrencyToDecimal(detail.Total) / HouseholdTonnagePlusLateReportingTonnage, 4);
            var culture = CultureInfo.CreateSpecificCulture("en-GB");
            culture.NumberFormat.CurrencySymbol = "£";
            culture.NumberFormat.CurrencyPositivePattern = 0;
            return value.ToString("C", culture);
        }


        private CalcResultLaDisposalCostDataDetail GetHeader()
        {
            return new CalcResultLaDisposalCostDataDetail()
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
