using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Features.Common;

namespace EPR.Calculator.Service.Function.Builder.LateReportingTonnages
{
    public interface ICalcResultLateReportingBuilder
    {
        Task<CalcResultLateReportingTonnage> ConstructAsync(RunContext runContext, IImmutableList<MaterialDetail> materials);
    }

    public class CalcResultLateReportingBuilder()
        : ICalcResultLateReportingBuilder
    {
        public async Task<CalcResultLateReportingTonnage> ConstructAsync(RunContext runContext, IImmutableList<MaterialDetail> materials)
        {
            var tonnageDetails = materials
                .Select(material =>
                {
                    var lrt   = runContext.DefaultParameters.LateReportingTonnageByMaterialCode[material.Code];

                    var red   = lrt.Red  !.Value; // Default params should never be null
                    var amber = lrt.Amber!.Value;
                    var green = lrt.Green!.Value;

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
    }
}
