using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Features.BillingRun.Contexts;
using EPR.Calculator.Service.Function.Features.CalculatorRun.Contexts;

namespace EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;

public static partial class TestDataHelper
{
    public static CalculatorRunContext CalculatorRun2024 => new()
    {
        RunId = 2024001,
        RunName = "2024 Calculator Run",
        ProcessingStartedAt = DateTimeOffset.Parse("2024-01-01T00:00:00Z"),
        RelativeYear = new RelativeYear(2024),
        User = "TestUser"
    };

    public static CalculatorRunContext CalculatorRun2025 => new()
    {
        RunId = 2025001,
        RunName = "2025 Calculator Run",
        ProcessingStartedAt = DateTimeOffset.Parse("2025-01-01T00:00:00Z"),
        RelativeYear = new RelativeYear(2025),
        User = "TestUser"
    };

    public static CalculatorRunContext CalculatorRun2026 => new()
    {
        RunId = 2026001,
        RunName = "2026 Calculator Run",
        ProcessingStartedAt = DateTimeOffset.Parse("2026-01-01T00:00:00Z"),
        RelativeYear = new RelativeYear(2026),
        User = "TestUser"
    };

    public static BillingRunContext BillingRun2024 => new()
    {
        RunId = 2024011,
        RunName = "2024 Billing Run",
        ProcessingStartedAt = DateTimeOffset.Parse("2024-02-01T00:00:00Z"),
        RelativeYear = new RelativeYear(2024),
        User = "TestUser",
        AcceptedProducerIds = [1, 2, 3]
    };

    public static BillingRunContext BillingRun2025 => new()
    {
        RunId = 2025011,
        RunName = "2025 Billing Run",
        ProcessingStartedAt = DateTimeOffset.Parse("2025-02-01T00:00:00Z"),
        RelativeYear = new RelativeYear(2025),
        User = "TestUser",
        AcceptedProducerIds = [1, 2, 3]
    };

    public static BillingRunContext BillingRun2026 => new()
    {
        RunId = 2026011,
        RunName = "2026 Billing Run",
        ProcessingStartedAt = DateTimeOffset.Parse("2026-02-01T00:00:00Z"),
        RelativeYear = new RelativeYear(2026),
        User = "TestUser",
        AcceptedProducerIds = [1, 2, 3]
    };
}
