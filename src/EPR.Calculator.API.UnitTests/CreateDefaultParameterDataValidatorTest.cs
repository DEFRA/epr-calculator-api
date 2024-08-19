using api.Validators;
using EPR.Calculator.API.Constants;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.Calculator.API.UnitTests
{
    [TestClass]
    public class CreateDefaultParameterDataValidatorTest
    {
        private ApplicationDBContext context;
        private CreateDefaultParameterDataValidator validator;

        List<DefaultParameterTemplateMaster> data = new List<DefaultParameterTemplateMaster>
            {
                new DefaultParameterTemplateMaster
                {
                    ParameterUniqueReferenceId = "COMC-AL",
                    ParameterCategory = "Aluminium",
                    ParameterType = "Communication costs",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m
                },
                new DefaultParameterTemplateMaster
                {
                    ParameterUniqueReferenceId = "BADEBT-P",
                    ParameterCategory = "BadDebt",
                    ParameterType = "Bad debt provision percentage",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999.99m
                },
                new DefaultParameterTemplateMaster
                {
                    ParameterUniqueReferenceId = "MATT-AD",
                    ParameterCategory = "Amount Decrease",
                    ParameterType = "Materiality threshold",
                    ValidRangeFrom =  -999999999.99m,
                    ValidRangeTo = 0m
                },
                new DefaultParameterTemplateMaster
                {
                    ParameterUniqueReferenceId = "MATT-PI",
                    ParameterCategory = "Percent Increase",
                    ParameterType = "Materiality threshold",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999.99m
                },
                new DefaultParameterTemplateMaster
                {
                    ParameterUniqueReferenceId = "MATT-PD",
                    ParameterCategory = "Percent Decrease",
                    ParameterType = "Materiality threshold",
                    ValidRangeFrom = -999.99m,
                    ValidRangeTo = 0m
                },
                new DefaultParameterTemplateMaster
                {
                    ParameterUniqueReferenceId = "TONT-AI",
                    ParameterCategory = "Amount Increase",
                    ParameterType = "Tonnage change threshold",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m
                }
            };

        private string GetInvalidValueForUniqueRef(string parameterUniqueReferenceId)
        {
            switch (parameterUniqueReferenceId)
            {
                case "COMC-AL":
                    return "-1";
                case "BADEBT-P":
                    return "-1";
                case "MATT-AD":
                    return "1";
                case "MATT-PI":
                    return "1000";
                case "MATT-PD":
                    return "-1000";
                case "TONT-AI":
                    return "-1";
                default:
                    return "0";
            }
        }

        [TestInitialize]
        public void Initialise()
        {
            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: "PayCal")
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            this.context = new ApplicationDBContext(dbContextOptions);
            this.context.DefaultParameterTemplateMasterList.AddRange(this.data);
            this.context.SaveChanges();
            this.context.Database.EnsureCreated();
            this.validator = new CreateDefaultParameterDataValidator(context);
        }

        [TestCleanup]
        public void TearDown()
        {
            this.context.Database.EnsureDeleted();
        }

        [TestMethod]
        public void ValidateTest_For_Missing_Unique_References()
        {
            var schemeParameterTemplateValues = new List<SchemeParameterTemplateValueDto>();
            var dto = new CreateDefaultParameterSettingDto
            {
                ParameterYear = "2024-25",
                SchemeParameterTemplateValues = schemeParameterTemplateValues
            };
            var vr = this.validator.Validate(dto);
            Assert.IsTrue(vr.Errors.Count(error => error.Message.Contains("Enter the")) == this.data.Count);
        }

        [TestMethod]
        public void ValidateTest_For_Unique_References_More_Than_One()
        {
            var schemeParameterTemplateValues = new List<SchemeParameterTemplateValueDto>();

            schemeParameterTemplateValues.Add(new SchemeParameterTemplateValueDto
            {
                ParameterUniqueReferenceId = "COMC-AL",
                ParameterValue = "0"
            });
            schemeParameterTemplateValues.Add(new SchemeParameterTemplateValueDto
            {
                ParameterUniqueReferenceId = "COMC-AL",
                ParameterValue = "0"
            });
            var dto = new CreateDefaultParameterSettingDto
            {
                ParameterYear = "2024-25",
                SchemeParameterTemplateValues = schemeParameterTemplateValues
            };
            var vr = this.validator.Validate(dto);
            Assert.IsTrue(vr.Errors.Count(error => error.Message.Contains("Expecting only One with Parameter Type")) == 1);
        }


        [TestMethod]
        public void ValidateTest_For_Invalid_Format()
        {
            var schemeParameterTemplateValues = new List<SchemeParameterTemplateValueDto>();

            foreach (var item in data)
            {
                schemeParameterTemplateValues.Add(new SchemeParameterTemplateValueDto
                {
                    ParameterUniqueReferenceId = item.ParameterUniqueReferenceId,
                    ParameterValue = "**"
                });
            }

            var dto = new CreateDefaultParameterSettingDto
            {
                ParameterYear = "2024-25",
                SchemeParameterTemplateValues = schemeParameterTemplateValues
            };
            var vr = this.validator.Validate(dto);
            Assert.IsTrue(vr.Errors.Count(error => error.Message.Contains("Communication costs for Aluminium can only include numbers, commas and decimal points")) == 1);
            Assert.IsTrue(vr.Errors.Count(error => error.Message.Contains("The Bad debt provision percentage percentage increase can only include numbers, commas, decimal points and a percentage symbol (%)")) == 1);
            Assert.IsTrue(vr.Errors.Count(error => error.Message.Contains("Materiality threshold for Amount Decrease can only include numbers, commas and decimal points")) == 1);
            Assert.IsTrue(vr.Errors.Count(error => error.Message.Contains("The Materiality threshold percentage increase can only include numbers, commas, decimal points and a percentage symbol (%)")) == 1);
            Assert.IsTrue(vr.Errors.Count(error => error.Message.Contains("The Materiality threshold percentage decrease can only include numbers, commas, decimal points and a percentage symbol (%)")) == 1);
            Assert.IsTrue(vr.Errors.Count(error => error.Message.Contains("Tonnage change threshold for Amount Increase can only include numbers, commas and decimal points")) == 1);
        }

        [TestMethod]
        public void ValidateTest_For_Unique_References_Invalid_Values()
        {
            var schemeParameterTemplateValues = new List<SchemeParameterTemplateValueDto>();
            foreach (var item in data)
            {
                schemeParameterTemplateValues.Add(new SchemeParameterTemplateValueDto 
                { 
                    ParameterUniqueReferenceId = item.ParameterUniqueReferenceId, 
                    ParameterValue = GetInvalidValueForUniqueRef(item.ParameterUniqueReferenceId)
                });
            }

            var dto = new CreateDefaultParameterSettingDto
            {
                ParameterYear = "2024-25",
                SchemeParameterTemplateValues = schemeParameterTemplateValues
            };

            var vr = this.validator.Validate(dto);
            Assert.IsTrue(vr.Errors.Count(error => error.Message.Contains("must be between")) == 6);
            Assert.IsTrue(vr.Errors.Count(error => error.Message.Contains("Communication costs for Aluminium must be between £0.00 and £999,999,999.99")) == 1);
            Assert.IsTrue(vr.Errors.Count(error => error.Message.Contains("The Bad debt provision percentage")) == 1);
            Assert.IsTrue(vr.Errors.Count(error => error.Message.Contains("Materiality threshold for Amount Decrease must be between -£999,999,999.99 and £0.00")) == 1);
            Assert.IsTrue(vr.Errors.Count(error => error.Message.Contains("The Materiality threshold percentage increase must be between 0% and 999.99%")) == 1);
            Assert.IsTrue(vr.Errors.Count(error => error.Message.Contains("The Materiality threshold percentage decrease must be between -999.99% and 0%")) == 1);
            Assert.IsTrue(vr.Errors.Count(error => error.Message.Contains("Tonnage change threshold for Amount Increase must be between £0.00 and £999,999,999.99")) == 1);
        }
    }
}
