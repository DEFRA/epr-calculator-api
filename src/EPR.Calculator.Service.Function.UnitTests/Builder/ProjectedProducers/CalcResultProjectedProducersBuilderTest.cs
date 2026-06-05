using System.Globalization;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Builder.ProjectedProducers;
using EPR.Calculator.Service.Function.Features.Common;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Helpers;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;

namespace EPR.Calculator.Service.Function.UnitTests.Builder.ProjectedProducers;

[TestCategory(TestCategories.ResultBuilder)]
[TestClass]
public class CalcResultProjectedProducersBuilderTest : TestsFor<CalcResultProjectedProducersBuilder>
{
    private readonly int AMTonnageI = 10;
    private readonly int ATonnageI = 9;
    private readonly int GMTonnageI = 12;
    private readonly int GTonnageI = 11;
    private readonly int LevelI = 3;
    private readonly int MaterialCodeI = 4;
    private readonly int PackagingTypeI = 5;
    private readonly int PeriodI = 2;
    private readonly int ProducerI = 0;
    private readonly int RMTonnageI = 8;
    private readonly int RTonnageI = 7;
    private readonly int SubsidiaryI = 1;
    private readonly int TotalTonnageI = 6;

    // Could move to CSV
    // Format: ProducerId, SubsidiaryId, SubmissionPeriod, Level, Material, PackagingType, TotalTonnage, RTonnage, RMTonnage, ATonnage, AMTonnage, GTonnage, GMTonnage

    [TestMethod]
    public void H1H2Projection_untouched()
    {
        // RAM is complete - no modifications made
        var given = new[]
        {
            new[] { "101", "", "2025-H1", "", "AL", "HH", "100", "20", "40", "40", "0", "", "" },
            new[] { "101", "", "2025-H2", "", "AL", "HH", "100", "20", "40", "40", "0", "", "" }
        };
        var expected = new []
        {
            new[] { "101", "", "2025-H1", "1", "AL", "HH", "100", "20.0", "40.0", "40.0", "0", "0", "0" },
            new[] { "101", "", "2025-H2", "1", "AL", "HH", "100", "20"  , "40"  , "40"  , "0", "0", "0" }
        };
        AssertExcepted(expected, FillGaps(given));
    }

    [TestMethod]
    public void H1H2Projection_untouched_previous()
    {
        // RAM is complete - no modifications made
        var given = new[]
        {
            new[] { "101", "", "2025-H1", "", "AL", "HH", "100", "20", "40", "40", "0", "", "" },
            new[] { "101", "", "2025-H2", "", "AL", "HH", "100", "20", "40", "40", "0", "", "" }
        };
        var expected = new []
        {
            new[] { "101", "", "2025-H1", "1", "AL", "HH", "100", "20.0", "40.0", "40.0", "0", "0", "0" },
            new[] { "101", "", "2025-H2", "1", "AL", "HH", "100", "20"  , "40"  , "40"  , "0", "0", "0" }
        };
        AssertExcepted(expected, FillGapsPrevious(given));
    }

    [TestMethod]
    public void H1H2Projection_untouched_onlyh2()
    {
        // RAM is complete - only H2 - no modifications made
        var given = new[]
        {
            new[] { "101", "", "2025-H2", "", "AL", "HH", "100", "20", "40", "40", "0", "", "" }
        };
        var expected = new []
        {
            new[] { "101", "", "2025-H2", "1", "AL", "HH", "100", "20", "40", "40", "0", "0", "0" }
        };
        AssertExcepted(expected, FillGaps(given));
    }

    [TestMethod]
    public void H1H2Projection_untouched_onlyh2_previous()
    {
        // RAM is complete - only H2 - no modifications made
        var given = new[]
        {
            new[] { "101", "", "2025-H2", "", "AL", "HH", "100", "20", "40", "40", "0", "", "" }
        };
        var expected = new []
        {
            new[] { "101", "", "2025-H2", "1", "AL", "HH", "100", "20", "40", "40", "0", "0", "0" }
        };
        AssertExcepted(expected, FillGapsPrevious(given));
    }

    [TestMethod]
    public void H1H2Projection_untouched_onlyh1()
    {
        // RAM is complete - only H1 - no modifications made
        var given = new[]
        {
            new[] { "101", "", "2025-H1", "", "AL", "HH", "100", "30", "40", "40", "0", "", "" }
        };
        var expected = new []
        {
            new[] { "101", "", "2025-H1", "1", "AL", "HH", "100", "30", "40", "40", "0", "0", "0" }
        };
        AssertExcepted(expected, FillGaps(given));
    }

