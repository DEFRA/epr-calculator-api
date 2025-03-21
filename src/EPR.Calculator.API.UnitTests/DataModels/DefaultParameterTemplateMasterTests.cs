namespace EPR.Calculator.API.UnitTests.DataModels
{
    using AutoFixture;
    using EPR.Calculator.API.Data.DataModels;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class DefaultParameterTemplateMasterTests
    {
        public DefaultParameterTemplateMasterTests()
        {
            this.Fixture = new Fixture();
            this.TestClass = this.Fixture.Create<DefaultParameterTemplateMaster>();
        }

        private DefaultParameterTemplateMaster TestClass { get; }

        private IFixture Fixture { get; }

        [TestMethod]
        public void CanSetAndGetParameterUniqueReferenceId()
        {
            // Arrange
            var testValue = this.Fixture.Create<string>();

            // Act
            this.TestClass.ParameterUniqueReferenceId = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.ParameterUniqueReferenceId);
        }

        [TestMethod]
        public void CanSetAndGetParameterType()
        {
            // Arrange
            var testValue = this.Fixture.Create<string>();

            // Act
            this.TestClass.ParameterType = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.ParameterType);
        }

        [TestMethod]
        public void CanSetAndGetParameterCategory()
        {
            // Arrange
            var testValue = this.Fixture.Create<string>();

            // Act
            this.TestClass.ParameterCategory = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.ParameterCategory);
        }

        [TestMethod]
        public void CanSetAndGetValidRangeFrom()
        {
            // Arrange
            var testValue = this.Fixture.Create<decimal>();

            // Act
            this.TestClass.ValidRangeFrom = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.ValidRangeFrom);
        }

        [TestMethod]
        public void CanSetAndGetValidRangeTo()
        {
            // Arrange
            var testValue = this.Fixture.Create<decimal>();

            // Act
            this.TestClass.ValidRangeTo = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.ValidRangeTo);
        }
    }
}