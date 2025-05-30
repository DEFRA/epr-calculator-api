using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.Calculator.API.UnitTests
{
    /// <summary>
    /// Provides an in-memory implementation of the <see cref="ApplicationDBContext"/> for unit testing purposes.
    /// </summary>
    [TestClass]
    public abstract class InMemoryApplicationDbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryApplicationDbContext"/> class.
        /// Sets up an in-memory database and ensures it is created.
        /// </summary>
        protected InMemoryApplicationDbContext()
        {
            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
                                       .UseInMemoryDatabase(databaseName: "PayCal")
                                       .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                                       .Options;

            this.DbContext = new ApplicationDBContext(dbContextOptions);

            this.DbContext.Database.EnsureCreated();

            if (!this.DbContext.FinancialYears.Any())
            {
                this.FinancialYear24_25 = new CalculatorRunFinancialYear { Name = "2024-25" };
                this.DbContext.FinancialYears.Add(this.FinancialYear24_25);
                this.DbContext.SaveChanges();
            }
            else
            {
                this.FinancialYear24_25 = this.DbContext.FinancialYears.First(x => x.Name == "2024-25");
            }

            if (!this.DbContext.CalculatorRuns.Any())
            {
                this.DbContext.CalculatorRuns.AddRange(this.GetCalculatorRuns());
                this.DbContext.SaveChanges();
            }

            if (!this.DbContext.ProducerResultFileSuggestedBillingInstruction.Any())
            {
                this.DbContext.ProducerResultFileSuggestedBillingInstruction.AddRange(GetProducerResultFileSuggestedBillingInstruction());
                this.DbContext.SaveChanges();
            }
        }

        /// <summary>
        /// Gets or sets the in-memory database context used for testing.
        /// </summary>
        protected ApplicationDBContext DbContext { get; set; }

        /// <summary>
        /// Gets the financial year 2024-2025 used for testing.
        /// </summary>
        protected CalculatorRunFinancialYear FinancialYear24_25 { get; init; }

        /// <summary>
        /// Creates a list of predefined <see cref="CalculatorRun"/> objects for testing purposes.
        /// </summary>
        /// <returns>A list of <see cref="CalculatorRun"/> objects.</returns>
        private List<CalculatorRun> GetCalculatorRuns()
        {
            var calculatorRuns = new List<CalculatorRun>
                {
                    new()
                    {
                        CalculatorRunClassificationId = (int)RunClassification.INITIAL_RUN,
                        Name = "Test Run",
                        Financial_Year = this.FinancialYear24_25,
                        CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc),
                        CreatedBy = "Test User",
                    },
                    new()
                    {
                        CalculatorRunClassificationId = (int)RunClassification.INTHEQUEUE,
                        Name = "Test Calculated Result",
                        Financial_Year = this.FinancialYear24_25,
                        CreatedAt = new DateTime(2024, 8, 21, 14, 16, 27, DateTimeKind.Utc),
                        CreatedBy = "Test User",
                    },
                    new()
                    {
                        CalculatorRunClassificationId = (int)RunClassification.INTHEQUEUE,
                        Name = "Test Run",
                        Financial_Year = this.FinancialYear24_25,
                        CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc),
                        CreatedBy = "Test User",
                        CalculatorRunOrganisationDataMasterId = 1,
                        CalculatorRunPomDataMasterId = 1,
                    },
                    new()
                    {
                        CalculatorRunClassificationId = (int)RunClassification.INTHEQUEUE,
                        Name = "Test 422 error",
                        Financial_Year = this.FinancialYear24_25,
                        CreatedAt = new DateTime(2024, 8, 21, 14, 16, 27, DateTimeKind.Utc),
                        CreatedBy = "Test User",
                        CalculatorRunOrganisationDataMasterId = 2,
                        CalculatorRunPomDataMasterId = 2,
                        LapcapDataMasterId = 2,
                        DefaultParameterSettingMasterId = 2,
                    },
                    new()
                    {
                        CalculatorRunClassificationId = (int)RunClassification.INTHEQUEUE,
                        Name = "Test Calculated Result",
                        Financial_Year = this.FinancialYear24_25,
                        CreatedAt = new DateTime(2024, 8, 21, 14, 16, 27, DateTimeKind.Utc),
                        CreatedBy = "Test User",
                        CalculatorRunOrganisationDataMasterId = 2,
                        CalculatorRunPomDataMasterId = 2,
                    },
                };

            return calculatorRuns;
        }

        private static List<ProducerResultFileSuggestedBillingInstruction> GetProducerResultFileSuggestedBillingInstruction()
        {
            var rows = new List<ProducerResultFileSuggestedBillingInstruction>
            {
                new()
                {
                    CalculatorRunId = 1,
                    ProducerId = 1,
                    SuggestedBillingInstruction = "Test",
                },
            };
            return rows;
        }
    }
}
