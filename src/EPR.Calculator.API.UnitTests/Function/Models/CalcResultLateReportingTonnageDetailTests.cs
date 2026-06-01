using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.UnitTests.Models
{
    [TestClass]
    public class CalcResultLateReportingTonnageDetailTests
    {
        public CalcResultLateReportingTonnageDetailTests()
        {
            Fixture = new Fixture();
            Total = Fixture.Create<decimal>();
            Red   = Fixture.Create<decimal>();
            Amber = Fixture.Create<decimal>();
            Green = Fixture.Create<decimal>();
            TestClass = new CalcResultLateReportingTonnageDetail
            {
                Total = Total,
                Red   = Red,
                Amber = Amber,
                Green = Green
            };
        }

        private CalcResultLateReportingTonnageDetail TestClass { get; init; }
        private IFixture Fixture { get; init; }
        private decimal Total { get; init; }
        private decimal Red { get; init; }
        private decimal Amber { get; init; }
        private decimal Green { get; init; }

        [TestMethod]
        public void CanInitialize()
        {
            var instance = new CalcResultLateReportingTonnageDetail
            {
                Total = Total,
                Red   = Red,
                Amber = Amber,
                Green = Green
            };
            Assert.IsNotNull(instance);
        }

        [TestMethod]
        public void ImplementsIEquatable_CalcResultLateReportingTonnageDetail()
        {
            var same = new CalcResultLateReportingTonnageDetail
            {
                Total = Total,
                Red   = Red,
                Amber = Amber,
                Green = Green
            };
            var different = Fixture.Create<CalcResultLateReportingTonnageDetail>();

            Assert.IsFalse(TestClass.Equals(default(object)));
            Assert.IsFalse(TestClass.Equals(new object()));
            Assert.IsTrue(TestClass.Equals((object)same));
            Assert.IsFalse(TestClass.Equals((object)different));
            Assert.IsTrue(TestClass.Equals(same));
            Assert.IsFalse(TestClass.Equals(different));
            Assert.AreEqual(same.GetHashCode(), TestClass.GetHashCode());
            Assert.AreNotEqual(different.GetHashCode(), TestClass.GetHashCode());
            Assert.IsTrue(TestClass == same);
            Assert.IsFalse(TestClass == different);
            Assert.IsFalse(TestClass != same);
            Assert.IsTrue(TestClass != different);
        }

        [TestMethod]
        public void TotalLateReportingTonnageIsInitializedCorrectly()
            => Assert.AreEqual(Total, TestClass.Total);
    }
}
