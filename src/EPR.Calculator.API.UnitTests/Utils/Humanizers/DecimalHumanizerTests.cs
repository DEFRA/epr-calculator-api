using EPR.Calculator.API.Utils.Humanizers;

namespace EPR.Calculator.API.UnitTests.Utils.Humanizers;

[TestClass]
public class DecimalHumanizerTests
{
    public static IEnumerable<object[]> DecimalCases =>
    [
        [0.0000m,  "0"        ],
        [0.0001m,  "0"        ],
        [0.0005m,  "0"        ],
        [0.0009m,  "0"        ],
        [0.0010m,  "0.001"    ],
        [0.0100m,  "0.010"    ],
        [1000.0m,  "1,000"    ],
        [1000.09m, "1,000.090"],
        [1000000m, "1,000,000"]
    ];

    [TestMethod]
    [DynamicData(nameof(DecimalCases))]
    public void Should_format_decimal_correctly(decimal d, string expected)
    {
        var result = d.Humanize();

        result.ShouldBe(expected);
    }

    public static IEnumerable<object[]> PercentageCases =>
    [
        [0.000m,   "0%"        ],
        [0.001m,   "0%"        ],
        [0.005m,   "0%"        ],
        [0.009m,   "0%"        ],
        [0.010m,   "0.01%"     ],
        [0.100m,   "0.10%"     ],
        [1000.0m,  "1,000%"    ],
        [1000.9m,  "1,000.90%" ],
        [1000000m, "1,000,000%"]
    ];

    [TestMethod]
    [DynamicData(nameof(PercentageCases))]
    public void Should_format_percentage_correctly(decimal d, string expected)
    {
        var result = d.Humanize("%");

        result.ShouldBe(expected);
    }

    public static IEnumerable<object[]> CurrencyCases =>
    [
        [0.000m,   "£0.00"        ],
        [0.001m,   "£0.00"        ],
        [0.005m,   "£0.00"        ],
        [0.009m,   "£0.00"        ],
        [0.010m,   "£0.01"        ],
        [0.100m,   "£0.10"        ],
        [1000.0m,  "£1,000.00"    ],
        [1000.9m,  "£1,000.90"    ],
        [1000000m, "£1,000,000.00"]
    ];

    [TestMethod]
    [DynamicData(nameof(CurrencyCases))]
    public void Should_format_currency_correctly(decimal d, string expected)
    {
        var result1 = d.Humanize("c");
        var result2 = d.Humanize("C");

        result1.ShouldBe(expected);
        result2.ShouldBe(expected);
    }

    public static IEnumerable<object[]> TonnageCases =>
    [
        [0.0000m,  "0 t"        ],
        [0.0001m,  "0 t"        ],
        [0.0005m,  "0 t"        ],
        [0.0009m,  "0 t"        ],
        [0.0010m,  "0.001 t"    ],
        [0.0100m,  "0.010 t"    ],
        [1000.0m,  "1,000 t"    ],
        [1000.09m, "1,000.090 t"],
        [1000000m, "1,000,000 t"]
    ];

    [TestMethod]
    [DynamicData(nameof(TonnageCases))]
    public void Should_format_tonne_correctly(decimal d, string expected)
    {
        var result1 = d.Humanize("t");
        var result2 = d.Humanize("T");

        result1.ShouldBe(expected);
        result2.ShouldBe(expected);
    }

    public static IEnumerable<object[]> CustomCases =>
    [
        ["°C", 0.0000m,  "0°C"        ],
        ["°C", 0.0001m,  "0°C"        ],
        ["°C", 0.0005m,  "0°C"        ],
        ["°C", 0.0009m,  "0°C"        ],
        ["°C", 0.0010m,  "0.001°C"    ],
        ["°C", 0.0100m,  "0.010°C"    ],
        ["°C", 1000.0m,  "1,000°C"    ],
        ["°C", 1000.09m, "1,000.090°C"],
        ["°C", 1000000m, "1,000,000°C"]
    ];

    [TestMethod]
    [DynamicData(nameof(CustomCases))]
    public void Should_format_custom_correctly(string unit, decimal d, string expected)
    {
        var result = d.Humanize(unit);

        result.ShouldBe(expected);
    }
}
