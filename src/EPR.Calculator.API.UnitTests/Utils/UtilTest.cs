using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.Calculator.API.UnitTests.Utils
{
    [TestClass]
    public class UtilTest
    {
        private string parameterType = "Paramter Type 1";
        private string parameterUniqueReferenceId = Guid.NewGuid().ToString();
        private string parameterCategory = "Parameter Category 1";

        [TestMethod]
        public void CreateErrorDtoTest()
        {
            DefaultParameterTemplateMaster template = new DefaultParameterTemplateMaster
            {
                ParameterType = this.parameterType,
                ParameterUniqueReferenceId = this.parameterUniqueReferenceId,
                ParameterCategory = this.parameterCategory,
            };
            string errorMessage = "Some error message";
            var errorDto = Util.CreateErrorDto(template, errorMessage);
            Assert.IsNotNull(errorDto);
            Assert.AreEqual(errorDto.ParameterType, this.parameterType);
            Assert.AreEqual(errorDto.ParameterCategory, this.parameterCategory);
            Assert.AreEqual(errorDto.ParameterUniqueRef, this.parameterUniqueReferenceId);
        }

        [TestMethod]
        public void GetParameterValueTest_For_Non_Percent()
        {
            DefaultParameterTemplateMaster template = new DefaultParameterTemplateMaster
            {
                ParameterType = this.parameterType,
                ParameterUniqueReferenceId = this.parameterUniqueReferenceId,
                ParameterCategory = this.parameterCategory,
            };
            var parameterValue = Util.GetParameterValue(template, "£100");
            Assert.IsNotNull(parameterValue);
            Assert.AreEqual("100", parameterValue);
        }

        [TestMethod]
        public void GetParameterValueTest_For_Percent()
        {
            this.parameterType = "Paramter Type 1 percent";
            this.parameterUniqueReferenceId = Guid.NewGuid().ToString();
            this.parameterCategory = "Parameter Category 1";
            DefaultParameterTemplateMaster template = new DefaultParameterTemplateMaster
            {
                ParameterType = this.parameterType,
                ParameterUniqueReferenceId = this.parameterUniqueReferenceId,
                ParameterCategory = this.parameterCategory,
            };
            var parameterValue = Util.GetParameterValue(template, "100%");
            Assert.IsNotNull(parameterValue);
            Assert.AreEqual("100", parameterValue);
        }
    }
}