    [TestMethod]
    public void H1H2Projection_untouched_hc()
    {
        // RAM is complete - no modifications made
        var given = new[]
        {
            new[] { "101", "" , "2025-H1", "", "AL", "HH" , "100", "20", "40", "40", "0", "", "" },
            new[] { "101", "A", "2025-H1", "", "GL", "HDC", "100", "20", "40", "40", "0", "", "" },
            new[] { "101", "B", "2025-H1", "", "AL", "PB" , "100", "20", "40", "40", "0", "", "" },
            new[] { "101", "" , "2025-H2", "", "PL", "HH" , "100", "20", "40", "40", "0", "", "" },
            new[] { "101", "A", "2025-H2", "", "ST", "PB" , "100", "20", "40", "40", "0", "", "" },
            new[] { "101", "B", "2025-H2", "", "AL", "HH" , "100", "20", "40", "40", "0", "", "" }
        };
        var expected = new []
        {
            //new string[] { "101", "" , "2025-H1", "1", "AL", "HH" , "100", "20", "40", "40", "0", "0", "0" },
            //new string[] { "101", "" , "2025-H1", "1", "AL", "PB" , "100", "20", "40", "40", "0", "0", "0" },
            //new string[] { "101", "" , "2025-H1", "1", "GL", "HDC", "100", "20", "40", "40", "0", "0", "0" },
            new [] { "101", "" , "2025-H1", "2", "AL", "HH" , "100", "20.0", "40.0", "40.0", "0", "0", "0" },
            new [] { "101", "B", "2025-H1", "2", "AL", "PB" , "100", "20.0", "40.0", "40.0", "0", "0", "0" },
            new [] { "101", "A", "2025-H1", "2", "GL", "HDC", "100", "20"  , "40"  , "40"  , "0", "0", "0" },
            //new string[] { "101", "" , "2025-H2", "1", "AL", "HH" , "100", "20", "40", "40", "0", "0", "0" },
            //new string[] { "101", "" , "2025-H2", "1", "PL", "HH" , "100", "20", "40", "40", "0", "0", "0" },
            //new string[] { "101", "" , "2025-H2", "1", "ST", "PB" , "100", "20", "40", "40", "0", "0", "0" },
            new [] { "101", "" , "2025-H2", "2", "PL", "HH" , "100", "20"  , "40"  , "40"  , "0", "0", "0" },
            new [] { "101", "B", "2025-H2", "2", "AL", "HH" , "100", "20"  , "40"  , "40"  , "0", "0", "0" },
            new [] { "101", "A", "2025-H2", "2", "ST", "PB" , "100", "20"  , "40"  , "40"  , "0", "0", "0" }
        };
        AssertExcepted(expected, FillGaps(given));
    }

    [TestMethod]
    public void H1H2Projection_untouched_hc_previous()
    {
        // RAM is complete - no modifications made
        var given = new[]
        {
            new[] { "101", "" , "2025-H1", "", "AL", "HH" , "100", "20", "40", "40", "0", "", "" },
            new[] { "101", "A", "2025-H1", "", "GL", "HDC", "100", "20", "40", "40", "0", "", "" },
            new[] { "101", "B", "2025-H1", "", "AL", "PB" , "100", "20", "40", "40", "0", "", "" },
            new[] { "101", "" , "2025-H2", "", "PL", "HH" , "100", "20", "40", "40", "0", "", "" },
            new[] { "101", "A", "2025-H2", "", "ST", "PB" , "100", "20", "40", "40", "0", "", "" },
            new[] { "101", "B", "2025-H2", "", "AL", "HH" , "100", "20", "40", "40", "0", "", "" }
        };
        var expected = new []
        {
            new [] { "101", "" , "2025-H1", "1", "AL", "HH" , "100", "20.0", "40.0", "40.0", "0", "0", "0" },
            new [] { "101", "" , "2025-H1", "1", "AL", "PB" , "100", "20.0", "40.0", "40.0", "0", "0", "0" },
            new [] { "101", "" , "2025-H1", "1", "GL", "HDC", "100", "20"  , "40"  , "40"  , "0", "0", "0" },
            new [] { "101", "" , "2025-H1", "2", "AL", "HH" , "100", "20.0", "40.0", "40.0", "0", "0", "0" },
            new [] { "101", "B", "2025-H1", "2", "AL", "PB" , "100", "20.0", "40.0", "40.0", "0", "0", "0" },
            new [] { "101", "A", "2025-H1", "2", "GL", "HDC", "100", "20"  , "40"  , "40"  , "0", "0", "0" },
            new [] { "101", "" , "2025-H2", "1", "AL", "HH" , "100", "20"  , "40"  , "40"  , "0", "0", "0" },
            new [] { "101", "" , "2025-H2", "1", "PL", "HH" , "100", "20"  , "40"  , "40"  , "0", "0", "0" },
            new [] { "101", "" , "2025-H2", "1", "ST", "PB" , "100", "20"  , "40"  , "40"  , "0", "0", "0" },
            new [] { "101", "" , "2025-H2", "2", "PL", "HH" , "100", "20"  , "40"  , "40"  , "0", "0", "0" },
            new [] { "101", "B", "2025-H2", "2", "AL", "HH" , "100", "20"  , "40"  , "40"  , "0", "0", "0" },
            new [] { "101", "A", "2025-H2", "2", "ST", "PB" , "100", "20"  , "40"  , "40"  , "0", "0", "0" }
        };
        AssertExcepted(expected, FillGapsPrevious(given));
    }

