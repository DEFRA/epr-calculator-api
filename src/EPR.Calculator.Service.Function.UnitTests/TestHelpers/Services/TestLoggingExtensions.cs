using Microsoft.Extensions.Logging;

namespace EPR.Calculator.Service.Function.UnitTests.TestHelpers.Services;

public static class TestLoggingExtensions
{
    public static void VerifyLogContains<T>(this Mock<ILogger<T>> logger, LogLevel level, string fragment)
    {
        logger.Verify(l => l.Log(
                level,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, _) => o.ToString()!.Contains(fragment, StringComparison.OrdinalIgnoreCase)),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce(),
            $"Log message should contain '{fragment}'.");
    }
}
