using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Builder;
using EPR.Calculator.Service.Function.Builder.ScaledupProducers;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Features.Common;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Helpers;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;

namespace EPR.Calculator.Service.Function.UnitTests.Builder;

using static CalcRunLaDisposalCostBuilderTests;

[TestCategory(TestCategories.ResultBuilder)]
[TestClass]
public class CalcResultScaledupProducersBuilderTest : TestsFor<CalcResultScaledupProducersBuilder>
{
    private CalculatorRunOrganisationDataMaster calcRunOrganisationDataMaster = null!;
    private CalculatorRunPomDataMaster calcRunPomDataMaster = null!;
    private IImmutableList<MaterialDetail> materialDetails = null!;
    private int pcId;
    private readonly RunContext runContext = TestDataHelper.CalculatorRun2024;

    protected override void TestInitialize()
    {
        calcRunPomDataMaster = new CalculatorRunPomDataMaster
        {
            Id = 1,
            RelativeYear = new RelativeYear(2024),
            EffectiveFrom = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "Test User"
        };
        dbContext.CalculatorRunPomDataMaster.Add(calcRunPomDataMaster);

        calcRunOrganisationDataMaster = new CalculatorRunOrganisationDataMaster
        {
            Id = 11,
            RelativeYear = new RelativeYear(2025),
            EffectiveFrom = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "Test User"
        };
        dbContext.CalculatorRunOrganisationDataMaster.Add(calcRunOrganisationDataMaster);

        dbContext.CalculatorRuns.Add(new CalculatorRun
        {
            Id = runContext.RunId,
            RelativeYear = runContext.RelativeYear,
            Name = runContext.RunName,
            CalculatorRunOrganisationDataMaster = calcRunOrganisationDataMaster,
            CalculatorRunPomDataMaster = calcRunPomDataMaster
        });

        pcId = 4;
        var materials = new List<Material>
        {
            new() { Id = 1, Code = "AL", Name = "Aluminium", Description = "Aluminium" },
            new() { Id = 2, Code = "FC", Name = "Fibre composite", Description = "Fibre composite" },
            new() { Id = 3, Code = "GL", Name = "Glass", Description = "Glass" },
            new() { Id = pcId, Code = "PC", Name = "Paper or card", Description = "Paper or card" },
            new() { Id = 5, Code = "PL", Name = "Plastic", Description = "Plastic" },
            new() { Id = 6, Code = "ST", Name = "Steel", Description = "Steel" },
            new() { Id = 7, Code = "WD", Name = "Wood", Description = "Wood" },
            new() { Id = 8, Code = "OT", Name = "Other materials", Description = "Other materials" }
        };
        dbContext.Material.AddRange(materials);
        materialDetails = materials.ToDetails();

        dbContext.SaveChanges();
    }

