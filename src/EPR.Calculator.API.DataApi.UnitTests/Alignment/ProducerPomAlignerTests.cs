using EPR.CommonDataService.DataApi.Alignment;

namespace EPR.Calculator.API.DataApi.UnitTests.Alignment;

[TestClass]
public class ProducerPomAlignerTests
{
    private static readonly Guid SubmitterId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private readonly ProducerPomAligner aligner = new();

    // ─────────────────────────── DedupeOrganisations ───────────────────────────

    [TestMethod]
    public void DedupeOrganisations_WithSingleOrganisation_ReturnsItUnchanged()
    {
        var result = aligner.DedupeOrganisations([Organisation()]);

        result.Count.ShouldBe(1);
    }

    [TestMethod]
    public void DedupeOrganisations_WithMultipleRegistrationsForSameOrganisation_PicksHasH2True()
    {
        var organisations = new[]
        {
            Organisation() with { TradingName = "Without H2", HasH2 = false },
            Organisation() with { TradingName = "With H2", HasH2 = true }
        };

        var result = aligner.DedupeOrganisations(organisations);

        result.Count.ShouldBe(1);
        result[0].TradingName.ShouldBe("With H2");
    }

    [TestMethod]
    public void DedupeOrganisations_WithMultipleRegistrationsAllHasH2False_PicksFirstOccurrence()
    {
        var organisations = new[]
        {
            Organisation() with { TradingName = "First" },
            Organisation() with { TradingName = "Second" }
        };

        var result = aligner.DedupeOrganisations(organisations);

        result.Count.ShouldBe(1);
        result[0].TradingName.ShouldBe("First");
    }

    [TestMethod]
    public void DedupeOrganisations_WithDifferentSubmitters_KeepsBoth()
    {
        var organisations = new[]
        {
            Organisation() with { SubmitterId = Guid.NewGuid() },
            Organisation() with { SubmitterId = Guid.NewGuid() }
        };

        var result = aligner.DedupeOrganisations(organisations);

        result.Count.ShouldBe(2);
    }

    [TestMethod]
    public void DedupeOrganisations_AppliesNoObligationOrNameFiltering()
    {
        var organisations = new[]
        {
            Organisation() with { ObligationStatus = "N" },
            Organisation() with { SubmitterId = Guid.NewGuid(), OrganisationName = "   " }
        };

        var result = aligner.DedupeOrganisations(organisations);

        result.Count.ShouldBe(2);
    }

    // ─────────────────────────── Align ───────────────────────────

    [TestMethod]
    public void Align_WithObligatedOrganisationAndMatchingPom_ProducesAlignedProducer()
    {
        var organisations = new[] { Organisation() };
        var poms = new[] { Pom() };

        var result = aligner.Align(organisations, poms, ["PL"]).ToList();

        result.Count.ShouldBe(1);
        var producer = result[0];
        producer.OrganisationId.ShouldBe(1);
        producer.SubsidiaryId.ShouldBe("SUB-1");
        producer.SubmitterId.ShouldBe(SubmitterId);
        producer.TradingName.ShouldBe("Trading Co");
        producer.ProducerName.ShouldBe("Org Co");
        producer.ObligationStatus.ShouldBe("O");
        producer.DaysObligated.ShouldBe(200);
        producer.JoinerDate.ShouldBe("2024-01-01");
        producer.LeaverDate.ShouldBe("2024-12-31");
        producer.StatusCode.ShouldBe("Active");
        producer.ReportedMaterials.Count.ShouldBe(1);

        var material = producer.ReportedMaterials[0];
        material.MaterialCode.ShouldBe("PL");
        material.PackagingType.ShouldBe("HH");
        material.SubmissionPeriod.ShouldBe("2024-P1");
        material.TotalWeight.ShouldBe(100d);
    }

    [TestMethod]
    public void Align_WithNonObligatedOrganisation_ExcludesOrganisation()
    {
        var organisations = new[] { Organisation() with { ObligationStatus = "N" } };
        var poms = new[] { Pom() };

        var result = aligner.Align(organisations, poms, ["PL"]);

        result.ShouldBeEmpty();
    }

