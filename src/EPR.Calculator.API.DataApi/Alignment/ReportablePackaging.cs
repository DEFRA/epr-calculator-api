namespace EPR.CommonDataService.DataApi.Alignment;

/// <summary>
///     The reportable-packaging rule from <c>sp_GetPaycalPomData</c>'s final WHERE clause: household,
///     consumer waste and public bin count regardless of material; household drinks containers count
///     only for glass. Applied upstream of both error detection and alignment, matching where the
///     stored procedure applied it.
/// </summary>
public static class ReportablePackaging
{
    private static readonly HashSet<string> ReportableTypes = ["HH", "CW", "PB"];
    private const string HouseholdDrinksContainersType = "HDC";
    private const string GlassMaterial = "GL";

    public static bool Includes(string? packagingType, string? packagingMaterial) =>
        packagingType is not null &&
        (ReportableTypes.Contains(packagingType) ||
         (packagingType == HouseholdDrinksContainersType && packagingMaterial == GlassMaterial));
}
