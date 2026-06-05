using System.Text.Json.Nodes;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.UnitTests.Utils
{
    /// <summary>
    /// Contains helper methods for comparing the contents of JsonNode objects to other values.
    /// </summary>
    public static class JsonNodeComparer
    {
        public static void AssertAreEqual(decimal expected, JsonNode? actual)
        {
            Assert.IsNotNull(actual, "Actual value should not be null.");
            Assert.AreEqual(
                CurrencyConverterUtils.FormatCurrencyWithGbpSymbol(expected, 2),
                actual.GetValue<string>(),
                $"Expected {expected} to be equal to {actual}");
        }

        public static void AssertAreEqual(string expected, JsonNode? actual)
        {
            Assert.IsNotNull(actual, "Actual value should not be null.");
            Assert.AreEqual(
                expected,
                actual.GetValue<string>(),
                $"Expected '{expected}' to be equal to '{actual}'");
        }
    }
}