    [TestMethod]
    public void H1H2Projection_onlyh2_incomplete()
    {
        // Incomplete H2 - inferred as Red
        var given = new[]
        {
            new[] { "101", "", "2025-H2", "", "AL", "HH", "100", "", "", "", "", "", "" }
        };
        var expected = new []
        {
            new[] { "101", "", "2025-H2", "1", "AL", "HH", "100", "100", "0", "0", "0", "0", "0" }
        };
        AssertExcepted(expected, FillGaps(given));
    }

    [TestMethod]
    public void H1H2Projection_onlyh2_incomplete_previous()
    {
        // Incomplete H2 - inferred as Red
        var given = new[]
        {
            new[] { "101", "", "2025-H2", "", "AL", "HH", "100", "", "", "", "", "", "" }
        };
        var expected = new []
        {
            new[] { "101", "", "2025-H2", "1", "AL", "HH", "100", "100", "0", "0", "0", "0", "0" }
        };
        AssertExcepted(expected, FillGapsPrevious(given));
    }

    [TestMethod]
    public void H1H2Projection_onlyh2_partial()
    {
        // Partial H2 - inferred as Red
        var given = new[]
        {
            new[] { "101", "", "2025-H2", "", "AL", "HH", "100", "50", "", "", "", "", "" }
        };
        var expected = new []
        {
            new[] { "101", "", "2025-H2", "1", "AL", "HH", "100", "100", "0", "0", "0", "0", "0" }
        };
        AssertExcepted(expected, FillGaps(given));
    }

    [TestMethod]
    public void H1H2Projection_onlyh2_partial_previous()
    {
        // Partial H2 - inferred as Red
        var given = new[]
        {
            new[] { "101", "", "2025-H2", "", "AL", "HH", "100", "50", "", "", "", "", "" }
        };
        var expected = new []
        {
            new[] { "101", "", "2025-H2", "1", "AL", "HH", "100", "100", "0", "0", "0", "0", "0" }
        };
        AssertExcepted(expected, FillGapsPrevious(given));
    }

    [TestMethod]
    public void H1H2Projection_onlyh2_partial2()
    {
        // Partial H2 - inferred as Red
        var given = new[]
        {
            new[] { "101", "", "2025-H2", "", "AL", "HH", "100", "", "50", "", "", "", "" }
        };
        var expected = new []
        {
            new[] { "101", "", "2025-H2", "1", "AL", "HH", "100", "50", "50", "0", "0", "0", "0" }
        };
        AssertExcepted(expected, FillGaps(given));
    }

    [TestMethod]
    public void H1H2Projection_onlyh2_partial2_previous2()
    {
        // Partial H2 - inferred as Red
        var given = new[]
        {
            new[] { "101", "", "2025-H2", "", "AL", "HH", "100", "", "50", "", "", "", "" }
        };
        var expected = new []
        {
            new[] { "101", "", "2025-H2", "1", "AL", "HH", "100", "50", "50", "0", "0", "0", "0" }
        };
        AssertExcepted(expected, FillGapsPrevious(given));
    }

    [TestMethod]
    public void H1H2Projection_incomplete_h1()
    {
        // Incomplete H1 - reflects proportions from H2
        var given = new[]
        {
            new[] { "101", "", "2025-H1", "", "AL", "HH", "42", "", "", "", "", "", "" },
            new[] { "101", "", "2025-H2", "", "AL", "HH", "21", "6", "5", "4", "3", "2", "1" }
        };
        var expected = new []
        {
            new[] { "101", "", "2025-H1", "1", "AL", "HH", "42", "12.000", "10.000", "8.000", "6.000", "4.000", "2.000" },
            new[] { "101", "", "2025-H2", "1", "AL", "HH", "21", "6", "5", "4", "3", "2", "1" }
        };
        AssertExcepted(expected, FillGaps(given));
    }

    [TestMethod]
    public void H1H2Projection_partial_h1()
    {
        // Incomplete H1 - reflects proportions from H2, perserving any H1
        var given = new[]
        {
            new[] { "101", "", "2025-H1", "", "AL", "HH", "43", "1", "", "", "", "", "" },
            new[] { "101", "", "2025-H2", "", "AL", "HH", "21", "6", "5", "4", "3", "2", "1" }
        };
        var expected = new []
        {
            new[] { "101", "", "2025-H1", "1", "AL", "HH", "43", "13.000", "10.000", "8.000", "6.000", "4.000", "2.000" },
            new[] { "101", "", "2025-H2", "1", "AL", "HH", "21", "6", "5", "4", "3", "2", "1" }
        };
        AssertExcepted(expected, FillGaps(given));
    }

