using EPR.Calculator.API.Data;
using EPR.Calculator.Service.Function.Features.Common;
using EPR.Calculator.Service.Function.Models;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.Builder.LateReportingTonnages
{
    public interface ICalcResultLateReportingBuilder
    {
        Task<CalcResultLateReportingTonnage> ConstructAsync(RunContext runContext, IImmutableList<MaterialDetail> materials);
    }

    public class CalcResultLateReportingBuilder(ApplicationDBContext dbContext)
        : ICalcResultLateReportingBuilder
    {
        private sealed record ParameterDetail(string ParameterCategory, decimal ParameterValue);

        public async Task<CalcResultLateReportingTonnage> ConstructAsync(RunContext runContext, IImmutableList<MaterialDetail> materials)
        {
            var result = await (
                from run in dbContext.CalculatorRuns
                join master in dbContext.DefaultParameterSettings on run.DefaultParameterSettingMasterId equals master.Id
                join detail in dbContext.DefaultParameterSettingDetail on master.Id equals detail.DefaultParameterSettingMasterId
                join template in dbContext.DefaultParameterTemplateMasterList on detail.ParameterUniqueReferenceId equals template.ParameterUniqueReferenceId
                where run.Id == runContext.RunId && template.ParameterType == "Late Reporting Tonnage"
                select new ParameterDetail(template.ParameterCategory, detail.ParameterValue)
            ).ToListAsync();

            var tonnageDetails = materials
                .Select(material =>
                {
                    var group = result
                        .Where(x => RemoveSuffix(x.ParameterCategory) == material.Name)
                        .ToList();

                    var red   = GetParameterValueBySuffix(group, "-R");
                    var amber = GetParameterValueBySuffix(group, "-A");
                    var green = GetParameterValueBySuffix(group, "-G");

                    return KeyValuePair.Create(
                        material.Code,
                        new CalcResultLateReportingTonnageDetail
                        {
                            Red   = red,
                            Amber = amber,
                            Green = green,
                            Total = red + amber + green
                        });
                })
                .ToDictionary();


            return new CalcResultLateReportingTonnage
            {
                ByMaterial = tonnageDetails
            };
        }

        private static string RemoveSuffix(string value)
        {
            if (value.EndsWith("-R") || value.EndsWith("-G") || value.EndsWith("-A"))
                return value.Substring(0, value.Length - 2);
            return value;
        }

        private static decimal GetParameterValueBySuffix(IEnumerable<ParameterDetail> values, string suffix)
            => values.First(x => x.ParameterCategory.EndsWith(suffix)).ParameterValue;
    }
}
