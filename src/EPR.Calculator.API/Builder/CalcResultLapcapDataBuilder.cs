using EPR.Calculator.API.Data;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Models;
using Microsoft.Extensions.Hosting;

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
            data.Add(new CalcResultLapcapDataDetails 
            {
                    Name = "Material",
                    EnglandDisposalCost = "England LA Disposal Cost",
                    WalesDisposalCost = "Wales LA Disposal Cost",
                    ScotlandDisposalCost = "Scotland LA Disposal Cost",
                    NorthernIrelandDisposalCost = "Northern Ireland LA Disposal Cost",
                    OrderId = 1,
                    TotalDisposalCost = "1 LA Disposal Cost Total"
            });

            var results = (from run in context.CalculatorRuns
                           join lapcapMaster in context.LapcapDataMaster on run.LapcapDataMasterId equals lapcapMaster.Id
                           join lapcapDetail in context.LapcapDataDetail on lapcapMaster.Id equals lapcapDetail.LapcapDataMasterId
                           join lapcapTemplate in context.LapcapDataTemplateMaster on lapcapDetail.UniqueReference equals lapcapTemplate.UniqueReference
                           where run.Id == resultsRequestDto.RunId
                           select new { run, lapcapMaster, lapcapDetail, lapcapTemplate }).ToList();



            return new CalcResultLapcapData { Name = "LAPCAP Data", CalcResultLapcapDataDetails = data };
        }
    }
}
