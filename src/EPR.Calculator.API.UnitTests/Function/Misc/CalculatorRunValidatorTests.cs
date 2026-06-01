using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Misc;

namespace EPR.Calculator.Service.Function.UnitTests.Misc
{
    [TestClass]
    public class CalculatorRunValidatorTests
    {
        private CalculatorRunValidator TestClass { get; init; }

        private IFixture Fixture { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CalculatorRunValidatorTests"/> class.
        /// </summary>
        public CalculatorRunValidatorTests()
        {
            Fixture = new Fixture();
            TestClass = new CalculatorRunValidator();
        }

        [TestMethod]
        public void CheckForNullIds_ShouldReturnErrorMessages_WhenIdsAreNull()
        {
            // Arrange
            var calculatorRun = new CalculatorRun
            {
                CalculatorRunOrganisationDataMasterId = null,
                DefaultParameterSettingMasterId = null,
                CalculatorRunPomDataMasterId = 1,
                LapcapDataMasterId = null,
                Name = "soe",
                RelativeYear = new RelativeYear(2024),
            };

            var validator = new CalculatorRunValidator();

            // Act
            ValidationResult result = validator.ValidateCalculatorRunIds(calculatorRun);

            // Assert
            var expectedErrors = new List<string>
            {
                "CalculatorRunOrganisationDataMasterId is null",
                "DefaultParameterSettingMasterId is null",
                "LapcapDataMasterId is null"
            };
            CollectionAssert.AreEqual(expectedErrors, result.ErrorMessages.ToList());
            Assert.IsFalse(result.IsValid);
        }

        [TestMethod]
        public void CheckForNullIds_ShouldReturnEmptyList_WhenNoIdsAreNull()
        {
            // Arrange
            var calculatorRun = new CalculatorRun
            {
                CalculatorRunOrganisationDataMasterId = 1,
                DefaultParameterSettingMasterId = 1,
                CalculatorRunPomDataMasterId = 1,
                LapcapDataMasterId = 1,
                Name = "soe",
                RelativeYear = new RelativeYear(2024),
            };

            var validator = new CalculatorRunValidator();

            // Act
            ValidationResult result = validator.ValidateCalculatorRunIds(calculatorRun);

            // Assert
            Assert.AreEqual(0, result.ErrorMessages.Count());
            Assert.IsTrue(result.IsValid);
        }
    }
}