using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures;
using EPR.Calculator.Service.Function.Utils;
using Microsoft.Identity.Client.AppConfig;

namespace EPR.Calculator.Service.Function.UnitTests.Services
{
    [TestClass]
    public class CalcResultServiceTest
    {
        private IFixture _fixture = null!;
        private ApplicationDBContext _dbContext = null!;
        private CalcResultService _sut = null!;

        [TestInitialize]
        public void Init()
        {
            _fixture = TestFixtures.New();
            _dbContext = _fixture.Freeze<ApplicationDBContext>();
            _sut = _fixture.Create<CalcResultService>();
        }

        [TestCleanup]
        public void TearDown()
        {
            _dbContext.Database.EnsureDeleted();
        }

        [TestMethod]
        public async Task StoreProjectedH1Data_WorksAsExpected()
        {
            var projectedProducers = ImmutableList.Create(
                new CalcResultH1ProjectedProducer
                {
                    ProducerId = 1,
                    SubsidiaryId = null,
                    Level = "1",
                    SubmissionPeriodCode = "2025-H1",
                    IsSubtotal = true,
                    H1ProjectedTonnageByMaterial = new Dictionary<string, CalcResultH1ProjectedProducerMaterialTonnage>
                    {
                        [MaterialCodes.Aluminium] = MkH1MaterialTonnage(),
                        [MaterialCodes.Glass] = MkH1MaterialTonnage(isGlass: true),
                        [MaterialCodes.PaperOrCard] = MkH1MaterialTonnage(),
                        [MaterialCodes.Steel] = MkH1MaterialTonnage()
                    }
                },
                new CalcResultH1ProjectedProducer
                {
                    ProducerId = 1,
                    SubsidiaryId = null,
                    Level = "2",
                    SubmissionPeriodCode = "2025-H1",
                    IsSubtotal = false,
                    H1ProjectedTonnageByMaterial = new Dictionary<string, CalcResultH1ProjectedProducerMaterialTonnage>
                    {
                        [MaterialCodes.Aluminium] = MkH1MaterialTonnage(),
                        [MaterialCodes.Glass] = MkH1MaterialTonnage(isGlass: true),
                        [MaterialCodes.PaperOrCard] = MkH1MaterialTonnage()
                    }
                },
                new CalcResultH1ProjectedProducer
                {
                    ProducerId = 1,
                    SubsidiaryId = "A",
                    Level = "2",
                    SubmissionPeriodCode = "2025-H1",
                    IsSubtotal = false,
                    H1ProjectedTonnageByMaterial = new Dictionary<string, CalcResultH1ProjectedProducerMaterialTonnage>
                    {
                        [MaterialCodes.Aluminium] = MkH1MaterialTonnage(),
                        [MaterialCodes.Steel] = MkH1MaterialTonnage()
                    }
                }
            );
            await _sut.StoreProjectedH1Data(1, projectedProducers);

            var storedH1 = await _dbContext.TransformProjectedH1.ToImmutableListAsync();
            storedH1.Count.ShouldBe(9);
            storedH1.Where(p => p.ProducerId == 1 && p.SubsidiaryId == null && p.Level == "1").ToList().Count.ShouldBe(4);
            storedH1.Where(p => p.ProducerId == 1 && p.SubsidiaryId == null && p.Level == "2").ToList().Count.ShouldBe(3);
            storedH1.Where(p => p.ProducerId == 1 && p.SubsidiaryId == "A" && p.Level == "2").ToList().Count.ShouldBe(2);
        }