    [TestMethod]
    public void H1H2Projection_partial_h2_and_partial_h1()
    {
        // Incomplete H2 - defaults some to red
        // Incomplete H1 - reflects proportions from H2, perserving any H1
        var given = new[]
        {
            new[] { "101", "", "2025-H1", "", "AL", "HH", "43", "", "", "3", "", "", "" },
            new[] { "101", "", "2025-H2", "", "AL", "HH", "100", "50", "", "", "", "", "" }
        };
        var expected = new []
        {
            new[] { "101", "", "2025-H1", "1", "AL", "HH", "43", "40", "0", "3", "0", "0", "0" },
            new[] { "101", "", "2025-H2", "1", "AL", "HH", "100", "100", "0", "0", "0", "0", "0" }
        };
        AssertExcepted(expected, FillGaps(given));
    }

    [TestMethod]
    public void H1H2Projection_partial_h1_no_h2()
    {
        // Incomplete H1 - cannot reflect proportions from H2, defaults remaining tonnage to red
        var given = new[]
        {
            new[] { "101", "", "2025-H1", "", "AL", "HH", "100", "", "", "10", "", "", "" }
        };
        var expected = new []
        {
            new[] { "101", "", "2025-H1", "1", "AL", "HH", "100", "90", "0", "10", "0", "0", "0" }
        };
        AssertExcepted(expected, FillGaps(given));
    }

    [TestMethod]
    public void H1H2Projection_subtotal_hg_h1_noh2()
    {
        // Level 1 subtotal added for parent who reports for themselves too
        var given = new[]
        {
            new[] { "101", "", "2025-H1", "", "PL", "PB", "100", "", "10", "10", "", "", "" },
            new[] { "101", "A", "2025-H1", "", "PL", "PB", "200", "", "", "20", "", "", "20" }
        };
        var expected = new []
        {
            //new[] { "101", "", "2025-H1", "1", "PL", "PB", "300", "240", "10", "30", "0", "0", "20" },
            new[] { "101", "", "2025-H1", "2", "PL", "PB", "100", "80", "10", "10", "0", "0", "0" },
            new[] { "101", "A", "2025-H1", "2", "PL", "PB", "200", "160", "0", "20", "0", "0", "20" }
        };
        AssertExcepted(expected, FillGaps(given));
    }

    [TestMethod]
    public void H1H2Projection_subtotal_hg_indv()
    {
        // Part of holding group but subsidiary reports individually - expected subtotal row for parent
        var given = new[]
        {
            new[] { "101", "A", "2025-H1", "", "GL", "HDC", "43", "1", "", "", "", "", "" },
            new[] { "101", "A", "2025-H2", "", "GL", "HDC", "21", "6", "5", "4", "3", "2", "1" }
        };
        var expected = new []
        {
            //new[] { "101", "", "2025-H1", "1", "GL", "HDC", "43", "13.000", "10.000", "8.000", "6.000", "4.000", "2.000" },
            new[] { "101", "A", "2025-H1", "2", "GL", "HDC", "43", "13.000", "10.000", "8.000", "6.000", "4.000", "2.000" },
            //new[] { "101", "", "2025-H2", "1", "GL", "HDC", "21", "6", "5", "4", "3", "2", "1" },
            new[] { "101", "A", "2025-H2", "2", "GL", "HDC", "21", "6", "5", "4", "3", "2", "1" }
        };
        AssertExcepted(expected, FillGaps(given));
    }

    [TestMethod]
    public void H1H2Projection_subtotal_hg_no_parent_report_multi_materials()
    {
        // Part of holding group where holding group doesn't report for themselves - different materials
        var given = new[]
        {
            new[] { "101", "A", "2025-H1", "", "ST", "HH", "100", "20",  "20", "40",  "0",  "", "" },
            new[] { "101", "A", "2025-H2", "", "AL", "HH", "100", "20",  "40", "40",  "0",  "", "" },
            new[] { "101", "A", "2025-H1", "", "PL", "PB", "100", "20",  "40", "40",  "0",  "", "" },
            new[] { "101", "A", "2025-H2", "", "PL", "PB", "100", "20",  "40", "40",  "0",  "", "" },
            new[] { "101", "B", "2025-H1", "", "ST", "HH", "200", "50",  "50", "100", "0",  "", "" },
            new[] { "101", "B", "2025-H2", "", "AL", "HH", "200", "150", "25", "25",  "0",  "", "" }
        };
        var expected = new[]
        {
            //new[] { "101", "",  "2025-H1", "1", "PL", "PB", "100", "20.0", "40.0", "40.0", "0", "0", "0" },
            //new[] { "101", "",  "2025-H1", "1", "ST", "HH", "300", "90",   "70",   "140",  "0", "0", "0" },
            new[] { "101", "A", "2025-H1", "2", "PL", "PB", "100", "20.0", "40.0", "40.0", "0", "0", "0" },
            new[] { "101", "A", "2025-H1", "2", "ST", "HH", "100", "40",   "20",   "40",   "0", "0", "0" },
            new[] { "101", "B", "2025-H1", "2", "ST", "HH", "200", "50",   "50",   "100",  "0", "0", "0" },

            //new[] { "101", "",  "2025-H2", "1", "AL", "HH", "300", "170",  "65",   "65",   "0", "0", "0" },
            //new[] { "101", "",  "2025-H2", "1", "PL", "PB", "100", "20",   "40",   "40",   "0", "0", "0" },
            new[] { "101", "A", "2025-H2", "2", "AL", "HH", "100", "20",   "40",   "40",   "0", "0", "0" },
            new[] { "101", "B", "2025-H2", "2", "AL", "HH", "200", "150",  "25",   "25",   "0", "0", "0" },
            new[] { "101", "A", "2025-H2", "2", "PL", "PB", "100", "20",   "40",   "40",   "0", "0", "0" }
        };
        AssertExcepted(expected, FillGaps(given));
    }

