using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.UnitTests.Utils
{
    [TestClass]
    public class ResetObjectUtilTests
    {
        [TestMethod]
        public void CanCallResetObject()
        {
            // Arrange
            var fixture = new Fixture();
            var j = new TestClass
            {
                Temperature = 10.9m,
                Fruits = 10,
                Name = "Test",
                IsTotalRow = true,
                IsTested = true,
                childTest = new ChildTestClass { IsChild = true },
                Humidity = fixture.Create<double>(),
                LeaverDate = "Totals",
                isOverallTotalRow = true,
                Numbers = new List<int> { 1, 2, 3 },
                CurrentDateTime = DateTime.UtcNow
            };

            // Act
            ResetObjectUtil.ResetObject(j);

            // Assert
            Assert.IsNotNull(j);
            Assert.AreEqual(j.Name, string.Empty);
            Assert.AreEqual(0, j.Temperature);
            Assert.AreEqual(0, j.Fruits);
            Assert.AreEqual(0, j.Humidity);
            Assert.AreEqual("Totals", j.LeaverDate);
            Assert.IsTrue(j.IsTotalRow);
            Assert.IsTrue(j.isOverallTotalRow);
            Assert.IsFalse(j.IsTested);
            Assert.IsFalse(j.childTest.IsChild);
            Assert.AreEqual(3 ,j.Numbers.Count);
            Assert.AreEqual(default(DateTime), j.CurrentDateTime);
        }
    }

    public class TestClass
    {
        public decimal Temperature { get; set; }
        public int Fruits { get; set; }
        public required string Name { get; set; }
        public bool IsTotalRow { get; set; }
        public bool isOverallTotalRow { get; set; }

        public bool IsTested { get; set; }

        public double Humidity { get; set; }

        public required ChildTestClass childTest { get; set; }

        public required string LeaverDate { get; set; }

        public required List<int> Numbers { get; set; }

        public DateTime CurrentDateTime {  get; set; }
    }

    public class ChildTestClass
    {
        public bool IsChild { get; set; }
    }
}