        [TestMethod]
        public async Task StoreProjectedH2Data_WorksAsExpected()
        {
            var projectedProducers = ImmutableList.Create(
                new CalcResultH2ProjectedProducer
                {
                    ProducerId = 1,
                    SubsidiaryId = null,
                    Level = "1",
                    SubmissionPeriodCode = "2025-H2",
                    IsSubtotal = true,
                    H2ProjectedTonnageByMaterial = new Dictionary<string, CalcResultH2ProjectedProducerMaterialTonnage>
                    {
                        [MaterialCodes.Aluminium] = MkH2MaterialTonnage(),
                        [MaterialCodes.Glass] = MkH2MaterialTonnage(isGlass: true),
                        [MaterialCodes.PaperOrCard] = MkH2MaterialTonnage(),
                        [MaterialCodes.Steel] = MkH2MaterialTonnage()
                    }
                },
                new CalcResultH2ProjectedProducer
                {
                    ProducerId = 1,
                    SubsidiaryId = null,
                    Level = "2",
                    SubmissionPeriodCode = "2025-H2",
                    IsSubtotal = false,
                    H2ProjectedTonnageByMaterial = new Dictionary<string, CalcResultH2ProjectedProducerMaterialTonnage>
                    {
                        [MaterialCodes.Aluminium] = MkH2MaterialTonnage(),
                        [MaterialCodes.Glass] = MkH2MaterialTonnage(isGlass: true),
                        [MaterialCodes.PaperOrCard] = MkH2MaterialTonnage()
                    }
                },
                new CalcResultH2ProjectedProducer
                {
                    ProducerId = 1,
                    SubsidiaryId = "A",
                    Level = "2",
                    SubmissionPeriodCode = "2025-H2",
                    IsSubtotal = false,
                    H2ProjectedTonnageByMaterial = new Dictionary<string, CalcResultH2ProjectedProducerMaterialTonnage>
                    {
                        [MaterialCodes.Aluminium] = MkH2MaterialTonnage(),
                        [MaterialCodes.Steel] = MkH2MaterialTonnage()
                    }
                }
            );
            await _sut.StoreProjectedH2Data(1, projectedProducers);

            var storedH2 = await _dbContext.TransformProjectedH2.ToImmutableListAsync();
            storedH2.Count.ShouldBe(9);
            storedH2.Where(p => p.ProducerId == 1 && p.SubsidiaryId == null && p.Level == "1").ToList().Count.ShouldBe(4);
            storedH2.Where(p => p.ProducerId == 1 && p.SubsidiaryId == null && p.Level == "2").ToList().Count.ShouldBe(3);
            storedH2.Where(p => p.ProducerId == 1 && p.SubsidiaryId == "A" && p.Level == "2").ToList().Count.ShouldBe(2);
        }

        [TestMethod]
        public async Task ReadH1ProjectedData_WorksAsExpected()
        {
            _dbContext.AddRange(new List<TransformProjectedH1>
            {
                MkTransformProjectedH1(1, 1, null, MaterialCodes.Aluminium, "1"),
                MkTransformProjectedH1(1, 1, null, MaterialCodes.Glass, "1", isGlass: true),
                MkTransformProjectedH1(1, 1, null, MaterialCodes.PaperOrCard, "1"),
                MkTransformProjectedH1(1, 1, null, MaterialCodes.Aluminium, "2"),
                MkTransformProjectedH1(1, 1, null, MaterialCodes.Glass, "2", isGlass: true),
                MkTransformProjectedH1(1, 1, null, MaterialCodes.PaperOrCard, "2"),
                MkTransformProjectedH1(1, 1, "A", MaterialCodes.Aluminium, "2"),
                MkTransformProjectedH1(1, 1, "A", MaterialCodes.Glass, "2", isGlass: true),
                MkTransformProjectedH1(1, 1, "A", MaterialCodes.PaperOrCard, "2"),
                MkTransformProjectedH1(2, 1, "A", MaterialCodes.Aluminium, "2"),
                MkTransformProjectedH1(2, 1, "A", MaterialCodes.Glass, "2", isGlass: true),
                MkTransformProjectedH1(2, 1, "A", MaterialCodes.PaperOrCard, "2")
             });
            await _dbContext.SaveChangesAsync();
            
            var result = await _sut.ReadH1ProjectedData(1);
            result.Count.ShouldBe(3);
            result.First(p => p.ProducerId == 1 && p.SubsidiaryId == null && p.Level == "1").H1ProjectedTonnageByMaterial.Count.ShouldBe(3);
            result.First(p => p.ProducerId == 1 && p.SubsidiaryId == null && p.Level == "2").H1ProjectedTonnageByMaterial.Count.ShouldBe(3);
            result.First(p => p.ProducerId == 1 && p.SubsidiaryId == "A" && p.Level == "2").H1ProjectedTonnageByMaterial.Count.ShouldBe(3);
        }

