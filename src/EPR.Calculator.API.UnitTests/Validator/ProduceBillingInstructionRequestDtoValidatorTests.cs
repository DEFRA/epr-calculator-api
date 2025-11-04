using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Enums;
using EPR.Calculator.API.Validators;

namespace EPR.Calculator.API.UnitTests.Validator
{
    [TestClass]
    public class ProduceBillingInstructionRequestDtoValidatorTests
    {
        private readonly ProduceBillingInstuctionRequestDtoValidator validator;

        public ProduceBillingInstructionRequestDtoValidatorTests()
        {
            validator = new ProduceBillingInstuctionRequestDtoValidator();
        }

        [TestMethod]
        public void Should_Have_Error_When_OrganisationIds_Is_Empty()
        {
            var model = new ProduceBillingInstuctionRequestDto { OrganisationIds = [], Status = "Accepted" };
            var result = validator.Validate(model);

            Assert.AreEqual("Organisation Id is required.", result.Errors[0].ErrorMessage);
        }

        [TestMethod]
        public void Should_Have_Error_When_Status_Is_Empty()
        {
            var model = new ProduceBillingInstuctionRequestDto { OrganisationIds = [1], Status = string.Empty };
            var result = validator.Validate(model);

            Assert.AreEqual("Status is required.", result.Errors[0].ErrorMessage);
        }

        [TestMethod]
        public void Should_Have_Error_When_Status_Is_Invalid()
        {
            var model = new ProduceBillingInstuctionRequestDto { OrganisationIds = [1], Status = "Test" };
            var result = validator.Validate(model);

            Assert.AreEqual("Invalid status value.", result.Errors[0].ErrorMessage);
        }

        [TestMethod]
        public void Should_Have_Error_When_ReasonForRejection_Is_Missing_For_Rejected_Status()
        {
            var model = new ProduceBillingInstuctionRequestDto
            {
                OrganisationIds = [1],
                Status = BillingStatus.Rejected.ToString(),
                ReasonForRejection = string.Empty,
            };
            var result = validator.Validate(model);

            Assert.AreEqual("Reason for rejection is required.", result.Errors[0].ErrorMessage);
        }
    }
}
