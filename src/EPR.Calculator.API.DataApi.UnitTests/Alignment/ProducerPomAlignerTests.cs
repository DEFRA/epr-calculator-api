using EPR.Calculator.Api.DataApi.Alignment;
using EPR.Calculator.Api.DataApi.CommonDataApi.Entities;
using EPR.Calculator.Api.DataApi.PomEligibility;

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
        var withoutH2 = Organisation();
        var withH2 = Organisation();
        var organisations = new[]
        {
            withoutH2 with { Org = withoutH2.Org with { TradingName = "Without H2" }, HasH2 = false },
            withH2 with { Org = withH2.Org with { TradingName = "With H2" }, HasH2 = true }
        };

        var result = aligner.DedupeOrganisations(organisations);

        result.Count.ShouldBe(1);
        result[0].Org.TradingName.ShouldBe("With H2");
    }

    [TestMethod]
    public void DedupeOrganisations_WithMultipleRegistrationsAllHasH2False_PicksFirstOccurrence()
    {
        var first = Organisation();
        var second = Organisation();
        var organisations = new[]
        {
            first with { Org = first.Org with { TradingName = "First" } },
            second with { Org = second.Org with { TradingName = "Second" } }
        };

        var result = aligner.DedupeOrganisations(organisations);

        result.Count.ShouldBe(1);
        result[0].Org.TradingName.ShouldBe("First");
    }

    [TestMethod]
    public void DedupeOrganisations_WithDifferentSubmitters_KeepsBoth()
    {
        var first = Organisation();
        var second = Organisation();
        var organisations = new[]
        {
            first with { Org = first.Org with { SubmitterId = Guid.NewGuid().ToString() } },
            second with { Org = second.Org with { SubmitterId = Guid.NewGuid().ToString() } }
        };

        var result = aligner.DedupeOrganisations(organisations);

        result.Count.ShouldBe(2);
    }

    [TestMethod]
    public void DedupeOrganisations_AppliesNoObligationOrNameFiltering()
    {
        var second = Organisation();
        var organisations = new[]
        {
            Organisation() with { ObligationStatus = "N" },
            second with { Org = second.Org with { SubmitterId = Guid.NewGuid().ToString(), OrganisationName = "   " } }
        };

        var result = aligner.DedupeOrganisations(organisations);

        result.Count.ShouldBe(2);
    }

    // ─────────────────────────── Align ───────────────────────────

    [TestMethod]
    public void Align_WithObligatedOrganisationAndMatchingPom_ProducesProducerRecord()
    {
        var organisations = new[] { Organisation() };
        var poms = new[] { Pom() };

        var result = aligner.Align(organisations, poms, ["PL"]).ToList();

        result.Count.ShouldBe(1);
        var producer = result[0];
        producer.OrganisationId.ShouldBe(1);
        producer.SubsidiaryId.ShouldBe("SUB-1");
        producer.TradingName.ShouldBe("Trading Co");
        producer.ProducerName.ShouldBe("Org Co");
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
    public void Align_WithObligatedOrganisationAndNoPoms_ProducesRecordWithNoReportedMaterials()
    {
        // E.g. a holding company obligated in its own right, whose subsidiaries submit all the POM
        // data on its behalf - it still needs a record so callers don't have to fall back to a
        // separate, unfiltered organisation population to find its identity.
        var organisations = new[] { Organisation() };

        var result = aligner.Align(organisations, [], ["PL"]).ToList();

        result.Count.ShouldBe(1);
        result[0].OrganisationId.ShouldBe(1);
        result[0].ReportedMaterials.ShouldBeEmpty();
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
        var organisation = Organisation();
        var organisations = new[] { organisation with { Org = organisation.Org with { OrganisationName = "   " } } };
        var poms = new[] { Pom() };

        var result = aligner.Align(organisations, poms, ["PL"]);

        result.ShouldBeEmpty();
    }

    [TestMethod]
    public void Align_WithNoMatchingPoms_ProducesRecordWithNoReportedMaterials()
    {
        // The org still gets a record - e.g. a holding company obligated in its own right, whose
        // subsidiaries report on its behalf - just with no reported materials.
        var organisations = new[] { Organisation() };
        var poms = new[] { Pom() with { SubsidiaryId = "OTHER-SUB" } };

        var result = aligner.Align(organisations, poms, ["PL"]).ToList();

        result.Count.ShouldBe(1);
        result[0].ReportedMaterials.ShouldBeEmpty();
    }

    [TestMethod]
    public void Align_WithPomForDifferentSubmitter_ProducesRecordWithNoReportedMaterials()
    {
        var organisations = new[] { Organisation() };
        var poms = new[] { Pom() with { SubmitterId = Guid.NewGuid().ToString() } };

        var result = aligner.Align(organisations, poms, ["PL"]).ToList();

        result.Count.ShouldBe(1);
        result[0].ReportedMaterials.ShouldBeEmpty();
    }

    [TestMethod]
    public void Align_WithPomMissingPackagingType_ProducesRecordWithNoReportedMaterials()
    {
        var organisations = new[] { Organisation() };
        var poms = new[] { Pom() with { PackagingType = null } };

        var result = aligner.Align(organisations, poms, ["PL"]).ToList();

        result.Count.ShouldBe(1);
        result[0].ReportedMaterials.ShouldBeEmpty();
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
    public void Align_WithHouseholdDrinksContainersAndGlassMaterial_IncludesPom()
    {
        var organisations = new[] { Organisation() };
        var poms = new[] { Pom() with { PackagingType = "HDC", PackagingMaterial = "GL" } };

        var result = aligner.Align(organisations, poms, ["GL"]).ToList();

        result[0].ReportedMaterials.ShouldNotBeEmpty();
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

    private static FlaggedOrganisation Organisation() => new()
    {
        Org = new PayCalOrganisation
        {
            OrganisationId = 1,
            SubsidiaryId = "SUB-1",
            SubmitterId = SubmitterId.ToString(),
            OrganisationName = "Org Co",
            TradingName = "Trading Co",
            SubmissionPeriodYear = 2024,
            RegulatorStatus = "Accepted",
            JoinerDate = "2024-01-01",
            LeaverDate = "2024-12-31",
            StatusCode = "Active"
        },
        ObligationStatus = "O",
        NumDaysObligated = 200,
        ErrorCode = null,
        HasH1 = false,
        HasH2 = false
    };

    private static PayCalPom Pom() => new()
    {
        OrganisationId = 1,
        SubsidiaryId = "SUB-1",
        SubmitterId = SubmitterId.ToString(),
        PackagingMaterial = "PL",
        PackagingType = "HH",
        SubmissionPeriod = "2024-P1",
        PackagingMaterialWeight = 100d,
        RamRagRating = "G"
    };
}