        [TestMethod]
        public async Task ReadH2ProjectedData_WorksAsExpected()
        {
            _dbContext.AddRange(new List<TransformProjectedH2>
            {
                MkTransformProjectedH2(1, 1, null, MaterialCodes.Aluminium, "1"),
                MkTransformProjectedH2(1, 1, null, MaterialCodes.Glass, "1", isGlass: true),
                MkTransformProjectedH2(1, 1, null, MaterialCodes.PaperOrCard, "1"),
                MkTransformProjectedH2(1, 1, null, MaterialCodes.Aluminium, "2"),
                MkTransformProjectedH2(1, 1, null, MaterialCodes.Glass, "2", isGlass: true),
                MkTransformProjectedH2(1, 1, null, MaterialCodes.PaperOrCard, "2"),
                MkTransformProjectedH2(1, 1, "A", MaterialCodes.Aluminium, "2"),
                MkTransformProjectedH2(1, 1, "A", MaterialCodes.Glass, "2", isGlass: true),
                MkTransformProjectedH2(1, 1, "A", MaterialCodes.PaperOrCard, "2"),
                MkTransformProjectedH2(2, 1, "A", MaterialCodes.Aluminium, "2"),
                MkTransformProjectedH2(2, 1, "A", MaterialCodes.Glass, "2", isGlass: true),
                MkTransformProjectedH2(2, 1, "A", MaterialCodes.PaperOrCard, "2")
             });
            await _dbContext.SaveChangesAsync();
            
            var result = await _sut.ReadH2ProjectedData(1);
            result.Count.ShouldBe(3);
            result.First(p => p.ProducerId == 1 && p.SubsidiaryId == null && p.Level == "1").H2ProjectedTonnageByMaterial.Count.ShouldBe(3);
            result.First(p => p.ProducerId == 1 && p.SubsidiaryId == null && p.Level == "2").H2ProjectedTonnageByMaterial.Count.ShouldBe(3);
            result.First(p => p.ProducerId == 1 && p.SubsidiaryId == "A" && p.Level == "2").H2ProjectedTonnageByMaterial.Count.ShouldBe(3);
        }

        [TestMethod]
        public async Task StoreScaledData_WorksAsExpected()
        {
            var scaled = ImmutableList.Create(
                new CalcResultScaledupProducer
                {
                    ProducerId = 101001,
                    SubsidiaryId = null,
                    ProducerName = "Allied Packaging",
                    Level = "1",
                    SubmissionPeriodCode = "2024-P2",
                    DaysInSubmissionPeriod = 91,
                    DaysInWholePeriod = 91,
                    ScaleupFactor = 2,
                    PomData = new List<ScaledupPomEntry>
                    {
                        new ScaledupPomEntry(1, PackagingTypes.Household, 1000, 2000),
                        new ScaledupPomEntry(1, PackagingTypes.PublicBin, 100, 200),
                        new ScaledupPomEntry(1, PackagingTypes.ConsumerWaste, 500, 1000),
                        new ScaledupPomEntry(2, PackagingTypes.Household, 1000, 2000),
                        new ScaledupPomEntry(2, PackagingTypes.PublicBin, 100, 200),
                        new ScaledupPomEntry(2, PackagingTypes.HouseholdDrinksContainers, 120, 240),
                        new ScaledupPomEntry(2, PackagingTypes.ConsumerWaste, 500, 1000),
                    },
                }
            );
            await _sut.StoreScaledData(1, scaled);

            var storedScaled = await _dbContext.TransformScaled.ToImmutableListAsync();
            storedScaled.Count.ShouldBe(7);
            storedScaled.Any(p => p.MaterialId == 1 && p.PackagingType == PackagingTypes.Household && p.Tonnage == 1000 & p.ScaledTonnage == 2000);
            storedScaled.Any(p => p.MaterialId == 1 && p.PackagingType == PackagingTypes.ConsumerWaste && p.Tonnage == 500 & p.ScaledTonnage == 1000);
            storedScaled.Any(p => p.MaterialId == 2 && p.PackagingType == PackagingTypes.HouseholdDrinksContainers && p.Tonnage == 120 & p.ScaledTonnage == 240);
        }

