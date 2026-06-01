namespace EPR.Calculator.Service.Function.Models;

public record CalcResultLateReportingTonnage
{
    required public Dictionary<string, CalcResultLateReportingTonnageDetail> ByMaterial { get; init; }

    private CalcResultLateReportingTonnageDetail? total;
    public CalcResultLateReportingTonnageDetail Total =>
        total ??=
            new CalcResultLateReportingTonnageDetail
            {
                 Total = ByMaterial.Values.Sum(v => v.Total),
                 Red   = ByMaterial.Values.Sum(v => v.Red),
                 Amber = ByMaterial.Values.Sum(v => v.Amber),
                 Green = ByMaterial.Values.Sum(v => v.Green)
            };
}

public record CalcResultLateReportingTonnageDetail
{
    required public decimal Total { get; init; }

    required public decimal Red { get; init; }

    required public decimal Amber { get; init; }

    required public decimal Green { get; init; }
}
