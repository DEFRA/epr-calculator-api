using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Builder.Lapcap;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Helpers;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;

namespace EPR.Calculator.Service.Function.UnitTests.Builder;

[TestCategory(TestCategories.ResultBuilder)]
[TestClass]
public class CalcResultLapcapDataBuilderTest : TestsFor<CalcResultLapcapDataBuilder>
{
    private Mock<ICalcCountryApportionmentService> mockService = null!;

    protected override void TestInitialize()
    {
        mockService = fixture.Freeze<Mock<ICalcCountryApportionmentService>>();
    }

    [TestMethod]
    public async Task ConstructTest_For_Aluminuim_Plastic()
    {
        const string aluminium = "Aluminium";
        const string plastic = "Plastic";

        var runContext = TestDataHelper.CalculatorRun2024;

        var run = new CalculatorRun
        {
            Id = runContext.RunId,
            RelativeYear = runContext.RelativeYear,
            Name = runContext.RunName,
            CalculatorRunClassificationId = (int)RunClassification.RUNNING,
            CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc),
            CreatedBy = "Test User",
            LapcapDataMasterId = 2
        };

        var lapcapDataMaster = new LapcapDataMaster
        {
            Id = 2,
            RelativeYear = runContext.RelativeYear,
            CreatedBy = "Testuser",
            CreatedAt = DateTime.UtcNow,
            EffectiveFrom = DateTime.UtcNow
        };
        var details = GetLapcapDetails(lapcapDataMaster);
        details.ForEach(detail => detail.LapcapDataMaster = lapcapDataMaster);

        dbContext.Country.Add(new Country { Code = "En", Name = "England", Description = "England" });
        dbContext.Country.Add(new Country { Code = "Wa", Name = "Wales", Description = "Wales" });
        dbContext.Country.Add(new Country { Code = "Sc", Name = "Scotland", Description = "Scotland" });
        dbContext.Country.Add(new Country { Code = "NI", Name = "Northern Ireland", Description = "Northern Ireland" });

        dbContext.CostType.Add(new CostType { Code = "1", Name = "Fee for LA Disposal Costs", Description = "Fee for LA Disposal Costs" });

        dbContext.LapcapDataMaster.Add(lapcapDataMaster);
        dbContext.LapcapDataDetail.AddRange(details);

        List<Material> materials =
        [
            new() { Name = aluminium, Code = "AL", Description = "Some" },
            new() { Name = plastic, Code = "PL", Description = "Some" }
        ];
        dbContext.Material.AddRange(materials);

        dbContext.CalculatorRuns.Add(run);
        await dbContext.SaveChangesAsync();

        var lapcapResults = await testSubject.ConstructAsync(runContext, materials.ToDetails());


        Assert.IsNotNull(lapcapResults);
        Assert.AreEqual(2, lapcapResults.ByMaterial.Count);

        var alRow = lapcapResults.ByMaterial["AL"];
        Assert.AreEqual(100, alRow.England);
        Assert.AreEqual(50, alRow.Wales);
        Assert.AreEqual(75, alRow.Scotland);
        Assert.AreEqual(25, alRow.NorthernIreland);
        Assert.AreEqual(250, alRow.Total);

        var plRow = lapcapResults.ByMaterial["PL"];
        Assert.AreEqual(100, plRow.England);
        Assert.AreEqual(50, plRow.Wales);
        Assert.AreEqual(75, plRow.Scotland);
        Assert.AreEqual(25, plRow.NorthernIreland);
        Assert.AreEqual(250, plRow.Total);

        var totalRow = lapcapResults.Total;
        Assert.IsNotNull(totalRow);
        Assert.AreEqual(200, totalRow.England);
        Assert.AreEqual(100, totalRow.Wales);
        Assert.AreEqual(150, totalRow.Scotland);
        Assert.AreEqual(50, totalRow.NorthernIreland);
        Assert.AreEqual(500, totalRow.Total);

        var countryApportionment = lapcapResults.CountryApportionment;
        Assert.IsNotNull(countryApportionment);
        Assert.AreEqual(40, countryApportionment.England);
        Assert.AreEqual(20, countryApportionment.Wales);
        Assert.AreEqual(30, countryApportionment.Scotland);
        Assert.AreEqual(10, countryApportionment.NorthernIreland);

        mockService.Verify(x => x.SaveChangesAsync(It.IsAny<CalcCountryApportionmentServiceDto>()));
    }

    private static List<LapcapDataDetail> GetLapcapDetails(LapcapDataMaster master)
    {
        List<string> refs =
        [
            "ENG-AL", "ENG-FC", "ENG-GL", "ENG-OT",
            "ENG-PC", "ENG-PL", "ENG-ST", "ENG-WD",
            "NI-AL", "NI-FC", "NI-GL", "NI-OT",
            "NI-PC", "NI-PL", "NI-ST", "NI-WD",
            "SCT-AL", "SCT-FC", "SCT-GL", "SCT-OT",
            "SCT-PC", "SCT-PL", "SCT-ST", "SCT-WD",
            "WLS-AL", "WLS-FC", "WLS-GL", "WLS-OT",
            "WLS-PC", "WLS-PL", "WLS-ST", "WLS-WD"
        ];

        var details = new List<LapcapDataDetail>();

        foreach (var uniqueRef in refs)
        {
            details.Add(
                new LapcapDataDetail
                {
                    LapcapDataMasterId = 2,
                    UniqueReference = uniqueRef,
                    TotalCost = GetTotalCostByCountry(uniqueRef),
                    LapcapDataMaster = master
                }
            );
        }

        return details;
    }

    public static decimal GetTotalCostByCountry(string uniqueRef)
    {
        if (uniqueRef.StartsWith("ENG-"))
            return 100M;

        if (uniqueRef.StartsWith("SCT-"))
            return 75M;

        if (uniqueRef.StartsWith("WLS-"))
            return 50M;

        return 25M;
    }
}