        [TestMethod]
        public async Task ReadScaledData_WorksAsExpected()
        {
            _dbContext.AddRange(new List<TransformScaled>
            {   
                MkTransformScaled(1, 1, null, 1, "HH"),
                MkTransformScaled(1, 1, null, 1, "PB"),
                MkTransformScaled(1, 1, "A", 2, "HH"),
                MkTransformScaled(1, 1, "A", 2, "PB"),
                MkTransformScaled(1, 2, "B", 3, "CW"),
                MkTransformScaled(1, 2, "B", 4, "HDC"),
                MkTransformScaled(2, 2, "B", 3, "CW"),
                MkTransformScaled(2, 2, "B", 4, "HDC"),
            });
            await _dbContext.SaveChangesAsync();
            
            var result = await _sut.ReadScaledData(1);
            result.Count.ShouldBe(3);
            result.First(p => p.ProducerId == 1 && p.SubsidiaryId == null).PomData.Count.ShouldBe(2);
            result.First(p => p.ProducerId == 1 && p.SubsidiaryId == "A").PomData.Count.ShouldBe(2);
            result.First(p => p.ProducerId == 2 && p.SubsidiaryId == "B").PomData.Count.ShouldBe(2);
        }

        [TestMethod]
        public async Task StorePartialData_WorksAsExpected()
        {
            var partial = ImmutableList.Create(
                new CalcResultPartialObligation
                {
                    ProducerId = 101001,
                    SubsidiaryId = null,
                    ProducerName = "Allied Packaging",
                    Level = "1",
                    SubmissionYear = 2024,
                    DaysInSubmissionYear = 365,
                    DaysObligated = 91,
                    ObligatedFactor = 0.3m,
                    JoiningDate = "15/07/2024",
                    PartialObligationTonnageByMaterial = new Dictionary<string, CalcResultPartialObligationTonnage>()
                    {
                        [MaterialCodes.Aluminium] = MkPartialMaterialTonnage(isModulated: true),
                        [MaterialCodes.Steel] = MkPartialMaterialTonnage(isModulated: true),
                        [MaterialCodes.Glass] = MkPartialMaterialTonnage(isModulated: true, isGlass: true),
                    }
                }
            );
            await _sut.StorePartialData(1, partial);

            var storedPartial = await _dbContext.TransformPartial.ToImmutableListAsync();
            storedPartial.Count.ShouldBe(3);
            storedPartial.Any(p => p.ProducerId == 101001 && p.SubsidiaryId == null && p.MaterialCode == MaterialCodes.Aluminium);
            storedPartial.Any(p => p.ProducerId == 101001 && p.SubsidiaryId == null && p.MaterialCode == MaterialCodes.Steel);
            storedPartial.Any(p => p.ProducerId == 101001 && p.SubsidiaryId == null && p.MaterialCode == MaterialCodes.Glass);
        }

        [TestMethod]
        public async Task ReadPartialData_WorksAsExpected()
        {
            _dbContext.AddRange(new List<TransformPartial>
            {   
                MkTransformPartial(1, 1, null, MaterialCodes.Aluminium, "1", isModulated: true),
                MkTransformPartial(1, 1, null, MaterialCodes.Glass, "1", isModulated: true, isGlass: true),
                MkTransformPartial(1, 1, "A", MaterialCodes.Steel, "2", isModulated: true),
                MkTransformPartial(1, 1, "A", MaterialCodes.OtherMaterials, "2", isModulated: true),
                MkTransformPartial(1, 2, "B", MaterialCodes.Aluminium, "2", isModulated: true),
                MkTransformPartial(1, 2, "B", MaterialCodes.Glass, "2", isModulated: true, isGlass: true),
                MkTransformPartial(2, 1, null, MaterialCodes.Aluminium, "1", isModulated: true),
                MkTransformPartial(2, 1, null, MaterialCodes.Glass, "1", isModulated: true, isGlass: true),
            });
            await _dbContext.SaveChangesAsync();
            
            var result = await _sut.ReadPartialData(1);
            result.Count.ShouldBe(3);
            result.First(p => p.ProducerId == 1 && p.SubsidiaryId == null).PartialObligationTonnageByMaterial.Count.ShouldBe(2);
            result.First(p => p.ProducerId == 1 && p.SubsidiaryId == "A").PartialObligationTonnageByMaterial.Count.ShouldBe(2);
            result.First(p => p.ProducerId == 2 && p.SubsidiaryId == "B").PartialObligationTonnageByMaterial.Count.ShouldBe(2);
        }

