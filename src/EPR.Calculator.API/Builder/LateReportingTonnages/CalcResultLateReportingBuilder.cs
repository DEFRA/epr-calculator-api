using EPR.Calculator.API.Data;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace EPR.Calculator.API.Builder.LateReportingTonnages
{
    public class CalcResultLateReportingBuilder : ICalcResultLateReportingBuilder
    {
        private readonly ApplicationDBContext context;
        public const string LateReportingHeader = "Parameters - Late Reporting Tonnages";
        public const string Total = "Total";
        public const string MaterialHeading = "Material";
        public const string TonnageHeading = "Late Reporting Tonnage";

        public CalcResultLateReportingBuilder(ApplicationDBContext context)
        {
            this.context = context;
        }

        public CalcResultLateReportingTonnage Construct(CalcResultsRequestDto resultsRequestDto)
        {
            var result = (from run in context.CalculatorRuns
                          join detail in context.DefaultParameterSettingDetail
                          on run.DefaultParameterSettingMasterId equals detail.DefaultParameterSettingMasterId
                          join template in context.DefaultParameterTemplateMasterList
                          on detail.ParameterUniqueReferenceId equals template.ParameterUniqueReferenceId
                          where run.Id == resultsRequestDto.RunId && template.ParameterType == "Late Reporting Tonnage" && detail.ParameterUniqueReferenceId.StartsWith("LRET")
                          select new CalcResultLateReportingTonnageDetail
                          {
                              Name = template.ParameterCategory,
                              TotalLateReportingTonnage = detail.ParameterValue
                          }).ToList();

            result.Add(new CalcResultLateReportingTonnageDetail
            {
                Name = Total,
                TotalLateReportingTonnage = result.Sum(r => r.TotalLateReportingTonnage)
            });

            return new CalcResultLateReportingTonnage 
            { 
                Name = LateReportingHeader, 
                MaterialHeading = MaterialHeading,
                TonnageHeading = TonnageHeading,  
                CalcResultLateReportingTonnageDetails = result 
            };
        }
    }
}