    [TestMethod]
    public void H1H2Projection_subtotal_hg_no_parent_report_multi_materials_previous()
    {
        // Part of holding group where holding group doesn't report for themselves - different materials
        var given = new[]
        {
            new[] { "101", "A", "2025-H1", "", "ST", "HH", "100", "20",  "20", "40",  "0",  "", "" },
            new[] { "101", "A", "2025-H2", "", "AL", "HH", "100", "20",  "40", "40",  "0",  "", "" },
            new[] { "101", "A", "2025-H1", "", "PL", "PB", "100", "20",  "40", "40",  "0",  "", "" },
            new[] { "101", "A", "2025-H2", "", "PL", "PB", "100", "20",  "40", "40",  "0",  "", "" },
            new[] { "101", "B", "2025-H1", "", "ST", "HH", "200", "50",  "50", "100", "0",  "", "" },
            new[] { "101", "B", "2025-H2", "", "AL", "HH", "200", "150", "25", "25",  "0",  "", "" }
        };
        var expected = new[]
        {
            new[] { "101", "",  "2025-H1", "1", "PL", "PB", "100", "20.0", "40.0", "40.0", "0", "0", "0" },
            new[] { "101", "",  "2025-H1", "1", "ST", "HH", "300", "90",   "70",   "140",  "0", "0", "0" },
            new[] { "101", "A", "2025-H1", "2", "PL", "PB", "100", "20.0", "40.0", "40.0", "0", "0", "0" },
            new[] { "101", "A", "2025-H1", "2", "ST", "HH", "100", "40",   "20",   "40",   "0", "0", "0" },
            new[] { "101", "B", "2025-H1", "2", "ST", "HH", "200", "50",   "50",   "100",  "0", "0", "0" },

            new[] { "101", "",  "2025-H2", "1", "AL", "HH", "300", "170",  "65",   "65",   "0", "0", "0" },
            new[] { "101", "",  "2025-H2", "1", "PL", "PB", "100", "20",   "40",   "40",   "0", "0", "0" },
            new[] { "101", "A", "2025-H2", "2", "AL", "HH", "100", "20",   "40",   "40",   "0", "0", "0" },
            new[] { "101", "B", "2025-H2", "2", "AL", "HH", "200", "150",  "25",   "25",   "0", "0", "0" },
            new[] { "101", "A", "2025-H2", "2", "PL", "PB", "100", "20",   "40",   "40",   "0", "0", "0" }
        };
        AssertExcepted(expected, FillGapsPrevious(given));
    }

