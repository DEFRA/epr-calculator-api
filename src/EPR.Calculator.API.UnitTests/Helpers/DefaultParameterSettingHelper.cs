using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.API.UnitTests.Helpers
{
    public static class DefaultParameterSettingHelper
    {
        public static IEnumerable<DefaultParameterTemplateMaster> GetDefaultParameterTemplateMasterData()
        {
            var list = new List<DefaultParameterTemplateMaster>
            {
                new()
                {
                    ParameterUniqueReferenceId = "COMC-ENG",
                    ParameterCategory = "England",
                    ParameterType = "Communication costs by country",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m,
                },
                new()
                {
                    ParameterUniqueReferenceId = "COMC-NIR",
                    ParameterCategory = "Northern Ireland",
                    ParameterType = "Communication costs by country",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m,
                },
                new()
                {
                    ParameterUniqueReferenceId = "COMC-SCT",
                    ParameterCategory = "Scotland",
                    ParameterType = "Communication costs by country",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m,
                },
                new()
                {
                    ParameterUniqueReferenceId = "COMC-UK",
                    ParameterCategory = "United Kingdom",
                    ParameterType = "Communication costs by country",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m,
                },
                new()
                {
                    ParameterUniqueReferenceId = "COMC-WLS",
                    ParameterCategory = "Wales",
                    ParameterType = "Communication costs by country",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m,
                },
                new()
                {
                    ParameterUniqueReferenceId = "COMC-AL",
                    ParameterCategory = "Aluminium",
                    ParameterType = "Communication costs by material",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m,
                },
                new()
                {
                    ParameterUniqueReferenceId = "COMC-FC",
                    ParameterCategory = "Fibre composite",
                    ParameterType = "Communication costs by material",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m,
                },
                new()
                {
                    ParameterUniqueReferenceId = "COMC-GL",
                    ParameterCategory = "Glass",
                    ParameterType = "Communication costs by material",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m,
                },
                new()
                {
                    ParameterUniqueReferenceId = "COMC-PC",
                    ParameterCategory = "Paper or card",
                    ParameterType = "Communication costs by material",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m,
                },
                new()
                {
                    ParameterUniqueReferenceId = "COMC-PL",
                    ParameterCategory = "Plastic",
                    ParameterType = "Communication costs by material",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m,
                },
                new()
                {
                    ParameterUniqueReferenceId = "COMC-ST",
                    ParameterCategory = "Steel",
                    ParameterType = "Communication costs by material",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m,
                },
                new()
                {
                    ParameterUniqueReferenceId = "COMC-WD",
                    ParameterCategory = "Wood",
                    ParameterType = "Communication costs by material",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m,
                },
                new()
                {
                    ParameterUniqueReferenceId = "LRET-FC",
                    ParameterCategory = "Fibre composite",
                    ParameterType = "Late reporting tonnage",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m,
                },
                new()
                {
                    ParameterUniqueReferenceId = "LRET-GL",
                    ParameterCategory = "Glass",
                    ParameterType = "Late reporting tonnage",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m,
                },
                new()
                {
                    ParameterUniqueReferenceId = "LRET-AL",
                    ParameterCategory = "Aluminium",
                    ParameterType = "Late reporting tonnage",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m,
                },
                new()
                {
                    ParameterUniqueReferenceId = "LRET-WD",
                    ParameterCategory = "Wood",
                    ParameterType = "Late reporting tonnage",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m,
                },
                new()
                {
                    ParameterUniqueReferenceId = "LRET-ST",
                    ParameterCategory = "Steel",
                    ParameterType = "Late reporting tonnage",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m,
                },
                new()
                {
                    ParameterUniqueReferenceId = "LRET-PC",
                    ParameterCategory = "Paper or card",
                    ParameterType = "Late reporting tonnage",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m,
                },
                new()
                {
                    ParameterUniqueReferenceId = "LAPC-ENG",
                    ParameterCategory = "England",
                    ParameterType = "Local authority data preparation costs",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m,
                },
                new()
                {
                    ParameterUniqueReferenceId = "LAPC-NIR",
                    ParameterCategory = "Northern Ireland",
                    ParameterType = "Local authority data preparation costs",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m,
                },
                new()
                {
                    ParameterUniqueReferenceId = "LAPC-SCT",
                    ParameterCategory = "Scotland",
                    ParameterType = "Local authority data preparation costs",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m,
                },
                new()
                {
                    ParameterUniqueReferenceId = "LAPC-WLS",
                    ParameterCategory = "Wales",
                    ParameterType = "Local authority data preparation costs",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m,
                },
                new()
                {
                    ParameterUniqueReferenceId = "MATT-AD",
                    ParameterCategory = "Amount Decrease",
                    ParameterType = "Materiality threshold",
                    ValidRangeFrom = -999999999.990m,
                    ValidRangeTo = 0.00m,
                },
                new()
                {
                    ParameterUniqueReferenceId = "MATT-AI",
                    ParameterCategory = "Amount Increase",
                    ParameterType = "Materiality threshold",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m,
                },
                new()
                {
                    ParameterUniqueReferenceId = "MATT-PD",
                    ParameterCategory = "Percent Decrease",
                    ParameterType = "Materiality threshold",
                    ValidRangeFrom = -999.990m,
                    ValidRangeTo = 0.00m,
                },
                new()
                {
                    ParameterUniqueReferenceId = "MATT-PI",
                    ParameterCategory = "Percent Increase",
                    ParameterType = "Materiality threshold",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999.990m,
                },
                new()
                {
                    ParameterUniqueReferenceId = "COMC-OT",
                    ParameterCategory = "Other",
                    ParameterType = "Other materials",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m,
                },
                new()
                {
                    ParameterUniqueReferenceId = "LRET-OT",
                    ParameterCategory = "Other materials",
                    ParameterType = "Late reporting tonnage",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m,
                },
                new()
                {
                    ParameterUniqueReferenceId = "BADEBT-P",
                    ParameterCategory = "Bad debt provision",
                    ParameterType = "Percentage",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 1000.000m,
                },
                new()
                {
                    ParameterUniqueReferenceId = "LRET-PL",
                    ParameterCategory = "Plastic",
                    ParameterType = "Late reporting tonnage",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m,
                },
                new()
                {
                    ParameterUniqueReferenceId = "SAOC-ENG",
                    ParameterCategory = "England",
                    ParameterType = "Scheme administrator operating costs",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m,
                },
                new()
                {
                    ParameterUniqueReferenceId = "SAOC-NIR",
                    ParameterCategory = "Northern Ireland",
                    ParameterType = "Scheme administrator operating costs",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m,
                },
                new()
                {
                    ParameterUniqueReferenceId = "SAOC-SCT",
                    ParameterCategory = "Scotland",
                    ParameterType = "Scheme administrator operating costs",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m,
                },
                new()
                {
                    ParameterUniqueReferenceId = "SAOC-WLS",
                    ParameterCategory = "Wales",
                    ParameterType = "Scheme administrator operating costs",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m,
                },
                new()
                {
                    ParameterUniqueReferenceId = "SCSC-ENG",
                    ParameterCategory = "England",
                    ParameterType = "Scheme setup costs",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m,
                },
                new()
                {
                    ParameterUniqueReferenceId = "SCSC-NIR",
                    ParameterCategory = "Northern Ireland",
                    ParameterType = "Scheme setup costs",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m,
                },
                new()
                {
                    ParameterUniqueReferenceId = "SCSC-SCT",
                    ParameterCategory = "Scotland",
                    ParameterType = "Scheme setup costs",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m,
                },
                new()
                {
                    ParameterUniqueReferenceId = "SCSC-WLS",
                    ParameterCategory = "Wales",
                    ParameterType = "Scheme setup costs",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m,
                },
                new()
                {
                    ParameterUniqueReferenceId = "TONT-AD",
                    ParameterCategory = "Amount Decrease",
                    ParameterType = "Tonnage change threshold",
                    ValidRangeFrom = -999999999.990m,
                    ValidRangeTo = 0.00m,
                },
                new()
                {
                    ParameterUniqueReferenceId = "TONT-AI",
                    ParameterCategory = "Amount Increase",
                    ParameterType = "Tonnage change threshold",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m,
                },
                new()
                {
                    ParameterUniqueReferenceId = "TONT-PD",
                    ParameterCategory = "Percent Decrease",
                    ParameterType = "Tonnage change threshold",
                    ValidRangeFrom = -999.990m,
                    ValidRangeTo = 0.00m,
                },
                new()
                {
                    ParameterUniqueReferenceId = "TONT-PI",
                    ParameterCategory = "Percent Increase",
                    ParameterType = "Tonnage change threshold",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999.990m,
                },
            };
            return list;
        }
    }
}