    private List<L1Producer> PrepareNonScaledUpProducer()
    {
        var producerDetail = new ProducerDetail
        {
            Id = 1,
            CalculatorRunId = runContext.RunId,
            ProducerId = 11,
            SubsidiaryId = "Subsidary 1",
            ProducerName = "Producer Name"
        };

        // TODO ProducerReportedMaterials is mutable, but cannot be defined on creation!?
        // It's also an ICollection, so can't add multiple (AddRAnge)
        producerDetail.ProducerReportedMaterials.Add(
            new ProducerReportedMaterial
            {
                MaterialId = pcId,
                PackagingType = "HH",
                SubmissionPeriod = "2025-H1"
            }
        );
        producerDetail.ProducerReportedMaterials.Add(
            new ProducerReportedMaterial
            {
                MaterialId = pcId,
                PackagingType = "HDC",
                SubmissionPeriod = "2025-H1"
            }
        );
        producerDetail.ProducerReportedMaterials.Add(
            new ProducerReportedMaterial
            {
                MaterialId = pcId,
                PackagingType = "HH",
                SubmissionPeriod = "2025-H2"
            }
        );
        producerDetail.ProducerReportedMaterials.Add(
            new ProducerReportedMaterial
            {
                MaterialId = pcId,
                PackagingType = "HDC",
                SubmissionPeriod = "2025-H2"
            }
        );

        dbContext.ProducerDetail.Add(producerDetail);

        foreach (var subPeriod in new[] { "2025-H1", "2025-H2" })
        {
            dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial
            {
                PackagingType = "HH",
                SubmissionPeriod = subPeriod,
                ProducerDetail = producerDetail
            });
            dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial
            {
                PackagingType = "HDC",
                SubmissionPeriod = subPeriod,
                ProducerDetail = producerDetail
            });
        }

        dbContext.CalculatorRunOrganisationDataDetails.AddRange(
            new CalculatorRunOrganisationDataDetail
            {
                Id = 1,
                OrganisationId = 11,
                SubsidiaryId = null,
                OrganisationName = "Allied Packaging",
                LoadTimeStamp = DateTime.UtcNow,
                CalculatorRunOrganisationDataMaster = calcRunOrganisationDataMaster,
                ObligationStatus = ObligationStates.Obligated
            }
        );

        dbContext.CalculatorRunPomDataDetails.Add(
            new CalculatorRunPomDataDetail
            {
                LoadTimeStamp = DateTime.UtcNow,
                SubmissionPeriod = "2024-P1",
                SubmissionPeriodDesc = "desc",
                CalculatorRunPomDataMaster = calcRunPomDataMaster,
                OrganisationId = 10
            });

        dbContext.SubmissionPeriodLookup.Add(
            new SubmissionPeriodLookup
            {
                DaysInSubmissionPeriod = 0,
                DaysInWholePeriod = 0,
                EndDate = DateTime.UtcNow,
                StartDate = DateTime.UtcNow,
                ScaleupFactor = 1,
                SubmissionPeriod = "2024-P1",
                SubmissionPeriodDesc = string.Empty
            });

        dbContext.CalculatorRuns.Add(
            new CalculatorRun
            {
                Id = 2,
                RelativeYear = new RelativeYear(2024),
                Name = "Name"
            });

        var producerDetail1 = new ProducerDetail
        {
            Id = 2,
            CalculatorRunId = 2,
            ProducerId = 11,
            ProducerName = "Producer Test"
        };


        foreach (var subPeriod in new[] { "2025-H1", "2025-H2" })
        {
            dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial
            {
                PackagingType = "HH",
                SubmissionPeriod = subPeriod,
                ProducerDetail = producerDetail1
            });
        }

        dbContext.SaveChanges();

        return [new L1Producer(producerDetail.ProducerId, [producerDetail])];
    }

    private List<L1Producer> PrepareScaledUpProducer()
    {
        dbContext.CalculatorRunOrganisationDataDetails.AddRange(
            new CalculatorRunOrganisationDataDetail
            {
                Id = 2,
                OrganisationId = 12,
                SubsidiaryId = null,
                OrganisationName = "Allied Packaging 2",
                LoadTimeStamp = DateTime.UtcNow,
                CalculatorRunOrganisationDataMaster = calcRunOrganisationDataMaster,
                ObligationStatus = ObligationStates.Obligated
            }
        );

        dbContext.CalculatorRunPomDataDetails.AddRange(
            new CalculatorRunPomDataDetail
            {
                LoadTimeStamp = DateTime.UtcNow,
                SubmissionPeriod = "2024-P2",
                SubmissionPeriodDesc = "desc",
                CalculatorRunPomDataMaster = dbContext.CalculatorRunPomDataMaster.First(),
                OrganisationId = 12
            },
            new CalculatorRunPomDataDetail
            {
                LoadTimeStamp = DateTime.UtcNow,
                SubmissionPeriod = "2024-P2",
                SubmissionPeriodDesc = "desc",
                CalculatorRunPomDataMaster = dbContext.CalculatorRunPomDataMaster.First(),
                OrganisationId = 12
            }
        );

        var producerDetail = new ProducerDetail
        {
            CalculatorRunId = runContext.RunId,
            ProducerId = 12,
            SubsidiaryId = null,
            ProducerName = "Producer 12"
        };
        foreach (var subPeriod in new[] { "2024-P2", "2024-P4" })
        {
            producerDetail.ProducerReportedMaterials.Add(new ProducerReportedMaterial
            {
                PackagingType = "HH",
                ProducerDetail = producerDetail,
                SubmissionPeriod = subPeriod,
                MaterialId = 4,
                PackagingTonnage = 1m
            });
        }

        dbContext.ProducerDetail.Add(producerDetail);

        dbContext.SubmissionPeriodLookup.Add(
            new SubmissionPeriodLookup
            {
                DaysInSubmissionPeriod = 0,
                DaysInWholePeriod = 0,
                EndDate = DateTime.UtcNow,
                StartDate = DateTime.UtcNow,
                ScaleupFactor = 2.999M,
                SubmissionPeriod = "2024-P2",
                SubmissionPeriodDesc = string.Empty
            }
        );

        dbContext.SaveChanges();

        return [new L1Producer(producerDetail.ProducerId, [producerDetail])];
    }

    [TestMethod]
    public async Task Construct_WhenScaledUpDataPresent()
    {
        // Arrange
        var producers = PrepareScaledUpProducer();

        // Act
        var result = (await testSubject.ConstructAsync(runContext, materialDetails, producers)).Item2;

        // Assert
        Assert.AreEqual(1, result.ScaledupProducers!.Count());
        var producer = result.ScaledupProducers!.First(x => x.ProducerId == 12 && !x.IsSubtotalRow);
        var pomEntry = producer.PomData.Single(e => e.PackagingType == PackagingTypes.Household);
        Assert.AreEqual(1m, pomEntry.Tonnage);
        Assert.AreEqual(2.999m, pomEntry.ScaledTonnage);
        Assert.AreEqual(pcId, pomEntry.MaterialId);
    }

    /// <summary>
    ///     When any submission period for a producer requires scaling, all submission periods
    ///     for that producer should appear in the scaled-up producers summary.
    /// </summary>
    [TestMethod]
    public async Task Construct_AllPeriodsIncluded_WhenAnyPeriodRequiresScaling()
    {
        // Arrange - producer 12 with one scaled period ("2024-P2") and one normal period ("2024-P3")
        var producers = PrepareScaledUpProducer();

        dbContext.CalculatorRunPomDataDetails.Add(new CalculatorRunPomDataDetail
        {
            LoadTimeStamp = DateTime.UtcNow,
            SubmissionPeriod = "2024-P3",
            SubmissionPeriodDesc = "desc",
            CalculatorRunPomDataMaster = dbContext.CalculatorRunPomDataMaster.First(),
            OrganisationId = 12
        });
        dbContext.SubmissionPeriodLookup.Add(new SubmissionPeriodLookup
        {
            DaysInSubmissionPeriod = 0,
            DaysInWholePeriod = 0,
            EndDate = DateTime.UtcNow,
            StartDate = DateTime.UtcNow,
            ScaleupFactor = 1.0M,
            SubmissionPeriod = "2024-P3",
            SubmissionPeriodDesc = string.Empty
        });
        await dbContext.SaveChangesAsync();

        // Act
        var result = (await testSubject.ConstructAsync(runContext, materialDetails, producers)).Item2;

        // Assert - both the scaled and non-scaled period rows should appear
        Assert.AreEqual(2, result.ScaledupProducers!.Count());
        Assert.IsTrue(result.ScaledupProducers!.Any(p => p.SubmissionPeriodCode == "2024-P2" && p.ScaleupFactor == 2.999M));
        Assert.IsTrue(result.ScaledupProducers!.Any(p => p.SubmissionPeriodCode == "2024-P3" && p.ScaleupFactor == 1.0M));
    }

    [TestMethod]
    public async Task Construct_ReturnsModifiedProducerData()
    {
        // Arrange
        var nonScaledupProducers = PrepareNonScaledUpProducer();
        var scaledupProducers = PrepareScaledUpProducer();
        var producers = nonScaledupProducers.Concat(scaledupProducers).ToList();

        // Act
        var updatedProducers = (await testSubject.ConstructAsync(runContext, materialDetails, producers)).Item1;

        // Assert
        Assert.AreEqual(producers.Count, updatedProducers.Count);

        var updatedPds = updatedProducers.SelectMany(l1 => l1.Producers).ToList();
        var originalPds = producers.SelectMany(l1 => l1.Producers).ToList();

        foreach (var producer in updatedPds)
        {
            if (producer.ProducerId == 12 && producer.SubsidiaryId == null)
            {
                var reportedAlHH = producer.ProducerReportedMaterials.First(rm => rm.MaterialId == 4 && rm.PackagingType == "HH" && rm.SubmissionPeriod == "2024-P2");
                Assert.AreEqual(2.999m, reportedAlHH.PackagingTonnage);
            }
            else
            {
                var expectedProducer = originalPds.First(p => p.ProducerId == producer.ProducerId && p.SubsidiaryId == producer.SubsidiaryId);
                Assert.AreEqual(expectedProducer, producer);
            }
        }
    }

    [TestMethod]
    public async Task Construct_WhenNoScaledUpDataPresent()
    {
        // Arrange
        var producers = PrepareNonScaledUpProducer();

        // Act
        var result = (await testSubject.ConstructAsync(runContext, materialDetails, producers)).Item2;

        // Assert
        Assert.AreEqual(0, result.ScaledupProducers?.Count());
    }

    /// <summary>
    ///     A producer that has subsidiaries should appear with Level 2 (as should each
    ///     subsidiary), and an additional Level 1 subtotal row should be generated for the
    ///     producer's submission period.
    /// </summary>
    [TestMethod]
    public async Task Construct_ProducerWithSubsidiaries_HasLevel2RowsAndLevel1Subtotal()
    {
        // Arrange
        var producers = PrepareProducerWithSubsidiaries();

        // Act
        var result = (await testSubject.ConstructAsync(runContext, materialDetails, producers)).Item2;

        // Assert – one row per entity (parent + subsidiary) plus one subtotal
        Assert.AreEqual(3, result.ScaledupProducers!.Count());

        var level2Rows = result.ScaledupProducers!
            .Where(p => p.Level == CommonConstants.LevelTwo.ToString())
            .ToList();
        Assert.AreEqual(2, level2Rows.Count, "Expected parent and subsidiary to be Level 2");
        Assert.IsTrue(level2Rows.Any(p => string.IsNullOrEmpty(p.SubsidiaryId) && !p.IsSubtotalRow), "Parent should be Level 2");
        Assert.IsTrue(level2Rows.Any(p => p.SubsidiaryId == "Sub1"), "Subsidiary should be Level 2");

        var level1Rows = result.ScaledupProducers!
            .Where(p => p.Level == CommonConstants.LevelOne.ToString())
            .ToList();
        Assert.AreEqual(1, level1Rows.Count, "Expected one Level 1 subtotal row");
        Assert.IsTrue(level1Rows.Single().IsSubtotalRow, "Level 1 row should be marked as a subtotal");
    }

    private List<L1Producer> PrepareProducerWithSubsidiaries()
    {
        const int producerId = 20;
        const string subsidiaryId = "Sub1";
        const string submissionPeriod = "2025-P1";

        dbContext.SubmissionPeriodLookup.Add(new SubmissionPeriodLookup
        {
            SubmissionPeriod = submissionPeriod,
            SubmissionPeriodDesc = string.Empty,
            ScaleupFactor = 2.0M,
            DaysInSubmissionPeriod = 184,
            DaysInWholePeriod = 365,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow
        });

        dbContext.CalculatorRunPomDataDetails.Add(new CalculatorRunPomDataDetail
        {
            LoadTimeStamp = DateTime.UtcNow,
            SubmissionPeriod = submissionPeriod,
            SubmissionPeriodDesc = "desc",
            CalculatorRunPomDataMaster = calcRunPomDataMaster,
            OrganisationId = producerId
        });

        dbContext.CalculatorRunOrganisationDataDetails.AddRange(
            new CalculatorRunOrganisationDataDetail
            {
                OrganisationId = producerId,
                SubsidiaryId = null,
                OrganisationName = "Parent Corp",
                LoadTimeStamp = DateTime.UtcNow,
                CalculatorRunOrganisationDataMaster = calcRunOrganisationDataMaster,
                ObligationStatus = ObligationStates.Obligated
            },
            new CalculatorRunOrganisationDataDetail
            {
                OrganisationId = producerId,
                SubsidiaryId = subsidiaryId,
                OrganisationName = "Sub Corp",
                LoadTimeStamp = DateTime.UtcNow,
                CalculatorRunOrganisationDataMaster = calcRunOrganisationDataMaster,
                ObligationStatus = ObligationStates.Obligated
            }
        );

        var parentPd = new ProducerDetail
        {
            CalculatorRunId = runContext.RunId,
            ProducerId = producerId,
            SubsidiaryId = null,
            ProducerName = "Parent Corp"
        };
        parentPd.ProducerReportedMaterials.Add(new ProducerReportedMaterial
        {
            PackagingType = PackagingTypes.Household,
            SubmissionPeriod = submissionPeriod,
            MaterialId = pcId,
            PackagingTonnage = 10M
        });

        var subsidiaryPd = new ProducerDetail
        {
            CalculatorRunId = runContext.RunId,
            ProducerId = producerId,
            SubsidiaryId = subsidiaryId,
            ProducerName = "Sub Corp"
        };
        subsidiaryPd.ProducerReportedMaterials.Add(new ProducerReportedMaterial
        {
            PackagingType = PackagingTypes.Household,
            SubmissionPeriod = submissionPeriod,
            MaterialId = pcId,
            PackagingTonnage = 5M
        });

        dbContext.ProducerDetail.AddRange(parentPd, subsidiaryPd);
        dbContext.SaveChanges();

        return [new L1Producer(producerId, [parentPd, subsidiaryPd])];
    }

    [TestMethod]
    public void Should_Set_And_Get_PackagingType()
    {
        // Arrange
        var producerData = new ProducerData { PackagingType = "HDC", MaterialName = "Glass" };

        // Act
        var result = producerData.PackagingType;

        // Assert
        Assert.AreEqual("HDC", result);
    }

    [TestMethod]
    public void TestProducerDataFilteringForGlass()
    {
        // Arrange
        var producerData = new List<ProducerData>
        {
            new() { ProducerDetail = new ProducerDetail { ProducerId = 1 }, MaterialName = "Aluminum", PackagingType = "AL" },
            new() { ProducerDetail = new ProducerDetail { ProducerId = 2 }, MaterialName = "Glass", PackagingType = "HDC" }
        };

        var calcResult = TestDataHelper.GetCalcResult();
        calcResult.CalcResultScaledupProducers = new CalcResultScaledupProducers
        {
            ScaledupProducers =
            [
                new CalcResultScaledupProducer { ProducerId = 1, Level = "1", SubmissionPeriodCode = "2025-P1" },
                new CalcResultScaledupProducer { ProducerId = 3, Level = "1", SubmissionPeriodCode = "2025-P1" }
            ]
        };

        // Act
        var filteredData = producerData.Where(t => !calcResult.CalcResultScaledupProducers.ScaledupProducers.Any(i => i.ProducerId == t.ProducerDetail?.ProducerId)).ToList();

        // Assert
        Assert.AreEqual(1, filteredData.Count);
        Assert.AreEqual(2, filteredData.First().ProducerDetail?.ProducerId);
    }

    [TestMethod]
    public void AddExtraRowsTest()
    {
        var runProducerMaterialDetails = new List<CalcResultScaledupProducer>();
        runProducerMaterialDetails.AddRange([
            new CalcResultScaledupProducer
            {
                ProducerId = 1,
                Level = "1",
                SubmissionPeriodCode = "2025-P1"
            },
            new CalcResultScaledupProducer
            {
                ProducerId = 1,
                SubsidiaryId = "Sub1",
                Level = "2",
                SubmissionPeriodCode = "2025-P1"
            },
            new CalcResultScaledupProducer
            {
                ProducerId = 1,
                SubsidiaryId = "Sub2",
                Level = "2",
                SubmissionPeriodCode = "2025-P1"
            },
            new CalcResultScaledupProducer
            {
                ProducerId = 2,
                Level = "1",
                SubmissionPeriodCode = "2025-P1"
            },
            new CalcResultScaledupProducer
            {
                ProducerId = 2,
                SubsidiaryId = "Sub3",
                Level = "2",
                SubmissionPeriodCode = "2025-P1"
            },
            new CalcResultScaledupProducer
            {
                ProducerId = 2,
                SubsidiaryId = "Sub4",
                Level = "2",
                SubmissionPeriodCode = "2025-P1"
            }
        ]);

        var scaledupOrganisations = new List<Organisation>();
        scaledupOrganisations.AddRange([
            new Organisation
            {
                OrganisationId = 1,
                OrganisationName = "Allied Packaging"
            },
            new Organisation
            {
                OrganisationId = 2,
                OrganisationName = "Beeline Materials"
            }
        ]);

        CalcResultScaledupProducersBuilder.AddExtraRows(runProducerMaterialDetails, scaledupOrganisations);

        Assert.AreEqual(8, runProducerMaterialDetails.Count);
        var allProducersWithLevel2 = runProducerMaterialDetails.Where(x => x.SubsidiaryId == null);
        Assert.IsTrue(allProducersWithLevel2.All(x => x.Level == CommonConstants.LevelTwo.ToString()));

        var extraRows = runProducerMaterialDetails.Skip(Math.Max(0, runProducerMaterialDetails.Count - 2)).ToList();
        Assert.AreEqual(2, extraRows.Count());
        Assert.IsTrue(extraRows.All(x => x.IsSubtotalRow));
        Assert.AreEqual(2, runProducerMaterialDetails.Count(x => x.IsSubtotalRow));
    }
}