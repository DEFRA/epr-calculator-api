using EPR.Calculator.Service.Function.Misc;

namespace EPR.Calculator.Service.Function.UnitTests.Misc
{
    [TestClass]
    public class TypeConverterUtilTests
    {
        private class TestClass
        {

        }

        [TestMethod]
        public void CanCallConvertToDecimal()
        {
            // Arrange
            var value = "10.6";

            // Act
            var result = TypeConverterUtil.ConvertTo<decimal>(value);

            // Assert
            Assert.AreEqual(10.6m, result);
        }

        [TestMethod]
        public void NotValueType_NotNull_CantConvert()
        {
            // Arrange
            var value = "10.6";

            // Act
            var result = TypeConverterUtil.ConvertTo<TestClass>(value);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void CanCallConvertToWithValueNull()
        {
            // Arrange
            string? value = null;

            // Act
            var result = TypeConverterUtil.ConvertTo<decimal>(value);

            // Assert
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void CanCallConvertToWithValueGuid()
        {
            // Arrange
            string value = Guid.NewGuid().ToString();

            // Act
            var result = TypeConverterUtil.ConvertTo<Guid>(value);

            // Assert
            Assert.AreEqual(Guid.Parse(value), result);
        }

        [TestMethod]
        public void CannotCallConvertToWithObject()
        {
            // Arrange
            var value = new object();

            // Act
            var result = TypeConverterUtil.ConvertTo<decimal>(value);


            // Assert
            Assert.AreEqual(0, result);
        }
    }
}