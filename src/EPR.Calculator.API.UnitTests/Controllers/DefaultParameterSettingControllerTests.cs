using EPR.Calculator.API.Constants;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Validators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Security.Claims;
using System.Security.Principal;

namespace EPR.Calculator.API.Tests.Controllers
{
    [TestClass]
    public class DefaultParameterSettingControllerTests : BaseControllerTest
    {
        [TestMethod]
        public void CreateTest_With_Records()
        {
            var actionResult = DataPostCall();
            Assert.AreEqual(201, actionResult?.StatusCode);

            Assert.AreEqual(DefaultParameterUniqueReferences.UniqueReferences.Length, dbContext?.DefaultParameterSettingDetail.Count());
            Assert.AreEqual(1, dbContext?.DefaultParameterSettings.Count());
            Assert.AreEqual(DefaultParameterUniqueReferences.UniqueReferences.Length, dbContext?.DefaultParameterTemplateMasterList.Count());
        }

        [TestMethod]
        public void CreateTest_With_Records_When_Existing_Updates()
        {
            var actionResult1 = DataPostCall();
            Assert.AreEqual(201, actionResult1?.StatusCode);

            var actionResult2 = DataPostCall();
            Assert.AreEqual(201, actionResult2?.StatusCode);

            var expectedLength = DefaultParameterUniqueReferences.UniqueReferences.Length * 2;
            Assert.AreEqual(expectedLength, dbContext?.DefaultParameterSettingDetail.Count());
            Assert.AreEqual(2, dbContext?.DefaultParameterSettings.Count());
            Assert.AreEqual(DefaultParameterUniqueReferences.UniqueReferences.Length, dbContext?.DefaultParameterTemplateMasterList.Count());

            Assert.AreEqual(DefaultParameterUniqueReferences.UniqueReferences.Length, dbContext?.DefaultParameterSettingDetail.Count(x => x.DefaultParameterSettingMasterId == 2));
            Assert.AreEqual(1, dbContext?.DefaultParameterSettings.Count(a => a.EffectiveTo == null));
        }
        //GET API
        [TestMethod]
        public void Get_RequestOkResult_WithDefaultSchemeParametersDto_WhenDataExist()
        {
            DataPostCall();

            var tempdateData = new DefaultSchemeParametersDto()
            {
                Id = 1,
                ParameterYear = "2024-25",
                EffectiveFrom = DateTime.Now,

                EffectiveTo = null,
                CreatedBy = "Testuser",
                CreatedAt = DateTime.Now,

                DefaultParameterSettingMasterId = 1,
                ParameterUniqueRef = "BADEBT-P",
                ParameterType = "Aluminium",
                ParameterCategory = "Communication costs",
                ParameterValue = 90m,
            };

            //Act
            var actionResult1 = defaultParameterSettingController?.Get("2024-25") as ObjectResult;

            //Assert
            var okResult = actionResult1 as ObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            var actionResul2 = okResult.Value as List<DefaultSchemeParametersDto>;
            Assert.AreEqual(actionResul2?.Count, DefaultParameterUniqueReferences.UniqueReferences.Length);

            Assert.AreEqual(tempdateData.Id, actionResul2?[0].Id);
            Assert.AreEqual(tempdateData.ParameterValue, actionResul2?[0].ParameterValue);
            Assert.AreEqual(tempdateData.ParameterUniqueRef, actionResul2?[0].ParameterUniqueRef);
        }

        [TestMethod]
        public void GetSchemeParameter_ReturnNotFound_WithDefaultSchemeParametersDoesNotExist()
        {
            DataPostCall();

            // Return 404 error if the year does not exist
            //Act
            var result = defaultParameterSettingController?.Get("2028-25");
            //Assert
            var okResult = result as ObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(404, okResult.StatusCode);

        }

        [TestMethod]
        public void GetSchemeParameter_Return_400_Error_With_No_YearSupplied()
        {
            ParameterYearValueValidationValidator _validator = new ParameterYearValueValidationValidator();
            string _parameter = string.Empty;
            var result = _validator.Validate(_parameter);

            Assert.IsNotNull(result);
            Assert.AreEqual("Parameter Year is required", result.Errors.First().ErrorMessage);
        }

