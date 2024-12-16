using System.Globalization;
using EPR.Calculator.API.Builder.Lapcap;
using EPR.Calculator.API.Constants;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Models;

namespace EPR.Calculator.API.Builder.OnePlusFourApportionment
{
    public class CalcResultOnePlusFourApportionmentBuilder : ICalcResultOnePlusFourApportionmentBuilder
    {
        public CalcResultOnePlusFourApportionment Construct(CalcResultsRequestDto resultsRequestDto, CalcResult calcResult)
        {
            const string totalLabel = "Total";
            var apportionmentDetails = new List<CalcResultOnePlusFourApportionmentDetail>();
            int orderId = 1;

            // Add the header row
            apportionmentDetails.Add(CreateHeaderRow(orderId));

            // Add disposal cost row
            var totalLACost = GetTotalCost(calcResult, totalLabel);
            apportionmentDetails.Add(CreateDisposalDetailRow(OnePlus4ApportionmentColumnHeaders.LADisposalCost, totalLACost, orderId++));

            // Add data preparation charge row
            var dataPrepCharge = calcResult.CalcResultParameterOtherCost.Details
                .Single(x => x.Name == OnePlus4ApportionmentColumnHeaders.LADataPrepCharge);

            apportionmentDetails.Add(CreateDataPrepChargeRow(dataPrepCharge, orderId++));

            // Add total row
            apportionmentDetails.Add(CreateTotalRow(totalLACost, dataPrepCharge, orderId++));

            // Calculate apportionment
            var apportionmentData = CalculateApportionment(apportionmentDetails.First(x => x.OrderId == 3), orderId++);
            apportionmentDetails.Add(apportionmentData);

            return new CalcResultOnePlusFourApportionment { Name = "1 + 4 Apportionment %s", CalcResultOnePlusFourApportionmentDetails = apportionmentDetails };
        }

        private static CalcResultOnePlusFourApportionmentDetail CreateHeaderRow(int orderId)
        {
            return new CalcResultOnePlusFourApportionmentDetail
            {
                Name = OnePlus4ApportionmentRowHeaders.Name,
                EnglandDisposalTotal = OnePlus4ApportionmentRowHeaders.EnglandDisposalCost,
                WalesDisposalTotal = OnePlus4ApportionmentRowHeaders.WalesDisposalCost,
                ScotlandDisposalTotal = OnePlus4ApportionmentRowHeaders.ScotlandDisposalCost,
                NorthernIrelandDisposalTotal = OnePlus4ApportionmentRowHeaders.NorthernIrelandDisposalCost,
                Total = OnePlus4ApportionmentRowHeaders.Total,
                OrderId = orderId,
            };
        }

