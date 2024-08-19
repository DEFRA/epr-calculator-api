using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.Calculator.API.UnitTests
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
                ParameterCategory = this.parameterCategory
            };
            string errorMessage = "Some error message";
            var errorDto = Util.CreateErrorDto(template, errorMessage);
            Assert.IsNotNull(errorDto);
            Assert.AreEqual(errorDto.ParameterType, parameterType);
            Assert.AreEqual(errorDto.ParameterCategory, parameterCategory);
            Assert.AreEqual(errorDto.ParameterUniqueRef, parameterUniqueReferenceId);
        }

        [TestMethod]
        public void GetParameterValueTest_For_Non_Percent()
        {
            DefaultParameterTemplateMaster template = new DefaultParameterTemplateMaster
            {
                ParameterType = this.parameterType,
                ParameterUniqueReferenceId = this.parameterUniqueReferenceId,
                ParameterCategory = this.parameterCategory
            };
            var parameterValue = Util.GetParameterValue(template, "£100");
            Assert.IsNotNull(parameterValue);
            Assert.AreEqual(parameterValue, "100");
        }

        [TestMethod]
        public void GetParameterValueTest_For_Percent()
        {
            var parameterType = "Paramter Type 1 percent";
            var parameterUniqueReferenceId = Guid.NewGuid().ToString();
            var parameterCategory = "Parameter Category 1";
            DefaultParameterTemplateMaster template = new DefaultParameterTemplateMaster
            {
                ParameterType = parameterType,
                ParameterUniqueReferenceId = parameterUniqueReferenceId,
                ParameterCategory = parameterCategory
            };
            var parameterValue = Util.GetParameterValue(template, "100%");
            Assert.IsNotNull(parameterValue);
            Assert.AreEqual(parameterValue, "100");
        }
    }
}
