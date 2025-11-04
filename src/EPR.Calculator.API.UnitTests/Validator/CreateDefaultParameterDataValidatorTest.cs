using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Validators;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EPR.Calculator.API.UnitTests.Validator
{
    [TestClass]
    public class CreateDefaultParameterDataValidatorTest
    {
        public CreateDefaultParameterDataValidatorTest()
        {
            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: "PayCal")
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            Context = new ApplicationDBContext(dbContextOptions);
            if (this?.Context?.DefaultParameterTemplateMasterList?.Count() > 0)
            {
                Context.DefaultParameterTemplateMasterList.UpdateRange(Data);
            }
            else
            {
                Context.DefaultParameterTemplateMasterList.AddRange(Data);
            }

            Context.SaveChanges();
            Context.Database.EnsureCreated();
            Validator = new CreateDefaultParameterDataValidator(Context);
        }

        private List<DefaultParameterTemplateMaster> Data { get; } = new List<DefaultParameterTemplateMaster>
            {
                new DefaultParameterTemplateMaster
                {
                    ParameterUniqueReferenceId = "COMC-AL",
                    ParameterCategory = "Aluminium",
                    ParameterType = "Communication costs",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m,
                },
                new DefaultParameterTemplateMaster
                {
                    ParameterUniqueReferenceId = "BADEBT-P",
                    ParameterCategory = "BadDebt",
                    ParameterType = "Bad debt provision percentage",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999.99m,
                },
                new DefaultParameterTemplateMaster
                {
                    ParameterUniqueReferenceId = "MATT-AD",
                    ParameterCategory = "Amount Decrease",
                    ParameterType = "Materiality threshold",
                    ValidRangeFrom = -999999999.99m,
                    ValidRangeTo = 0m,
                },
                new DefaultParameterTemplateMaster
                {
                    ParameterUniqueReferenceId = "MATT-PI",
                    ParameterCategory = "Percent Increase",
                    ParameterType = "Materiality threshold",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999.99m,
                },
                new DefaultParameterTemplateMaster
                {
                    ParameterUniqueReferenceId = "MATT-PD",
                    ParameterCategory = "Percent Decrease",
                    ParameterType = "Materiality threshold",
                    ValidRangeFrom = -999.99m,
                    ValidRangeTo = 0m,
                },
                new DefaultParameterTemplateMaster
                {
                    ParameterUniqueReferenceId = "TONT-AI",
                    ParameterCategory = "Amount Increase",
                    ParameterType = "Tonnage change threshold",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m,
                },
            };

        private ApplicationDBContext Context { get; init; }

        private CreateDefaultParameterDataValidator Validator { get; init; }

        [TestCleanup]
        public void TearDown()
        {
            Context.Database.EnsureDeleted();
        }

        [TestMethod]
        public void ValidateTest_For_Missing_Unique_References()
        {
            var schemeParameterTemplateValues = new List<SchemeParameterTemplateValueDto>();
            var dto = new CreateDefaultParameterSettingDto
            {
                ParameterYear = "2024-25",
                SchemeParameterTemplateValues = schemeParameterTemplateValues,
                ParameterFileName = "TestFileName",
            };
            var vr = Validator.Validate(dto);
            Assert.AreEqual(Data.Count, vr.Errors.Count(error => error.Message.Contains("Enter the")));
        }

        [TestMethod]
        public void ValidateTest_For_Unique_References_More_Than_One()
        {
            var schemeParameterTemplateValues = new List<SchemeParameterTemplateValueDto>
            {
                new SchemeParameterTemplateValueDto
                {
                    ParameterUniqueReferenceId = "COMC-AL",
                    ParameterValue = "0",
                },
                new SchemeParameterTemplateValueDto
                {
                    ParameterUniqueReferenceId = "COMC-AL",
                    ParameterValue = "0",
                },
            };
            var dto = new CreateDefaultParameterSettingDto
            {
                ParameterYear = "2024-25",
                SchemeParameterTemplateValues = schemeParameterTemplateValues,
                ParameterFileName = "TestFileName",
            };
            var vr = Validator.Validate(dto);
            Assert.AreEqual(1, vr.Errors.Count(error => error.Message.Contains("Expecting only One with Parameter Type")));
        }

        [TestMethod]
        public void ValidateTest_For_Invalid_Format()
        {
            var schemeParameterTemplateValues = new List<SchemeParameterTemplateValueDto>();

            foreach (var item in Data)
            {
                schemeParameterTemplateValues.Add(new SchemeParameterTemplateValueDto
                {
                    ParameterUniqueReferenceId = item.ParameterUniqueReferenceId,
                    ParameterValue = "**",
                });
            }

            var dto = new CreateDefaultParameterSettingDto
            {
                ParameterYear = "2024-25",
                SchemeParameterTemplateValues = schemeParameterTemplateValues,
                ParameterFileName = "TestFileName",
            };
            var vr = Validator.Validate(dto);
            Assert.AreEqual(1, vr.Errors.Count(error => error.Message.Contains("Communication costs for Aluminium can only include numbers, commas and decimal points")));
            Assert.AreEqual(1, vr.Errors.Count(error => error.Message.Contains("The Bad debt provision percentage percentage increase can only include numbers, commas, decimal points and a percentage symbol (%)")));
            Assert.AreEqual(1, vr.Errors.Count(error => error.Message.Contains("Materiality threshold for Amount Decrease can only include numbers, commas and decimal points")));
            Assert.AreEqual(1, vr.Errors.Count(error => error.Message.Contains("The Materiality threshold percentage increase can only include numbers, commas, decimal points and a percentage symbol (%)")));
            Assert.AreEqual(1, vr.Errors.Count(error => error.Message.Contains("The Materiality threshold percentage decrease can only include numbers, commas, decimal points and a percentage symbol (%)")));
            Assert.AreEqual(1, vr.Errors.Count(error => error.Message.Contains("Tonnage change threshold for Amount Increase can only include numbers, commas and decimal points")));
        }

        public void ValidateTest_For_Unique_References_Invalid_Values()
        {
            var schemeParameterTemplateValues = new List<SchemeParameterTemplateValueDto>();
            foreach (var item in Data)
            {
                schemeParameterTemplateValues.Add(new SchemeParameterTemplateValueDto
                {
                    ParameterUniqueReferenceId = item.ParameterUniqueReferenceId,
                    ParameterValue = GetInvalidValueForUniqueRef(item.ParameterUniqueReferenceId),
                });
            }

            var dto = new CreateDefaultParameterSettingDto
            {
                ParameterYear = "2024-25",
                SchemeParameterTemplateValues = schemeParameterTemplateValues,
                ParameterFileName = "TestFileName",
            };

            var vr = Validator.Validate(dto);
            Assert.AreEqual(6, vr?.Errors.Count(error => error.Message.Contains("must be between")));
            Assert.AreEqual(1, vr?.Errors.Count(error => error.Message.Contains("Communication costs for Aluminium must be between £0.00 and £999,999,999.99")));
            Assert.AreEqual(1, vr?.Errors.Count(error => error.Message.Contains("The Bad debt provision percentage")));
            Assert.AreEqual(1, vr?.Errors.Count(error => error.Message.Contains("Materiality threshold for Amount Decrease must be between -£999,999,999.99 and £0.00")));
            Assert.AreEqual(1, vr?.Errors.Count(error => error.Message.Contains("The Materiality threshold percentage increase must be between 0% and 999.99%")));
            Assert.AreEqual(1, vr?.Errors.Count(error => error.Message.Contains("The Materiality threshold percentage decrease must be between -999.99% and 0%")));
            Assert.AreEqual(1, vr?.Errors.Count(error => error.Message.Contains("Tonnage change threshold for Amount Increase must be between £0.00 and £999,999,999.99")));
        }

        private static string GetInvalidValueForUniqueRef(string parameterUniqueReferenceId)
        {
            return parameterUniqueReferenceId switch
            {
                "COMC-AL" => "-1",
                "BADEBT-P" => "-1",
                "MATT-AD" => "1",
                "MATT-PI" => "1000",
                "MATT-PD" => "-1000",
                "TONT-AI" => "-1",
                _ => "0",
            };
        }
    }
}