        private TransformProjectedH1 MkTransformProjectedH1(int runId, int producerId, string? subsidiaryId, string materialCode, string level, bool isGlass = false) {
            return new TransformProjectedH1
            {
                CalculatorRunId = runId,
                ProducerId = producerId,
                SubsidiaryId = subsidiaryId,
                Level = level,
                SubmissionPeriodCode = "2025-H1",
                MaterialCode = materialCode,
                HouseholdTonnage = 100,
                HouseholdTonnageRed = 50,
                HouseholdTonnageAmber = 0,
                HouseholdTonnageGreen = 0,
                HouseholdTonnageRedMedical = 0,
                HouseholdTonnageAmberMedical = 0,
                HouseholdTonnageGreenMedical = 0,
                PublicBinTonnage = 200,
                PublicBinTonnageRed = 100,
                PublicBinTonnageAmber = 0,
                PublicBinTonnageGreen = 0,
                PublicBinTonnageRedMedical = 0,
                PublicBinTonnageAmberMedical = 0,
                PublicBinTonnageGreenMedical = 0,
                HDCTonnage = isGlass ? 300 : null,
                HDCTonnageRed = isGlass ? 150 : null,
                HDCTonnageAmber = isGlass ? 0 : null,
                HDCTonnageGreen = isGlass ? 0 : null,
                HDCTonnageRedMedical = isGlass ? 0 : null,
                HDCTonnageAmberMedical = isGlass ? 0 : null,
                HDCTonnageGreenMedical = isGlass ? 0 : null,
                ProjectedHouseholdTonnage = 100,
                ProjectedHouseholdTonnageRed = 50,
                ProjectedHouseholdTonnageAmber = 0,
                ProjectedHouseholdTonnageGreen = 0,
                ProjectedHouseholdTonnageRedMedical = 0,
                ProjectedHouseholdTonnageAmberMedical = 0,
                ProjectedHouseholdTonnageGreenMedical = 0,
                ProjectedPublicBinTonnage = 200,
                ProjectedPublicBinTonnageRed = 100,
                ProjectedPublicBinTonnageAmber = 0,
                ProjectedPublicBinTonnageGreen = 0,
                ProjectedPublicBinTonnageRedMedical = 0,
                ProjectedPublicBinTonnageAmberMedical = 0,
                ProjectedPublicBinTonnageGreenMedical = 0,
                ProjectedHDCTonnage = isGlass ? 300 : null,
                ProjectedHDCTonnageRed = isGlass ? 150 : null,
                ProjectedHDCTonnageAmber = isGlass ? 0 : null,
                ProjectedHDCTonnageGreen = isGlass ? 0 : null,
                ProjectedHDCTonnageRedMedical = isGlass ? 0 : null,
                ProjectedHDCTonnageAmberMedical = isGlass ? 0 : null,
                ProjectedHDCTonnageGreenMedical = isGlass ? 0 : null,
                H2RamProportionsRed = 100,
                H2RamProportionsAmber = 0,
                H2RamProportionsGreen = 0,
                H2RamProportionsRedMedical = 0,
                H2RamProportionsAmberMedical = 0,
                H2RamProportionsGreenMedical = 0
            };
        }