    [TestMethod]
    public void Align_WithBlankOrganisationName_ExcludesOrganisation()
    {
        var organisations = new[] { Organisation() with { OrganisationName = "   " } };
        var poms = new[] { Pom() };

        var result = aligner.Align(organisations, poms, ["PL"]);

        result.ShouldBeEmpty();
    }

    [TestMethod]
    public void Align_WithNoMatchingPoms_ExcludesOrganisation()
    {
        var organisations = new[] { Organisation() };
        var poms = new[] { Pom() with { SubsidiaryId = "OTHER-SUB" } };

        var result = aligner.Align(organisations, poms, ["PL"]);

        result.ShouldBeEmpty();
    }

    [TestMethod]
    public void Align_WithPomForDifferentSubmitter_ExcludesPom()
    {
        var organisations = new[] { Organisation() };
        var poms = new[] { Pom() with { SubmitterId = Guid.NewGuid() } };

        var result = aligner.Align(organisations, poms, ["PL"]);

        result.ShouldBeEmpty();
    }

    [TestMethod]
    public void Align_WithPomMissingPackagingType_ExcludesPom()
    {
        var organisations = new[] { Organisation() };
        var poms = new[] { Pom() with { PackagingType = null } };

        var result = aligner.Align(organisations, poms, ["PL"]);

        result.ShouldBeEmpty();
    }

    [TestMethod]
    [DataRow("HH")]
    [DataRow("CW")]
    [DataRow("PB")]
    public void Align_WithReportablePackagingType_IncludesPom(string packagingType)
    {
        var organisations = new[] { Organisation() };
        var poms = new[] { Pom() with { PackagingType = packagingType } };

        var result = aligner.Align(organisations, poms, ["PL"]).ToList();

        result[0].ReportedMaterials.ShouldNotBeEmpty();
    }

    [TestMethod]
    public void Align_WithUnreportablePackagingType_ExcludesPom()
    {
        var organisations = new[] { Organisation() };
        var poms = new[] { Pom() with { PackagingType = "NH" } };

        var result = aligner.Align(organisations, poms, ["PL"]).ToList();

        result.ShouldBeEmpty();
    }

    [TestMethod]
    public void Align_WithHouseholdDrinksContainersAndGlassMaterial_IncludesPom()
    {
        var organisations = new[] { Organisation() };
        var poms = new[] { Pom() with { PackagingType = "HDC", PackagingMaterial = "GL" } };

        var result = aligner.Align(organisations, poms, ["GL"]).ToList();

        result[0].ReportedMaterials.ShouldNotBeEmpty();
    }

    [TestMethod]
    public void Align_WithHouseholdDrinksContainersAndNonGlassMaterial_ExcludesPom()
    {
        var organisations = new[] { Organisation() };
        var poms = new[] { Pom() with { PackagingType = "HDC", PackagingMaterial = "PL" } };

        var result = aligner.Align(organisations, poms, ["PL"]).ToList();

        result.ShouldBeEmpty();
    }

    [TestMethod]
    public void Align_WithMaterialCodeNotInKnownList_ExcludesMaterial_ButKeepsProducer()
    {
        var organisations = new[] { Organisation() };
        var poms = new[] { Pom() with { PackagingMaterial = "UNKNOWN" } };

        var result = aligner.Align(organisations, poms, ["PL"]).ToList();

        // Producer is still included (it had matching poms), just with no reported materials -
        // matches the original ProducerDataTransposer behaviour.
        result.Count.ShouldBe(1);
        result[0].ReportedMaterials.ShouldBeEmpty();
    }

    [TestMethod]
    public void Align_OrdersReportedMaterialsByMaterialCodeList_NotByPomOrder()
    {
        var organisations = new[] { Organisation() };
        var poms = new[]
        {
            Pom() with { PackagingMaterial = "GL", SubmissionPeriod = "P-GL" },
            Pom() with { PackagingMaterial = "PL", SubmissionPeriod = "P-PL" }
        };

        // materialCodes lists PL before GL, even though the GL pom appears first in the input.
        var result = aligner.Align(organisations, poms, ["PL", "GL"]).ToList();

        result[0].ReportedMaterials.Select(m => m.MaterialCode).ShouldBe(["PL", "GL"]);
    }

