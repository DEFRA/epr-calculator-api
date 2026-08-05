using EPR.Calculator.Service.Function.Builder.Summary.Common;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.Service.Function.UnitTests.Utils
{
    [TestClass]
    public class TonnageChangeUtilTests
    {
        // ---------- ComputePerMaterialChange ----------

        [TestMethod]
        public void ComputePerMaterialChange_levelNot1_returnsNull()
        {
            var result = TonnageChangeUtil.ComputePerMaterialChange(
                level: "2",
                netReportedTonnage: 100m,
                previousInvoicedTonnage: 80m);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void ComputePerMaterialChange_previousNull_returnsNull()
        {
            var result = TonnageChangeUtil.ComputePerMaterialChange(
                level: CommonConstants.LevelOne.ToString(),
                netReportedTonnage: 100m,
                previousInvoicedTonnage: null);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void ComputePerMaterialChange_previousZero_returnsZero()
        {
            var result = TonnageChangeUtil.ComputePerMaterialChange(
                level: CommonConstants.LevelOne.ToString(),
                netReportedTonnage: 100m,
                previousInvoicedTonnage: 0m);

            Assert.AreEqual(0m, result);
        }

        [TestMethod]
        public void ComputePerMaterialChange_validInputs_returnsDifference()
        {
            var result = TonnageChangeUtil.ComputePerMaterialChange(
                level: CommonConstants.LevelOne.ToString(),
                netReportedTonnage: 105.5m,
                previousInvoicedTonnage: 100.25m);

            Assert.AreEqual(5.25m, result);
        }

        // ---------- ComputeCountAndAdvice ----------

        [TestMethod]
        public void ComputeCountAndAdvice_levelNot1_returnsNulls()
        {
            var byMaterial = new Dictionary<string, DisposalFee>
            {
                ["PAPER"] = DisposalFee.Empty with { TonnageChange = 5m }
            };

            var (count, advice) = TonnageChangeUtil.ComputeCountAndAdvice("2", byMaterial);

            Assert.IsNull(count);
            Assert.IsNull(advice);
        }

        [TestMethod]
        public void ComputeCountAndAdvice_changesPresent_returnsCountAndCHANGE()
        {
            var byMaterial = new Dictionary<string, DisposalFee>
            {
                ["PAPER"] = DisposalFee.Empty with { TonnageChange = 0m },  // ignored
                ["GLASS"] = DisposalFee.Empty with { TonnageChange = null },  // ignored
                ["METAL"] = DisposalFee.Empty with { TonnageChange = 3m },  // counted
                ["PLASTIC"] = DisposalFee.Empty with { TonnageChange = -1m }  // counted
            };

            var (count, advice) = TonnageChangeUtil.ComputeCountAndAdvice(
                CommonConstants.LevelOne.ToString(), byMaterial);

            Assert.AreEqual("2", count);
            Assert.AreEqual("CHANGE", advice);
        }

        [TestMethod]
        public void ComputeCountAndAdvice_noChanges_returnsZeroAndEmptyAdvice()
        {
            var byMaterial = new Dictionary<string, DisposalFee>
            {
                ["PAPER"] = DisposalFee.Empty with { TonnageChange = 0m },
                ["GLASS"] = DisposalFee.Empty with { TonnageChange = null }
            };

            var (count, advice) = TonnageChangeUtil.ComputeCountAndAdvice(
                CommonConstants.LevelOne.ToString(), byMaterial);

            Assert.AreEqual("0", count);
            Assert.AreEqual(string.Empty, advice);
        }
    }
}