        private TransformProjectedH2 MkTransformProjectedH2(int runId, int producerId, string? subsidiaryId, string materialCode, string level, bool isGlass = false) {
            return new TransformProjectedH2
            {
                CalculatorRunId = runId,
                ProducerId = producerId,
                SubsidiaryId = subsidiaryId,
                Level = level,
                SubmissionPeriodCode = "2025-H2",
                MaterialCode = materialCode,
                HouseholdTonnage = 100,
                HouseholdTonnageRed = 50,
                HouseholdTonnageAmber = 0,
                HouseholdTonnageGreen = 0,
                HouseholdTonnageRedMedical = 0,
                HouseholdTonnageAmberMedical = 0,
                HouseholdTonnageGreenMedical = 0,
                PublicBinTonnage = 200,
                PublicBinTonnageRed = 100,
                PublicBinTonnageAmber = 0,
                PublicBinTonnageGreen = 0,
                PublicBinTonnageRedMedical = 0,
                PublicBinTonnageAmberMedical = 0,
                PublicBinTonnageGreenMedical = 0,
                HDCTonnage = isGlass ? 300 : null,
                HDCTonnageRed = isGlass ? 150 : null,
                HDCTonnageAmber = isGlass ? 0 : null,
                HDCTonnageGreen = isGlass ? 0 : null,
                HDCTonnageRedMedical = isGlass ? 0 : null,
                HDCTonnageAmberMedical = isGlass ? 0 : null,
                HDCTonnageGreenMedical = isGlass ? 0 : null,
                ProjectedHouseholdTonnage = 100,
                ProjectedHouseholdTonnageRed = 50,
                ProjectedHouseholdTonnageAmber = 0,
                ProjectedHouseholdTonnageGreen = 0,
                ProjectedHouseholdTonnageRedMedical = 0,
                ProjectedHouseholdTonnageAmberMedical = 0,
                ProjectedHouseholdTonnageGreenMedical = 0,
                ProjectedPublicBinTonnage = 200,
                ProjectedPublicBinTonnageRed = 100,
                ProjectedPublicBinTonnageAmber = 0,
                ProjectedPublicBinTonnageGreen = 0,
                ProjectedPublicBinTonnageRedMedical = 0,
                ProjectedPublicBinTonnageAmberMedical = 0,
                ProjectedPublicBinTonnageGreenMedical = 0,
                ProjectedHDCTonnage = isGlass ? 300 : null,
                ProjectedHDCTonnageRed = isGlass ? 150 : null,
                ProjectedHDCTonnageAmber = isGlass ? 0 : null,
                ProjectedHDCTonnageGreen = isGlass ? 0 : null,
                ProjectedHDCTonnageRedMedical = isGlass ? 0 : null,
                ProjectedHDCTonnageAmberMedical = isGlass ? 0 : null,
                ProjectedHDCTonnageGreenMedical = isGlass ? 0 : null
            };
        }

        private CalcResultH2ProjectedProducerMaterialTonnage MkH2MaterialTonnage(bool isGlass = false)
        {
            return new CalcResultH2ProjectedProducerMaterialTonnage
            {
                HouseholdTonnage = 100,
                HouseholdRAMTonnage = new RAMTonnage
                {
                    RedTonnage = 50,
                    AmberTonnage = 0,
                    GreenTonnage = 0,
                    RedMedicalTonnage = 0,
                    AmberMedicalTonnage = 0,
                    GreenMedicalTonnage = 0
                },
                PublicBinTonnage = 200,
                PublicBinRAMTonnage = new RAMTonnage
                {
                    RedTonnage = 100,
                    AmberTonnage = 0,
                    GreenTonnage = 0,
                    RedMedicalTonnage = 0,
                    AmberMedicalTonnage = 0,
                    GreenMedicalTonnage = 0
                },
                HouseholdDrinksContainerTonnage = isGlass ? 300 : 0,
                HouseholdDrinksContainerRAMTonnage = isGlass ? new RAMTonnage
                {
                    RedTonnage = 150,
                    AmberTonnage = 0,
                    GreenTonnage = 0,
                    RedMedicalTonnage = 0,
                    AmberMedicalTonnage = 0,
                    GreenMedicalTonnage = 0
                } : null,
                HouseholdTonnageWithoutRAM = 50,
                PublicBinTonnageWithoutRAM = 100,
                HouseholdDrinksContainerTonnageWithoutRAM = isGlass ? 150 : null,
                ProjectedHouseholdTonnage = 50,
                ProjectedHouseholdRAMTonnage = new RAMTonnage
                {
                    RedTonnage = 100,
                    AmberTonnage = 0,
                    GreenTonnage = 0,
                    RedMedicalTonnage = 0,
                    AmberMedicalTonnage = 0,
                    GreenMedicalTonnage = 0
                },
                ProjectedPublicBinTonnage = 200,
                ProjectedPublicBinRAMTonnage = new RAMTonnage
                {
                    RedTonnage = 200,
                    AmberTonnage = 0,
                    GreenTonnage = 0,
                    RedMedicalTonnage = 0,
                    AmberMedicalTonnage = 0,
                    GreenMedicalTonnage = 0
                },
                ProjectedHouseholdDrinksContainerTonnage = isGlass ? 300 : null,
                ProjectedHouseholdDrinksContainerRAMTonnage = isGlass ? new RAMTonnage
                {
                    RedTonnage = 300,
                    AmberTonnage = 0,
                    GreenTonnage = 0,
                    RedMedicalTonnage = 0,
                    AmberMedicalTonnage = 0,
                    GreenMedicalTonnage = 0
                } : null
            };
        }