    [TestMethod]
    public void Align_GroupsSeparatelyBySubmissionPeriodAndPackagingType()
    {
        var organisations = new[] { Organisation() };
        var poms = new[]
        {
            Pom() with { SubmissionPeriod = "2024-P1", PackagingMaterialWeight = 100d },
            Pom() with { SubmissionPeriod = "2024-P2", PackagingMaterialWeight = 200d },
            Pom() with { PackagingType = "CW", PackagingMaterialWeight = 50d }
        };

        var result = aligner.Align(organisations, poms, ["PL"]).ToList();

        result[0].ReportedMaterials.Count.ShouldBe(3);
        result[0].ReportedMaterials.Sum(m => m.TotalWeight).ShouldBe(350d);
    }

    [TestMethod]
    [DataRow("R", 100d, 0d, 0d, 0d, 0d, 0d)]
    [DataRow("A", 0d, 100d, 0d, 0d, 0d, 0d)]
    [DataRow("G", 0d, 0d, 100d, 0d, 0d, 0d)]
    [DataRow("R-M", 0d, 0d, 0d, 100d, 0d, 0d)]
    [DataRow("A-M", 0d, 0d, 0d, 0d, 100d, 0d)]
    [DataRow("G-M", 0d, 0d, 0d, 0d, 0d, 100d)]
    [DataRow(null, 0d, 0d, 0d, 0d, 0d, 0d)]
    public void Align_BucketsWeightByRagRating(
        string? ragRating, double red, double amber, double green, double redMedical, double amberMedical, double greenMedical)
    {
        var organisations = new[] { Organisation() };
        var poms = new[] { Pom() with { RamRagRating = ragRating, PackagingMaterialWeight = 100d } };

        var result = aligner.Align(organisations, poms, ["PL"]).ToList();

        var material = result[0].ReportedMaterials[0];
        material.TotalWeight.ShouldBe(100d);
        material.RedWeight.ShouldBe(red);
        material.AmberWeight.ShouldBe(amber);
        material.GreenWeight.ShouldBe(green);
        material.RedMedicalWeight.ShouldBe(redMedical);
        material.AmberMedicalWeight.ShouldBe(amberMedical);
        material.GreenMedicalWeight.ShouldBe(greenMedical);
    }

    [TestMethod]
    public void Align_SumsMultiplePomsForSameMaterialAndPeriod()
    {
        var organisations = new[] { Organisation() };
        var poms = new[]
        {
            Pom() with { RamRagRating = "R", PackagingMaterialWeight = 100d },
            Pom() with { RamRagRating = "A", PackagingMaterialWeight = 50d }
        };

        var result = aligner.Align(organisations, poms, ["PL"]).ToList();

        var material = result[0].ReportedMaterials[0];
        material.TotalWeight.ShouldBe(150d);
        material.RedWeight.ShouldBe(100d);
        material.AmberWeight.ShouldBe(50d);
    }

    private static AlignmentOrganisation Organisation() => new()
    {
        OrganisationId = 1,
        SubsidiaryId = "SUB-1",
        SubmitterId = SubmitterId,
        OrganisationName = "Org Co",
        TradingName = "Trading Co",
        ObligationStatus = "O",
        DaysObligated = 200,
        JoinerDate = "2024-01-01",
        LeaverDate = "2024-12-31",
        StatusCode = "Active",
        ErrorCode = null,
        HasH2 = false
    };

    private static AlignmentPom Pom() => new()
    {
        OrganisationId = 1,
        SubsidiaryId = "SUB-1",
        SubmitterId = SubmitterId,
        PackagingMaterial = "PL",
        PackagingType = "HH",
        SubmissionPeriod = "2024-P1",
        PackagingMaterialWeight = 100d,
        RamRagRating = "G"
    };
}
