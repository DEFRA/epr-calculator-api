using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Utils;
using EPR.Calculator.Service.Function.Builder.ProjectedProducers;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.UnitTests.Builder.ProjectedProducers
{
    [TestCategory(TestCategories.ResultBuilder)]
    [TestClass]
    public class H1ProjectedProducersBuilderUtilsTest
    {
        private IImmutableList<MaterialDetail> materials = ImmutableList.Create<MaterialDetail>
        (
            new MaterialDetail { Id = 1, Code = "AL", Name = "Aluminium" },
            new MaterialDetail { Id = 2, Code = "GL", Name = "Glass" },
            new MaterialDetail { Id = 3, Code = "OT", Name = "Other materials" }
        );

        [TestMethod]
        public void GetProjectedProducers_With_H1_NoRam_H2_FullRam()
        {
            var (h1HHAlm, h1HdcGlass, h1NoRamReportedMaterials) = GetH1NoRamReportedMaterials();
            var (h2Alm, h2Glass, h2FullRamProjectedProducers) = GetH2FullRamProjectedProducers();
            var result = H1ProjectedProducersBuilderUtils.GetProjectedProducers(h1NoRamReportedMaterials, h2FullRamProjectedProducers, materials, "2026-H1");

            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(3, result.First().H1ProjectedTonnageByMaterial.Count());

            var projectedAluminium = result.First().H1ProjectedTonnageByMaterial[MaterialCodes.Aluminium];
            var projectedGlass = result.First().H1ProjectedTonnageByMaterial[MaterialCodes.Glass];

            Assert.AreEqual(ProducerReportedMaterialToRAMTonnage(h1HHAlm), projectedAluminium.HouseholdRAMTonnage);
            Assert.AreEqual(RamTonnage.Empty, projectedAluminium.PublicBinRAMTonnage);
            Assert.IsNull(projectedAluminium.HouseholdDrinksContainerRAMTonnage);
            Assert.AreEqual(RamTonnage.Empty, projectedGlass.HouseholdRAMTonnage);
            Assert.AreEqual(RamTonnage.Empty, projectedGlass.PublicBinRAMTonnage);
            Assert.AreEqual(ProducerReportedMaterialToRAMTonnage(h1HdcGlass), projectedGlass.HouseholdDrinksContainerRAMTonnage);

            Assert.AreEqual(h1HHAlm.PackagingTonnage, projectedAluminium.HouseholdTonnageWithoutRAM);
            Assert.AreEqual(0, projectedAluminium.PublicBinTonnageWithoutRAM);
            Assert.IsNull(projectedAluminium.HouseholdDrinksContainerTonnageWithoutRAM);
            Assert.AreEqual(0, projectedGlass.HouseholdTonnageWithoutRAM);
            Assert.AreEqual(0, projectedGlass.PublicBinTonnageWithoutRAM);
            Assert.AreEqual(h1HdcGlass.PackagingTonnage, projectedGlass.HouseholdDrinksContainerTonnageWithoutRAM);

            var h2AlmTotalTonnage = h2Alm.TotalTonnage();
            var expAlmH2Proportions = new RAMProportions{
                Red          = To6DP((h2Alm.ProjectedHouseholdRAMTonnage.Red + h2Alm.ProjectedPublicBinRAMTonnage.Red) / h2AlmTotalTonnage),
                Amber        = To6DP((h2Alm.ProjectedHouseholdRAMTonnage.Amber + h2Alm.ProjectedPublicBinRAMTonnage.Amber) / h2AlmTotalTonnage),
                Green        = To6DP((h2Alm.ProjectedHouseholdRAMTonnage.Green + h2Alm.ProjectedPublicBinRAMTonnage.Green) / h2AlmTotalTonnage),
                RedMedical   = To6DP((h2Alm.ProjectedHouseholdRAMTonnage.RedMedical + h2Alm.ProjectedPublicBinRAMTonnage.RedMedical) / h2AlmTotalTonnage),
                AmberMedical = To6DP((h2Alm.ProjectedHouseholdRAMTonnage.AmberMedical + h2Alm.ProjectedPublicBinRAMTonnage.AmberMedical) / h2AlmTotalTonnage),
                GreenMedical = To6DP((h2Alm.ProjectedHouseholdRAMTonnage.GreenMedical + h2Alm.ProjectedPublicBinRAMTonnage.GreenMedical) / h2AlmTotalTonnage)
            };
            var h2GlassTotalTonnage = h2Glass.TotalTonnage();
            var expGlassH2Proportions = new RAMProportions{
                Red          = To6DP((h2Glass.ProjectedHouseholdRAMTonnage.Red + h2Glass.ProjectedPublicBinRAMTonnage.Red + (h2Glass.ProjectedHouseholdDrinksContainerRAMTonnage?.Red ?? 0)) / h2GlassTotalTonnage),
                Amber        = To6DP((h2Glass.ProjectedHouseholdRAMTonnage.Amber + h2Glass.ProjectedPublicBinRAMTonnage.Amber + (h2Glass.ProjectedHouseholdDrinksContainerRAMTonnage?.Amber ?? 0)) / h2GlassTotalTonnage),
                Green        = To6DP((h2Glass.ProjectedHouseholdRAMTonnage.Green + h2Glass.ProjectedPublicBinRAMTonnage.Green + (h2Glass.ProjectedHouseholdDrinksContainerRAMTonnage?.Green ?? 0)) / h2GlassTotalTonnage),
                RedMedical   = To6DP((h2Glass.ProjectedHouseholdRAMTonnage.RedMedical + h2Glass.ProjectedPublicBinRAMTonnage.RedMedical + (h2Glass.ProjectedHouseholdDrinksContainerRAMTonnage?.RedMedical ?? 0)) / h2GlassTotalTonnage),
                AmberMedical = To6DP((h2Glass.ProjectedHouseholdRAMTonnage.AmberMedical + h2Glass.ProjectedPublicBinRAMTonnage.AmberMedical + (h2Glass.ProjectedHouseholdDrinksContainerRAMTonnage?.AmberMedical ?? 0)) / h2GlassTotalTonnage),
                GreenMedical = To6DP((h2Glass.ProjectedHouseholdRAMTonnage.GreenMedical + h2Glass.ProjectedPublicBinRAMTonnage.GreenMedical + (h2Glass.ProjectedHouseholdDrinksContainerRAMTonnage?.GreenMedical ?? 0)) / h2GlassTotalTonnage)
            };

            Assert.AreEqual(h1HHAlm.PackagingTonnage, projectedAluminium.ProjectedHouseholdTonnage);
            AssertWithin(To3DP(h1HHAlm.PackagingTonnage * expAlmH2Proportions.Red), projectedAluminium.ProjectedHouseholdRAMTonnage.Red);
            AssertWithin(To3DP(h1HHAlm.PackagingTonnage * expAlmH2Proportions.Amber), projectedAluminium.ProjectedHouseholdRAMTonnage.Amber);
            AssertWithin(To3DP(h1HHAlm.PackagingTonnage * expAlmH2Proportions.Green), projectedAluminium.ProjectedHouseholdRAMTonnage.Green);
            AssertWithin(To3DP(h1HHAlm.PackagingTonnage * expAlmH2Proportions.RedMedical), projectedAluminium.ProjectedHouseholdRAMTonnage.RedMedical);
            AssertWithin(To3DP(h1HHAlm.PackagingTonnage * expAlmH2Proportions.AmberMedical), projectedAluminium.ProjectedHouseholdRAMTonnage.AmberMedical);
            AssertWithin(To3DP(h1HHAlm.PackagingTonnage * expAlmH2Proportions.GreenMedical), projectedAluminium.ProjectedHouseholdRAMTonnage.GreenMedical);

            Assert.AreEqual(RamTonnage.Empty, projectedAluminium.ProjectedPublicBinRAMTonnage);
            Assert.IsNull(projectedAluminium.ProjectedHouseholdDrinksContainerRAMTonnage);
            Assert.AreEqual(RamTonnage.Empty, projectedGlass.ProjectedHouseholdRAMTonnage);
            Assert.AreEqual(RamTonnage.Empty, projectedGlass.ProjectedPublicBinRAMTonnage);

            Assert.AreEqual(h1HdcGlass.PackagingTonnage, projectedGlass.ProjectedHouseholdDrinksContainerTonnage!);
            AssertWithin(To3DP(h1HdcGlass.PackagingTonnage * expGlassH2Proportions.Red), projectedGlass.ProjectedHouseholdDrinksContainerRAMTonnage!.Red);
            AssertWithin(To3DP(h1HdcGlass.PackagingTonnage * expGlassH2Proportions.Amber), projectedGlass.ProjectedHouseholdDrinksContainerRAMTonnage!.Amber);
            AssertWithin(To3DP(h1HdcGlass.PackagingTonnage * expGlassH2Proportions.Green), projectedGlass.ProjectedHouseholdDrinksContainerRAMTonnage!.Green);
            AssertWithin(To3DP(h1HdcGlass.PackagingTonnage * expGlassH2Proportions.RedMedical), projectedGlass.ProjectedHouseholdDrinksContainerRAMTonnage!.RedMedical);
            AssertWithin(To3DP(h1HdcGlass.PackagingTonnage * expGlassH2Proportions.AmberMedical), projectedGlass.ProjectedHouseholdDrinksContainerRAMTonnage!.AmberMedical);
            AssertWithin(To3DP(h1HdcGlass.PackagingTonnage * expGlassH2Proportions.GreenMedical), projectedGlass.ProjectedHouseholdDrinksContainerRAMTonnage!.GreenMedical);
        }

        [TestMethod]
        public void GetProjectedProducers_With_H1_NoRam_H2_PartialRam()
        {
            var (h1HHAlm, h1HdcGlass, h1NoRamReportedMaterials) = GetH1NoRamReportedMaterials();
            var (h2Alm, h2Glass, h2PartialRamProjectedProducers) = GetH2PartialRamProjectedProducers();

            var result = H1ProjectedProducersBuilderUtils.GetProjectedProducers(h1NoRamReportedMaterials, h2PartialRamProjectedProducers, materials, "2026-H1");

            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(3, result.First().H1ProjectedTonnageByMaterial.Count());

            var projectedAluminium = result.First().H1ProjectedTonnageByMaterial[MaterialCodes.Aluminium];
            var projectedGlass = result.First().H1ProjectedTonnageByMaterial[MaterialCodes.Glass];

            var h2AlmTotalTonnage = h2Alm.TotalTonnage();
            var expAlmH2Proportions = new RAMProportions {
                Red          = To6DP((h2Alm.ProjectedHouseholdRAMTonnage.Red + h2Alm.ProjectedPublicBinRAMTonnage.Red) / h2AlmTotalTonnage),
                Amber        = To6DP((h2Alm.ProjectedHouseholdRAMTonnage.Amber + h2Alm.ProjectedPublicBinRAMTonnage.Amber) / h2AlmTotalTonnage),
                Green        = To6DP((h2Alm.ProjectedHouseholdRAMTonnage.Green + h2Alm.ProjectedPublicBinRAMTonnage.Green) / h2AlmTotalTonnage),
                RedMedical   = To6DP((h2Alm.ProjectedHouseholdRAMTonnage.RedMedical + h2Alm.ProjectedPublicBinRAMTonnage.RedMedical) / h2AlmTotalTonnage),
                AmberMedical = To6DP((h2Alm.ProjectedHouseholdRAMTonnage.AmberMedical + h2Alm.ProjectedPublicBinRAMTonnage.AmberMedical) / h2AlmTotalTonnage),
                GreenMedical = To6DP((h2Alm.ProjectedHouseholdRAMTonnage.GreenMedical + h2Alm.ProjectedPublicBinRAMTonnage.GreenMedical) / h2AlmTotalTonnage)
            };

            var h2GlassTotalTonnage = h2Glass.TotalTonnage();
            var expGlassH2Proportions = new RAMProportions {
                Red          = To6DP((h2Glass.ProjectedHouseholdRAMTonnage.Red + h2Glass.ProjectedPublicBinRAMTonnage.Red + (h2Glass.ProjectedHouseholdDrinksContainerRAMTonnage?.Red ?? 0)) / h2GlassTotalTonnage),
                Amber        = To6DP((h2Glass.ProjectedHouseholdRAMTonnage.Amber + h2Glass.ProjectedPublicBinRAMTonnage.Amber + (h2Glass.ProjectedHouseholdDrinksContainerRAMTonnage?.Amber ?? 0)) / h2GlassTotalTonnage),
                Green        = To6DP((h2Glass.ProjectedHouseholdRAMTonnage.Green + h2Glass.ProjectedPublicBinRAMTonnage.Green + (h2Glass.ProjectedHouseholdDrinksContainerRAMTonnage?.Green ?? 0)) / h2GlassTotalTonnage),
                RedMedical   = To6DP((h2Glass.ProjectedHouseholdRAMTonnage.RedMedical + h2Glass.ProjectedPublicBinRAMTonnage.RedMedical + (h2Glass.ProjectedHouseholdDrinksContainerRAMTonnage?.RedMedical ?? 0)) / h2GlassTotalTonnage),
                AmberMedical = To6DP((h2Glass.ProjectedHouseholdRAMTonnage.AmberMedical + h2Glass.ProjectedPublicBinRAMTonnage.AmberMedical + (h2Glass.ProjectedHouseholdDrinksContainerRAMTonnage?.AmberMedical ?? 0)) / h2GlassTotalTonnage),
                GreenMedical = To6DP((h2Glass.ProjectedHouseholdRAMTonnage.GreenMedical + h2Glass.ProjectedPublicBinRAMTonnage.GreenMedical + (h2Glass.ProjectedHouseholdDrinksContainerRAMTonnage?.GreenMedical ?? 0)) / h2GlassTotalTonnage)
            };

            Assert.AreEqual(h1HHAlm.PackagingTonnage, projectedAluminium.ProjectedHouseholdTonnage);
            AssertWithin(To3DP(h1HHAlm.PackagingTonnage * expAlmH2Proportions.Red), projectedAluminium.ProjectedHouseholdRAMTonnage.Red);
            AssertWithin(To3DP(h1HHAlm.PackagingTonnage * expAlmH2Proportions.Amber), projectedAluminium.ProjectedHouseholdRAMTonnage.Amber);
            AssertWithin(To3DP(h1HHAlm.PackagingTonnage * expAlmH2Proportions.Green), projectedAluminium.ProjectedHouseholdRAMTonnage.Green);
            AssertWithin(To3DP(h1HHAlm.PackagingTonnage * expAlmH2Proportions.RedMedical), projectedAluminium.ProjectedHouseholdRAMTonnage.RedMedical);
            AssertWithin(To3DP(h1HHAlm.PackagingTonnage * expAlmH2Proportions.AmberMedical), projectedAluminium.ProjectedHouseholdRAMTonnage.AmberMedical);
            AssertWithin(To3DP(h1HHAlm.PackagingTonnage * expAlmH2Proportions.GreenMedical), projectedAluminium.ProjectedHouseholdRAMTonnage.GreenMedical);

            Assert.AreEqual(RamTonnage.Empty, projectedAluminium.ProjectedPublicBinRAMTonnage);
            Assert.IsNull(projectedAluminium.ProjectedHouseholdDrinksContainerRAMTonnage);
            Assert.AreEqual(RamTonnage.Empty, projectedGlass.ProjectedHouseholdRAMTonnage);
            Assert.AreEqual(RamTonnage.Empty, projectedGlass.ProjectedPublicBinRAMTonnage);

            Assert.AreEqual(h1HdcGlass.PackagingTonnage, projectedGlass.ProjectedHouseholdDrinksContainerTonnage!);
            AssertWithin(To3DP(h1HdcGlass.PackagingTonnage * expGlassH2Proportions.Red), projectedGlass.ProjectedHouseholdDrinksContainerRAMTonnage!.Red);
            AssertWithin(To3DP(h1HdcGlass.PackagingTonnage * expGlassH2Proportions.Amber), projectedGlass.ProjectedHouseholdDrinksContainerRAMTonnage!.Amber);
            AssertWithin(To3DP(h1HdcGlass.PackagingTonnage * expGlassH2Proportions.Green), projectedGlass.ProjectedHouseholdDrinksContainerRAMTonnage!.Green);
            AssertWithin(To3DP(h1HdcGlass.PackagingTonnage * expGlassH2Proportions.RedMedical), projectedGlass.ProjectedHouseholdDrinksContainerRAMTonnage!.RedMedical);
            AssertWithin(To3DP(h1HdcGlass.PackagingTonnage * expGlassH2Proportions.AmberMedical), projectedGlass.ProjectedHouseholdDrinksContainerRAMTonnage!.AmberMedical);
            AssertWithin(To3DP(h1HdcGlass.PackagingTonnage * expGlassH2Proportions.GreenMedical), projectedGlass.ProjectedHouseholdDrinksContainerRAMTonnage!.GreenMedical);
        }

        [TestMethod]
        public void GetProjectedProducers_With_H1_PartialRam_H2_FullRam()
        {
            var (h1HHAlm, h1HdcGlass, h1PartialRamReportedMaterials) = GetH1PartialRamReportedMaterials();
            var (h2Alm, h2Glass, h2FullRamProjectedProducers) = GetH2FullRamProjectedProducers();
            var result = H1ProjectedProducersBuilderUtils.GetProjectedProducers(h1PartialRamReportedMaterials, h2FullRamProjectedProducers, materials, "2026-H1");

            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(3, result.First().H1ProjectedTonnageByMaterial.Count());

            var projectedAluminium = result.First().H1ProjectedTonnageByMaterial[MaterialCodes.Aluminium];
            var projectedGlass = result.First().H1ProjectedTonnageByMaterial[MaterialCodes.Glass];

            Assert.AreEqual(ProducerReportedMaterialToRAMTonnage(h1HHAlm), projectedAluminium.HouseholdRAMTonnage);
            Assert.AreEqual(RamTonnage.Empty, projectedAluminium.PublicBinRAMTonnage);
            Assert.IsNull(projectedAluminium.HouseholdDrinksContainerRAMTonnage);
            Assert.AreEqual(RamTonnage.Empty, projectedGlass.HouseholdRAMTonnage);
            Assert.AreEqual(RamTonnage.Empty, projectedGlass.PublicBinRAMTonnage);
            Assert.AreEqual(ProducerReportedMaterialToRAMTonnage(h1HdcGlass), projectedGlass.HouseholdDrinksContainerRAMTonnage);

            var expH1AlmWithoutRam = CalcWithoutRam(h1HHAlm);
            var expH1HdcGlassWithoutRam = CalcWithoutRam(h1HdcGlass);
            Assert.AreEqual(expH1AlmWithoutRam, projectedAluminium.HouseholdTonnageWithoutRAM);
            Assert.AreEqual(0, projectedAluminium.PublicBinTonnageWithoutRAM);
            Assert.IsNull(projectedAluminium.HouseholdDrinksContainerTonnageWithoutRAM);
            Assert.AreEqual(0, projectedGlass.HouseholdTonnageWithoutRAM);
            Assert.AreEqual(0, projectedGlass.PublicBinTonnageWithoutRAM);
            Assert.AreEqual(expH1HdcGlassWithoutRam, projectedGlass.HouseholdDrinksContainerTonnageWithoutRAM);

            var h2AlmTotalTonnage = h2Alm.TotalTonnage();
            var expAlmH2Proportions = new RAMProportions {
                Red          = To6DP((h2Alm.ProjectedHouseholdRAMTonnage.Red + h2Alm.ProjectedPublicBinRAMTonnage.Red) / h2AlmTotalTonnage),
                Amber        = To6DP((h2Alm.ProjectedHouseholdRAMTonnage.Amber + h2Alm.ProjectedPublicBinRAMTonnage.Amber) / h2AlmTotalTonnage),
                Green        = To6DP((h2Alm.ProjectedHouseholdRAMTonnage.Green + h2Alm.ProjectedPublicBinRAMTonnage.Green) / h2AlmTotalTonnage),
                RedMedical   = To6DP((h2Alm.ProjectedHouseholdRAMTonnage.RedMedical + h2Alm.ProjectedPublicBinRAMTonnage.RedMedical) / h2AlmTotalTonnage),
                AmberMedical = To6DP((h2Alm.ProjectedHouseholdRAMTonnage.AmberMedical + h2Alm.ProjectedPublicBinRAMTonnage.AmberMedical) / h2AlmTotalTonnage),
                GreenMedical = To6DP((h2Alm.ProjectedHouseholdRAMTonnage.GreenMedical + h2Alm.ProjectedPublicBinRAMTonnage.GreenMedical) / h2AlmTotalTonnage)
            };
            var h2GlassTotalTonnage = h2Glass.TotalTonnage();
            var expGlassH2Proportions = new RAMProportions {
                Red          = To6DP((h2Glass.ProjectedHouseholdRAMTonnage.Red + h2Glass.ProjectedPublicBinRAMTonnage.Red + (h2Glass.ProjectedHouseholdDrinksContainerRAMTonnage?.Red ?? 0)) / h2GlassTotalTonnage),
                Amber        = To6DP((h2Glass.ProjectedHouseholdRAMTonnage.Amber + h2Glass.ProjectedPublicBinRAMTonnage.Amber + (h2Glass.ProjectedHouseholdDrinksContainerRAMTonnage?.Amber ?? 0)) / h2GlassTotalTonnage),
                Green        = To6DP((h2Glass.ProjectedHouseholdRAMTonnage.Green + h2Glass.ProjectedPublicBinRAMTonnage.Green + (h2Glass.ProjectedHouseholdDrinksContainerRAMTonnage?.Green ?? 0)) / h2GlassTotalTonnage),
                RedMedical   = To6DP((h2Glass.ProjectedHouseholdRAMTonnage.RedMedical + h2Glass.ProjectedPublicBinRAMTonnage.RedMedical + (h2Glass.ProjectedHouseholdDrinksContainerRAMTonnage?.RedMedical ?? 0)) / h2GlassTotalTonnage),
                AmberMedical = To6DP((h2Glass.ProjectedHouseholdRAMTonnage.AmberMedical + h2Glass.ProjectedPublicBinRAMTonnage.AmberMedical + (h2Glass.ProjectedHouseholdDrinksContainerRAMTonnage?.AmberMedical ?? 0)) / h2GlassTotalTonnage),
                GreenMedical = To6DP((h2Glass.ProjectedHouseholdRAMTonnage.GreenMedical + h2Glass.ProjectedPublicBinRAMTonnage.GreenMedical + (h2Glass.ProjectedHouseholdDrinksContainerRAMTonnage?.GreenMedical ?? 0)) / h2GlassTotalTonnage)
            };

            Assert.AreEqual(h1HHAlm.PackagingTonnage, projectedAluminium.ProjectedHouseholdTonnage);
            AssertWithin(To3DP(expH1AlmWithoutRam * expAlmH2Proportions.Red), projectedAluminium.ProjectedHouseholdRAMTonnage.Red);
            AssertWithin(To3DP((h1HHAlm.PackagingTonnageAmber ?? 0) + (expH1AlmWithoutRam * expAlmH2Proportions.Amber)), projectedAluminium.ProjectedHouseholdRAMTonnage.Amber);
            AssertWithin(To3DP(expH1AlmWithoutRam * expAlmH2Proportions.Green), projectedAluminium.ProjectedHouseholdRAMTonnage.Green);
            AssertWithin(To3DP(expH1AlmWithoutRam * expAlmH2Proportions.RedMedical), projectedAluminium.ProjectedHouseholdRAMTonnage.RedMedical);
            AssertWithin(To3DP((h1HHAlm.PackagingTonnageAmberMedical ?? 0) + (expH1AlmWithoutRam * expAlmH2Proportions.AmberMedical)), projectedAluminium.ProjectedHouseholdRAMTonnage.AmberMedical);
            AssertWithin(To3DP(expH1AlmWithoutRam * expAlmH2Proportions.GreenMedical), projectedAluminium.ProjectedHouseholdRAMTonnage.GreenMedical);

            Assert.AreEqual(RamTonnage.Empty, projectedAluminium.ProjectedPublicBinRAMTonnage);
            Assert.IsNull(projectedAluminium.ProjectedHouseholdDrinksContainerRAMTonnage);
            Assert.AreEqual(RamTonnage.Empty, projectedGlass.ProjectedHouseholdRAMTonnage);
            Assert.AreEqual(RamTonnage.Empty, projectedGlass.ProjectedPublicBinRAMTonnage);

            Assert.AreEqual(h1HdcGlass.PackagingTonnage, projectedGlass.ProjectedHouseholdDrinksContainerTonnage!);
            AssertWithin(To3DP((h1HdcGlass.PackagingTonnageRed ?? 0) + (expH1HdcGlassWithoutRam * expGlassH2Proportions.Red)), projectedGlass.ProjectedHouseholdDrinksContainerRAMTonnage!.Red);
            AssertWithin(To3DP(expH1HdcGlassWithoutRam * expGlassH2Proportions.Amber), projectedGlass.ProjectedHouseholdDrinksContainerRAMTonnage!.Amber);
            AssertWithin(To3DP((h1HdcGlass.PackagingTonnageGreen ?? 0) + (expH1HdcGlassWithoutRam * expGlassH2Proportions.Green)), projectedGlass.ProjectedHouseholdDrinksContainerRAMTonnage!.Green);
            AssertWithin(To3DP((h1HdcGlass.PackagingTonnageRedMedical ?? 0) + (expH1HdcGlassWithoutRam * expGlassH2Proportions.RedMedical)), projectedGlass.ProjectedHouseholdDrinksContainerRAMTonnage!.RedMedical);
            AssertWithin(To3DP(expH1HdcGlassWithoutRam * expGlassH2Proportions.AmberMedical), projectedGlass.ProjectedHouseholdDrinksContainerRAMTonnage!.AmberMedical);
            AssertWithin(To3DP(expH1HdcGlassWithoutRam * expGlassH2Proportions.GreenMedical), projectedGlass.ProjectedHouseholdDrinksContainerRAMTonnage!.GreenMedical);
        }

        [TestMethod]
        public void GetProjectedProducers_With_H1_FullRam_H2_FullRam()
        {
            var (h1HHAlm, h1HdcGlass, h1FullRamReportedMaterials) = GetH1FullRamReportedMaterials();
            var (_, _, h2FullRamProjectedProducers) = GetH2FullRamProjectedProducers();
            var result = H1ProjectedProducersBuilderUtils.GetProjectedProducers(h1FullRamReportedMaterials, h2FullRamProjectedProducers, materials, "2026-H1");

            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(3, result.First().H1ProjectedTonnageByMaterial.Count());

            var projectedAluminium = result.First().H1ProjectedTonnageByMaterial[MaterialCodes.Aluminium];
            var projectedGlass = result.First().H1ProjectedTonnageByMaterial[MaterialCodes.Glass];

            Assert.AreEqual(ProducerReportedMaterialToRAMTonnage(h1HHAlm), projectedAluminium.HouseholdRAMTonnage);
            Assert.AreEqual(RamTonnage.Empty, projectedAluminium.PublicBinRAMTonnage);
            Assert.IsNull(projectedAluminium.HouseholdDrinksContainerRAMTonnage);
            Assert.AreEqual(RamTonnage.Empty, projectedGlass.HouseholdRAMTonnage);
            Assert.AreEqual(RamTonnage.Empty, projectedGlass.PublicBinRAMTonnage);
            Assert.AreEqual(ProducerReportedMaterialToRAMTonnage(h1HdcGlass), projectedGlass.HouseholdDrinksContainerRAMTonnage);

            Assert.AreEqual(0, projectedAluminium.HouseholdTonnageWithoutRAM);
            Assert.AreEqual(0, projectedAluminium.PublicBinTonnageWithoutRAM);
            Assert.IsNull(projectedAluminium.HouseholdDrinksContainerTonnageWithoutRAM);
            Assert.AreEqual(0, projectedGlass.HouseholdTonnageWithoutRAM);
            Assert.AreEqual(0, projectedGlass.PublicBinTonnageWithoutRAM);
            Assert.AreEqual(0, projectedGlass.HouseholdDrinksContainerTonnageWithoutRAM);

            Assert.AreEqual(ProducerReportedMaterialToRAMTonnage(h1HHAlm),projectedAluminium.ProjectedHouseholdRAMTonnage);
            Assert.AreEqual(RamTonnage.Empty, projectedAluminium.ProjectedPublicBinRAMTonnage);
            Assert.IsNull(projectedAluminium.ProjectedHouseholdDrinksContainerRAMTonnage);
            Assert.AreEqual(RamTonnage.Empty, projectedGlass.ProjectedHouseholdRAMTonnage);
            Assert.AreEqual(RamTonnage.Empty, projectedGlass.ProjectedPublicBinRAMTonnage);
            Assert.AreEqual(ProducerReportedMaterialToRAMTonnage(h1HdcGlass),projectedGlass.ProjectedHouseholdDrinksContainerRAMTonnage);
        }

        [TestMethod]
        public void GetProjectedProducers_With_H1_PartialRam_H2_NoReportedMaterial()
        {
            var (h1HHAlm, h1HdcGlass, h1NoRamReportedMaterials) = GetH1PartialRamReportedMaterials();
            var (_, _, h2NoReportedMaterialProjectedProducers) = GetH2NoReportedMaterialProjectedProducers();
            var result = H1ProjectedProducersBuilderUtils.GetProjectedProducers(h1NoRamReportedMaterials, h2NoReportedMaterialProjectedProducers, materials, "2026-H1");

            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(3, result.First().H1ProjectedTonnageByMaterial.Count());

            var projectedAluminium = result.First().H1ProjectedTonnageByMaterial[MaterialCodes.Aluminium];
            var projectedGlass = result.First().H1ProjectedTonnageByMaterial[MaterialCodes.Glass];

            Assert.AreEqual(RAMProportions.Empty, projectedAluminium.H2RamProportions);
            Assert.AreEqual(RAMProportions.Empty, projectedGlass.H2RamProportions);

            var expH1AlmWithoutRam = CalcWithoutRam(h1HHAlm);
            var expH1HdcGlassWithoutRam = CalcWithoutRam(h1HdcGlass);
            Assert.AreEqual(expH1AlmWithoutRam, projectedAluminium.HouseholdTonnageWithoutRAM);
            Assert.AreEqual(0, projectedAluminium.PublicBinTonnageWithoutRAM);
            Assert.IsNull(projectedAluminium.HouseholdDrinksContainerTonnageWithoutRAM);
            Assert.AreEqual(0, projectedGlass.HouseholdTonnageWithoutRAM);
            Assert.AreEqual(0, projectedGlass.PublicBinTonnageWithoutRAM);
            Assert.AreEqual(expH1HdcGlassWithoutRam, projectedGlass.HouseholdDrinksContainerTonnageWithoutRAM);

            Assert.AreEqual(
                new RamTonnage {
                    Red          = (h1HHAlm.PackagingTonnageRed ?? 0) + expH1AlmWithoutRam,
                    Amber        = h1HHAlm.PackagingTonnageAmber ?? 0,
                    Green        = h1HHAlm.PackagingTonnageGreen ?? 0,
                    RedMedical   = h1HHAlm.PackagingTonnageRedMedical ?? 0,
                    AmberMedical = h1HHAlm.PackagingTonnageAmberMedical ?? 0,
                    GreenMedical = h1HHAlm.PackagingTonnageGreenMedical ?? 0
                },
                projectedAluminium.ProjectedHouseholdRAMTonnage
            );

            Assert.AreEqual(RamTonnage.Empty, projectedAluminium.ProjectedPublicBinRAMTonnage);
            Assert.IsNull(projectedAluminium.ProjectedHouseholdDrinksContainerRAMTonnage);
            Assert.AreEqual(RamTonnage.Empty, projectedGlass.ProjectedHouseholdRAMTonnage);
            Assert.AreEqual(RamTonnage.Empty, projectedGlass.ProjectedPublicBinRAMTonnage);

            Assert.AreEqual(
                new RamTonnage {
                    Red          = (h1HdcGlass.PackagingTonnageRed ?? 0) + expH1HdcGlassWithoutRam,
                    Amber        = h1HdcGlass.PackagingTonnageAmber ?? 0,
                    Green        = h1HdcGlass.PackagingTonnageGreen ?? 0,
                    RedMedical   = h1HdcGlass.PackagingTonnageRedMedical ?? 0,
                    AmberMedical = h1HdcGlass.PackagingTonnageAmberMedical ?? 0,
                    GreenMedical = h1HdcGlass.PackagingTonnageGreenMedical ?? 0
                },
                projectedGlass.ProjectedHouseholdDrinksContainerRAMTonnage
            );
        }

        [TestMethod]
        public void GetProjectedProducers_With_H1_NoReportedMaterial()
        {
            var (_, _, h1FullRamReportedMaterials) = GetH1FullRamReportedMaterials();
            var (_, _, h2FullRamProjectedProducers) = GetH2FullRamProjectedProducers();
            var result = H1ProjectedProducersBuilderUtils.GetProjectedProducers(h1FullRamReportedMaterials, h2FullRamProjectedProducers, materials, "2026-H1");

            Assert.AreEqual(1, result.Count());

            var projectedOtherMaterials = result.First().H1ProjectedTonnageByMaterial[MaterialCodes.OtherMaterials];
            Assert.AreEqual(RamTonnage.Empty, projectedOtherMaterials.HouseholdRAMTonnage);
            Assert.AreEqual(RamTonnage.Empty, projectedOtherMaterials.PublicBinRAMTonnage);
            Assert.IsNull(projectedOtherMaterials.HouseholdDrinksContainerRAMTonnage);
            Assert.AreEqual(0, projectedOtherMaterials.H2RamProportions.Red);
            Assert.AreEqual(0, projectedOtherMaterials.H2RamProportions.Amber);
            Assert.AreEqual(0, projectedOtherMaterials.H2RamProportions.Green);
            Assert.AreEqual(0, projectedOtherMaterials.H2RamProportions.RedMedical);
            Assert.AreEqual(0, projectedOtherMaterials.H2RamProportions.AmberMedical);
            Assert.AreEqual(0, projectedOtherMaterials.H2RamProportions.GreenMedical);
            Assert.AreEqual(RamTonnage.Empty, projectedOtherMaterials.HouseholdRAMTonnage);
            Assert.AreEqual(RamTonnage.Empty, projectedOtherMaterials.PublicBinRAMTonnage);
            Assert.IsNull(projectedOtherMaterials.HouseholdDrinksContainerRAMTonnage);
        }

        [TestMethod]
        public void GetProjectedProducers_With_H1_NoH2Producer()
        {
            var (h1HHAlm, h1HdcGlass, h1FullRamReportedMaterials) = GetH1FullRamReportedMaterials();
            var noH2Producers = new List<CalcResultH2ProjectedProducer>();
            var result = H1ProjectedProducersBuilderUtils.GetProjectedProducers(h1FullRamReportedMaterials, noH2Producers, materials, "2026-H1");

            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(3, result.First().ProjectedTonnageByMaterial.Count());

            var projectedAluminium = result.First().H1ProjectedTonnageByMaterial[MaterialCodes.Aluminium];
            var projectedGlass = result.First().H1ProjectedTonnageByMaterial[MaterialCodes.Glass];

            Assert.AreEqual(ProducerReportedMaterialToRAMTonnage(h1HHAlm), projectedAluminium.HouseholdRAMTonnage);
            Assert.AreEqual(RamTonnage.Empty, projectedAluminium.PublicBinRAMTonnage);
            Assert.IsNull(projectedAluminium.HouseholdDrinksContainerRAMTonnage);
            Assert.AreEqual(RamTonnage.Empty, projectedGlass.HouseholdRAMTonnage);
            Assert.AreEqual(RamTonnage.Empty, projectedGlass.PublicBinRAMTonnage);
            Assert.AreEqual(ProducerReportedMaterialToRAMTonnage(h1HdcGlass), projectedGlass.HouseholdDrinksContainerRAMTonnage);

            Assert.AreEqual(0, projectedAluminium.HouseholdTonnageWithoutRAM);
            Assert.AreEqual(0, projectedAluminium.PublicBinTonnageWithoutRAM);
            Assert.IsNull(projectedAluminium.HouseholdDrinksContainerTonnageWithoutRAM);
            Assert.AreEqual(0, projectedGlass.HouseholdTonnageWithoutRAM);
            Assert.AreEqual(0, projectedGlass.PublicBinTonnageWithoutRAM);
            Assert.AreEqual(0, projectedGlass.HouseholdDrinksContainerTonnageWithoutRAM);

            Assert.AreEqual(ProducerReportedMaterialToRAMTonnage(h1HHAlm),projectedAluminium.ProjectedHouseholdRAMTonnage);
            Assert.AreEqual(RamTonnage.Empty, projectedAluminium.ProjectedPublicBinRAMTonnage);
            Assert.IsNull(projectedAluminium.ProjectedHouseholdDrinksContainerRAMTonnage);
            Assert.AreEqual(RamTonnage.Empty, projectedGlass.ProjectedHouseholdRAMTonnage);
            Assert.AreEqual(RamTonnage.Empty, projectedGlass.ProjectedPublicBinRAMTonnage);
            Assert.AreEqual(ProducerReportedMaterialToRAMTonnage(h1HdcGlass),projectedGlass.ProjectedHouseholdDrinksContainerRAMTonnage);
        }

        [TestMethod]
        public void GetProjectedProducers_Use_HC_Subtotal_H2_Proportions()
        {
            var (h1HHAlm, h1HdcGlass, subH1HHAlm, subH1HdcGlass, h1NoRamReportedMaterials) = GetH1NoRamReportedMaterialsWithSubsidiary();
            var (h2SubtotalAlm, h2SubtotalGlass, h2FullRamProjectedProducers) = GetH2WithHCSubtotal();
            var result = H1ProjectedProducersBuilderUtils.GetProjectedProducers(h1NoRamReportedMaterials, h2FullRamProjectedProducers, materials, "2026-H1");

            Assert.AreEqual(2, result.Count());
            var hc = result.Where(p => p.ProducerId == 11 && p.SubsidiaryId == null).FirstOrDefault();
            var sub = result.Where(p => p.ProducerId == 11 && p.SubsidiaryId == "A").FirstOrDefault();
            Assert.IsNotNull(hc);
            Assert.IsNotNull(sub);
            Assert.AreEqual(3, hc.H1ProjectedTonnageByMaterial.Count());
            Assert.AreEqual(3, sub.H1ProjectedTonnageByMaterial.Count());

            var projectedHCAluminium = hc.H1ProjectedTonnageByMaterial[MaterialCodes.Aluminium];
            var projectedHCGlass = hc.H1ProjectedTonnageByMaterial[MaterialCodes.Glass];
            var projectedSubAluminium = sub.H1ProjectedTonnageByMaterial[MaterialCodes.Aluminium];
            var projectedSubGlass = sub.H1ProjectedTonnageByMaterial[MaterialCodes.Glass];

            var h2SubtotalAlmTotal = h2SubtotalAlm.TotalTonnage();
            var expAlmH2SubtotalProportions = new RAMProportions{
                Red          = To6DP((h2SubtotalAlm.ProjectedHouseholdRAMTonnage.Red + h2SubtotalAlm.ProjectedPublicBinRAMTonnage.Red) / h2SubtotalAlmTotal),
                Amber        = To6DP((h2SubtotalAlm.ProjectedHouseholdRAMTonnage.Amber + h2SubtotalAlm.ProjectedPublicBinRAMTonnage.Amber) / h2SubtotalAlmTotal),
                Green        = To6DP((h2SubtotalAlm.ProjectedHouseholdRAMTonnage.Green + h2SubtotalAlm.ProjectedPublicBinRAMTonnage.Green) / h2SubtotalAlmTotal),
                RedMedical   = To6DP((h2SubtotalAlm.ProjectedHouseholdRAMTonnage.RedMedical + h2SubtotalAlm.ProjectedPublicBinRAMTonnage.RedMedical) / h2SubtotalAlmTotal),
                AmberMedical = To6DP((h2SubtotalAlm.ProjectedHouseholdRAMTonnage.AmberMedical + h2SubtotalAlm.ProjectedPublicBinRAMTonnage.AmberMedical) / h2SubtotalAlmTotal),
                GreenMedical = To6DP((h2SubtotalAlm.ProjectedHouseholdRAMTonnage.GreenMedical + h2SubtotalAlm.ProjectedPublicBinRAMTonnage.GreenMedical) / h2SubtotalAlmTotal)
            };

            var h2SubtotalGlassTotal = h2SubtotalGlass.TotalTonnage();
            var expGlassH2SubtotalProportions = new RAMProportions{
                Red          = To6DP((h2SubtotalGlass.ProjectedHouseholdRAMTonnage.Red + h2SubtotalGlass.ProjectedPublicBinRAMTonnage.Red + (h2SubtotalGlass.ProjectedHouseholdDrinksContainerRAMTonnage?.Red ?? 0)) / h2SubtotalGlassTotal),
                Amber        = To6DP((h2SubtotalGlass.ProjectedHouseholdRAMTonnage.Amber + h2SubtotalGlass.ProjectedPublicBinRAMTonnage.Amber + (h2SubtotalGlass.ProjectedHouseholdDrinksContainerRAMTonnage?.Amber ?? 0)) / h2SubtotalGlassTotal),
                Green        = To6DP((h2SubtotalGlass.ProjectedHouseholdRAMTonnage.Green + h2SubtotalGlass.ProjectedPublicBinRAMTonnage.Green + (h2SubtotalGlass.ProjectedHouseholdDrinksContainerRAMTonnage?.Green ?? 0)) / h2SubtotalGlassTotal),
                RedMedical   = To6DP((h2SubtotalGlass.ProjectedHouseholdRAMTonnage.RedMedical + h2SubtotalGlass.ProjectedPublicBinRAMTonnage.RedMedical + (h2SubtotalGlass.ProjectedHouseholdDrinksContainerRAMTonnage?.RedMedical ?? 0)) / h2SubtotalGlassTotal),
                AmberMedical = To6DP((h2SubtotalGlass.ProjectedHouseholdRAMTonnage.AmberMedical + h2SubtotalGlass.ProjectedPublicBinRAMTonnage.AmberMedical + (h2SubtotalGlass.ProjectedHouseholdDrinksContainerRAMTonnage?.AmberMedical ?? 0)) / h2SubtotalGlassTotal),
                GreenMedical = To6DP((h2SubtotalGlass.ProjectedHouseholdRAMTonnage.GreenMedical + h2SubtotalGlass.ProjectedPublicBinRAMTonnage.GreenMedical + (h2SubtotalGlass.ProjectedHouseholdDrinksContainerRAMTonnage?.GreenMedical ?? 0)) / h2SubtotalGlassTotal)
            };

            Assert.AreEqual(h1HHAlm.PackagingTonnage, projectedHCAluminium.ProjectedHouseholdTonnage);
            AssertWithin(To3DP(h1HHAlm.PackagingTonnage * expAlmH2SubtotalProportions.Red), projectedHCAluminium.ProjectedHouseholdRAMTonnage.Red);
            AssertWithin(To3DP(h1HHAlm.PackagingTonnage * expAlmH2SubtotalProportions.Amber), projectedHCAluminium.ProjectedHouseholdRAMTonnage.Amber);
            AssertWithin(To3DP(h1HHAlm.PackagingTonnage * expAlmH2SubtotalProportions.Green), projectedHCAluminium.ProjectedHouseholdRAMTonnage.Green);
            AssertWithin(To3DP(h1HHAlm.PackagingTonnage * expAlmH2SubtotalProportions.RedMedical), projectedHCAluminium.ProjectedHouseholdRAMTonnage.RedMedical);
            AssertWithin(To3DP(h1HHAlm.PackagingTonnage * expAlmH2SubtotalProportions.AmberMedical), projectedHCAluminium.ProjectedHouseholdRAMTonnage.AmberMedical);
            AssertWithin(To3DP(h1HHAlm.PackagingTonnage * expAlmH2SubtotalProportions.GreenMedical), projectedHCAluminium.ProjectedHouseholdRAMTonnage.GreenMedical);

            Assert.AreEqual(h1HdcGlass.PackagingTonnage, projectedHCGlass.ProjectedHouseholdDrinksContainerTonnage!);
            AssertWithin(To3DP(h1HdcGlass.PackagingTonnage * expGlassH2SubtotalProportions.Red), projectedHCGlass.ProjectedHouseholdDrinksContainerRAMTonnage!.Red);
            AssertWithin(To3DP(h1HdcGlass.PackagingTonnage * expGlassH2SubtotalProportions.Amber), projectedHCGlass.ProjectedHouseholdDrinksContainerRAMTonnage!.Amber);
            AssertWithin(To3DP(h1HdcGlass.PackagingTonnage * expGlassH2SubtotalProportions.Green), projectedHCGlass.ProjectedHouseholdDrinksContainerRAMTonnage!.Green);
            AssertWithin(To3DP(h1HdcGlass.PackagingTonnage * expGlassH2SubtotalProportions.RedMedical), projectedHCGlass.ProjectedHouseholdDrinksContainerRAMTonnage!.RedMedical);
            AssertWithin(To3DP(h1HdcGlass.PackagingTonnage * expGlassH2SubtotalProportions.AmberMedical), projectedHCGlass.ProjectedHouseholdDrinksContainerRAMTonnage!.AmberMedical);
            AssertWithin(To3DP(h1HdcGlass.PackagingTonnage * expGlassH2SubtotalProportions.GreenMedical), projectedHCGlass.ProjectedHouseholdDrinksContainerRAMTonnage!.GreenMedical);

            Assert.AreEqual(subH1HHAlm.PackagingTonnage, projectedSubAluminium.ProjectedHouseholdTonnage);
            AssertWithin(To3DP(subH1HHAlm.PackagingTonnage * expAlmH2SubtotalProportions.Red), projectedSubAluminium.ProjectedHouseholdRAMTonnage.Red);
            AssertWithin(To3DP(subH1HHAlm.PackagingTonnage * expAlmH2SubtotalProportions.Amber), projectedSubAluminium.ProjectedHouseholdRAMTonnage.Amber);
            AssertWithin(To3DP(subH1HHAlm.PackagingTonnage * expAlmH2SubtotalProportions.Green), projectedSubAluminium.ProjectedHouseholdRAMTonnage.Green);
            AssertWithin(To3DP(subH1HHAlm.PackagingTonnage * expAlmH2SubtotalProportions.RedMedical), projectedSubAluminium.ProjectedHouseholdRAMTonnage.RedMedical);
            AssertWithin(To3DP(subH1HHAlm.PackagingTonnage * expAlmH2SubtotalProportions.AmberMedical), projectedSubAluminium.ProjectedHouseholdRAMTonnage.AmberMedical);
            AssertWithin(To3DP(subH1HHAlm.PackagingTonnage * expAlmH2SubtotalProportions.GreenMedical), projectedSubAluminium.ProjectedHouseholdRAMTonnage.GreenMedical);

            Assert.AreEqual(subH1HdcGlass.PackagingTonnage, projectedSubGlass.ProjectedHouseholdDrinksContainerTonnage!);
            AssertWithin(To3DP(subH1HdcGlass.PackagingTonnage * expGlassH2SubtotalProportions.Red), projectedSubGlass.ProjectedHouseholdDrinksContainerRAMTonnage!.Red);
            AssertWithin(To3DP(subH1HdcGlass.PackagingTonnage * expGlassH2SubtotalProportions.Amber), projectedSubGlass.ProjectedHouseholdDrinksContainerRAMTonnage!.Amber);
            AssertWithin(To3DP(subH1HdcGlass.PackagingTonnage * expGlassH2SubtotalProportions.Green), projectedSubGlass.ProjectedHouseholdDrinksContainerRAMTonnage!.Green);
            AssertWithin(To3DP(subH1HdcGlass.PackagingTonnage * expGlassH2SubtotalProportions.RedMedical), projectedSubGlass.ProjectedHouseholdDrinksContainerRAMTonnage!.RedMedical);
            AssertWithin(To3DP(subH1HdcGlass.PackagingTonnage * expGlassH2SubtotalProportions.AmberMedical), projectedSubGlass.ProjectedHouseholdDrinksContainerRAMTonnage!.AmberMedical);
            AssertWithin(To3DP(subH1HdcGlass.PackagingTonnage * expGlassH2SubtotalProportions.GreenMedical), projectedSubGlass.ProjectedHouseholdDrinksContainerRAMTonnage!.GreenMedical);
        }

        [TestMethod]
        public void ReconcileRoundingDifference_NoMissingRam()
        {
            var tonnage = 200;
            var ramTonnage = new RamTonnage { Red = 0, Amber = 20, Green = 30, RedMedical = 0, AmberMedical = 50, GreenMedical = 0 };
            var h2Proportions = new RAMProportions { Red = 0.5m, Amber = 0, Green = 0.5m, RedMedical = 0, AmberMedical = 0, GreenMedical = 0 };
            var withProportions = H1ProjectedProducersBuilderUtils.GetProportionateRam(ramTonnage, 100, h2Proportions);
            var roundingDiff = tonnage - withProportions.TotalRamTonnage();
            Assert.AreEqual(0, roundingDiff);
            var reconciled = H1ProjectedProducersBuilderUtils.ReconcileRoundingDifference(tonnage, withProportions);
            Assert.AreEqual(withProportions, reconciled);
        }

        [TestMethod]
        public void ReconcileRoundingDifference_MissingRam()
        {
            var tonnage = 200;
            var ramTonnage = new RamTonnage { Red = 0, Amber = 20, Green = 30, RedMedical = 0, AmberMedical = 50, GreenMedical = 0 };
            var h2Proportions = new RAMProportions { Red = 0.333333m, Amber = 0.333333m, Green = 0.333334m, RedMedical = 0, AmberMedical = 0, GreenMedical = 0 };
            var withProportions = H1ProjectedProducersBuilderUtils.GetProportionateRam(ramTonnage, 100, h2Proportions);
            var roundingDiff = tonnage - withProportions.TotalRamTonnage();
            Assert.IsTrue(roundingDiff > 0);
            var reconciled = H1ProjectedProducersBuilderUtils.ReconcileRoundingDifference(tonnage, withProportions);
            Assert.AreEqual(0, tonnage - reconciled.TotalRamTonnage());
            Assert.AreEqual(withProportions with { Green = withProportions.Green + roundingDiff }, reconciled);
        }

        [TestMethod]
        public void ReconcileRoundingDifference_SurplusRam()
        {
            var tonnage = 200;
            var ramTonnage = new RamTonnage { Red = 0, Amber = 20, Green = 30, RedMedical = 0, AmberMedical = 50, GreenMedical = 0 };
            var h2Proportions = new RAMProportions { Red = 0.333335m, Amber = 0.333335m, Green = 0.333330m, RedMedical = 0, AmberMedical = 0, GreenMedical = 0 };
            var withProportions = H1ProjectedProducersBuilderUtils.GetProportionateRam(ramTonnage, 100, h2Proportions);
            var roundingDiff = tonnage - withProportions.TotalRamTonnage();
            Assert.IsTrue(roundingDiff < 0);
            var reconciled = H1ProjectedProducersBuilderUtils.ReconcileRoundingDifference(tonnage, withProportions);
            Assert.AreEqual(0, tonnage - reconciled.TotalRamTonnage());
            Assert.AreEqual(withProportions with { Green = withProportions.Green + roundingDiff }, reconciled);
        }

        [TestMethod]
        public void ReconcileRoundingDifference_MissingRam_EqualDominantRam()
        {
            // Red and amber medical result in the same tonnages - assign missing ram to amber medical due to priority order
            var tonnage = 200;
            var ramTonnage = new RamTonnage { Red = 30, Amber = 40, Green = 0, RedMedical = 0, AmberMedical = 30, GreenMedical = 0 };
            var h2Proportions = new RAMProportions { Red = 0.333333m, Amber = 0, Green = 0.333334m, RedMedical = 0, AmberMedical = 0.333333m, GreenMedical = 0 };
            var withProportions = H1ProjectedProducersBuilderUtils.GetProportionateRam(ramTonnage, 100, h2Proportions);
            var roundingDiff = tonnage - withProportions.TotalRamTonnage();
            Assert.IsTrue(roundingDiff > 0);
            Assert.AreEqual(withProportions.Red, withProportions.AmberMedical);
            var reconciled = H1ProjectedProducersBuilderUtils.ReconcileRoundingDifference(tonnage, withProportions);
            Assert.AreEqual(0, tonnage - reconciled.TotalRamTonnage());
            Assert.AreEqual(withProportions with { AmberMedical = withProportions.AmberMedical + roundingDiff }, reconciled);
        }

        [TestMethod]
        public void SumProducerGroupTonnages_CorrectlyTotalsProportionForHC()
        {
            var expRamH2Proportions = new RAMProportions { Red = 0.5m, Amber = 0.5m, Green = 0, RedMedical = 0, AmberMedical = 0, GreenMedical = 0 };
            var h2Producers = new List<CalcResultH2ProjectedProducer>
            {
                new()
                {
                    ProducerId = 11,
                    Level = "1",
                    SubmissionPeriodCode = "2025-H1",
                    IsSubtotal = true,
                    H2ProjectedTonnageByMaterial = new Dictionary<string, CalcResultH2ProjectedProducerMaterialTonnage>
                    {
                        ["AL"] = new()
                        {
                            HouseholdTonnage = 200,
                            HouseholdRAMTonnage = new RamTonnage { Red = 100, Amber = 100, Green = 0, RedMedical = 0, AmberMedical = 0, GreenMedical = 0 },
                            PublicBinTonnage = 0,
                            PublicBinRAMTonnage = new RamTonnage { Red = 0, Amber = 0, Green = 0, RedMedical = 0, AmberMedical = 0, GreenMedical = 0 },
                            HouseholdTonnageWithoutRAM = 0,
                            PublicBinTonnageWithoutRAM = 0,
                            ProjectedHouseholdTonnage = 200,
                            ProjectedHouseholdRAMTonnage = new RamTonnage { Red = 100, Amber = 100, Green = 0, RedMedical = 0, AmberMedical = 0, GreenMedical = 0 },
                            ProjectedPublicBinTonnage = 0,
                            ProjectedPublicBinRAMTonnage = new RamTonnage { Red = 0, Amber = 0, Green = 0, RedMedical = 0, AmberMedical = 0, GreenMedical = 0 }
                        }
                    }
                }
            };
            var prodGroup = new List<CalcResultH1ProjectedProducer>()
            {
                new (){
                    ProducerId = 11,
                    SubsidiaryId = null,
                    SubmissionPeriodCode = "2025-H1",
                    Level = string.Empty,
                    H1ProjectedTonnageByMaterial = new Dictionary<string, CalcResultH1ProjectedProducerMaterialTonnage>() {
                        ["AL"] = new() {
                            HouseholdTonnage = 100,
                            HouseholdRAMTonnage = new RamTonnage { Red = 100, Amber = 0, Green = 0, RedMedical = 0, AmberMedical = 0, GreenMedical = 0 },
                            PublicBinTonnage = 200,
                            PublicBinRAMTonnage = new RamTonnage { Red = 0, Amber = 100, Green = 0, RedMedical = 0, AmberMedical = 0, GreenMedical = 0 },
                            HouseholdTonnageWithoutRAM = 0,
                            PublicBinTonnageWithoutRAM = 100,
                            H2RamProportions = expRamH2Proportions,
                            ProjectedHouseholdTonnage = 100,
                            ProjectedHouseholdRAMTonnage = new RamTonnage { Red = 100, Amber = 0, Green = 0, RedMedical = 0, AmberMedical = 0, GreenMedical = 0 },
                            ProjectedPublicBinTonnage = 200,
                            ProjectedPublicBinRAMTonnage = new RamTonnage { Red = 50, Amber = 150, Green = 0, RedMedical = 0, AmberMedical = 0, GreenMedical = 0 }
                        }
                    }
                },
                new (){
                    ProducerId = 11,
                    SubsidiaryId = "A",
                    SubmissionPeriodCode = "2025-H1",
                    Level = string.Empty,
                    H1ProjectedTonnageByMaterial = new Dictionary<string, CalcResultH1ProjectedProducerMaterialTonnage>() {
                        ["AL"] = new() {
                            HouseholdTonnage = 100,
                            HouseholdRAMTonnage = new RamTonnage { Red = 100, Amber = 0, Green = 0, RedMedical = 0, AmberMedical = 0, GreenMedical = 0 },
                            PublicBinTonnage = 200,
                            PublicBinRAMTonnage = new RamTonnage { Red = 0, Amber = 100, Green = 0, RedMedical = 0, AmberMedical = 0, GreenMedical = 0 },
                            HouseholdTonnageWithoutRAM = 0,
                            PublicBinTonnageWithoutRAM = 100,
                            H2RamProportions = expRamH2Proportions,
                            ProjectedHouseholdTonnage = 100,
                            ProjectedHouseholdRAMTonnage = new RamTonnage { Red = 100, Amber = 0, Green = 0, RedMedical = 0, AmberMedical = 0, GreenMedical = 0 },
                            ProjectedPublicBinTonnage = 200,
                            ProjectedPublicBinRAMTonnage = new RamTonnage { Red = 50, Amber = 150, Green = 0, RedMedical = 0, AmberMedical = 0, GreenMedical = 0 }
                        }
                    }
                },
            };

            var expSummedAlm = new CalcResultH1ProjectedProducerMaterialTonnage() {
                HouseholdTonnage = 200,
                HouseholdRAMTonnage = new RamTonnage { Red = 200, Amber = 0, Green = 0, RedMedical = 0, AmberMedical = 0, GreenMedical = 0 },
                PublicBinTonnage = 400,
                PublicBinRAMTonnage = new RamTonnage { Red = 0, Amber = 200, Green = 0, RedMedical = 0, AmberMedical = 0, GreenMedical = 0 },
                HouseholdTonnageWithoutRAM = 0,
                PublicBinTonnageWithoutRAM = 200,
                H2RamProportions = expRamH2Proportions,
                ProjectedHouseholdTonnage = 200,
                ProjectedHouseholdRAMTonnage = new RamTonnage { Red = 200, Amber = 0, Green = 0, RedMedical = 0, AmberMedical = 0, GreenMedical = 0 },
                ProjectedPublicBinTonnage = 400,
                ProjectedPublicBinRAMTonnage = new RamTonnage { Red = 100, Amber = 300, Green = 0, RedMedical = 0, AmberMedical = 0, GreenMedical = 0 }
            };

            var result = H1ProjectedProducersBuilderUtils.SumProducerGroupTonnages(prodGroup, h2Producers);

            Assert.IsTrue(result.IsSubtotal);
            Assert.AreEqual("1", result.Level);
            Assert.AreEqual(expSummedAlm, result.H1ProjectedTonnageByMaterial["AL"]);
        }

        [TestMethod]
        public void CreateParentProducer_Standalone_ComputesProportionsFromH2L1Row()
        {
            // Standalone: h2Producers has one L1 row with IsSubtotal = false (no subtotal exists).
            // CreateParentProducer must fall back to FirstOrDefault to get proportions.
            var (h2Alm, _, h2Producers) = GetH2FullRamProjectedProducers();

            var h1Producer = new CalcResultH1ProjectedProducer
            {
                ProducerId           = 11,
                SubsidiaryId         = null,
                Level                = string.Empty,
                SubmissionPeriodCode = "2026-H1",
                H1ProjectedTonnageByMaterial = new Dictionary<string, CalcResultH1ProjectedProducerMaterialTonnage>
                {
                    [MaterialCodes.Aluminium] = new()
                    {
                        HouseholdTonnage             = 100,
                        HouseholdRAMTonnage          = RamTonnage.Empty,
                        PublicBinTonnage             = 0,
                        PublicBinRAMTonnage          = RamTonnage.Empty,
                        HouseholdTonnageWithoutRAM   = 100,
                        PublicBinTonnageWithoutRAM   = 0,
                        H2RamProportions             = RAMProportions.Empty,
                        ProjectedHouseholdTonnage    = 100,
                        ProjectedHouseholdRAMTonnage = RamTonnage.Empty,
                        ProjectedPublicBinTonnage    = 0,
                        ProjectedPublicBinRAMTonnage = RamTonnage.Empty
                    }
                }
            };

            var result = H1ProjectedProducersBuilderUtils.CreateParentProducer(h1Producer, h2Producers);

            var almTotal = h2Alm.TotalTonnage();
            var expProportions = new RAMProportions
            {
                Red          = To6DP((h2Alm.ProjectedHouseholdRAMTonnage.Red          + h2Alm.ProjectedPublicBinRAMTonnage.Red)          / almTotal),
                Amber        = To6DP((h2Alm.ProjectedHouseholdRAMTonnage.Amber        + h2Alm.ProjectedPublicBinRAMTonnage.Amber)        / almTotal),
                Green        = To6DP((h2Alm.ProjectedHouseholdRAMTonnage.Green        + h2Alm.ProjectedPublicBinRAMTonnage.Green)        / almTotal),
                RedMedical   = To6DP((h2Alm.ProjectedHouseholdRAMTonnage.RedMedical   + h2Alm.ProjectedPublicBinRAMTonnage.RedMedical)   / almTotal),
                AmberMedical = To6DP((h2Alm.ProjectedHouseholdRAMTonnage.AmberMedical + h2Alm.ProjectedPublicBinRAMTonnage.AmberMedical) / almTotal),
                GreenMedical = To6DP((h2Alm.ProjectedHouseholdRAMTonnage.GreenMedical + h2Alm.ProjectedPublicBinRAMTonnage.GreenMedical) / almTotal)
            };

            Assert.AreEqual(expProportions, result.H1ProjectedTonnageByMaterial[MaterialCodes.Aluminium].H2RamProportions);
        }

        [TestMethod]
        public void CreateParentProducer_Subsidiary_ComputesProportionsFromH2Subtotal()
        {
            // Subsidiary: first entry in h2Producers has an IsSubtotal = true - proportions should be used from this
            var (h2Alm, _, h2Producers) = GetH2WithHCSubtotal();

            var h1Producer = new CalcResultH1ProjectedProducer
            {
                ProducerId           = 11,
                SubsidiaryId         = null,
                Level                = string.Empty,
                SubmissionPeriodCode = "2026-H1",
                H1ProjectedTonnageByMaterial = new Dictionary<string, CalcResultH1ProjectedProducerMaterialTonnage>
                {
                    [MaterialCodes.Aluminium] = new()
                    {
                        HouseholdTonnage             = 100,
                        HouseholdRAMTonnage          = RamTonnage.Empty,
                        PublicBinTonnage             = 0,
                        PublicBinRAMTonnage          = RamTonnage.Empty,
                        HouseholdTonnageWithoutRAM   = 100,
                        PublicBinTonnageWithoutRAM   = 0,
                        H2RamProportions             = RAMProportions.Empty,
                        ProjectedHouseholdTonnage    = 100,
                        ProjectedHouseholdRAMTonnage = RamTonnage.Empty,
                        ProjectedPublicBinTonnage    = 0,
                        ProjectedPublicBinRAMTonnage = RamTonnage.Empty
                    }
                }
            };

            var result = H1ProjectedProducersBuilderUtils.CreateParentProducer(h1Producer, h2Producers);

            var almTotal = h2Alm.TotalTonnage();
            var expProportions = new RAMProportions
            {
                Red          = To6DP((h2Alm.ProjectedHouseholdRAMTonnage.Red          + h2Alm.ProjectedPublicBinRAMTonnage.Red)          / almTotal),
                Amber        = To6DP((h2Alm.ProjectedHouseholdRAMTonnage.Amber        + h2Alm.ProjectedPublicBinRAMTonnage.Amber)        / almTotal),
                Green        = To6DP((h2Alm.ProjectedHouseholdRAMTonnage.Green        + h2Alm.ProjectedPublicBinRAMTonnage.Green)        / almTotal),
                RedMedical   = To6DP((h2Alm.ProjectedHouseholdRAMTonnage.RedMedical   + h2Alm.ProjectedPublicBinRAMTonnage.RedMedical)   / almTotal),
                AmberMedical = To6DP((h2Alm.ProjectedHouseholdRAMTonnage.AmberMedical + h2Alm.ProjectedPublicBinRAMTonnage.AmberMedical) / almTotal),
                GreenMedical = To6DP((h2Alm.ProjectedHouseholdRAMTonnage.GreenMedical + h2Alm.ProjectedPublicBinRAMTonnage.GreenMedical) / almTotal)
            };

            Assert.AreEqual(expProportions, result.H1ProjectedTonnageByMaterial[MaterialCodes.Aluminium].H2RamProportions);
        }

        private static decimal To3DP(decimal value) =>
            MathUtils.RoundAwayFromZero(value, 3);

        private static decimal To6DP(decimal value) =>
            MathUtils.RoundAwayFromZero(value, 6);

        private static void AssertWithin(decimal expected, decimal actual, decimal tolerance = 0.001m) =>
            Assert.IsTrue(Math.Abs(expected - actual) <= tolerance);

        private static RamTonnage ProducerReportedMaterialToRAMTonnage(ProducerReportedMaterial material) =>
            new RamTonnage
            {
                Red          = material.PackagingTonnageRed ?? 0,
                Amber        = material.PackagingTonnageAmber ?? 0,
                Green        = material.PackagingTonnageGreen ?? 0,
                RedMedical   = material.PackagingTonnageRedMedical ?? 0,
                AmberMedical = material.PackagingTonnageAmberMedical ?? 0,
                GreenMedical = material.PackagingTonnageGreenMedical ?? 0
            };

        private static decimal CalcWithoutRam(ProducerReportedMaterial material) =>
            material.PackagingTonnage - (material.PackagingTonnageRed ?? 0) - (material.PackagingTonnageAmber ?? 0) - (material.PackagingTonnageGreen ?? 0) - (material.PackagingTonnageRedMedical ?? 0) - (material.PackagingTonnageAmberMedical ?? 0) - (material.PackagingTonnageGreenMedical ?? 0);

        private static (ProducerReportedMaterial, ProducerReportedMaterial, List<ProducerDetail>) GetH1NoRamReportedMaterials()
        {
            var hhAlm = new ProducerReportedMaterial { MaterialId = 1, PackagingType = PackagingTypes.Household, PackagingTonnage = 100, SubmissionPeriod = "2026-H1" };
            var hdcGlass = new ProducerReportedMaterial { MaterialId = 2, PackagingType = PackagingTypes.HouseholdDrinksContainers, PackagingTonnage = 200, SubmissionPeriod = "2026-H1" };
            var producer = new ProducerDetail
            {
                ProducerId = 11,
                SubsidiaryId = null,
            };
            producer.ProducerReportedMaterials.Add(hhAlm);
            producer.ProducerReportedMaterials.Add(hdcGlass);
            return (hhAlm, hdcGlass, new List<ProducerDetail> { producer });
        }

        private static (ProducerReportedMaterial, ProducerReportedMaterial, ProducerReportedMaterial, ProducerReportedMaterial, List<ProducerDetail>) GetH1NoRamReportedMaterialsWithSubsidiary()
        {
            var hhAlm = new ProducerReportedMaterial { MaterialId = 1, PackagingType = PackagingTypes.Household, PackagingTonnage = 100, SubmissionPeriod = "2026-H1" };
            var hhSubAlm = new ProducerReportedMaterial { MaterialId = 1, PackagingType = PackagingTypes.Household, PackagingTonnage = 200, SubmissionPeriod = "2026-H1" };
            var hdcGlass = new ProducerReportedMaterial { MaterialId = 2, PackagingType = PackagingTypes.HouseholdDrinksContainers, PackagingTonnage = 200, SubmissionPeriod = "2026-H1" };
            var hdcSubGlass = new ProducerReportedMaterial { MaterialId = 2, PackagingType = PackagingTypes.HouseholdDrinksContainers, PackagingTonnage = 50, SubmissionPeriod = "2026-H1" };
            var producer1 = new ProducerDetail
            {
                ProducerId = 11,
                SubsidiaryId = null,
            };
            producer1.ProducerReportedMaterials.Add(hhAlm);
            producer1.ProducerReportedMaterials.Add(hdcGlass);
            var producer2 = new ProducerDetail
            {
                ProducerId = 11,
                SubsidiaryId = "A",
            };
            producer2.ProducerReportedMaterials.Add(hhSubAlm);
            producer2.ProducerReportedMaterials.Add(hdcSubGlass);
            return (hhAlm, hdcGlass, hhSubAlm, hdcSubGlass, new List<ProducerDetail> { producer1, producer2 });
        }

        private static (ProducerReportedMaterial, ProducerReportedMaterial, List<ProducerDetail>) GetH1PartialRamReportedMaterials()
        {
            var hhAlm = new ProducerReportedMaterial { MaterialId = 1, PackagingType = PackagingTypes.Household, PackagingTonnage = 100, PackagingTonnageRed = 0, PackagingTonnageAmber = 50, PackagingTonnageGreen = 0, PackagingTonnageRedMedical = 0, PackagingTonnageAmberMedical = 25, PackagingTonnageGreenMedical = 0, SubmissionPeriod = "2026-H1" };
            var hdcGlass = new ProducerReportedMaterial { MaterialId = 2, PackagingType = PackagingTypes.HouseholdDrinksContainers, PackagingTonnage = 200, PackagingTonnageRed = 20, PackagingTonnageAmber = 0, PackagingTonnageGreen = 100, PackagingTonnageRedMedical = 20, PackagingTonnageAmberMedical = 0, PackagingTonnageGreenMedical = 0, SubmissionPeriod = "2026-H1" };
            var producer = new ProducerDetail
            {
                ProducerId = 11,
                SubsidiaryId = null,
            };
            producer.ProducerReportedMaterials.Add(hhAlm);
            producer.ProducerReportedMaterials.Add(hdcGlass);
            return (hhAlm, hdcGlass, new List<ProducerDetail> { producer });
        }

        private static (ProducerReportedMaterial, ProducerReportedMaterial, List<ProducerDetail>) GetH1FullRamReportedMaterials()
        {
            var hhAlm = new ProducerReportedMaterial { MaterialId = 1, PackagingType = PackagingTypes.Household, PackagingTonnage = 100, PackagingTonnageRed = 0, PackagingTonnageAmber = 50, PackagingTonnageGreen = 25, PackagingTonnageRedMedical = 0, PackagingTonnageAmberMedical = 25, PackagingTonnageGreenMedical = 0, SubmissionPeriod = "2026-H1" };
            var hdcGlass = new ProducerReportedMaterial { MaterialId = 2, PackagingType = PackagingTypes.HouseholdDrinksContainers, PackagingTonnage = 200, PackagingTonnageRed = 20, PackagingTonnageAmber = 60, PackagingTonnageGreen = 100, PackagingTonnageRedMedical = 20, PackagingTonnageAmberMedical = 0, PackagingTonnageGreenMedical = 0, SubmissionPeriod = "2026-H1" };
            var producer = new ProducerDetail
            {
                ProducerId = 11,
                SubsidiaryId = null,
            };
            producer.ProducerReportedMaterials.Add(hhAlm);
            producer.ProducerReportedMaterials.Add(hdcGlass);
            return (hhAlm, hdcGlass, new List<ProducerDetail> { producer });
        }

        private static (CalcResultH2ProjectedProducerMaterialTonnage, CalcResultH2ProjectedProducerMaterialTonnage, List<CalcResultH2ProjectedProducer>) GetH2WithHCSubtotal()
        {
            var alm = new CalcResultH2ProjectedProducerMaterialTonnage
            {
                HouseholdTonnage = 75,
                HouseholdRAMTonnage = new RamTonnage { Red = 0, Amber = 11, Green = 12, RedMedical = 13, AmberMedical = 14, GreenMedical = 15 },
                PublicBinTonnage = 135,
                PublicBinRAMTonnage = new RamTonnage { Red = 0, Amber = 21, Green = 22, RedMedical = 23, AmberMedical = 24, GreenMedical = 25 },
                HouseholdTonnageWithoutRAM = 10,
                PublicBinTonnageWithoutRAM = 20,
                ProjectedHouseholdTonnage = 75,
                ProjectedHouseholdRAMTonnage = new RamTonnage { Red = 10, Amber = 11, Green = 12, RedMedical = 13, AmberMedical = 14, GreenMedical = 15 },
                ProjectedPublicBinTonnage = 135,
                ProjectedPublicBinRAMTonnage = new RamTonnage { Red = 20, Amber = 21, Green = 22, RedMedical = 23, AmberMedical = 24, GreenMedical = 25 }
            };
            var glass = new CalcResultH2ProjectedProducerMaterialTonnage
            {
                HouseholdTonnage = 75,
                HouseholdRAMTonnage = new RamTonnage { Red = 10, Amber = 11, Green = 5, RedMedical = 0, AmberMedical = 14, GreenMedical = 0 },
                PublicBinTonnage = 135,
                PublicBinRAMTonnage = new RamTonnage { Red = 10, Amber = 21, Green = 22, RedMedical = 23, AmberMedical = 24, GreenMedical = 25 },
                HouseholdDrinksContainerTonnage = 195,
                HouseholdDrinksContainerRAMTonnage = new RamTonnage { Red = 30, Amber = 11, Green = 32, RedMedical = 33, AmberMedical = 34, GreenMedical = 35 },
                HouseholdTonnageWithoutRAM = 35,
                PublicBinTonnageWithoutRAM = 10,
                HouseholdDrinksContainerTonnageWithoutRAM = 20,
                ProjectedHouseholdTonnage = 75,
                ProjectedHouseholdRAMTonnage = new RamTonnage { Red = 45, Amber = 11, Green = 5, RedMedical = 0, AmberMedical = 14, GreenMedical = 0 },
                ProjectedPublicBinTonnage = 135,
                ProjectedPublicBinRAMTonnage = new RamTonnage { Red = 20, Amber = 21, Green = 22, RedMedical = 23, AmberMedical = 24, GreenMedical = 25 },
                ProjectedHouseholdDrinksContainerTonnage = 195,
                ProjectedHouseholdDrinksContainerRAMTonnage = new RamTonnage { Red = 50, Amber = 11, Green = 32, RedMedical = 33, AmberMedical = 34, GreenMedical = 35 },
            };
            return (alm, glass, new List<CalcResultH2ProjectedProducer>()
            {
                new()
                {
                    ProducerId = 11,
                    SubsidiaryId = null,
                    Level = string.Empty,
                    SubmissionPeriodCode = "2026-H2",
                    IsSubtotal = true,
                    H2ProjectedTonnageByMaterial = new Dictionary<string, CalcResultH2ProjectedProducerMaterialTonnage>
                    {
                        { MaterialCodes.Aluminium, alm },
                        { MaterialCodes.Glass, glass }
                    }
                },
                new()
                {
                    ProducerId = 11,
                    SubsidiaryId = null,
                    Level = string.Empty,
                    SubmissionPeriodCode = "2026-H2",
                    H2ProjectedTonnageByMaterial = new Dictionary<string, CalcResultH2ProjectedProducerMaterialTonnage>
                    {
                        { MaterialCodes.Aluminium, GetEmptyH2MaterialTonnage() },
                        { MaterialCodes.Glass, GetEmptyH2MaterialTonnageWithHDC() }
                    }
                },
                new()
                {
                    ProducerId = 11,
                    SubsidiaryId = "A",
                    Level = string.Empty,
                    SubmissionPeriodCode = "2026-H2",
                    H2ProjectedTonnageByMaterial = new Dictionary<string, CalcResultH2ProjectedProducerMaterialTonnage>
                    {
                        { MaterialCodes.Aluminium, GetEmptyH2MaterialTonnage() },
                        { MaterialCodes.Glass, GetEmptyH2MaterialTonnageWithHDC() }
                    }
                }
            });
        }

        private static (CalcResultH2ProjectedProducerMaterialTonnage, CalcResultH2ProjectedProducerMaterialTonnage, List<CalcResultH2ProjectedProducer>) GetH2FullRamProjectedProducers()
        {
            var alm = new CalcResultH2ProjectedProducerMaterialTonnage
            {
                HouseholdTonnage = 75,
                HouseholdRAMTonnage = new RamTonnage { Red = 10, Amber = 11, Green = 12, RedMedical = 13, AmberMedical = 14, GreenMedical = 15 },
                PublicBinTonnage = 135,
                PublicBinRAMTonnage = new RamTonnage { Red = 20, Amber = 21, Green = 22, RedMedical = 23, AmberMedical = 24, GreenMedical = 25 },
                HouseholdTonnageWithoutRAM = 10,
                PublicBinTonnageWithoutRAM = 20,
                ProjectedHouseholdTonnage = 75,
                ProjectedHouseholdRAMTonnage = new RamTonnage { Red = 10, Amber = 11, Green = 12, RedMedical = 13, AmberMedical = 14, GreenMedical = 15 },
                ProjectedPublicBinTonnage = 135,
                ProjectedPublicBinRAMTonnage = new RamTonnage { Red = 20, Amber = 21, Green = 22, RedMedical = 23, AmberMedical = 24, GreenMedical = 25 },
            };
            var glass = new CalcResultH2ProjectedProducerMaterialTonnage
            {
                HouseholdTonnage = 75,
                HouseholdRAMTonnage = new RamTonnage { Red = 10, Amber = 11, Green = 12, RedMedical = 13, AmberMedical = 14, GreenMedical = 15 },
                PublicBinTonnage = 135,
                PublicBinRAMTonnage = new RamTonnage { Red = 20, Amber = 21, Green = 22, RedMedical = 23, AmberMedical = 24, GreenMedical = 25 },
                HouseholdDrinksContainerTonnage = 195,
                HouseholdDrinksContainerRAMTonnage = new RamTonnage { Red = 30, Amber = 31, Green = 32, RedMedical = 33, AmberMedical = 34, GreenMedical = 35 },
                HouseholdTonnageWithoutRAM = 35,
                PublicBinTonnageWithoutRAM = 10,
                HouseholdDrinksContainerTonnageWithoutRAM = 20,
                ProjectedHouseholdTonnage = 75,
                ProjectedHouseholdRAMTonnage = new RamTonnage { Red = 10, Amber = 11, Green = 12, RedMedical = 13, AmberMedical = 14, GreenMedical = 15 },
                ProjectedPublicBinTonnage = 135,
                ProjectedPublicBinRAMTonnage = new RamTonnage { Red = 20, Amber = 21, Green = 22, RedMedical = 23, AmberMedical = 24, GreenMedical = 25 },
                ProjectedHouseholdDrinksContainerTonnage = 195,
                ProjectedHouseholdDrinksContainerRAMTonnage = new RamTonnage { Red = 30, Amber = 31, Green = 32, RedMedical = 33, AmberMedical = 34, GreenMedical = 35 },
            };
            return (alm, glass, new List<CalcResultH2ProjectedProducer>()
            {
                new()
                {
                    ProducerId = 11,
                    SubsidiaryId = null,
                    Level = string.Empty,
                    SubmissionPeriodCode = "2026-H2",
                    H2ProjectedTonnageByMaterial = new Dictionary<string, CalcResultH2ProjectedProducerMaterialTonnage>
                    {
                        { MaterialCodes.Aluminium, alm},
                        { MaterialCodes.Glass, glass }
                    }
                }
            });
        }

        private static (CalcResultH2ProjectedProducerMaterialTonnage, CalcResultH2ProjectedProducerMaterialTonnage, List<CalcResultH2ProjectedProducer>) GetH2PartialRamProjectedProducers()
        {
            var alm = new CalcResultH2ProjectedProducerMaterialTonnage
            {
                HouseholdTonnage = 75,
                HouseholdRAMTonnage = new RamTonnage { Red = 0, Amber = 11, Green = 12, RedMedical = 13, AmberMedical = 14, GreenMedical = 15 },
                PublicBinTonnage = 135,
                PublicBinRAMTonnage = new RamTonnage { Red = 0, Amber = 21, Green = 22, RedMedical = 23, AmberMedical = 24, GreenMedical = 25 },
                HouseholdTonnageWithoutRAM = 10,
                PublicBinTonnageWithoutRAM = 20,
                ProjectedHouseholdTonnage = 75,
                ProjectedHouseholdRAMTonnage = new RamTonnage { Red = 10, Amber = 11, Green = 12, RedMedical = 13, AmberMedical = 14, GreenMedical = 15 },
                ProjectedPublicBinTonnage = 135,
                ProjectedPublicBinRAMTonnage = new RamTonnage { Red = 20, Amber = 21, Green = 22, RedMedical = 23, AmberMedical = 24, GreenMedical = 25 }
            };
            var glass = new CalcResultH2ProjectedProducerMaterialTonnage
            {
                HouseholdTonnage = 75,
                HouseholdRAMTonnage = new RamTonnage { Red = 10, Amber = 11, Green = 5, RedMedical = 0, AmberMedical = 14, GreenMedical = 0 },
                PublicBinTonnage = 135,
                PublicBinRAMTonnage = new RamTonnage { Red = 10, Amber = 21, Green = 22, RedMedical = 23, AmberMedical = 24, GreenMedical = 25 },
                HouseholdDrinksContainerTonnage = 195,
                HouseholdDrinksContainerRAMTonnage = new RamTonnage { Red = 30, Amber = 11, Green = 32, RedMedical = 33, AmberMedical = 34, GreenMedical = 35 },
                HouseholdTonnageWithoutRAM = 35,
                PublicBinTonnageWithoutRAM = 10,
                HouseholdDrinksContainerTonnageWithoutRAM = 20,
                ProjectedHouseholdTonnage = 75,
                ProjectedHouseholdRAMTonnage = new RamTonnage { Red = 45, Amber = 11, Green = 5, RedMedical = 0, AmberMedical = 14, GreenMedical = 0 },
                ProjectedPublicBinTonnage = 135,
                ProjectedPublicBinRAMTonnage = new RamTonnage { Red = 20, Amber = 21, Green = 22, RedMedical = 23, AmberMedical = 24, GreenMedical = 25 },
                ProjectedHouseholdDrinksContainerTonnage = 195,
                ProjectedHouseholdDrinksContainerRAMTonnage = new RamTonnage { Red = 50, Amber = 11, Green = 32, RedMedical = 33, AmberMedical = 34, GreenMedical = 35 },
            };
            return (alm, glass, new List<CalcResultH2ProjectedProducer>()
            {
                new()
                {
                    ProducerId = 11,
                    SubsidiaryId = null,
                    Level = string.Empty,
                    SubmissionPeriodCode = "2026-H2",
                    H2ProjectedTonnageByMaterial = new Dictionary<string, CalcResultH2ProjectedProducerMaterialTonnage>
                    {
                        { MaterialCodes.Aluminium, alm },
                        { MaterialCodes.Glass, glass }
                    }
                }
            });
        }

        private static CalcResultH2ProjectedProducerMaterialTonnage GetEmptyH2MaterialTonnage() =>
            new CalcResultH2ProjectedProducerMaterialTonnage
            {
                HouseholdTonnage = 0,
                HouseholdRAMTonnage = RamTonnage.Empty,
                PublicBinTonnage = 0,
                PublicBinRAMTonnage = RamTonnage.Empty,
                HouseholdTonnageWithoutRAM = 0,
                PublicBinTonnageWithoutRAM = 0,
                ProjectedHouseholdTonnage = 0,
                ProjectedHouseholdRAMTonnage = RamTonnage.Empty,
                ProjectedPublicBinTonnage = 0,
                ProjectedPublicBinRAMTonnage = RamTonnage.Empty,
            };

        private static CalcResultH2ProjectedProducerMaterialTonnage GetEmptyH2MaterialTonnageWithHDC() =>
            new CalcResultH2ProjectedProducerMaterialTonnage
            {
                HouseholdTonnage = 0,
                HouseholdRAMTonnage = RamTonnage.Empty,
                PublicBinTonnage = 0,
                PublicBinRAMTonnage = RamTonnage.Empty,
                HouseholdDrinksContainerTonnage = 0,
                HouseholdDrinksContainerRAMTonnage = RamTonnage.Empty,
                HouseholdTonnageWithoutRAM = 0,
                PublicBinTonnageWithoutRAM = 0,
                ProjectedHouseholdTonnage = 0,
                ProjectedHouseholdRAMTonnage = RamTonnage.Empty,
                ProjectedPublicBinTonnage = 0,
                ProjectedPublicBinRAMTonnage = RamTonnage.Empty,
                ProjectedHouseholdDrinksContainerTonnage = 0,
                ProjectedHouseholdDrinksContainerRAMTonnage = RamTonnage.Empty,
            };

        private static (CalcResultH2ProjectedProducerMaterialTonnage, CalcResultH2ProjectedProducerMaterialTonnage, List<CalcResultH2ProjectedProducer>) GetH2NoReportedMaterialProjectedProducers()
        {
            var alm = GetEmptyH2MaterialTonnage();
            var glass = GetEmptyH2MaterialTonnageWithHDC();
            return (alm, glass, new List<CalcResultH2ProjectedProducer>()
            {
                new()
                {
                    ProducerId = 11,
                    SubsidiaryId = null,
                    Level = string.Empty,
                    SubmissionPeriodCode = "2026-H2",
                    H2ProjectedTonnageByMaterial = new Dictionary<string, CalcResultH2ProjectedProducerMaterialTonnage>
                    {
                        { MaterialCodes.Aluminium, alm},
                        { MaterialCodes.Glass, glass },
                        { MaterialCodes.OtherMaterials,
                            new CalcResultH2ProjectedProducerMaterialTonnage {
                                HouseholdTonnage = 75,
                                HouseholdRAMTonnage = new RamTonnage { Red = 10, Amber = 11, Green = 12, RedMedical = 13, AmberMedical = 14, GreenMedical = 15 },
                                PublicBinTonnage = 135,
                                PublicBinRAMTonnage = new RamTonnage { Red = 20, Amber = 21, Green = 22, RedMedical = 23, AmberMedical = 24, GreenMedical = 25 },
                                HouseholdTonnageWithoutRAM = 10,
                                PublicBinTonnageWithoutRAM = 20,
                                ProjectedHouseholdTonnage = 75,
                                ProjectedHouseholdRAMTonnage = new RamTonnage { Red = 20, Amber = 11, Green = 12, RedMedical = 13, AmberMedical = 14, GreenMedical = 15 },
                                ProjectedPublicBinTonnage = 135,
                                ProjectedPublicBinRAMTonnage = new RamTonnage { Red = 40, Amber = 21, Green = 22, RedMedical = 23, AmberMedical = 24, GreenMedical = 25 },

                            }
                        }
                    }
                }
            });
        }
    }
}