        private CalcResultH1ProjectedProducerMaterialTonnage MkH1MaterialTonnage(bool isGlass = false)
        {
            return new CalcResultH1ProjectedProducerMaterialTonnage
            {
                HouseholdTonnage = 100,
                HouseholdRAMTonnage = new RAMTonnage
                {
                    RedTonnage = 50,
                    AmberTonnage = 0,
                    GreenTonnage = 0,
                    RedMedicalTonnage = 0,
                    AmberMedicalTonnage = 0,
                    GreenMedicalTonnage = 0
                },
                PublicBinTonnage = 200,
                PublicBinRAMTonnage = new RAMTonnage
                {
                    RedTonnage = 100,
                    AmberTonnage = 0,
                    GreenTonnage = 0,
                    RedMedicalTonnage = 0,
                    AmberMedicalTonnage = 0,
                    GreenMedicalTonnage = 0
                },
                HouseholdDrinksContainerTonnage = isGlass ? 300 : 0,
                HouseholdDrinksContainerRAMTonnage = isGlass ? new RAMTonnage
                {
                    RedTonnage = 150,
                    AmberTonnage = 0,
                    GreenTonnage = 0,
                    RedMedicalTonnage = 0,
                    AmberMedicalTonnage = 0,
                    GreenMedicalTonnage = 0
                } : null,
                HouseholdTonnageWithoutRAM = 50,
                PublicBinTonnageWithoutRAM = 100,
                HouseholdDrinksContainerTonnageWithoutRAM = isGlass ? 150 : null,
                ProjectedHouseholdTonnage = 50,
                ProjectedHouseholdRAMTonnage = new RAMTonnage
                {
                    RedTonnage = 100,
                    AmberTonnage = 0,
                    GreenTonnage = 0,
                    RedMedicalTonnage = 0,
                    AmberMedicalTonnage = 0,
                    GreenMedicalTonnage = 0
                },
                ProjectedPublicBinTonnage = 200,
                ProjectedPublicBinRAMTonnage = new RAMTonnage
                {
                    RedTonnage = 200,
                    AmberTonnage = 0,
                    GreenTonnage = 0,
                    RedMedicalTonnage = 0,
                    AmberMedicalTonnage = 0,
                    GreenMedicalTonnage = 0
                },
                ProjectedHouseholdDrinksContainerTonnage = isGlass ? 300 : null,
                ProjectedHouseholdDrinksContainerRAMTonnage = isGlass ? new RAMTonnage
                {
                    RedTonnage = 300,
                    AmberTonnage = 0,
                    GreenTonnage = 0,
                    RedMedicalTonnage = 0,
                    AmberMedicalTonnage = 0,
                    GreenMedicalTonnage = 0
                } : null,
                H2RamProportions = new RAMProportions
                {
                    Red = 100,
                    Amber = 0,
                    Green = 0,
                    RedMedical = 0,
                    AmberMedical = 0,
                    GreenMedical = 0
                }
            };
        }

