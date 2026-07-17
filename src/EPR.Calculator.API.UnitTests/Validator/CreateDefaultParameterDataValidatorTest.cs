using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.DataTypes;
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
                new DefaultParameterTemplateMaster
                {
                    ParameterUniqueReferenceId = "LRET-AL",
                    ParameterCategory = "Aluminium",
                    ParameterType = "Late reporting tonnage",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.999m,
                },
                new DefaultParameterTemplateMaster
                {
                    ParameterUniqueReferenceId = "REDM-RF",
                    ParameterType = "Red modulation factor",
                    ParameterCategory = "Modulation Factor",
                    ValidRangeFrom = 1.000m,
                    ValidRangeTo = 2.000m,
                },
                new DefaultParameterTemplateMaster
                {
                    ParameterUniqueReferenceId = "COFF-DT",
                    ParameterCategory = "Optional Date",
                    ParameterType = "Cut-off date",
                }
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
                RelativeYear = new RelativeYear(2024),
                SchemeParameterTemplateValues = schemeParameterTemplateValues,
                ParameterFileName = "TestFileName",
            };
            var vr = Validator.Validate(dto);
            Assert.AreEqual(Data.Count, vr.Errors.Count);
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
                RelativeYear = new RelativeYear(2024),
                SchemeParameterTemplateValues = schemeParameterTemplateValues,
                ParameterFileName = "TestFileName",
            };
            var vr = Validator.Validate(dto);
            Assert.AreEqual(1, vr.Errors.Count(error => error.Message.Contains("duplicate")));
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
                RelativeYear = new RelativeYear(2024),
                SchemeParameterTemplateValues = schemeParameterTemplateValues,
                ParameterFileName = "TestFileName",
            };

            CollectionAssert.AreEquivalent(
                new[]
                {
                    "The parameter COMC-AL can only include numbers, commas and decimal points.",
                    "The parameter BADEBT-P can only include numbers, commas, decimal points and a percentage symbol (%).",
                    "The parameter MATT-AD can only include numbers, commas and decimal points.",
                    "The parameter MATT-PI can only include numbers, commas, decimal points and a percentage symbol (%).",
                    "The parameter MATT-PD can only include numbers, commas, decimal points and a percentage symbol (%).",
                    "The parameter TONT-AI can only include numbers, commas and decimal points.",
                    "The parameter REDM-RF can only include numbers, commas and decimal points.",
                    "The parameter LRET-AL can only include numbers, commas and decimal points.",
                    "The parameter COFF-DT value is invalid. Enter a valid date or 'NA'."
                },
                Validator.Validate(dto).Errors.Select(x => x.Message).ToList());
        }

        [TestMethod]
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
                RelativeYear = new RelativeYear(2024),
                SchemeParameterTemplateValues = schemeParameterTemplateValues,
                ParameterFileName = "TestFileName",
            };


            Validator.Validate(dto).Errors.Select(x => x.Message).ToList().ForEach(x => Console.WriteLine(x));

            CollectionAssert.AreEquivalent(
                new[]
                {

                    "The parameter COMC-AL must be between £0 and £999999999.99.",
                    "The parameter BADEBT-P must be between 0% and 999.99%.",
                    "The parameter MATT-AD must be between £-999999999.99 and £0.",
                    "The parameter MATT-PI must be between 0% and 999.99%.",
                    "The parameter MATT-PD must be between -999.99% and 0%.",
                    "The parameter TONT-AI must be between £0 and £999999999.99.",
                    "The parameter REDM-RF must be between 1.000 and 2.000.",
                    "The parameter LRET-AL must be between 0 and 999999999.999 tons."
                },
                Validator.Validate(dto).Errors.Select(x => x.Message).ToList());
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
                "REDM-RF" => "2.500",
                "COFF-DT" => "01/01/2000", // TODO has not min/max?
                "LRET-AL" => "1,000,000,000.00",
                _ => "0",
            };
        }
    }
}
