namespace EPR.Calculator.API.UnitTests.DataModels
{
    using AutoFixture;
    using EPR.Calculator.API.Data.DataModels;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class LapcapDataDetailTests
    {
        private LapcapDataDetail TestClass { get; }

        private IFixture Fixture { get; }

        public LapcapDataDetailTests()
        {
            Fixture = new Fixture();
            this.TestClass = Fixture.Create<LapcapDataDetail>();
        }

        [TestMethod]
        public void CanSetAndGetId()
        {
            // Arrange
            var testValue = Fixture.Create<int>();

            // Act
            this.TestClass.Id = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.Id);
        }

        [TestMethod]
        public void CanSetAndGetLapcapDataMasterId()
        {
            // Arrange
            var testValue = Fixture.Create<int>();

            // Act
            this.TestClass.LapcapDataMasterId = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.LapcapDataMasterId);
        }

        [TestMethod]
        public void CanSetAndGetUniqueReference()
        {
            // Arrange
            var testValue = Fixture.Create<string>();

            // Act
            this.TestClass.UniqueReference = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.UniqueReference);
        }

        [TestMethod]
        public void CanSetAndGetTotalCost()
        {
            // Arrange
            var testValue = Fixture.Create<decimal>();

            // Act
            this.TestClass.TotalCost = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.TotalCost);
        }

        [TestMethod]
        public void CanSetAndGetLapcapDataMaster()
        {
            // Arrange
            var testValue = Fixture.Create<LapcapDataMaster>();

            // Act
            this.TestClass.LapcapDataMaster = testValue;

            // Assert
            Assert.AreSame(testValue, this.TestClass.LapcapDataMaster);
        }

        [TestMethod]
        public void CanSetAndGetLapcapDataTemplateMaster()
        {
            // Arrange
            var testValue = Fixture.Create<LapcapDataTemplateMaster>();

            // Act
            this.TestClass.LapcapDataTemplateMaster = testValue;

            // Assert
            Assert.AreSame(testValue, this.TestClass.LapcapDataTemplateMaster);
        }
    }
}