using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.UnitTests.Models
{
    [TestClass]
    public class CalcResultLateReportingTonnageTests
    {
        public CalcResultLateReportingTonnageTests()
        {
            Fixture = new Fixture();
            LateReportingTonnageByMaterial = new Dictionary<string, CalcResultLateReportingTonnageDetail>
            {
                [Fixture.Create<string>()] = Fixture.Create<CalcResultLateReportingTonnageDetail>()
            };
            TestClass = new CalcResultLateReportingTonnage
            {
                ByMaterial = LateReportingTonnageByMaterial
            };
        }

        private CalcResultLateReportingTonnage TestClass { get; init; }
        private IFixture Fixture { get; init; }
        private Dictionary<string, CalcResultLateReportingTonnageDetail> LateReportingTonnageByMaterial { get; init; }

        [TestMethod]
        public void CanInitialize()
        {
            var instance = new CalcResultLateReportingTonnage
            {
                ByMaterial = LateReportingTonnageByMaterial
            };
            Assert.IsNotNull(instance);
        }

        [TestMethod]
        public void ImplementsIEquatable_CalcResultLateReportingTonnage()
        {
            var same = new CalcResultLateReportingTonnage
            {
                ByMaterial = LateReportingTonnageByMaterial
            };
            var different = new CalcResultLateReportingTonnage
            {
                ByMaterial = new Dictionary<string, CalcResultLateReportingTonnageDetail>
                {
                    [Fixture.Create<string>()] = Fixture.Create<CalcResultLateReportingTonnageDetail>()
                }
            };

            Assert.IsFalse(TestClass.Equals(default(object)));
            Assert.IsFalse(TestClass.Equals(new object()));
            Assert.IsTrue(TestClass.Equals((object)same));
            Assert.IsFalse(TestClass.Equals((object)different));
            Assert.IsTrue(TestClass.Equals(same));
            Assert.IsFalse(TestClass.Equals(different));
            Assert.AreEqual(same.GetHashCode(), TestClass.GetHashCode());
            Assert.IsTrue(TestClass == same);
            Assert.IsFalse(TestClass == different);
            Assert.IsFalse(TestClass != same);
            Assert.IsTrue(TestClass != different);
        }

        [TestMethod]
        public void LateReportingTonnageByMaterialIsInitializedCorrectly()
            => Assert.AreSame(LateReportingTonnageByMaterial, TestClass.ByMaterial);
    }
}
