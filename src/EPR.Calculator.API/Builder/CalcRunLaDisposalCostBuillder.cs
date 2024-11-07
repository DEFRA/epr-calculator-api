using EPR.Calculator.API.Constants;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.Builder
{
    public class CalcRunLaDisposalCostBuillder : ICalcRunLaDisposalCostBuilder
    {
        private readonly ApplicationDBContext context;
        public CalcRunLaDisposalCostBuillder(ApplicationDBContext context)
        {
            this.context = context;
        }


        public CalcResultLaDisposalCostData Construct(CalcResultsRequestDto resultsRequestDto, CalcResult calcResult)
        {
           

            var laDisposalCostDetails = new List<CalcResultLaDisposalCostDataDetail>();

            laDisposalCostDetails.Add(new CalcResultLaDisposalCostDataDetail()
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
                DisposalCostPricePerTonne = CommonConstants.DisposalCostPricePerTonne
            });

            return new CalcResultLaDisposalCostData() { Name = "", CalcResultLaDisposalCostDetails = (IEnumerable<CalcResultParameterCostDetail>)laDisposalCostDetails.AsEnumerable() };




            
        }
    }
}