    /*
    Don't understand this test
    Multiple 101 ! 2025-H2 AL - testing ability to merge?
    https://github.com/DEFRA/epr-calculator-service/blob/feature/ECV-512/src/EPR.Calculator.Service.Function.UnitTests/Builder/ProjectedProducers/CalcResultProjectedProducersBuilderTest.cs#L387
*/
    [TestMethod]
    public void H1H2Projection_h1_use_subtotal_h2_projection()
    {
        var given = new[]
        {
            new[] { "101", "" , "2025-H2", "", "AL", "HH", "100", "20",  "40", "40",  "0",  "", "" },
            new[] { "101", "A", "2025-H2", "", "AL", "HH", "200", "20",  "40", "40",  "0",  "", "" },
            new[] { "101", "B", "2025-H2", "", "AL", "HH", "300", "150", "25", "25",  "0",  "", "" },
            new[] { "101", "" , "2025-H1", "", "AL", "HH", "100", "20",  "40", "40",  "0",  "", "" },
            new[] { "101", "A", "2025-H1", "", "AL", "HH", "200", "20",  "40", "40",  "0",  "", "" },
            new[] { "101", "B", "2025-H1", "", "AL", "HH", "300", "150", "25", "25",  "0",  "", "" }
        };
        var expected = new[]
        {
            //new[] { "101", "" , "2025-H1", "1", "AL", "HH", "600", "320.00", "140.000", "140.000", "0", "0", "0" },
            new[] { "101", "" , "2025-H1", "2", "AL", "HH", "100",  "20.00",  "40.000",  "40.000", "0", "0", "0" },
            new[] { "101", "A", "2025-H1", "2", "AL", "HH", "200",  "85.00",  "57.500",  "57.500", "0", "0", "0" },
            new[] { "101", "B", "2025-H1", "2", "AL", "HH", "300", "215.00",  "42.500",  "42.500", "0", "0", "0" },
            //new[] { "101", "" , "2025-H2", "1", "AL", "HH", "600",    "390",     "105",     "105", "0", "0", "0" },
            new[] { "101", "" , "2025-H2", "2", "AL", "HH", "100",     "20",      "40",      "40", "0", "0", "0" },
            new[] { "101", "A", "2025-H2", "2", "AL", "HH", "200",    "120",      "40",      "40", "0", "0", "0" },
            new[] { "101", "B", "2025-H2", "2", "AL", "HH", "300",    "250",      "25",      "25", "0", "0", "0" }
        };
        AssertExcepted(expected, FillGaps(given));
    }

    [TestMethod]
    public void H1H2Projection_h1_use_subtotal_h2_projection_previous()
    {
        var given = new[]
        {
            new[] { "101", "" , "2025-H2", "", "AL", "HH", "100", "20",  "40", "40",  "0",  "", "" },
            new[] { "101", "A", "2025-H2", "", "AL", "HH", "200", "20",  "40", "40",  "0",  "", "" },
            new[] { "101", "B", "2025-H2", "", "AL", "HH", "300", "150", "25", "25",  "0",  "", "" },
            new[] { "101", "" , "2025-H1", "", "AL", "HH", "100", "20",  "40", "40",  "0",  "", "" },
            new[] { "101", "A", "2025-H1", "", "AL", "HH", "200", "20",  "40", "40",  "0",  "", "" },
            new[] { "101", "B", "2025-H1", "", "AL", "HH", "300", "150", "25", "25",  "0",  "", "" }
        };
        var expected = new[]
        {
            new[] { "101", "" , "2025-H1", "1", "AL", "HH", "600", "320.00", "140.000", "140.000", "0", "0", "0" },
            new[] { "101", "" , "2025-H1", "2", "AL", "HH", "100",  "20.00",  "40.000",  "40.000", "0", "0", "0" },
            new[] { "101", "A", "2025-H1", "2", "AL", "HH", "200",  "85.00",  "57.500",  "57.500", "0", "0", "0" },
            new[] { "101", "B", "2025-H1", "2", "AL", "HH", "300", "215.00",  "42.500",  "42.500", "0", "0", "0" },
            new[] { "101", "" , "2025-H2", "1", "AL", "HH", "600",    "390",     "105",     "105", "0", "0", "0" },
            new[] { "101", "" , "2025-H2", "2", "AL", "HH", "100",     "20",      "40",      "40", "0", "0", "0" },
            new[] { "101", "A", "2025-H2", "2", "AL", "HH", "200",    "120",      "40",      "40", "0", "0", "0" },
            new[] { "101", "B", "2025-H2", "2", "AL", "HH", "300",    "250",      "25",      "25", "0", "0", "0" }
        };
        AssertExcepted(expected, FillGapsPrevious(given));
    }

    private (IImmutableList<MaterialDetail>, RunContext) InsertData(string[][] given)
    {
        var runContext = TestDataHelper.CalculatorRun2026;

        for (var i = 0; i < given.GetLength(0); i++)
        {
            var entry = given[i];
            if (entry.Length != 13)
                throw new Exception($"Supplied test data should have length 13 - entry {i} had length {entry.Length}.");
        }

        dbContext.CalculatorRuns.Add(new CalculatorRun
        {
            Id = runContext.RunId,
            RelativeYear = runContext.RelativeYear,
            Name = runContext.RunName
        });

        var materials = given
            .Select(row => row[MaterialCodeI])
            .Distinct()
            .Select((m, i) => new Material { Id = i + 1, Code = m, Name = m, Description = m })
            .ToArray();
        dbContext.Material.AddRange(materials);

        dbContext.SaveChanges();

        return (materials.ToDetails(), runContext);
    }

