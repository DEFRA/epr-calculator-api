using EPR.Calculator.Service.Function.Models.JsonExporter;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;

namespace EPR.Calculator.Service.Function.UnitTests.Models.JsonExporter
{
    [TestClass]
    public class BillingFileJsonTests
    {
        [TestMethod]
        public void From_MapFieldsCorrectly()
        {
            var calcResult = TestDataHelper.GetCalcResult();
            var materials = TestDataHelper.GetMaterialDetails();
            var result = BillingFileJson.From(TestDataHelper.BillingRun2025, calcResult, materials);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.CalcResultDetail);
            Assert.AreEqual(1, result.CalcResultDetail!.RunId);
            Assert.IsNotNull(result.CalcResultLapcapData);
            // Lapcap mapping assertions
            var lapcap = result.CalcResultLapcapData as CalcResultLapcapDataJson;
            Assert.IsNotNull(lapcap);
            Assert.IsNotNull(lapcap.CalcResultLapcapDataTotal);
            Assert.AreEqual("£203,150.00", lapcap.CalcResultLapcapDataTotal!.TotalLaDisposalCost);
            var ladetails = result.CalcResultLaDisposalCostData!.CalcResultLaDisposalCostDetails.ToList();
            Assert.IsTrue(ladetails.Any(d => d.DisposalCostPricePerTonne == "£5.4000"));
            Assert.IsNotNull(result.CalcResult2aCommsDataByMaterial);
            var comms = result.CalcResult2aCommsDataByMaterial!.CalcResult2aCommsDataDetails;
            Assert.IsTrue(comms.Any(d => d.MaterialName == "Aluminium"));
            var aluminium = comms.Single(d => d.MaterialName == "Aluminium");
            Assert.AreEqual("£0.4200", aluminium.CommsCostByMaterialPricePerTonne);
            Assert.IsNotNull(result.CalcResult2bCommsDataByUkWide);
            Assert.AreEqual("£1,500.00", result.CalcResult2bCommsDataByUkWide!.EnglandCommsCostUKWide);
            Assert.IsNotNull(result.CalcResult2cCommsDataByCountry);
            Assert.AreEqual("£250.00", result.CalcResult2cCommsDataByCountry.WalesCommsCostByCountry);
            Assert.IsNotNull(result.ParametersCommsCost);
            var onePlusFourPct = result.ParametersCommsCost!.OnePlusFourCommsCostApportionmentPercentages;
            Assert.IsNotNull(onePlusFourPct);
            Assert.AreEqual("50.23000000%", onePlusFourPct.England);
            Assert.AreEqual("30.34000000%", onePlusFourPct.Wales);
            Assert.IsNotNull(result.ScaleUpProducers!.ProducerSubmissions);
            var subs = result.ScaleUpProducers.ProducerSubmissions!.ToList();
            Assert.IsTrue(subs.Count >= 1);
            Assert.AreEqual(1, subs[0].ProducerId);
            var calcResults = result.CalculationResults as CalculationResultsJson;
            Assert.IsNotNull(calcResults);
            Assert.IsNotNull(calcResults.ProducerCalculationResultsSummary);
        }
    }
}
