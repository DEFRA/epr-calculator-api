using EPR.Calculator.Service.Function.Exporter.JsonExporter;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;
using Json.Schema;
using System.Text.Json;

namespace EPR.Calculator.Service.Function.UnitTests.Exporter.JsonExporter
{
    [TestClass]
    public class BillingFileJsonWriterTests
    {
        public TestContext TestContext { get; set; } = null!;

        [TestMethod]
        public async Task WriteToString_2025_ConformsToSchema()
        {
            var schema = JsonSchema.FromText(await File.ReadAllTextAsync("Schemas/2025-billing.schema.json"));

            var writer = CreateWriter();
            var json = await writer.WriteToString(
                TestDataHelper.BillingRun2025,
                TestDataHelper.GetCalcResult(applyModulation: false));

            var result = schema.Evaluate(
                JsonDocument.Parse(json).RootElement,
                new EvaluationOptions { OutputFormat = OutputFormat.List });

            result.IsValid.ShouldBeTrue(FormatErrors(result));
        }

        [TestMethod]
        public async Task WriteToString_2026_ConformsToSchema()
        {
            var schema = JsonSchema.FromText(await File.ReadAllTextAsync("Schemas/2026-billing.schema.json"));

            var writer = CreateWriter();
            var json = await writer.WriteToString(
                TestDataHelper.BillingRun2026,
                TestDataHelper.GetCalcResult(applyModulation: true));

            var result = schema.Evaluate(
                JsonDocument.Parse(json).RootElement,
                new EvaluationOptions { OutputFormat = OutputFormat.List });

            result.IsValid.ShouldBeTrue(FormatErrors(result));
        }

        private static IBillingFileJsonWriter CreateWriter()
        {
            var materialService = new Mock<IMaterialService>();
            materialService
                .Setup(s => s.GetMaterials())
                .ReturnsAsync(TestDataHelper.GetMaterialDetails());
            return new BillingFileJsonWriter(materialService.Object);
        }

        private static string FormatErrors(EvaluationResults result)
        {
            var lines = new List<string>();
            Collect(result, lines);
            return string.Join("\n", lines);
        }

        private static void Collect(EvaluationResults result, List<string> lines)
        {
            if (result.IsValid) { return; }
            if (result.Errors != null)
            {
                foreach (var (keyword, message) in result.Errors)
                    lines.Add($"{result.InstanceLocation} [{keyword}]: {message}");
            }
            foreach (var detail in result.Details ?? [])
                Collect(detail, lines);
        }
    }
}
