namespace EPR.Calculator.API.UnitTests.Dtos
{
    using System;
    using AutoFixture;
    using EPR.Calculator.API.Dtos;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class RelativeYearDtoTests
    {
        public RelativeYearDtoTests()
        {
            this.Fixture = new Fixture();
            this.Value = this.Fixture.Create<int>();
            this.Description = this.Fixture.Create<string>();
            this.TestClass = new RelativeYearDto
            {
                Value = this.Value,
                Description = this.Description,
            };
        }

        private RelativeYearDto TestClass { get; init; }

        private IFixture Fixture { get; init; }

        private int Value { get; init; }

        private string Description { get; init; }

        [TestMethod]
        public void CanInitialize()
        {
            // Act
            var instance = new RelativeYearDto
            {
                Value = this.Value,
                Description = this.Description,
            };

            // Assert
            Assert.IsNotNull(instance);
        }

        [TestMethod]
        public void ImplementsIEquatable_RelativeYearDto()
        {
            // Arrange
            var same = new RelativeYearDto
            {
                Value = this.Value,
                Description = this.Description,
            };
            var different = this.Fixture.Create<RelativeYearDto>();

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
            Assert.AreEqual(this.Value, this.TestClass.Value);
        }

        [TestMethod]
        public void DescriptionIsInitializedCorrectly()
        {
            Assert.AreEqual(this.Description, this.TestClass.Description);
        }
    }
}