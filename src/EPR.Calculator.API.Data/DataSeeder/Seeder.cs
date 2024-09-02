using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.Data.DataModels;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.Data.DataSeeder
{
    [ExcludeFromCodeCoverage]
    public static class Seeder
    {
        public static void Initialize(ModelBuilder modelBuilder)
        {
            InitializeDefaultParameterTemplateMaster(modelBuilder);
            InitializeCalculatorRunClassification(modelBuilder);
            InitializeCalculatorRuns(modelBuilder);
        }

        public static void InitializeDefaultParameterTemplateMaster(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DefaultParameterTemplateMaster>().HasData(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "COMC-AL",
                ParameterCategory = "Communication costs",
                ParameterType = "Aluminium",
                ValidRangeFrom = 0,
                ValidRangeTo = 999999999.99M,
            },
            new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "COMC-FC",
                ParameterCategory = "Communication costs",
                ParameterType = "Fibre composite",
                ValidRangeFrom = 0,
                ValidRangeTo = 999999999.99M,
            },
            new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "COMC-GL",
                ParameterCategory = "Communication costs",
                ParameterType = "Glass",
                ValidRangeFrom = 0,
                ValidRangeTo = 999999999.99M,
            },
            new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "COMC-PC",
                ParameterCategory = "Communication costs",
                ParameterType = "Paper or card",
                ValidRangeFrom = 0,
                ValidRangeTo = 999999999.99M,
            },
            new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "COMC-PL",
                ParameterCategory = "Communication costs",
                ParameterType = "Plastic",
                ValidRangeFrom = 0,
                ValidRangeTo = 999999999.99M,
            },
            new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "COMC-ST",
                ParameterCategory = "Communication costs",
                ParameterType = "Steel",
                ValidRangeFrom = 0,
                ValidRangeTo = 999999999.99M,
            },
            new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "COMC-WD",
                ParameterCategory = "Communication costs",
                ParameterType = "Wood",
                ValidRangeFrom = 0,
                ValidRangeTo = 999999999.99M,
            },
            new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "COMC-OT",
                ParameterCategory = "Communication costs",
                ParameterType = "Other",
                ValidRangeFrom = 0,
                ValidRangeTo = 999999999.99M,
            },
            new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "SAOC-ENG",
                ParameterCategory = "Scheme administrator operating costs",
                ParameterType = "England",
                ValidRangeFrom = 0,
                ValidRangeTo = 999999999.99M,
            },
            new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "SAOC-WLS",
                ParameterCategory = "Scheme administrator operating costs",
                ParameterType = "Wales",
                ValidRangeFrom = 0,
                ValidRangeTo = 999999999.99M,
            },
            new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "SAOC-SCT",
                ParameterCategory = "Scheme administrator operating costs",
                ParameterType = "Scotland",
                ValidRangeFrom = 0,
                ValidRangeTo = 999999999.99M,
            },
            new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "SAOC-NIR",
                ParameterCategory = "Scheme administrator operating costs",
                ParameterType = "Northern Ireland",
                ValidRangeFrom = 0,
                ValidRangeTo = 999999999.99M,
            },
            new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "LAPC-ENG",
                ParameterCategory = "Local authority data preparation costs",
                ParameterType = "England",
                ValidRangeFrom = 0,
                ValidRangeTo = 999999999.99M,
            },
            new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "LAPC-WLS",
                ParameterCategory = "Local authority data preparation costs",
                ParameterType = "Wales",
                ValidRangeFrom = 0,
                ValidRangeTo = 999999999.99M,
            },
            new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "LAPC-SCT",
                ParameterCategory = "Local authority data preparation costs",
                ParameterType = "Scotland",
                ValidRangeFrom = 0,
                ValidRangeTo = 999999999.99M,
            },
            new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "LAPC-NIR",
                ParameterCategory = "Local authority data preparation costs",
                ParameterType = "Northern Ireland",
                ValidRangeFrom = 0,
                ValidRangeTo = 999999999.99M,
            },
            new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "SCSC-ENG",
                ParameterCategory = "Scheme setup costs",
                ParameterType = "England",
                ValidRangeFrom = 0,
                ValidRangeTo = 999999999.99M,
            },
            new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "SCSC-WLS",
                ParameterCategory = "Scheme setup costs",
                ParameterType = "Wales",
                ValidRangeFrom = 0,
                ValidRangeTo = 999999999.99M,
            },
            new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "SCSC-SCT",
                ParameterCategory = "Scheme setup costs",
                ParameterType = "Scotland",
                ValidRangeFrom = 0,
                ValidRangeTo = 999999999.99M,
            },
            new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "SCSC-NIR",
                ParameterCategory = "Scheme setup costs",
                ParameterType = "Northern Ireland",
                ValidRangeFrom = 0,
                ValidRangeTo = 999999999.99M,
            },
            new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "LRET-AL",
                ParameterCategory = "Late reporting tonnage",
                ParameterType = "Aluminium",
                ValidRangeFrom = 0,
                ValidRangeTo = 999999999.99M,
            },
            new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "LRET-FC",
                ParameterCategory = "Late reporting tonnage",
                ParameterType = "Aluminium",
                ValidRangeFrom = 0,
                ValidRangeTo = 999999999.99M,
            },
            new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "LRET-GL",
                ParameterCategory = "Late reporting tonnage",
                ParameterType = "Aluminium",
                ValidRangeFrom = 0,
                ValidRangeTo = 999999999.99M,
            },
            new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "LRET-PC",
                ParameterCategory = "Late reporting tonnage",
                ParameterType = "Aluminium",
                ValidRangeFrom = 0,
                ValidRangeTo = 999999999.99M,
            },
            new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "LRET-PL",
                ParameterCategory = "Late reporting tonnage",
                ParameterType = "Aluminium",
                ValidRangeFrom = 0,
                ValidRangeTo = 999999999.99M,
            },
            new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "LRET-ST",
                ParameterCategory = "Late reporting tonnage",
                ParameterType = "Aluminium",
                ValidRangeFrom = 0,
                ValidRangeTo = 999999999.99M,
            },
            new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "LRET-WD",
                ParameterCategory = "Late reporting tonnage",
                ParameterType = "Aluminium",
                ValidRangeFrom = 0,
                ValidRangeTo = 999999999.99M,
            },
            new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "LRET-OT",
                ParameterCategory = "Late reporting tonnage",
                ParameterType = "Aluminium",
                ValidRangeFrom = 0,
                ValidRangeTo = 999999999.99M,
            },
            new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "BADEBT-P",
                ParameterCategory = "Communication costs",
                ParameterType = "Aluminium",
                ValidRangeFrom = 0,
                ValidRangeTo = 999999999.99M,
            },
            new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "MATT-AI",
                ParameterCategory = "Materiality threshold",
                ParameterType = "Amount Increase",
                ValidRangeFrom = 0,
                ValidRangeTo = 999999999.99M,
            },
            new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "MATT-AD",
                ParameterCategory = "Materiality threshold",
                ParameterType = "Amount Decrease",
                ValidRangeFrom = 0,
                ValidRangeTo = 999999999.99M,
            },
            new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "MATT-PI",
                ParameterCategory = "Materiality threshold",
                ParameterType = "Percent Increase",
                ValidRangeFrom = 0,
                ValidRangeTo = 1000.00M,
            },
            new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "MATT-PD",
                ParameterCategory = "Materiality threshold",
                ParameterType = "Percent Decrease",
                ValidRangeFrom = 0,
                ValidRangeTo = -1000.00M,
            },
            new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "TONT-AI",
                ParameterCategory = "Tonnage change threshold",
                ParameterType = "Amount Increase",
                ValidRangeFrom = 0,
                ValidRangeTo = 999999999.99M,
            },
            new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "TONT-DI",
                ParameterCategory = "Tonnage change threshold",
                ParameterType = "Amount Decrease",
                ValidRangeFrom = 0,
                ValidRangeTo = 999999999.99M,
            },
            new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "TONT-PI",
                ParameterCategory = "Tonnage change threshold",
                ParameterType = "Percent Increase",
                ValidRangeFrom = 0,
                ValidRangeTo = 1000.00M,
            },
            new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "TONT-PD",
                ParameterCategory = "Tonnage change threshold",
                ParameterType = "Percent Decrease",
                ValidRangeFrom = 0,
                ValidRangeTo = -1000.00M,
            },
            new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "LEVY-ENG",
                ParameterCategory = "Levy",
                ParameterType = "England",
                ValidRangeFrom = 0,
                ValidRangeTo = 999999999.99M,
            },
            new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "LEVY-WLS",
                ParameterCategory = "Levy",
                ParameterType = "Wales",
                ValidRangeFrom = 0,
                ValidRangeTo = 999999999.99M,
            },
            new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "LEVY-SCT",
                ParameterCategory = "Levy",
                ParameterType = "Scotland",
                ValidRangeFrom = 0,
                ValidRangeTo = 999999999.99M,
            },
            new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "LEVY-NIR",
                ParameterCategory = "Levy",
                ParameterType = "Northern Ireland",
                ValidRangeFrom = 0,
                ValidRangeTo = 999999999.99M,
            });
        }

        public static void InitializeCalculatorRunClassification(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CalculatorRunClassification>().HasData(new CalculatorRunClassification
            {
                Id = 1,
                Status = "IN THE QUEUE",
                CreatedBy = "Test User",
                CreatedAt = DateTime.Now
            },
            new CalculatorRunClassification
            {
                Id = 2,
                Status = "RUNNING",
                CreatedBy = "Test User",
                CreatedAt = DateTime.Now
            },
            new CalculatorRunClassification
            {
                Id = 3,
                Status = "UNCLASSIFIED",
                CreatedBy = "Test User",
                CreatedAt = DateTime.Now
            },
            new CalculatorRunClassification
            {
                Id = 4,
                Status = "PLAY",
                CreatedBy = "Test User",
                CreatedAt = DateTime.Now
            },
            new CalculatorRunClassification
            {
                Id = 5,
                Status = "ERROR",
                CreatedBy = "Test User",
                CreatedAt = DateTime.Now
            });
        }

        // TODO: The below method is to store the mock data and should be deleted after the create calculation run API is implemented
        public static void InitializeCalculatorRuns(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CalculatorRun>().HasData(new CalculatorRun
            {
                Id = 1,
                Name = "Default settings check",
                CalculatorRunClassificationId = 1,
                Financial_Year = "2024-25",
                CreatedBy = "Test User",
                CreatedAt = new DateTime(2025, 8, 28, 10, 01, 0, DateTimeKind.Utc)
            },
            new CalculatorRun {
                Id = 2,
                Name = "Alteration check",
                CalculatorRunClassificationId = 2,
                Financial_Year = "2024-25",
                CreatedBy = "Test User",
                CreatedAt = new DateTime(2025, 8, 21, 12, 9, 0, DateTimeKind.Utc)
            },
            new CalculatorRun
            {
                Id = 3,
                Name = "Test 10",
                CalculatorRunClassificationId = 3,
                Financial_Year = "2024-25",
                CreatedBy = "Test User",
                CreatedAt = new DateTime(2025, 5, 11, 9, 14, 0, DateTimeKind.Utc)
            },
            new CalculatorRun
            {
                Id = 4,
                Name = "June check",
                CalculatorRunClassificationId = 4,
                Financial_Year = "2024-25",
                CreatedBy = "Test User",
                CreatedAt = new DateTime(2025, 5, 13, 11, 18, 0, DateTimeKind.Utc)
            },
            new CalculatorRun
            {
                Id = 5,
                Name = "Pre June check",
                CalculatorRunClassificationId = 4,
                Financial_Year = "2024-25",
                CreatedBy = "Test User",
                CreatedAt = new DateTime(2025, 5, 10, 8, 13, 0, DateTimeKind.Utc)
            },
            new CalculatorRun
            {
                Id = 6,
                Name = "Local Authority data check 5",
                CalculatorRunClassificationId = 4,
                Financial_Year = "2024-25",
                CreatedBy = "Test User",
                CreatedAt = new DateTime(2025, 5, 8, 10, 0, 0, DateTimeKind.Utc)
            },
            new CalculatorRun
            {
                Id = 7,
                Name = "Local Authority data check 4",
                CalculatorRunClassificationId = 4,
                Financial_Year = "2024-25",
                CreatedBy = "Test User",
                CreatedAt = new DateTime(2025, 5, 7, 11, 20, 0, DateTimeKind.Utc)
            },
            new CalculatorRun
            {
                Id = 8,
                Name = "Local Authority data check 3",
                CalculatorRunClassificationId = 4,
                Financial_Year = "2024-25",
                CreatedBy = "Test User",
                CreatedAt = new DateTime(2025, 6, 24, 14, 29, 0, DateTimeKind.Utc)
            },
            new CalculatorRun
            {
                Id = 9,
                Name = "Local Authority data check 2",
                CalculatorRunClassificationId = 4,
                Financial_Year = "2024-25",
                CreatedBy = "Test User",
                CreatedAt = new DateTime(2025, 6, 27, 16, 39, 12, DateTimeKind.Utc)
            },
            new CalculatorRun
            {
                Id = 10,
                Name = "Local Authority data check",
                CalculatorRunClassificationId = 4,
                Financial_Year = "2024-25",
                CreatedBy = "Test User",
                CreatedAt = new DateTime(2025, 6, 14, 17, 6, 26, DateTimeKind.Utc)
            },
            new CalculatorRun
            {
                Id = 11,
                Name = "Fee adjustment check",
                CalculatorRunClassificationId = 5,
                Financial_Year = "2024-25",
                CreatedBy = "Test User",
                CreatedAt = new DateTime(2025, 5, 1, 9, 12, 0, DateTimeKind.Utc)
            });
        }
    }
}
