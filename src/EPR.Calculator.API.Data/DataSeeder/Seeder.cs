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
            InitializeLapcapData(modelBuilder);
        }

        public static void InitializeDefaultParameterTemplateMaster(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DefaultParameterTemplateMaster>().HasData(
                new DefaultParameterTemplateMaster
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
            modelBuilder.Entity<CalculatorRunClassification>().HasData(
                new CalculatorRunClassification
                {
                    Id = 1,
                    Status = "IN THE QUEUE",
                    CreatedBy = "Test User",
                },
                new CalculatorRunClassification
                {
                    Id = 2,
                    Status = "RUNNING",
                    CreatedBy = "Test User",
                },
                new CalculatorRunClassification
                {
                    Id = 3,
                    Status = "UNCLASSIFIED",
                    CreatedBy = "Test User",
                },
                new CalculatorRunClassification
                {
                    Id = 4,
                    Status = "PLAY",
                    CreatedBy = "Test User",
                },
                new CalculatorRunClassification
                {
                    Id = 5,
                    Status = "ERROR",
                    CreatedBy = "Test User",
                });
        }

        public static void InitializeLapcapData(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LapcapDataTemplateMaster>().HasData(
                new LapcapDataTemplateMaster
                {
                    UniqueReference = "ENG-AL",
                    Country = "England",
                    Material = "Aluminium",
                    TotalCostFrom = 0M,
                    TotalCostTo = 999999999.99M,
                },
                new LapcapDataTemplateMaster
                {
                    UniqueReference = "ENG-FC",
                    Country = "England",
                    Material = "Fibre composite",
                    TotalCostFrom = 0M,
                    TotalCostTo = 999999999.99M,
                },
                new LapcapDataTemplateMaster
                {
                    UniqueReference = "ENG-GL",
                    Country = "England",
                    Material = "Glass",
                    TotalCostFrom = 0M,
                    TotalCostTo = 999999999.99M,
                },
                new LapcapDataTemplateMaster
                {
                    UniqueReference = "ENG-PC",
                    Country = "England",
                    Material = "Paper or card",
                    TotalCostFrom = 0M,
                    TotalCostTo = 999999999.99M,
                },
                new LapcapDataTemplateMaster
                {
                    UniqueReference = "ENG-PL",
                    Country = "England",
                    Material = "Plastic",
                    TotalCostFrom = 0M,
                    TotalCostTo = 999999999.99M,
                },
                new LapcapDataTemplateMaster
                {
                    UniqueReference = "ENG-ST",
                    Country = "England",
                    Material = "Steel",
                    TotalCostFrom = 0M,
                    TotalCostTo = 999999999.99M,
                },
                new LapcapDataTemplateMaster
                {
                    UniqueReference = "ENG-WD",
                    Country = "England",
                    Material = "Wood",
                    TotalCostFrom = 0M,
                    TotalCostTo = 999999999.99M,
                },
                new LapcapDataTemplateMaster
                {
                    UniqueReference = "ENG-OT",
                    Country = "England",
                    Material = "Other",
                    TotalCostFrom = 0M,
                    TotalCostTo = 999999999.99M,
                },
                new LapcapDataTemplateMaster
                {
                    UniqueReference = "NI-AL",
                    Country = "Northern Ireland",
                    Material = "Aluminium",
                    TotalCostFrom = 0M,
                    TotalCostTo = 999999999.99M,
                },
                new LapcapDataTemplateMaster
                {
                    UniqueReference = "NI-FC",
                    Country = "Northern Ireland",
                    Material = "Fibre composite",
                    TotalCostFrom = 0M,
                    TotalCostTo = 999999999.99M,
                },
                new LapcapDataTemplateMaster
                {
                    UniqueReference = "NI-GL",
                    Country = "Northern Ireland",
                    Material = "Glass",
                    TotalCostFrom = 0M,
                    TotalCostTo = 999999999.99M,
                },
                new LapcapDataTemplateMaster
                {
                    UniqueReference = "NI-PC",
                    Country = "Northern Ireland",
                    Material = "Paper or card",
                    TotalCostFrom = 0M,
                    TotalCostTo = 999999999.99M,
                },
                new LapcapDataTemplateMaster
                {
                    UniqueReference = "NI-PL",
                    Country = "Northern Ireland",
                    Material = "Plastic",
                    TotalCostFrom = 0M,
                    TotalCostTo = 999999999.99M,
                },
                new LapcapDataTemplateMaster
                {
                    UniqueReference = "NI-ST",
                    Country = "Northern Ireland",
                    Material = "Steel",
                    TotalCostFrom = 0M,
                    TotalCostTo = 999999999.99M,
                },
                new LapcapDataTemplateMaster
                {
                    UniqueReference = "NI-WD",
                    Country = "Northern Ireland",
                    Material = "Wood",
                    TotalCostFrom = 0M,
                    TotalCostTo = 999999999.99M,
                },
                new LapcapDataTemplateMaster
                {
                    UniqueReference = "NI-OT",
                    Country = "Northern Ireland",
                    Material = "Other",
                    TotalCostFrom = 0M,
                    TotalCostTo = 999999999.99M,
                },
                new LapcapDataTemplateMaster
                {
                    UniqueReference = "SCT-AL",
                    Country = "Scotland",
                    Material = "Aluminium",
                    TotalCostFrom = 0M,
                    TotalCostTo = 999999999.99M,
                },
                new LapcapDataTemplateMaster
                {
                    UniqueReference = "SCT-FC",
                    Country = "Scotland",
                    Material = "Fibre composite",
                    TotalCostFrom = 0M,
                    TotalCostTo = 999999999.99M,
                },
                new LapcapDataTemplateMaster
                {
                    UniqueReference = "SCT-GL",
                    Country = "Scotland",
                    Material = "Glass",
                    TotalCostFrom = 0M,
                    TotalCostTo = 999999999.99M,
                },
                new LapcapDataTemplateMaster
                {
                    UniqueReference = "SCT-PC",
                    Country = "Scotland",
                    Material = "Paper or card",
                    TotalCostFrom = 0M,
                    TotalCostTo = 999999999.99M,
                },
                new LapcapDataTemplateMaster
                {
                    UniqueReference = "SCT-PL",
                    Country = "Scotland",
                    Material = "Plastic",
                    TotalCostFrom = 0M,
                    TotalCostTo = 999999999.99M,
                },
                new LapcapDataTemplateMaster
                {
                    UniqueReference = "SCT-ST",
                    Country = "Scotland",
                    Material = "Steel",
                    TotalCostFrom = 0M,
                    TotalCostTo = 999999999.99M,
                },
                new LapcapDataTemplateMaster
                {
                    UniqueReference = "SCT-WD",
                    Country = "Scotland",
                    Material = "Wood",
                    TotalCostFrom = 0M,
                    TotalCostTo = 999999999.99M,
                },
                new LapcapDataTemplateMaster
                {
                    UniqueReference = "SCT-OT",
                    Country = "Scotland",
                    Material = "Other",
                    TotalCostFrom = 0M,
                    TotalCostTo = 999999999.99M,
                },
                new LapcapDataTemplateMaster
                {
                    UniqueReference = "WLS-AL",
                    Country = "Wales",
                    Material = "Aluminium",
                    TotalCostFrom = 0M,
                    TotalCostTo = 999999999.99M,
                },
                new LapcapDataTemplateMaster
                {
                    UniqueReference = "WLS-FC",
                    Country = "Wales",
                    Material = "Fibre composite",
                    TotalCostFrom = 0M,
                    TotalCostTo = 999999999.99M,
                },
                new LapcapDataTemplateMaster
                {
                    UniqueReference = "WLS-GL",
                    Country = "Wales",
                    Material = "Glass",
                    TotalCostFrom = 0M,
                    TotalCostTo = 999999999.99M,
                },
                new LapcapDataTemplateMaster
                {
                    UniqueReference = "WLS-PC",
                    Country = "Wales",
                    Material = "Paper or card",
                    TotalCostFrom = 0M,
                    TotalCostTo = 999999999.99M,
                },
                new LapcapDataTemplateMaster
                {
                    UniqueReference = "WLS-PL",
                    Country = "Wales",
                    Material = "Plastic",
                    TotalCostFrom = 0M,
                    TotalCostTo = 999999999.99M,
                },
                new LapcapDataTemplateMaster
                {
                    UniqueReference = "WLS-ST",
                    Country = "Wales",
                    Material = "Steel",
                    TotalCostFrom = 0M,
                    TotalCostTo = 999999999.99M,
                },
                new LapcapDataTemplateMaster
                {
                    UniqueReference = "WLS-WD",
                    Country = "Wales",
                    Material = "Wood",
                    TotalCostFrom = 0M,
                    TotalCostTo = 999999999.99M,
                },
                new LapcapDataTemplateMaster
                {
                    UniqueReference = "WLS-OT",
                    Country = "Wales",
                    Material = "Other",
                    TotalCostFrom = 0M,
                    TotalCostTo = 999999999.99M,
                });
        }
    }
}
