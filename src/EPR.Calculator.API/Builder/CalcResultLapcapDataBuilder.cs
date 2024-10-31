using EPR.Calculator.API.Data;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Models;

namespace EPR.Calculator.API.Builder
{
    public class CalcResultLapcapDataBuilder : ICalcResultLapcapDataBuilder
    {
        private readonly ApplicationDBContext context;
        public CalcResultLapcapDataBuilder(ApplicationDBContext context)
        {
            this.context = context;
        }

        public CalcResultLapcapData Construct(CalcResultsRequestDto resultsRequestDto)
        {
            var data = new List<CalcResultLapcapDataDetails>();

            data.AddRange([
                new CalcResultLapcapDataDetails {
                    Key = "Material",
                    EnglandDisposalCost = "England LA Disposal Cost",
                    WalesDisposalCost = "Wales LA Disposal Cost",
                    ScotlandDisposalCost = "Scotland LA Disposal Cost",
                    NorthernIrelandDisposalCost = "Northern Ireland LA Disposal Cost",
                    TotalDisposalCost = "1 LA Disposal Cost Total",
                    isHeader = true
                },
                new CalcResultLapcapDataDetails { 
                    Key = "Aluminium",
                    EnglandDisposalCost = "£5,000.00",
                    WalesDisposalCost = "£1,750.00",
                    ScotlandDisposalCost = "£2,000.00",
                    NorthernIrelandDisposalCost = "£1,250.00",
                    TotalDisposalCost = "£10,000.00"
                },
                new CalcResultLapcapDataDetails
                {
                    Key = "Fibre composite",
                    EnglandDisposalCost = "£7,500.00",
                    WalesDisposalCost = "£2,100.00",
                    ScotlandDisposalCost = "£3,400.00",
                    NorthernIrelandDisposalCost = "£1,750.00",
                    TotalDisposalCost = "£14,700.00"
                },
                new CalcResultLapcapDataDetails
                {
                    Key = "Total",
                    EnglandDisposalCost = "£109,800.00",
                    WalesDisposalCost = "£24,750.00",
                    ScotlandDisposalCost = "£49,300.00",
                    NorthernIrelandDisposalCost = "£19,300.00",
                    TotalDisposalCost = "£203,150.00"
                },
                new CalcResultLapcapDataDetails
                {
                    Key = "1 Country Apportionment %s",
                    EnglandDisposalCost = "54.04873246%",
                    WalesDisposalCost = "12.18311592%",
                    ScotlandDisposalCost = "24.26778243%",
                    NorthernIrelandDisposalCost = "9.50036919%",
                    TotalDisposalCost = "100.00000000%"
                }
            ]);

            return new CalcResultLapcapData { Name = "LAPCAP Data", CalcResultLapcapDataDetails = data };
        }
    }
}