        private TransformScaled MkTransformScaled(int runId, int producerId, string? subId, int materialId, string packagingType)
        {
            return new TransformScaled()
            {
                CalculatorRunId = runId,
                ProducerId = producerId, 
                SubsidiaryId = subId,
                ProducerName = "Producer",
                TradingName = "Trading",
                SubmissionPeriodCode = "2024-P2",
                Level = "1",
                IsSubTotal = false,
                DaysInSubmissionPeriod = 180,
                DaysInWholePeriod = 365,
                ScaleupFactor = 2,
                MaterialId = materialId,
                PackagingType = packagingType,
                Tonnage = 10,
                ScaledTonnage = 20
            };
        }

        private CalcResultPartialObligationTonnage MkPartialMaterialTonnage(bool isModulated = true, bool isGlass = false, decimal obligatedFactor = 0.5m)
        {
            return new CalcResultPartialObligationTonnage
            {
                ObligatedFactor = obligatedFactor,
                HouseholdTonnage = 100,
                HouseholdRAMTonnage = isModulated ? new RAMTonnage
                {
                    RedTonnage = 50,
                    AmberTonnage = 0,
                    GreenTonnage = 0,
                    RedMedicalTonnage = 0,
                    AmberMedicalTonnage = 0,
                    GreenMedicalTonnage = 0
                } : null,
                PublicBinTonnage = 200,
                PublicBinRAMTonnage = isModulated ? new RAMTonnage
                {
                    RedTonnage = 100,
                    AmberTonnage = 0,
                    GreenTonnage = 0,
                    RedMedicalTonnage = 0,
                    AmberMedicalTonnage = 0,
                    GreenMedicalTonnage = 0
                } : null,
                HouseholdDrinksContainersTonnage = isGlass ? 300 : 0,
                HouseholdDrinksContainersRAMTonnage = isModulated && isGlass ? new RAMTonnage
                {
                    RedTonnage = 150,
                    AmberTonnage = 0,
                    GreenTonnage = 0,
                    RedMedicalTonnage = 0,
                    AmberMedicalTonnage = 0,
                    GreenMedicalTonnage = 0
                } : null,
                SelfManagedConsumerWasteTonnage = 50
            };
        }

        private TransformPartial MkTransformPartial(int runId, int producerId, string? subsidiaryId, string materialCode, string level, bool isModulated = true, bool isGlass = false) {
            return new TransformPartial
            {
                CalculatorRunId = runId,
                ProducerId = producerId,
                SubsidiaryId = subsidiaryId,
                ProducerName = "Producer",
                TradingName = "Trading",
                Level = level,
                SubmissionYear = 2025,
                DaysInSubmissionYear = 365,
                JoiningDate = "15/07/2025",
                DaysObligated = 180,
                ObligatedFactor = 0.5m,
                MaterialCode = materialCode,
                HouseholdTonnage = 100,
                HouseholdTonnageRed = isModulated ? 50 : null,
                HouseholdTonnageAmber = isModulated ? 0 : null,
                HouseholdTonnageGreen = isModulated ? 0 : null,
                HouseholdTonnageRedMedical = isModulated ? 0 : null,
                HouseholdTonnageAmberMedical = isModulated ? 0 : null,
                HouseholdTonnageGreenMedical = isModulated ? 0 : null,
                PublicBinTonnage = 200,
                PublicBinTonnageRed = isModulated ? 100 : null,
                PublicBinTonnageAmber = isModulated ? 0 : null,
                PublicBinTonnageGreen = isModulated ? 0 : null,
                PublicBinTonnageRedMedical = isModulated ? 0 : null,
                PublicBinTonnageAmberMedical = isModulated ? 0 : null,
                PublicBinTonnageGreenMedical = isModulated ? 0 : null,
                HDCTonnage = isGlass ? 300 : null,
                HDCTonnageRed = isModulated && isGlass ? 150 : null,
                HDCTonnageAmber = isModulated && isGlass ? 0 : null,
                HDCTonnageGreen = isModulated && isGlass ? 0 : null,
                HDCTonnageRedMedical = isModulated && isGlass ? 0 : null,
                HDCTonnageAmberMedical = isModulated && isGlass ? 0 : null,
                HDCTonnageGreenMedical = isModulated && isGlass ? 0 : null,
                SMCWTonnage = 50
            };
        }
    }
}

