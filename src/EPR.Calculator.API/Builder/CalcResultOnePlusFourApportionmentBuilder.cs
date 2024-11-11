using EPR.Calculator.API.Builder.Lapcap;
using EPR.Calculator.API.Constants;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Models;
using Microsoft.Azure.Amqp.Framing;
using System.Globalization;

namespace EPR.Calculator.API.Builder
{
    public class CalcResultOnePlusFourApportionmentBuilder : ICalcResultOnePlusFourApportionmentBuilder
    {
        private readonly ApplicationDBContext context;
        public CalcResultOnePlusFourApportionmentBuilder(ApplicationDBContext context)
        {
            this.context = context;
        }

        public CalcResultOnePlusFourApportionment Construct(CalcResultsRequestDto resultsRequestDto, CalcResult calcResult)
        {
            const string totalLabel = "Total";
            var data = new List<CalcResultOnePlusFourApportionmentDetail>();
            int orderId = 1;

            // Add the header row
            data.Add(CreateHeaderRow(orderId));

            // Get totals for the second row
            var totalLACost = GetTotalCost(calcResult, totalLabel);
            data.Add(CreateLADisposalDetailRow(OnePlus4ApportionmentColumnHeaders.LADisposalCost, totalLACost, orderId++));

            // Add third row (still in progress)
            var totalParam = GetTotalCost(calcResult, totalLabel);
            data.Add(CreateLADisposalDetailRow(OnePlus4ApportionmentColumnHeaders.LADataPrepCharge, totalLACost, orderId++));

            // Add fourth row (still in progress)
            var onePlusFourTotals = GetTotalCost(calcResult, totalLabel);
            data.Add(CreateTotalOnePlusFourRow(totalLACost, onePlusFourTotals, orderId++));

            // Calculate apportionment for the fifth row
            var items = data.First(x => x.OrderId == 3);


            var apportionmentData = CalculateApportionment(items, orderId++);
            data.Add(apportionmentData);

            return new CalcResultOnePlusFourApportionment { Name = "1 + 4 Apportionment %s", CalcResultOnePlusFourApportionmentDetails = data };
        }

        private CalcResultOnePlusFourApportionmentDetail CreateHeaderRow(int orderId)
        {
            return new CalcResultOnePlusFourApportionmentDetail
            {
                Name = OnePlus4ApportionmentRowHeaders.Name,
                Total = OnePlus4ApportionmentRowHeaders.Total,
                EnglandDisposalTotal = OnePlus4ApportionmentRowHeaders.EnglandDisposalCost,
                WalesDisposalTotal = OnePlus4ApportionmentRowHeaders.WalesDisposalCost,
                ScotlandDisposalTotal = OnePlus4ApportionmentRowHeaders.ScotlandDisposalCost,
                NorthernIrelandDisposalTotal = OnePlus4ApportionmentRowHeaders.NorthernIrelandDisposalCost,
                OrderId = orderId,
            };
        }

        private CalcResultLapcapDataDetails GetTotalCost(CalcResult calcResult, string name)
        {
            return calcResult.CalcResultLapcapData.CalcResultLapcapDataDetails
                .Single(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        private CalcResultOnePlusFourApportionmentDetail CreateLADisposalDetailRow(string name, CalcResultLapcapDataDetails totalLACost, int orderId)
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

        private CalcResultOnePlusFourApportionmentDetail CreateTotalOnePlusFourRow(CalcResultLapcapDataDetails totalLACost, CalcResultLapcapDataDetails onePlusFourTotals, int orderId)
        {
            return new CalcResultOnePlusFourApportionmentDetail
            {
                Name = OnePlus4ApportionmentColumnHeaders.TotalOnePlusFour,
                EnglandTotal = totalLACost.EnglandCost + onePlusFourTotals.EnglandCost,
                WalesTotal = totalLACost.WalesCost + onePlusFourTotals.WalesCost,
                ScotlandTotal = totalLACost.ScotlandCost + onePlusFourTotals.ScotlandCost,
                NorthernIrelandTotal = totalLACost.NorthernIrelandCost + onePlusFourTotals.NorthernIrelandCost,
                AllTotal = totalLACost.TotalCost + onePlusFourTotals.TotalCost,
                Total = (totalLACost.TotalCost + onePlusFourTotals.TotalCost).ToString("C"),
                EnglandDisposalTotal = (totalLACost.EnglandCost + onePlusFourTotals.EnglandCost).ToString("C"),
                WalesDisposalTotal = (totalLACost.WalesCost + onePlusFourTotals.WalesCost).ToString("C"),
                ScotlandDisposalTotal = (totalLACost.ScotlandCost + onePlusFourTotals.ScotlandCost).ToString("C"),
                NorthernIrelandDisposalTotal = (totalLACost.NorthernIrelandCost + onePlusFourTotals.NorthernIrelandCost).ToString("C"),
                OrderId = orderId,
            };
        }

        private CalcResultOnePlusFourApportionmentDetail CalculateApportionment(CalcResultOnePlusFourApportionmentDetail apportionmentData, int orderId)
        {
            return new CalcResultOnePlusFourApportionmentDetail
            {
                Name = OnePlus4ApportionmentColumnHeaders.OnePluseFourApportionment,
                Total = $"{CalcResultLapcapDataBuilder.CalculateApportionment(apportionmentData.AllTotal, apportionmentData.AllTotal).ToString("N", new NumberFormatInfo { NumberDecimalDigits = 8 })}%",
                EnglandDisposalTotal = $"{CalcResultLapcapDataBuilder.CalculateApportionment(apportionmentData.EnglandTotal, apportionmentData.AllTotal).ToString("N", new NumberFormatInfo { NumberDecimalDigits = 8 })}%",
                WalesDisposalTotal = $"{CalcResultLapcapDataBuilder.CalculateApportionment(apportionmentData.WalesTotal, apportionmentData.AllTotal).ToString("N", new NumberFormatInfo { NumberDecimalDigits = 8 })}%",
                ScotlandDisposalTotal = $"{CalcResultLapcapDataBuilder.CalculateApportionment(apportionmentData.ScotlandTotal, apportionmentData.AllTotal).ToString("N", new NumberFormatInfo { NumberDecimalDigits = 8 })}%",
                NorthernIrelandDisposalTotal = $"{CalcResultLapcapDataBuilder.CalculateApportionment(apportionmentData.NorthernIrelandTotal, apportionmentData.AllTotal).ToString("N", new NumberFormatInfo { NumberDecimalDigits = 8 })}%",
                OrderId = orderId,
            };
        }
    }
}