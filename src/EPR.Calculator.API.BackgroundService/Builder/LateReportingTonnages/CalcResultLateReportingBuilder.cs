using EPR.Calculator.API.BackgroundService.Features.Common;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.API.BackgroundService.Builder.LateReportingTonnages
{
    public interface ICalcResultLateReportingBuilder
    {
        CalcResultLateReportingTonnage Construct(RunContext runContext, IImmutableList<MaterialDetail> materials);
    }

    public class CalcResultLateReportingBuilder()
        : ICalcResultLateReportingBuilder
    {
        [ActivityTrace]
        public CalcResultLateReportingTonnage Construct(RunContext runContext, IImmutableList<MaterialDetail> materials)
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

            return Create(tonnageDetails);
        }

        public static CalcResultLateReportingTonnage Create(IReadOnlyDictionary<string, CalcResultLateReportingTonnageDetail> byMaterial) =>
            new()
            {
                ByMaterial = byMaterial,
                Total = new CalcResultLateReportingTonnageDetail
                {
                    Total = byMaterial.Values.Sum(v => v.Total),
                    Red   = byMaterial.Values.Sum(v => v.Red),
                    Amber = byMaterial.Values.Sum(v => v.Amber),
                    Green = byMaterial.Values.Sum(v => v.Green)
                }
            };
    }
}