        private static CalcResultLapcapDataDetails GetTotalCost(CalcResult calcResult, string name)
        {
            return calcResult.CalcResultLapcapData.CalcResultLapcapDataDetails
                .Single(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        private static CalcResultOnePlusFourApportionmentDetail CreateDisposalDetailRow(string name, CalcResultLapcapDataDetails totalLACost, int orderId)
        {
            return new CalcResultOnePlusFourApportionmentDetail
            {
                Name = name,
                Total = totalLACost.TotalDisposalCost,
                EnglandDisposalTotal = totalLACost.EnglandDisposalCost,
                WalesDisposalTotal = totalLACost.WalesDisposalCost,
                ScotlandDisposalTotal = totalLACost.ScotlandDisposalCost,
                NorthernIrelandDisposalTotal = totalLACost.NorthernIrelandDisposalCost,
                EnglandTotal = totalLACost.EnglandCost,
                WalesTotal = totalLACost.WalesCost,
                ScotlandTotal = totalLACost.ScotlandCost,
                NorthernIrelandTotal = totalLACost.NorthernIrelandCost,
                OrderId = orderId,
            };
        }

        private static CalcResultOnePlusFourApportionmentDetail CreateDataPrepChargeRow(CalcResultParameterOtherCostDetail dataPrepCharge, int orderId)
        {
            return new CalcResultOnePlusFourApportionmentDetail
            {
                Name = OnePlus4ApportionmentColumnHeaders.LADataPrepCharge,
                Total = dataPrepCharge.Total,
                EnglandDisposalTotal = dataPrepCharge.England,
                WalesDisposalTotal = dataPrepCharge.Wales,
                ScotlandDisposalTotal = dataPrepCharge.Scotland,
                NorthernIrelandDisposalTotal = dataPrepCharge.NorthernIreland,
                AllTotal = dataPrepCharge.TotalValue,
                EnglandTotal = dataPrepCharge.EnglandValue,
                WalesTotal = dataPrepCharge.WalesValue,
                ScotlandTotal = dataPrepCharge.ScotlandValue,
                NorthernIrelandTotal = dataPrepCharge.NorthernIrelandValue,
                OrderId = orderId,
            };
        }

        private CalcResultOnePlusFourApportionmentDetail CreateTotalRow(CalcResultLapcapDataDetails totalLACost, CalcResultParameterOtherCostDetail dataPrepCharge, int orderId)
        {
            var culture = CultureInfo.CreateSpecificCulture("en-GB");
            culture.NumberFormat.CurrencySymbol = "£";
            culture.NumberFormat.CurrencyPositivePattern = 0;

            return new CalcResultOnePlusFourApportionmentDetail
            {
                Name = OnePlus4ApportionmentColumnHeaders.TotalOnePlusFour,
                EnglandTotal = totalLACost.EnglandCost + dataPrepCharge.EnglandValue,
                WalesTotal = totalLACost.WalesCost + dataPrepCharge.WalesValue,
                ScotlandTotal = totalLACost.ScotlandCost + dataPrepCharge.ScotlandValue,
                NorthernIrelandTotal = totalLACost.NorthernIrelandCost + dataPrepCharge.NorthernIrelandValue,
                AllTotal = totalLACost.TotalCost + dataPrepCharge.TotalValue,
                Total = (totalLACost.TotalCost + dataPrepCharge.TotalValue).ToString("C", culture),
                EnglandDisposalTotal = (totalLACost.EnglandCost + dataPrepCharge.EnglandValue).ToString("C", culture),
                WalesDisposalTotal = (totalLACost.WalesCost + dataPrepCharge.WalesValue).ToString("C", culture),
                ScotlandDisposalTotal = (totalLACost.ScotlandCost + dataPrepCharge.ScotlandValue).ToString("C", culture),
                NorthernIrelandDisposalTotal = (totalLACost.NorthernIrelandCost + dataPrepCharge.NorthernIrelandValue).ToString("C", culture),
                OrderId = orderId,
            };
        }

        private CalcResultOnePlusFourApportionmentDetail CalculateApportionment(CalcResultOnePlusFourApportionmentDetail apportionmentData, int orderId)
        {
            var englandTotal =
                CalcResultLapcapDataBuilder.CalculateApportionment(apportionmentData.EnglandTotal,
                    apportionmentData.AllTotal);
            var walesTotal =
                CalcResultLapcapDataBuilder.CalculateApportionment(apportionmentData.WalesTotal,
                    apportionmentData.AllTotal);

            var scotlandTotal =
                CalcResultLapcapDataBuilder.CalculateApportionment(apportionmentData.ScotlandTotal,
                    apportionmentData.AllTotal);
            var niTotal = CalcResultLapcapDataBuilder.CalculateApportionment(apportionmentData.NorthernIrelandTotal,
                apportionmentData.AllTotal);
            var total = CalcResultLapcapDataBuilder.CalculateApportionment(apportionmentData.AllTotal,
                apportionmentData.AllTotal);
            return new CalcResultOnePlusFourApportionmentDetail
            {
                Name = OnePlus4ApportionmentColumnHeaders.OnePluseFourApportionment,
                Total = $"{total.ToString("N", new NumberFormatInfo { NumberDecimalDigits = 8 })}%",
                EnglandDisposalTotal = $"{englandTotal.ToString("N", new NumberFormatInfo { NumberDecimalDigits = 8 })}%",
                WalesDisposalTotal = $"{walesTotal.ToString("N", new NumberFormatInfo { NumberDecimalDigits = 8 })}%",
                ScotlandDisposalTotal = $"{scotlandTotal.ToString("N", new NumberFormatInfo { NumberDecimalDigits = 8 })}%",
                NorthernIrelandDisposalTotal = $"{niTotal.ToString("N", new NumberFormatInfo { NumberDecimalDigits = 8 })}%",
                AllTotal = total,
                ScotlandTotal = scotlandTotal,
                EnglandTotal = englandTotal,
                NorthernIrelandTotal = niTotal,
                WalesTotal = walesTotal,
                OrderId = orderId,
            };
        }
    }
}