    private string[][] ConvertResult((List<L1Producer>, CalcResultProjectedProducers) given)
    {
        var materials = dbContext.Material.Select(e => e).ToList();
        var allProducers = given.Item1.SelectMany(l1 => l1.Producers).ToList();

        string[]? createRow(int producerId, string? subsidiaryId, string level, ProducerReportedMaterial submission)
        {
            if (submission.PackagingTonnage == 0)
                return null;
            var row = new string[13];
            row[ProducerI]      = producerId.ToString();
            row[SubsidiaryI]    = subsidiaryId ?? "";
            row[PeriodI]        = submission.SubmissionPeriod;
            row[LevelI]         = level;
            row[MaterialCodeI]  = materials.Single(m => m.Id == submission.MaterialId).Code;
            row[PackagingTypeI] = submission.PackagingType;
            row[TotalTonnageI]  = submission.PackagingTonnage.ToString();
            row[RTonnageI]      = (submission.PackagingTonnageRed          ?? 0).ToString();
            row[RMTonnageI]     = (submission.PackagingTonnageRedMedical   ?? 0).ToString();
            row[ATonnageI]      = (submission.PackagingTonnageAmber        ?? 0).ToString();
            row[AMTonnageI]     = (submission.PackagingTonnageAmberMedical ?? 0).ToString();
            row[GTonnageI]      = (submission.PackagingTonnageGreen        ?? 0).ToString();
            row[GMTonnageI]     = (submission.PackagingTonnageGreenMedical ?? 0).ToString();
            return row;
        }

        // TODO if L2 - then add L1?
        var levelLookup = allProducers.Select(p => (p.ProducerId, p.SubsidiaryId)).Distinct()
            .GroupBy(p => p.ProducerId).SelectMany(producerGroup =>
            {
                return producerGroup.GroupBy(p => p.SubsidiaryId).Select(subsidiaryGroup =>
                    ((producerGroup.Key, subsidiaryGroup.Key), producerGroup.Count() == 1 && string.IsNullOrEmpty(subsidiaryGroup.Key) ? "1" : "2")
                ).ToList();
            }).ToDictionary();

        var result = allProducers.SelectMany(p =>
            p.ProducerReportedMaterials.Select(r =>
                createRow(p.ProducerId, p.SubsidiaryId, levelLookup[(p.ProducerId, p.SubsidiaryId)], r)
            )
        ).Where(row => row is not null).Cast<string[]>();

        return result
            .OrderBy(a => a[PeriodI])
            .ThenBy(a => a[ProducerI])
            .ThenBy(a => string.IsNullOrEmpty(a[SubsidiaryI]) ? 0 : 1)
            .ThenBy(a => a[LevelI])
            .ThenBy(a => a[MaterialCodeI])
            .ThenBy(a => a[PackagingTypeI])
            .ToArray();
    }

    private List<L1Producer> ToL1Producers(string[][] given) =>
        ToProducers(given)
            .GroupBy(pd => pd.ProducerId)
            .Select(g => new L1Producer(g.Key, g.ToList()))
            .ToList();

    private List<ProducerDetail> ToProducers(string[][] given)
    {
        decimal? ToDecimal(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            return decimal.Parse(s, CultureInfo.InvariantCulture);
        }

        var materials = dbContext.Material.Select(e => e).ToList();

        var res = given.GroupBy(row => (ProducerId: row[ProducerI], SubsidiaryId: row[SubsidiaryI])).Distinct()
            .Select(producerRows =>
            {
                var producer =  new ProducerDetail
                {
                    ProducerId  = int.Parse(producerRows.Key.ProducerId),
                    SubsidiaryId = string.IsNullOrWhiteSpace(producerRows.Key.SubsidiaryId) ? null : producerRows.Key.SubsidiaryId
                };
                foreach (var row in producerRows)
                {
                    producer.ProducerReportedMaterials.Add(
                        new ProducerReportedMaterial
                        {
                            MaterialId                   = materials.Single(m => m.Code == row[MaterialCodeI]).Id,
                            PackagingType                = row[PackagingTypeI],
                            PackagingTonnage             = ToDecimal(row[TotalTonnageI]) ?? 0m,
                            PackagingTonnageRed          = ToDecimal(row[RTonnageI]),
                            PackagingTonnageAmber        = ToDecimal(row[ATonnageI]),
                            PackagingTonnageGreen        = ToDecimal(row[GTonnageI]),
                            PackagingTonnageRedMedical   = ToDecimal(row[RMTonnageI]),
                            PackagingTonnageAmberMedical = ToDecimal(row[AMTonnageI]),
                            PackagingTonnageGreenMedical = ToDecimal(row[GMTonnageI]),
                            SubmissionPeriod             = row[PeriodI]
                        }
                    );
                }

                return producer;
            }).ToList();
        //Console.WriteLine($"{res.Count()}: {JsonConvert.SerializeObject(res, Formatting.Indented)}");
        return res;
    }

    private string[][] FillGaps(string[][] given)
    {
        var (materialDetails, runContext) = InsertData(given);
        return ConvertResult(testSubject.Construct(runContext, materialDetails, ToL1Producers(given)));
    }

