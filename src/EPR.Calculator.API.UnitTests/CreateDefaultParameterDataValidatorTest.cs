using api.Validators;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
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
            Assert.IsTrue(vr.Errors.Count(error => error.Message.Contains("Expecting at least One with")) == this.data.Count);
        }

        [TestMethod]
        public void ValidateTest_For_Unique_References_More_Than_One()
        {
            var schemeParameterTemplateValues = new List<SchemeParameterTemplateValueDto>();

            schemeParameterTemplateValues.Add(new SchemeParameterTemplateValueDto
            {
                ParameterUniqueReferenceId = "COMC-AL",
                ParameterValue = 0
            });
            schemeParameterTemplateValues.Add(new SchemeParameterTemplateValueDto
            {
                ParameterUniqueReferenceId = "COMC-AL",
                ParameterValue = 0
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
        }

        private decimal GetInvalidValueForUniqueRef(string parameterUniqueReferenceId)
        {
            switch (parameterUniqueReferenceId) 
            {
                case "COMC-AL":
                    return -1m;
                case "BADEBT-P":
                    return -1m;
                case "MATT-AD":
                    return 1m;
                case "MATT-PI":
                    return 1000m;
                case "MATT-PD":
                    return -1000m;
                case "TONT-AI":
                    return -1m;
                default:
                    return 0m;
            }
        }
    }
}
