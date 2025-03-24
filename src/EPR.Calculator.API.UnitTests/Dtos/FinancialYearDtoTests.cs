namespace EPR.Calculator.API.UnitTests.Dtos
{
    using System;
    using AutoFixture;
    using EPR.Calculator.API.Dtos;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class FinancialYearDtoTests
    {
        private FinancialYearDto TestClass { get; init; }

        private IFixture Fixture { get; init; }

        private string Name { get; init; }

        private string Description { get; init; }

        public FinancialYearDtoTests()
        {
            this.Fixture = new Fixture();
            this.Name = this.Fixture.Create<string>();
            this.Description = this.Fixture.Create<string>();
            this.TestClass = new FinancialYearDto
            {
                Name = this.Name,
                Description = this.Description,
            };
        }

        [TestMethod]
        public void CanInitialize()
        {
            // Act
            var instance = new FinancialYearDto
            {
                Name = this.Name,
                Description = this.Description,
            };

            // Assert
            Assert.IsNotNull(instance);
        }

        [TestMethod]
        public void ImplementsIEquatable_FinancialYearDto()
        {
            // Arrange
            var same = new FinancialYearDto
            {
                Name = this.Name,
                Description = this.Description,
            };
            var different = this.Fixture.Create<FinancialYearDto>();

            // Assert
            Assert.IsFalse(this.TestClass.Equals(default(object)));
            Assert.IsFalse(this.TestClass.Equals(new object()));
            Assert.IsTrue(this.TestClass.Equals((object)same));
            Assert.IsFalse(this.TestClass.Equals((object)different));
            Assert.IsTrue(this.TestClass.Equals(same));
            Assert.IsFalse(this.TestClass.Equals(different));
            Assert.AreEqual(same.GetHashCode(), this.TestClass.GetHashCode());
            Assert.AreNotEqual(different.GetHashCode(), this.TestClass.GetHashCode());
            Assert.IsTrue(this.TestClass == same);
            Assert.IsFalse(this.TestClass == different);
            Assert.IsFalse(this.TestClass != same);
            Assert.IsTrue(this.TestClass != different);
        }

        [TestMethod]
        public void NameIsInitializedCorrectly()
        {
            Assert.AreEqual(this.Name, this.TestClass.Name);
        }

        [TestMethod]
        public void DescriptionIsInitializedCorrectly()
        {
            Assert.AreEqual(this.Description, this.TestClass.Description);
        }
    }
}