        [TestMethod]
        public void Create_Default_Parameter_Setting_With_No_FileName()
        {
            var schemeParameterTemplateValues = new List<SchemeParameterTemplateValueDto>();
            foreach (var item in DefaultParameterUniqueReferences.UniqueReferences)
            {
                if (item == "MATT-AD" || item == "MATT-PD" || item == "TONT-AD" || item == "TONT-PD")
                {
                    schemeParameterTemplateValues.Add(new SchemeParameterTemplateValueDto
                    {
                        ParameterValue = "-90",
                        ParameterUniqueReferenceId = item
                    });
                }
                else
                {

                    schemeParameterTemplateValues.Add(new SchemeParameterTemplateValueDto
                    {
                        ParameterValue = "90",
                        ParameterUniqueReferenceId = item
                    });

                }
            }
           

            CreateDefaultParameterSettingValidator _validator = new CreateDefaultParameterSettingValidator();
            CreateDefaultParameterSettingDto  _parameter =  new CreateDefaultParameterSettingDto()
            { ParameterFileName = string.Empty, ParameterYear="2024-25", SchemeParameterTemplateValues = schemeParameterTemplateValues  };
            var result = _validator.Validate(_parameter);

            Assert.IsNotNull(result);
            Assert.AreEqual("FileName is required", result.Errors.First().ErrorMessage);
        }


        [TestMethod]
        public void Create_Default_Parameter_Setting_With_Max_FileName_Length()
        {
            var schemeParameterTemplateValues = new List<SchemeParameterTemplateValueDto>();
            foreach (var item in DefaultParameterUniqueReferences.UniqueReferences)
            {
                if (item == "MATT-AD" || item == "MATT-PD" || item == "TONT-AD" || item == "TONT-PD")
                {
                    schemeParameterTemplateValues.Add(new SchemeParameterTemplateValueDto
                    {
                        ParameterValue = "-90",
                        ParameterUniqueReferenceId = item
                    });
                }
                else
                {

                    schemeParameterTemplateValues.Add(new SchemeParameterTemplateValueDto
                    {
                        ParameterValue = "90",
                        ParameterUniqueReferenceId = item
                    });

                }
            }


            CreateDefaultParameterSettingValidator _validator = new CreateDefaultParameterSettingValidator();
            CreateDefaultParameterSettingDto _parameter = new CreateDefaultParameterSettingDto()
            { ParameterFileName = new string('A', 257), ParameterYear = "2024-25", SchemeParameterTemplateValues = schemeParameterTemplateValues };
            var result = _validator.Validate(_parameter);

            Assert.IsNotNull(result);
            Assert.AreEqual(ErrorMessages.MaxFileNameLength, result.Errors.First().ErrorMessage);
        }



        // Private Methods
        public ObjectResult? DataPostCall()
        {
            var schemeParameterTemplateValues = new List<SchemeParameterTemplateValueDto>();
            foreach (var item in DefaultParameterUniqueReferences.UniqueReferences)
            {
                if (item == "MATT-AD" || item == "MATT-PD" || item == "TONT-AD" || item == "TONT-PD")
                {
                    schemeParameterTemplateValues.Add(new SchemeParameterTemplateValueDto
                    {
                        ParameterValue = "-90",
                        ParameterUniqueReferenceId = item
                    });
                }
                else
                {

                    schemeParameterTemplateValues.Add(new SchemeParameterTemplateValueDto
                    {
                        ParameterValue = "90",
                        ParameterUniqueReferenceId = item
                    });

                }
            }
            var createDefaultParameterDto = new CreateDefaultParameterSettingDto
            {
                ParameterYear = "2024-25",
                SchemeParameterTemplateValues = schemeParameterTemplateValues,
                ParameterFileName = "TestFileName"
            };
            var identity = new GenericIdentity("TestUser");
            identity.AddClaim(new Claim("name", "TestUser"));
            var principal = new ClaimsPrincipal(identity);

            var context = new DefaultHttpContext()
            {
                User = principal
            };

            defaultParameterSettingController.ControllerContext = new ControllerContext
            {
                HttpContext = context
            };
            var actionResult = defaultParameterSettingController?.Create(createDefaultParameterDto) as ObjectResult;
            return actionResult;
        }
    }
}