    private string[][] ConvertResultPrevious(CalcResultProjectedProducers given)
    {
        var result = new List<string[]>();

        string[]? createRow(int producerId, string? subsidiaryId, string submissonPeriodCode, string materialCode, string packagingType, string? level, RAMTonnage? projectedRamTonnage, decimal? tonnage)
        {
            if (projectedRamTonnage == null || tonnage == 0)
                return null;
            var row = new string[13];
            row[ProducerI]      = producerId.ToString();
            row[SubsidiaryI]    = subsidiaryId ?? "";
            row[PeriodI]        = submissonPeriodCode;
            row[LevelI]         = level ?? "";
            row[MaterialCodeI]  = materialCode;
            row[PackagingTypeI] = packagingType;
            row[TotalTonnageI]  = (tonnage ?? 0m).ToString();
            row[RTonnageI]      = (projectedRamTonnage?.RedTonnage ?? 0m).ToString();
            row[RMTonnageI]     = (projectedRamTonnage?.RedMedicalTonnage ?? 0m).ToString();
            row[ATonnageI]      = (projectedRamTonnage?.AmberTonnage ?? 0m).ToString();
            row[AMTonnageI]     = (projectedRamTonnage?.AmberMedicalTonnage ?? 0m).ToString();
            row[GTonnageI]      = (projectedRamTonnage?.GreenTonnage ?? 0m).ToString();
            row[GMTonnageI]     = (projectedRamTonnage?.GreenMedicalTonnage ?? 0m).ToString();
            return row;
        }

        if (given.H1ProjectedProducers != null && given.H2ProjectedProducers != null)
        {
            foreach (var producer in given.H1ProjectedProducers.Cast<ICalcResultProjectedProducer>().Concat(given.H2ProjectedProducers))
            {
                var producerId = producer.ProducerId;
                var subsidiaryId = producer.SubsidiaryId;
                var submissionPeriod = producer.SubmissionPeriodCode;

                foreach (var kv in producer.ProjectedTonnageByMaterial)
                {
                    var materialCode = kv.Key;
                    var v = kv.Value;
                    var hhRow = createRow(producerId, subsidiaryId, producer.SubmissionPeriodCode, materialCode, "HH", producer.Level, v.ProjectedHouseholdRAMTonnage, v.ProjectedHouseholdTonnage);
                    if (hhRow != null) result.Add(hhRow);
                    var pbRow = createRow(producerId, subsidiaryId, producer.SubmissionPeriodCode, materialCode, "PB", producer.Level, v.ProjectedPublicBinRAMTonnage, v.ProjectedPublicBinTonnage);
                    if (pbRow != null) result.Add(pbRow);
                    var hdcRow = createRow(producerId, subsidiaryId, producer.SubmissionPeriodCode, materialCode, "HDC", producer.Level, v.ProjectedHouseholdDrinksContainerRAMTonnage, v.ProjectedHouseholdDrinksContainerTonnage);
                    if (hdcRow != null) result.Add(hdcRow);
                }
            }
        }

        return result
            .OrderBy(a => a[PeriodI])
            .ThenBy(a => a[ProducerI])
            .ThenBy(a => string.IsNullOrEmpty(a[SubsidiaryI]) ? 0 : 1)
            .ThenBy(a => a[LevelI])
            .ThenBy(a => a[MaterialCodeI])
            .ThenBy(a => a[PackagingTypeI])
            .ToArray();
    }

    private string[][] FillGapsPrevious(string[][] given)
    {
        var (materialDetails, runContext) = InsertData(given);
        return ConvertResultPrevious(testSubject.Construct(runContext, materialDetails, ToL1Producers(given)).Item2);
    }

    private string ToPrintable(string[] arr) =>
        arr is null ? "null" : "[" + string.Join(", ", arr.Select(x => x?.ToString() ?? "null")) + "]";

    private string ToPrintableArray(string[][] arr) =>
        arr is null ? "null" : "[\n" + string.Join("\n", arr.Select(x => ToPrintable(x))) + "\n]";

    private void AssertExcepted(string[][] expected, string[][] actual)
    {
        var data = $"Arrays differ. expected={ToPrintableArray(expected)} actual={ToPrintableArray(actual)}";

        Assert.AreEqual(expected.Length, actual.Length, $"Results array sizes differ\n{data}");

        for (var i = 0; i < expected.Length; i++)
        {
            var expRow = expected[i] ?? Array.Empty<string>();
            var actRow = actual[i] ?? Array.Empty<string>();

            Assert.AreEqual(expRow.Length, actRow.Length, $"Result entry array differ at row {i}\n{data}");

            for (var j = 0; j < expRow.Length; j++)
            {
                var exp = expRow[j];
                var act = actRow[j];
                Assert.AreEqual(exp, act, $"Mismatch at [{i},{j}]: expected '{exp}' but was '{act}'.\n{data}");
            }
        }
    }
}
