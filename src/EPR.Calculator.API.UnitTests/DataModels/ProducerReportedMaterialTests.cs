namespace EPR.Calculator.API.UnitTests.DataModels
{
    using AutoFixture;
    using EPR.Calculator.API.Data.DataModels;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ProducerReportedMaterialTests
    {
        private ProducerReportedMaterial TestClass { get; }

        private IFixture Fixture { get; }

        public ProducerReportedMaterialTests()
        {
            Fixture = new Fixture();
            this.TestClass = Fixture.Create<ProducerReportedMaterial>();
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
        public void CanSetAndGetMaterialId()
        {
            // Arrange
            var testValue = Fixture.Create<int>();

            // Act
            this.TestClass.MaterialId = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.MaterialId);
        }

        [TestMethod]
        public void CanSetAndGetProducerDetailId()
        {
            // Arrange
            var testValue = Fixture.Create<int>();

            // Act
            this.TestClass.ProducerDetailId = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.ProducerDetailId);
        }

        [TestMethod]
        public void CanSetAndGetPackagingType()
        {
            // Arrange
            var testValue = Fixture.Create<string>();

            // Act
            this.TestClass.PackagingType = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.PackagingType);
        }

        [TestMethod]
        public void CanSetAndGetPackagingTonnage()
        {
            // Arrange
            var testValue = Fixture.Create<decimal>();

            // Act
            this.TestClass.PackagingTonnage = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.PackagingTonnage);
        }

        [TestMethod]
        public void CanSetAndGetProducerDetail()
        {
            // Arrange
            var testValue = Fixture.Create<ProducerDetail>();

            // Act
            this.TestClass.ProducerDetail = testValue;

            // Assert
            Assert.AreSame(testValue, this.TestClass.ProducerDetail);
        }

        [TestMethod]
        public void CanSetAndGetMaterial()
        {
            // Arrange
            var testValue = Fixture.Create<Material>();

            // Act
            this.TestClass.Material = testValue;

            // Assert
            Assert.AreSame(testValue, this.TestClass.Material);
        }
    }
}