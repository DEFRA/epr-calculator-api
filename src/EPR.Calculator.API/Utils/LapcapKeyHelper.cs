using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;

namespace EPR.Calculator.API.Utils;

public static class LapcapKeyHelper
{
    public static string KeyFor(CreateLapcapDataRequest.LapcapValue lv) => MakeKey(lv.Country, lv.Material);
    public static string KeyFor(LapcapDataTemplateMaster t) => MakeKey(t.Country, t.Material);

    private static string MakeKey(string? country, string? material) => $"{country?.ToUpperInvariant()} {material?.ToUpperInvariant()}";
}
