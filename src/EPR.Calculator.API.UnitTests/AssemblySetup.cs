using System.Globalization;

namespace EPR.Calculator.API.UnitTests;

/// <summary>
///     Applies configuration that must be in place before any test in this assembly runs.
/// </summary>
[TestClass]
public class AssemblySetup
{
    /// <summary>
    ///     Pins the culture for every test thread to en-GB, mirroring the culture the application
    ///     configures for itself in <c>Program.cs</c>. Without this, tests asserting on
    ///     culture-sensitive formatting (currency, numbers, dates) pass or fail depending on the
    ///     culture of the machine running them.
    /// </summary>
    [AssemblyInitialize]
    public static void Initialise(TestContext testContext)
    {
        ArgumentNullException.ThrowIfNull(testContext);

        var enGb = new CultureInfo("en-GB");
        CultureInfo.DefaultThreadCurrentCulture = enGb;
        CultureInfo.DefaultThreadCurrentUICulture = enGb;
    }
}
