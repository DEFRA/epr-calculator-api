using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Builder.ProjectedProducers;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;

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

        private RAMTonnage EmptyRAMTonnage() { return new RAMTonnage { RedTonnage = 0, AmberTonnage = 0, GreenTonnage = 0, RedMedicalTonnage = 0, AmberMedicalTonnage = 0, GreenMedicalTonnage = 0 }; }

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
            Assert.AreEqual(EmptyRAMTonnage(), projectedAluminium.PublicBinRAMTonnage);
            Assert.IsNull(projectedAluminium.HouseholdDrinksContainerRAMTonnage);
            Assert.AreEqual(EmptyRAMTonnage(), projectedGlass.HouseholdRAMTonnage);
            Assert.AreEqual(EmptyRAMTonnage(), projectedGlass.PublicBinRAMTonnage);
            Assert.AreEqual(ProducerReportedMaterialToRAMTonnage(h1HdcGlass), projectedGlass.HouseholdDrinksContainerRAMTonnage);

            Assert.AreEqual(h1HHAlm.PackagingTonnage, projectedAluminium.HouseholdTonnageWithoutRAM);
            Assert.AreEqual(0, projectedAluminium.PublicBinTonnageWithoutRAM);
            Assert.IsNull(projectedAluminium.HouseholdDrinksContainerTonnageWithoutRAM);
            Assert.AreEqual(0, projectedGlass.HouseholdTonnageWithoutRAM);
            Assert.AreEqual(0, projectedGlass.PublicBinTonnageWithoutRAM);
            Assert.AreEqual(h1HdcGlass.PackagingTonnage, projectedGlass.HouseholdDrinksContainerTonnageWithoutRAM);

            var h2AlmTotalTonnage = h2Alm.TotalTonnage();
            var expAlmH2Proportions = new RAMProportions{
                Red = To6DP((h2Alm.ProjectedHouseholdRAMTonnage.RedTonnage + h2Alm.ProjectedPublicBinRAMTonnage.RedTonnage) / h2AlmTotalTonnage),
                Amber = To6DP((h2Alm.ProjectedHouseholdRAMTonnage.AmberTonnage + h2Alm.ProjectedPublicBinRAMTonnage.AmberTonnage) / h2AlmTotalTonnage),
                Green = To6DP((h2Alm.ProjectedHouseholdRAMTonnage.GreenTonnage + h2Alm.ProjectedPublicBinRAMTonnage.GreenTonnage) / h2AlmTotalTonnage),
                RedMedical = To6DP((h2Alm.ProjectedHouseholdRAMTonnage.RedMedicalTonnage + h2Alm.ProjectedPublicBinRAMTonnage.RedMedicalTonnage) / h2AlmTotalTonnage),
                AmberMedical = To6DP((h2Alm.ProjectedHouseholdRAMTonnage.AmberMedicalTonnage + h2Alm.ProjectedPublicBinRAMTonnage.AmberMedicalTonnage) / h2AlmTotalTonnage),
                GreenMedical = To6DP((h2Alm.ProjectedHouseholdRAMTonnage.GreenMedicalTonnage + h2Alm.ProjectedPublicBinRAMTonnage.GreenMedicalTonnage) / h2AlmTotalTonnage)
            };
            Assert.AreEqual(expAlmH2Proportions, projectedAluminium.H2RamProportions);

            var h2GlassTotalTonnage = h2Glass.TotalTonnage();
            var expGlassH2Proportions = new RAMProportions{
                Red = To6DP((h2Glass.ProjectedHouseholdRAMTonnage.RedTonnage + h2Glass.ProjectedPublicBinRAMTonnage.RedTonnage + (h2Glass.ProjectedHouseholdDrinksContainerRAMTonnage?.RedTonnage ?? 0)) / h2GlassTotalTonnage),
                Amber = To6DP((h2Glass.ProjectedHouseholdRAMTonnage.AmberTonnage + h2Glass.ProjectedPublicBinRAMTonnage.AmberTonnage + (h2Glass.ProjectedHouseholdDrinksContainerRAMTonnage?.AmberTonnage ?? 0)) / h2GlassTotalTonnage),
                Green = To6DP((h2Glass.ProjectedHouseholdRAMTonnage.GreenTonnage + h2Glass.ProjectedPublicBinRAMTonnage.GreenTonnage + (h2Glass.ProjectedHouseholdDrinksContainerRAMTonnage?.GreenTonnage ?? 0)) / h2GlassTotalTonnage),
                RedMedical = To6DP((h2Glass.ProjectedHouseholdRAMTonnage.RedMedicalTonnage + h2Glass.ProjectedPublicBinRAMTonnage.RedMedicalTonnage + (h2Glass.ProjectedHouseholdDrinksContainerRAMTonnage?.RedMedicalTonnage ?? 0)) / h2GlassTotalTonnage),
                AmberMedical = To6DP((h2Glass.ProjectedHouseholdRAMTonnage.AmberMedicalTonnage + h2Glass.ProjectedPublicBinRAMTonnage.AmberMedicalTonnage + (h2Glass.ProjectedHouseholdDrinksContainerRAMTonnage?.AmberMedicalTonnage ?? 0)) / h2GlassTotalTonnage),
                GreenMedical = To6DP((h2Glass.ProjectedHouseholdRAMTonnage.GreenMedicalTonnage + h2Glass.ProjectedPublicBinRAMTonnage.GreenMedicalTonnage + (h2Glass.ProjectedHouseholdDrinksContainerRAMTonnage?.GreenMedicalTonnage ?? 0)) / h2GlassTotalTonnage)
            };
            Assert.AreEqual(expGlassH2Proportions, projectedGlass.H2RamProportions);

            Assert.AreEqual(h1HHAlm.PackagingTonnage, projectedAluminium.ProjectedHouseholdTonnage);
            AssertWithin(To3DP(h1HHAlm.PackagingTonnage * expAlmH2Proportions.Red), projectedAluminium.ProjectedHouseholdRAMTonnage.RedTonnage);
            AssertWithin(To3DP(h1HHAlm.PackagingTonnage * expAlmH2Proportions.Amber), projectedAluminium.ProjectedHouseholdRAMTonnage.AmberTonnage);
            AssertWithin(To3DP(h1HHAlm.PackagingTonnage * expAlmH2Proportions.Green), projectedAluminium.ProjectedHouseholdRAMTonnage.GreenTonnage);
            AssertWithin(To3DP(h1HHAlm.PackagingTonnage * expAlmH2Proportions.RedMedical), projectedAluminium.ProjectedHouseholdRAMTonnage.RedMedicalTonnage);
            AssertWithin(To3DP(h1HHAlm.PackagingTonnage * expAlmH2Proportions.AmberMedical), projectedAluminium.ProjectedHouseholdRAMTonnage.AmberMedicalTonnage);
            AssertWithin(To3DP(h1HHAlm.PackagingTonnage * expAlmH2Proportions.GreenMedical), projectedAluminium.ProjectedHouseholdRAMTonnage.GreenMedicalTonnage);

            Assert.AreEqual(EmptyRAMTonnage(), projectedAluminium.ProjectedPublicBinRAMTonnage);
            Assert.IsNull(projectedAluminium.ProjectedHouseholdDrinksContainerRAMTonnage);
            Assert.AreEqual(EmptyRAMTonnage(), projectedGlass.ProjectedHouseholdRAMTonnage);
            Assert.AreEqual(EmptyRAMTonnage(), projectedGlass.ProjectedPublicBinRAMTonnage);

            Assert.AreEqual(h1HdcGlass.PackagingTonnage, projectedGlass.ProjectedHouseholdDrinksContainerTonnage!);
            AssertWithin(To3DP(h1HdcGlass.PackagingTonnage * expGlassH2Proportions.Red), projectedGlass.ProjectedHouseholdDrinksContainerRAMTonnage!.RedTonnage);
            AssertWithin(To3DP(h1HdcGlass.PackagingTonnage * expGlassH2Proportions.Amber), projectedGlass.ProjectedHouseholdDrinksContainerRAMTonnage!.AmberTonnage);
            AssertWithin(To3DP(h1HdcGlass.PackagingTonnage * expGlassH2Proportions.Green), projectedGlass.ProjectedHouseholdDrinksContainerRAMTonnage!.GreenTonnage);
            AssertWithin(To3DP(h1HdcGlass.PackagingTonnage * expGlassH2Proportions.RedMedical), projectedGlass.ProjectedHouseholdDrinksContainerRAMTonnage!.RedMedicalTonnage);
            AssertWithin(To3DP(h1HdcGlass.PackagingTonnage * expGlassH2Proportions.AmberMedical), projectedGlass.ProjectedHouseholdDrinksContainerRAMTonnage!.AmberMedicalTonnage);
            AssertWithin(To3DP(h1HdcGlass.PackagingTonnage * expGlassH2Proportions.GreenMedical), projectedGlass.ProjectedHouseholdDrinksContainerRAMTonnage!.GreenMedicalTonnage);
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
                Red = To6DP((h2Alm.ProjectedHouseholdRAMTonnage.RedTonnage + h2Alm.ProjectedPublicBinRAMTonnage.RedTonnage) / h2AlmTotalTonnage),
                Amber = To6DP((h2Alm.ProjectedHouseholdRAMTonnage.AmberTonnage + h2Alm.ProjectedPublicBinRAMTonnage.AmberTonnage) / h2AlmTotalTonnage),
                Green = To6DP((h2Alm.ProjectedHouseholdRAMTonnage.GreenTonnage + h2Alm.ProjectedPublicBinRAMTonnage.GreenTonnage) / h2AlmTotalTonnage),
                RedMedical = To6DP((h2Alm.ProjectedHouseholdRAMTonnage.RedMedicalTonnage + h2Alm.ProjectedPublicBinRAMTonnage.RedMedicalTonnage) / h2AlmTotalTonnage),
                AmberMedical = To6DP((h2Alm.ProjectedHouseholdRAMTonnage.AmberMedicalTonnage + h2Alm.ProjectedPublicBinRAMTonnage.AmberMedicalTonnage) / h2AlmTotalTonnage),
                GreenMedical = To6DP((h2Alm.ProjectedHouseholdRAMTonnage.GreenMedicalTonnage + h2Alm.ProjectedPublicBinRAMTonnage.GreenMedicalTonnage) / h2AlmTotalTonnage)
            };
            Assert.AreEqual(expAlmH2Proportions, projectedAluminium.H2RamProportions);


            var h2GlassTotalTonnage = h2Glass.TotalTonnage();
            var expGlassH2Proportions = new RAMProportions {
                Red = To6DP((h2Glass.ProjectedHouseholdRAMTonnage.RedTonnage + h2Glass.ProjectedPublicBinRAMTonnage.RedTonnage + (h2Glass.ProjectedHouseholdDrinksContainerRAMTonnage?.RedTonnage ?? 0)) / h2GlassTotalTonnage),
                Amber = To6DP((h2Glass.ProjectedHouseholdRAMTonnage.AmberTonnage + h2Glass.ProjectedPublicBinRAMTonnage.AmberTonnage + (h2Glass.ProjectedHouseholdDrinksContainerRAMTonnage?.AmberTonnage ?? 0)) / h2GlassTotalTonnage),
                Green = To6DP((h2Glass.ProjectedHouseholdRAMTonnage.GreenTonnage + h2Glass.ProjectedPublicBinRAMTonnage.GreenTonnage + (h2Glass.ProjectedHouseholdDrinksContainerRAMTonnage?.GreenTonnage ?? 0)) / h2GlassTotalTonnage),
                RedMedical = To6DP((h2Glass.ProjectedHouseholdRAMTonnage.RedMedicalTonnage + h2Glass.ProjectedPublicBinRAMTonnage.RedMedicalTonnage + (h2Glass.ProjectedHouseholdDrinksContainerRAMTonnage?.RedMedicalTonnage ?? 0)) / h2GlassTotalTonnage),
                AmberMedical = To6DP((h2Glass.ProjectedHouseholdRAMTonnage.AmberMedicalTonnage + h2Glass.ProjectedPublicBinRAMTonnage.AmberMedicalTonnage + (h2Glass.ProjectedHouseholdDrinksContainerRAMTonnage?.AmberMedicalTonnage ?? 0)) / h2GlassTotalTonnage),
                GreenMedical = To6DP((h2Glass.ProjectedHouseholdRAMTonnage.GreenMedicalTonnage + h2Glass.ProjectedPublicBinRAMTonnage.GreenMedicalTonnage + (h2Glass.ProjectedHouseholdDrinksContainerRAMTonnage?.GreenMedicalTonnage ?? 0)) / h2GlassTotalTonnage)
            };
            Assert.AreEqual(expGlassH2Proportions, projectedGlass.H2RamProportions);

            Assert.AreEqual(h1HHAlm.PackagingTonnage, projectedAluminium.ProjectedHouseholdTonnage);
            AssertWithin(To3DP(h1HHAlm.PackagingTonnage * expAlmH2Proportions.Red), projectedAluminium.ProjectedHouseholdRAMTonnage.RedTonnage);
            AssertWithin(To3DP(h1HHAlm.PackagingTonnage * expAlmH2Proportions.Amber), projectedAluminium.ProjectedHouseholdRAMTonnage.AmberTonnage);
            AssertWithin(To3DP(h1HHAlm.PackagingTonnage * expAlmH2Proportions.Green), projectedAluminium.ProjectedHouseholdRAMTonnage.GreenTonnage);
            AssertWithin(To3DP(h1HHAlm.PackagingTonnage * expAlmH2Proportions.RedMedical), projectedAluminium.ProjectedHouseholdRAMTonnage.RedMedicalTonnage);
            AssertWithin(To3DP(h1HHAlm.PackagingTonnage * expAlmH2Proportions.AmberMedical), projectedAluminium.ProjectedHouseholdRAMTonnage.AmberMedicalTonnage);
            AssertWithin(To3DP(h1HHAlm.PackagingTonnage * expAlmH2Proportions.GreenMedical), projectedAluminium.ProjectedHouseholdRAMTonnage.GreenMedicalTonnage);

            Assert.AreEqual(EmptyRAMTonnage(), projectedAluminium.ProjectedPublicBinRAMTonnage);
            Assert.IsNull(projectedAluminium.ProjectedHouseholdDrinksContainerRAMTonnage);
            Assert.AreEqual(EmptyRAMTonnage(), projectedGlass.ProjectedHouseholdRAMTonnage);
            Assert.AreEqual(EmptyRAMTonnage(), projectedGlass.ProjectedPublicBinRAMTonnage);

            Assert.AreEqual(h1HdcGlass.PackagingTonnage, projectedGlass.ProjectedHouseholdDrinksContainerTonnage!);
            AssertWithin(To3DP(h1HdcGlass.PackagingTonnage * expGlassH2Proportions.Red), projectedGlass.ProjectedHouseholdDrinksContainerRAMTonnage!.RedTonnage);
            AssertWithin(To3DP(h1HdcGlass.PackagingTonnage * expGlassH2Proportions.Amber), projectedGlass.ProjectedHouseholdDrinksContainerRAMTonnage!.AmberTonnage);
            AssertWithin(To3DP(h1HdcGlass.PackagingTonnage * expGlassH2Proportions.Green), projectedGlass.ProjectedHouseholdDrinksContainerRAMTonnage!.GreenTonnage);
            AssertWithin(To3DP(h1HdcGlass.PackagingTonnage * expGlassH2Proportions.RedMedical), projectedGlass.ProjectedHouseholdDrinksContainerRAMTonnage!.RedMedicalTonnage);
            AssertWithin(To3DP(h1HdcGlass.PackagingTonnage * expGlassH2Proportions.AmberMedical), projectedGlass.ProjectedHouseholdDrinksContainerRAMTonnage!.AmberMedicalTonnage);
            AssertWithin(To3DP(h1HdcGlass.PackagingTonnage * expGlassH2Proportions.GreenMedical), projectedGlass.ProjectedHouseholdDrinksContainerRAMTonnage!.GreenMedicalTonnage);
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
            Assert.AreEqual(EmptyRAMTonnage(), projectedAluminium.PublicBinRAMTonnage);
            Assert.IsNull(projectedAluminium.HouseholdDrinksContainerRAMTonnage);
            Assert.AreEqual(EmptyRAMTonnage(), projectedGlass.HouseholdRAMTonnage);
            Assert.AreEqual(EmptyRAMTonnage(), projectedGlass.PublicBinRAMTonnage);
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
                Red = To6DP((h2Alm.ProjectedHouseholdRAMTonnage.RedTonnage + h2Alm.ProjectedPublicBinRAMTonnage.RedTonnage) / h2AlmTotalTonnage),
                Amber = To6DP((h2Alm.ProjectedHouseholdRAMTonnage.AmberTonnage + h2Alm.ProjectedPublicBinRAMTonnage.AmberTonnage) / h2AlmTotalTonnage),
                Green = To6DP((h2Alm.ProjectedHouseholdRAMTonnage.GreenTonnage + h2Alm.ProjectedPublicBinRAMTonnage.GreenTonnage) / h2AlmTotalTonnage),
                RedMedical = To6DP((h2Alm.ProjectedHouseholdRAMTonnage.RedMedicalTonnage + h2Alm.ProjectedPublicBinRAMTonnage.RedMedicalTonnage) / h2AlmTotalTonnage),
                AmberMedical = To6DP((h2Alm.ProjectedHouseholdRAMTonnage.AmberMedicalTonnage + h2Alm.ProjectedPublicBinRAMTonnage.AmberMedicalTonnage) / h2AlmTotalTonnage),
                GreenMedical = To6DP((h2Alm.ProjectedHouseholdRAMTonnage.GreenMedicalTonnage + h2Alm.ProjectedPublicBinRAMTonnage.GreenMedicalTonnage) / h2AlmTotalTonnage)
            };
            Assert.AreEqual(expAlmH2Proportions, projectedAluminium.H2RamProportions);

            var h2GlassTotalTonnage = h2Glass.TotalTonnage();
            var expGlassH2Proportions = new RAMProportions {
                Red = To6DP((h2Glass.ProjectedHouseholdRAMTonnage.RedTonnage + h2Glass.ProjectedPublicBinRAMTonnage.RedTonnage + (h2Glass.ProjectedHouseholdDrinksContainerRAMTonnage?.RedTonnage ?? 0)) / h2GlassTotalTonnage),
                Amber = To6DP((h2Glass.ProjectedHouseholdRAMTonnage.AmberTonnage + h2Glass.ProjectedPublicBinRAMTonnage.AmberTonnage + (h2Glass.ProjectedHouseholdDrinksContainerRAMTonnage?.AmberTonnage ?? 0)) / h2GlassTotalTonnage),
                Green = To6DP((h2Glass.ProjectedHouseholdRAMTonnage.GreenTonnage + h2Glass.ProjectedPublicBinRAMTonnage.GreenTonnage + (h2Glass.ProjectedHouseholdDrinksContainerRAMTonnage?.GreenTonnage ?? 0)) / h2GlassTotalTonnage),
                RedMedical = To6DP((h2Glass.ProjectedHouseholdRAMTonnage.RedMedicalTonnage + h2Glass.ProjectedPublicBinRAMTonnage.RedMedicalTonnage + (h2Glass.ProjectedHouseholdDrinksContainerRAMTonnage?.RedMedicalTonnage ?? 0)) / h2GlassTotalTonnage),
                AmberMedical = To6DP((h2Glass.ProjectedHouseholdRAMTonnage.AmberMedicalTonnage + h2Glass.ProjectedPublicBinRAMTonnage.AmberMedicalTonnage + (h2Glass.ProjectedHouseholdDrinksContainerRAMTonnage?.AmberMedicalTonnage ?? 0)) / h2GlassTotalTonnage),
                GreenMedical = To6DP((h2Glass.ProjectedHouseholdRAMTonnage.GreenMedicalTonnage + h2Glass.ProjectedPublicBinRAMTonnage.GreenMedicalTonnage + (h2Glass.ProjectedHouseholdDrinksContainerRAMTonnage?.GreenMedicalTonnage ?? 0)) / h2GlassTotalTonnage)
            };
            Assert.AreEqual(expGlassH2Proportions, projectedGlass.H2RamProportions);

            Assert.AreEqual(h1HHAlm.PackagingTonnage, projectedAluminium.ProjectedHouseholdTonnage);
            AssertWithin(To3DP(expH1AlmWithoutRam * expAlmH2Proportions.Red), projectedAluminium.ProjectedHouseholdRAMTonnage.RedTonnage);
            AssertWithin(To3DP((h1HHAlm.PackagingTonnageAmber ?? 0) + (expH1AlmWithoutRam * expAlmH2Proportions.Amber)), projectedAluminium.ProjectedHouseholdRAMTonnage.AmberTonnage);
            AssertWithin(To3DP(expH1AlmWithoutRam * expAlmH2Proportions.Green), projectedAluminium.ProjectedHouseholdRAMTonnage.GreenTonnage);
            AssertWithin(To3DP(expH1AlmWithoutRam * expAlmH2Proportions.RedMedical), projectedAluminium.ProjectedHouseholdRAMTonnage.RedMedicalTonnage);
            AssertWithin(To3DP((h1HHAlm.PackagingTonnageAmberMedical ?? 0) + (expH1AlmWithoutRam * expAlmH2Proportions.AmberMedical)), projectedAluminium.ProjectedHouseholdRAMTonnage.AmberMedicalTonnage);
            AssertWithin(To3DP(expH1AlmWithoutRam * expAlmH2Proportions.GreenMedical), projectedAluminium.ProjectedHouseholdRAMTonnage.GreenMedicalTonnage);

            Assert.AreEqual(EmptyRAMTonnage(), projectedAluminium.ProjectedPublicBinRAMTonnage);
            Assert.IsNull(projectedAluminium.ProjectedHouseholdDrinksContainerRAMTonnage);
            Assert.AreEqual(EmptyRAMTonnage(), projectedGlass.ProjectedHouseholdRAMTonnage);
            Assert.AreEqual(EmptyRAMTonnage(), projectedGlass.ProjectedPublicBinRAMTonnage);

            Assert.AreEqual(h1HdcGlass.PackagingTonnage, projectedGlass.ProjectedHouseholdDrinksContainerTonnage!);
            AssertWithin(To3DP((h1HdcGlass.PackagingTonnageRed ?? 0) + (expH1HdcGlassWithoutRam * expGlassH2Proportions.Red)), projectedGlass.ProjectedHouseholdDrinksContainerRAMTonnage!.RedTonnage);
            AssertWithin(To3DP(expH1HdcGlassWithoutRam * expGlassH2Proportions.Amber), projectedGlass.ProjectedHouseholdDrinksContainerRAMTonnage!.AmberTonnage);
            AssertWithin(To3DP((h1HdcGlass.PackagingTonnageGreen ?? 0) + (expH1HdcGlassWithoutRam * expGlassH2Proportions.Green)), projectedGlass.ProjectedHouseholdDrinksContainerRAMTonnage!.GreenTonnage);
            AssertWithin(To3DP((h1HdcGlass.PackagingTonnageRedMedical ?? 0) + (expH1HdcGlassWithoutRam * expGlassH2Proportions.RedMedical)), projectedGlass.ProjectedHouseholdDrinksContainerRAMTonnage!.RedMedicalTonnage);
            AssertWithin(To3DP(expH1HdcGlassWithoutRam * expGlassH2Proportions.AmberMedical), projectedGlass.ProjectedHouseholdDrinksContainerRAMTonnage!.AmberMedicalTonnage);
            AssertWithin(To3DP(expH1HdcGlassWithoutRam * expGlassH2Proportions.GreenMedical), projectedGlass.ProjectedHouseholdDrinksContainerRAMTonnage!.GreenMedicalTonnage);
        }

        [TestMethod]
        public void GetProjectedProducers_With_H1_FullRam_H2_FullRam()
        {
            var (h1HHAlm, h1HdcGlass, h1FullRamReportedMaterials) = GetH1FullRamReportedMaterials();
            var (h2Alm, h2Glass, h2FullRamProjectedProducers) = GetH2FullRamProjectedProducers();
            var result = H1ProjectedProducersBuilderUtils.GetProjectedProducers(h1FullRamReportedMaterials, h2FullRamProjectedProducers, materials, "2026-H1");

            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(3, result.First().H1ProjectedTonnageByMaterial.Count());

            var projectedAluminium = result.First().H1ProjectedTonnageByMaterial[MaterialCodes.Aluminium];
            var projectedGlass = result.First().H1ProjectedTonnageByMaterial[MaterialCodes.Glass];

            Assert.AreEqual(ProducerReportedMaterialToRAMTonnage(h1HHAlm), projectedAluminium.HouseholdRAMTonnage);
            Assert.AreEqual(EmptyRAMTonnage(), projectedAluminium.PublicBinRAMTonnage);
            Assert.IsNull(projectedAluminium.HouseholdDrinksContainerRAMTonnage);
            Assert.AreEqual(EmptyRAMTonnage(), projectedGlass.HouseholdRAMTonnage);
            Assert.AreEqual(EmptyRAMTonnage(), projectedGlass.PublicBinRAMTonnage);
            Assert.AreEqual(ProducerReportedMaterialToRAMTonnage(h1HdcGlass), projectedGlass.HouseholdDrinksContainerRAMTonnage);

            Assert.AreEqual(0, projectedAluminium.HouseholdTonnageWithoutRAM);
            Assert.AreEqual(0, projectedAluminium.PublicBinTonnageWithoutRAM);
            Assert.IsNull(projectedAluminium.HouseholdDrinksContainerTonnageWithoutRAM);
            Assert.AreEqual(0, projectedGlass.HouseholdTonnageWithoutRAM);
            Assert.AreEqual(0, projectedGlass.PublicBinTonnageWithoutRAM);
            Assert.AreEqual(0, projectedGlass.HouseholdDrinksContainerTonnageWithoutRAM);

            Assert.AreEqual(ProducerReportedMaterialToRAMTonnage(h1HHAlm),projectedAluminium.ProjectedHouseholdRAMTonnage);
            Assert.AreEqual(EmptyRAMTonnage(), projectedAluminium.ProjectedPublicBinRAMTonnage);
            Assert.IsNull(projectedAluminium.ProjectedHouseholdDrinksContainerRAMTonnage);
            Assert.AreEqual(EmptyRAMTonnage(), projectedGlass.ProjectedHouseholdRAMTonnage);
            Assert.AreEqual(EmptyRAMTonnage(), projectedGlass.ProjectedPublicBinRAMTonnage);
            Assert.AreEqual(ProducerReportedMaterialToRAMTonnage(h1HdcGlass),projectedGlass.ProjectedHouseholdDrinksContainerRAMTonnage);
        }

        [TestMethod]
        public void GetProjectedProducers_With_H1_PartialRam_H2_NoReportedMaterial()
        {
            var (h1HHAlm, h1HdcGlass, h1NoRamReportedMaterials) = GetH1PartialRamReportedMaterials();
            var (h2Alm, h2Glass, h2NoReportedMaterialProjectedProducers) = GetH2NoReportedMaterialProjectedProducers();
            var result = H1ProjectedProducersBuilderUtils.GetProjectedProducers(h1NoRamReportedMaterials, h2NoReportedMaterialProjectedProducers, materials, "2026-H1");

            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(3, result.First().H1ProjectedTonnageByMaterial.Count());

            var projectedAluminium = result.First().H1ProjectedTonnageByMaterial[MaterialCodes.Aluminium];
            var projectedGlass = result.First().H1ProjectedTonnageByMaterial[MaterialCodes.Glass];

            var expAlmH2RamProportions = new RAMProportions { Red = 0, Amber = 0, Green = 0, RedMedical = 0, AmberMedical = 0, GreenMedical = 0 };
            var expGlassH2RamProportions = new RAMProportions { Red = 0, Amber = 0, Green = 0, RedMedical = 0, AmberMedical = 0, GreenMedical = 0 };
            Assert.AreEqual(expAlmH2RamProportions, projectedAluminium.H2RamProportions);
            Assert.AreEqual(expAlmH2RamProportions, projectedGlass.H2RamProportions);

            var expH1AlmWithoutRam = CalcWithoutRam(h1HHAlm);
            var expH1HdcGlassWithoutRam = CalcWithoutRam(h1HdcGlass);
            Assert.AreEqual(expH1AlmWithoutRam, projectedAluminium.HouseholdTonnageWithoutRAM);
            Assert.AreEqual(0, projectedAluminium.PublicBinTonnageWithoutRAM);
            Assert.IsNull(projectedAluminium.HouseholdDrinksContainerTonnageWithoutRAM);
            Assert.AreEqual(0, projectedGlass.HouseholdTonnageWithoutRAM);
            Assert.AreEqual(0, projectedGlass.PublicBinTonnageWithoutRAM);
            Assert.AreEqual(expH1HdcGlassWithoutRam, projectedGlass.HouseholdDrinksContainerTonnageWithoutRAM);

            Assert.AreEqual(
                new RAMTonnage {
                    RedTonnage = (h1HHAlm.PackagingTonnageRed ?? 0) + expH1AlmWithoutRam,
                    AmberTonnage = h1HHAlm.PackagingTonnageAmber ?? 0,
                    GreenTonnage = h1HHAlm.PackagingTonnageGreen ?? 0,
                    RedMedicalTonnage = h1HHAlm.PackagingTonnageRedMedical ?? 0,
                    AmberMedicalTonnage = h1HHAlm.PackagingTonnageAmberMedical ?? 0,
                    GreenMedicalTonnage = h1HHAlm.PackagingTonnageGreenMedical ?? 0
                },
                projectedAluminium.ProjectedHouseholdRAMTonnage
            );

            Assert.AreEqual(EmptyRAMTonnage(), projectedAluminium.ProjectedPublicBinRAMTonnage);
            Assert.IsNull(projectedAluminium.ProjectedHouseholdDrinksContainerRAMTonnage);
            Assert.AreEqual(EmptyRAMTonnage(), projectedGlass.ProjectedHouseholdRAMTonnage);
            Assert.AreEqual(EmptyRAMTonnage(), projectedGlass.ProjectedPublicBinRAMTonnage);

            Assert.AreEqual(
                new RAMTonnage {
                    RedTonnage = (h1HdcGlass.PackagingTonnageRed ?? 0) + expH1HdcGlassWithoutRam,
                    AmberTonnage = h1HdcGlass.PackagingTonnageAmber ?? 0,
                    GreenTonnage = h1HdcGlass.PackagingTonnageGreen ?? 0,
                    RedMedicalTonnage = h1HdcGlass.PackagingTonnageRedMedical ?? 0,
                    AmberMedicalTonnage = h1HdcGlass.PackagingTonnageAmberMedical ?? 0,
                    GreenMedicalTonnage = h1HdcGlass.PackagingTonnageGreenMedical ?? 0
                },
                projectedGlass.ProjectedHouseholdDrinksContainerRAMTonnage
            );
        }

        [TestMethod]
        public void GetProjectedProducers_With_H1_NoReportedMaterial()
        {
            var (h1HHAlm, h1HdcGlass, h1FullRamReportedMaterials) = GetH1FullRamReportedMaterials();
            var (h2Alm, h2Glass, h2FullRamProjectedProducers) = GetH2FullRamProjectedProducers();
            var result = H1ProjectedProducersBuilderUtils.GetProjectedProducers(h1FullRamReportedMaterials, h2FullRamProjectedProducers, materials, "2026-H1");

            Assert.AreEqual(1, result.Count());

            var projectedOtherMaterials = result.First().H1ProjectedTonnageByMaterial[MaterialCodes.OtherMaterials];
            Assert.AreEqual(EmptyRAMTonnage(), projectedOtherMaterials.HouseholdRAMTonnage);
            Assert.AreEqual(EmptyRAMTonnage(), projectedOtherMaterials.PublicBinRAMTonnage);
            Assert.IsNull(projectedOtherMaterials.HouseholdDrinksContainerRAMTonnage);
            Assert.AreEqual(0, projectedOtherMaterials.H2RamProportions.Red);
            Assert.AreEqual(0, projectedOtherMaterials.H2RamProportions.Amber);
            Assert.AreEqual(0, projectedOtherMaterials.H2RamProportions.Green);
            Assert.AreEqual(0, projectedOtherMaterials.H2RamProportions.RedMedical);
            Assert.AreEqual(0, projectedOtherMaterials.H2RamProportions.AmberMedical);
            Assert.AreEqual(0, projectedOtherMaterials.H2RamProportions.GreenMedical);
            Assert.AreEqual(EmptyRAMTonnage(), projectedOtherMaterials.HouseholdRAMTonnage);
            Assert.AreEqual(EmptyRAMTonnage(), projectedOtherMaterials.PublicBinRAMTonnage);
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
            Assert.AreEqual(EmptyRAMTonnage(), projectedAluminium.PublicBinRAMTonnage);
            Assert.IsNull(projectedAluminium.HouseholdDrinksContainerRAMTonnage);
            Assert.AreEqual(EmptyRAMTonnage(), projectedGlass.HouseholdRAMTonnage);
            Assert.AreEqual(EmptyRAMTonnage(), projectedGlass.PublicBinRAMTonnage);
            Assert.AreEqual(ProducerReportedMaterialToRAMTonnage(h1HdcGlass), projectedGlass.HouseholdDrinksContainerRAMTonnage);

            Assert.AreEqual(0, projectedAluminium.HouseholdTonnageWithoutRAM);
            Assert.AreEqual(0, projectedAluminium.PublicBinTonnageWithoutRAM);
            Assert.IsNull(projectedAluminium.HouseholdDrinksContainerTonnageWithoutRAM);
            Assert.AreEqual(0, projectedGlass.HouseholdTonnageWithoutRAM);
            Assert.AreEqual(0, projectedGlass.PublicBinTonnageWithoutRAM);
            Assert.AreEqual(0, projectedGlass.HouseholdDrinksContainerTonnageWithoutRAM);

            Assert.AreEqual(ProducerReportedMaterialToRAMTonnage(h1HHAlm),projectedAluminium.ProjectedHouseholdRAMTonnage);
            Assert.AreEqual(EmptyRAMTonnage(), projectedAluminium.ProjectedPublicBinRAMTonnage);
            Assert.IsNull(projectedAluminium.ProjectedHouseholdDrinksContainerRAMTonnage);
            Assert.AreEqual(EmptyRAMTonnage(), projectedGlass.ProjectedHouseholdRAMTonnage);
            Assert.AreEqual(EmptyRAMTonnage(), projectedGlass.ProjectedPublicBinRAMTonnage);
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
                Red = To6DP((h2SubtotalAlm.ProjectedHouseholdRAMTonnage.RedTonnage + h2SubtotalAlm.ProjectedPublicBinRAMTonnage.RedTonnage) / h2SubtotalAlmTotal),
                Amber = To6DP((h2SubtotalAlm.ProjectedHouseholdRAMTonnage.AmberTonnage + h2SubtotalAlm.ProjectedPublicBinRAMTonnage.AmberTonnage) / h2SubtotalAlmTotal),
                Green = To6DP((h2SubtotalAlm.ProjectedHouseholdRAMTonnage.GreenTonnage + h2SubtotalAlm.ProjectedPublicBinRAMTonnage.GreenTonnage) / h2SubtotalAlmTotal),
                RedMedical = To6DP((h2SubtotalAlm.ProjectedHouseholdRAMTonnage.RedMedicalTonnage + h2SubtotalAlm.ProjectedPublicBinRAMTonnage.RedMedicalTonnage) / h2SubtotalAlmTotal),
                AmberMedical = To6DP((h2SubtotalAlm.ProjectedHouseholdRAMTonnage.AmberMedicalTonnage + h2SubtotalAlm.ProjectedPublicBinRAMTonnage.AmberMedicalTonnage) / h2SubtotalAlmTotal),
                GreenMedical = To6DP((h2SubtotalAlm.ProjectedHouseholdRAMTonnage.GreenMedicalTonnage + h2SubtotalAlm.ProjectedPublicBinRAMTonnage.GreenMedicalTonnage) / h2SubtotalAlmTotal)
            };

            var h2SubtotalGlassTotal = h2SubtotalGlass.TotalTonnage();
            var expGlassH2SubtotalProportions = new RAMProportions{
                Red = To6DP((h2SubtotalGlass.ProjectedHouseholdRAMTonnage.RedTonnage + h2SubtotalGlass.ProjectedPublicBinRAMTonnage.RedTonnage + (h2SubtotalGlass.ProjectedHouseholdDrinksContainerRAMTonnage?.RedTonnage ?? 0)) / h2SubtotalGlassTotal),
                Amber = To6DP((h2SubtotalGlass.ProjectedHouseholdRAMTonnage.AmberTonnage + h2SubtotalGlass.ProjectedPublicBinRAMTonnage.AmberTonnage + (h2SubtotalGlass.ProjectedHouseholdDrinksContainerRAMTonnage?.AmberTonnage ?? 0)) / h2SubtotalGlassTotal),
                Green = To6DP((h2SubtotalGlass.ProjectedHouseholdRAMTonnage.GreenTonnage + h2SubtotalGlass.ProjectedPublicBinRAMTonnage.GreenTonnage + (h2SubtotalGlass.ProjectedHouseholdDrinksContainerRAMTonnage?.GreenTonnage ?? 0)) / h2SubtotalGlassTotal),
                RedMedical = To6DP((h2SubtotalGlass.ProjectedHouseholdRAMTonnage.RedMedicalTonnage + h2SubtotalGlass.ProjectedPublicBinRAMTonnage.RedMedicalTonnage + (h2SubtotalGlass.ProjectedHouseholdDrinksContainerRAMTonnage?.RedMedicalTonnage ?? 0)) / h2SubtotalGlassTotal),
                AmberMedical = To6DP((h2SubtotalGlass.ProjectedHouseholdRAMTonnage.AmberMedicalTonnage + h2SubtotalGlass.ProjectedPublicBinRAMTonnage.AmberMedicalTonnage + (h2SubtotalGlass.ProjectedHouseholdDrinksContainerRAMTonnage?.AmberMedicalTonnage ?? 0)) / h2SubtotalGlassTotal),
                GreenMedical = To6DP((h2SubtotalGlass.ProjectedHouseholdRAMTonnage.GreenMedicalTonnage + h2SubtotalGlass.ProjectedPublicBinRAMTonnage.GreenMedicalTonnage + (h2SubtotalGlass.ProjectedHouseholdDrinksContainerRAMTonnage?.GreenMedicalTonnage ?? 0)) / h2SubtotalGlassTotal)
            };

            //H2 proportions for HC and Sub should be the same - i.e. using the subtotal proproptions
            Assert.AreEqual(expAlmH2SubtotalProportions, projectedHCAluminium.H2RamProportions);
            Assert.AreEqual(expAlmH2SubtotalProportions, projectedSubAluminium.H2RamProportions);
            Assert.AreEqual(expGlassH2SubtotalProportions, projectedHCGlass.H2RamProportions);
            Assert.AreEqual(expGlassH2SubtotalProportions, projectedSubGlass.H2RamProportions);

            Assert.AreEqual(h1HHAlm.PackagingTonnage, projectedHCAluminium.ProjectedHouseholdTonnage);
            AssertWithin(To3DP(h1HHAlm.PackagingTonnage * expAlmH2SubtotalProportions.Red), projectedHCAluminium.ProjectedHouseholdRAMTonnage.RedTonnage);
            AssertWithin(To3DP(h1HHAlm.PackagingTonnage * expAlmH2SubtotalProportions.Amber), projectedHCAluminium.ProjectedHouseholdRAMTonnage.AmberTonnage);
            AssertWithin(To3DP(h1HHAlm.PackagingTonnage * expAlmH2SubtotalProportions.Green), projectedHCAluminium.ProjectedHouseholdRAMTonnage.GreenTonnage);
            AssertWithin(To3DP(h1HHAlm.PackagingTonnage * expAlmH2SubtotalProportions.RedMedical), projectedHCAluminium.ProjectedHouseholdRAMTonnage.RedMedicalTonnage);
            AssertWithin(To3DP(h1HHAlm.PackagingTonnage * expAlmH2SubtotalProportions.AmberMedical), projectedHCAluminium.ProjectedHouseholdRAMTonnage.AmberMedicalTonnage);
            AssertWithin(To3DP(h1HHAlm.PackagingTonnage * expAlmH2SubtotalProportions.GreenMedical), projectedHCAluminium.ProjectedHouseholdRAMTonnage.GreenMedicalTonnage);

            Assert.AreEqual(h1HdcGlass.PackagingTonnage, projectedHCGlass.ProjectedHouseholdDrinksContainerTonnage!);
            AssertWithin(To3DP(h1HdcGlass.PackagingTonnage * expGlassH2SubtotalProportions.Red), projectedHCGlass.ProjectedHouseholdDrinksContainerRAMTonnage!.RedTonnage);
            AssertWithin(To3DP(h1HdcGlass.PackagingTonnage * expGlassH2SubtotalProportions.Amber), projectedHCGlass.ProjectedHouseholdDrinksContainerRAMTonnage!.AmberTonnage);
            AssertWithin(To3DP(h1HdcGlass.PackagingTonnage * expGlassH2SubtotalProportions.Green), projectedHCGlass.ProjectedHouseholdDrinksContainerRAMTonnage!.GreenTonnage);
            AssertWithin(To3DP(h1HdcGlass.PackagingTonnage * expGlassH2SubtotalProportions.RedMedical), projectedHCGlass.ProjectedHouseholdDrinksContainerRAMTonnage!.RedMedicalTonnage);
            AssertWithin(To3DP(h1HdcGlass.PackagingTonnage * expGlassH2SubtotalProportions.AmberMedical), projectedHCGlass.ProjectedHouseholdDrinksContainerRAMTonnage!.AmberMedicalTonnage);
            AssertWithin(To3DP(h1HdcGlass.PackagingTonnage * expGlassH2SubtotalProportions.GreenMedical), projectedHCGlass.ProjectedHouseholdDrinksContainerRAMTonnage!.GreenMedicalTonnage);

            Assert.AreEqual(subH1HHAlm.PackagingTonnage, projectedSubAluminium.ProjectedHouseholdTonnage);
            AssertWithin(To3DP(subH1HHAlm.PackagingTonnage * expAlmH2SubtotalProportions.Red), projectedSubAluminium.ProjectedHouseholdRAMTonnage.RedTonnage);
            AssertWithin(To3DP(subH1HHAlm.PackagingTonnage * expAlmH2SubtotalProportions.Amber), projectedSubAluminium.ProjectedHouseholdRAMTonnage.AmberTonnage);
            AssertWithin(To3DP(subH1HHAlm.PackagingTonnage * expAlmH2SubtotalProportions.Green), projectedSubAluminium.ProjectedHouseholdRAMTonnage.GreenTonnage);
            AssertWithin(To3DP(subH1HHAlm.PackagingTonnage * expAlmH2SubtotalProportions.RedMedical), projectedSubAluminium.ProjectedHouseholdRAMTonnage.RedMedicalTonnage);
            AssertWithin(To3DP(subH1HHAlm.PackagingTonnage * expAlmH2SubtotalProportions.AmberMedical), projectedSubAluminium.ProjectedHouseholdRAMTonnage.AmberMedicalTonnage);
            AssertWithin(To3DP(subH1HHAlm.PackagingTonnage * expAlmH2SubtotalProportions.GreenMedical), projectedSubAluminium.ProjectedHouseholdRAMTonnage.GreenMedicalTonnage);

            Assert.AreEqual(subH1HdcGlass.PackagingTonnage, projectedSubGlass.ProjectedHouseholdDrinksContainerTonnage!);
            AssertWithin(To3DP(subH1HdcGlass.PackagingTonnage * expGlassH2SubtotalProportions.Red), projectedSubGlass.ProjectedHouseholdDrinksContainerRAMTonnage!.RedTonnage);
            AssertWithin(To3DP(subH1HdcGlass.PackagingTonnage * expGlassH2SubtotalProportions.Amber), projectedSubGlass.ProjectedHouseholdDrinksContainerRAMTonnage!.AmberTonnage);
            AssertWithin(To3DP(subH1HdcGlass.PackagingTonnage * expGlassH2SubtotalProportions.Green), projectedSubGlass.ProjectedHouseholdDrinksContainerRAMTonnage!.GreenTonnage);
            AssertWithin(To3DP(subH1HdcGlass.PackagingTonnage * expGlassH2SubtotalProportions.RedMedical), projectedSubGlass.ProjectedHouseholdDrinksContainerRAMTonnage!.RedMedicalTonnage);
            AssertWithin(To3DP(subH1HdcGlass.PackagingTonnage * expGlassH2SubtotalProportions.AmberMedical), projectedSubGlass.ProjectedHouseholdDrinksContainerRAMTonnage!.AmberMedicalTonnage);
            AssertWithin(To3DP(subH1HdcGlass.PackagingTonnage * expGlassH2SubtotalProportions.GreenMedical), projectedSubGlass.ProjectedHouseholdDrinksContainerRAMTonnage!.GreenMedicalTonnage);
        }

        [TestMethod]
        public void ReconcileRoundingDifference_NoMissingRam()
        {
            var tonnage = 200;
            var ramTonnage = new RAMTonnage { RedTonnage = 0, AmberTonnage = 20, GreenTonnage = 30, RedMedicalTonnage = 0, AmberMedicalTonnage = 50, GreenMedicalTonnage = 0 };
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
            var ramTonnage = new RAMTonnage { RedTonnage = 0, AmberTonnage = 20, GreenTonnage = 30, RedMedicalTonnage = 0, AmberMedicalTonnage = 50, GreenMedicalTonnage = 0 };
            var h2Proportions = new RAMProportions { Red = 0.333333m, Amber = 0.333333m, Green = 0.333334m, RedMedical = 0, AmberMedical = 0, GreenMedical = 0 };
            var withProportions = H1ProjectedProducersBuilderUtils.GetProportionateRam(ramTonnage, 100, h2Proportions);
            var roundingDiff = tonnage - withProportions.TotalRamTonnage();
            Assert.IsTrue(roundingDiff > 0);
            var reconciled = H1ProjectedProducersBuilderUtils.ReconcileRoundingDifference(tonnage, withProportions);
            Assert.AreEqual(0, tonnage - reconciled.TotalRamTonnage());
            Assert.AreEqual(withProportions with { GreenTonnage = withProportions.GreenTonnage + roundingDiff }, reconciled);
        }

        [TestMethod]
        public void ReconcileRoundingDifference_SurplusRam()
        {
            var tonnage = 200;
            var ramTonnage = new RAMTonnage { RedTonnage = 0, AmberTonnage = 20, GreenTonnage = 30, RedMedicalTonnage = 0, AmberMedicalTonnage = 50, GreenMedicalTonnage = 0 };
            var h2Proportions = new RAMProportions { Red = 0.333335m, Amber = 0.333335m, Green = 0.333330m, RedMedical = 0, AmberMedical = 0, GreenMedical = 0 };
            var withProportions = H1ProjectedProducersBuilderUtils.GetProportionateRam(ramTonnage, 100, h2Proportions);
            var roundingDiff = tonnage - withProportions.TotalRamTonnage();
            Assert.IsTrue(roundingDiff < 0);
            var reconciled = H1ProjectedProducersBuilderUtils.ReconcileRoundingDifference(tonnage, withProportions);
            Assert.AreEqual(0, tonnage - reconciled.TotalRamTonnage());
            Assert.AreEqual(withProportions with { GreenTonnage = withProportions.GreenTonnage + roundingDiff }, reconciled);
        }

        [TestMethod]
        public void ReconcileRoundingDifference_MissingRam_EqualDominantRam()
        {
            // Red and amber medical result in the same tonnages - assign missing ram to amber medical due to priority order
            var tonnage = 200;
            var ramTonnage = new RAMTonnage { RedTonnage = 30, AmberTonnage = 40, GreenTonnage = 0, RedMedicalTonnage = 0, AmberMedicalTonnage = 30, GreenMedicalTonnage = 0 };
            var h2Proportions = new RAMProportions { Red = 0.333333m, Amber = 0, Green = 0.333334m, RedMedical = 0, AmberMedical = 0.333333m, GreenMedical = 0 };
            var withProportions = H1ProjectedProducersBuilderUtils.GetProportionateRam(ramTonnage, 100, h2Proportions);
            var roundingDiff = tonnage - withProportions.TotalRamTonnage();
            Assert.IsTrue(roundingDiff > 0);
            Assert.AreEqual(withProportions.RedTonnage, withProportions.AmberMedicalTonnage);
            var reconciled = H1ProjectedProducersBuilderUtils.ReconcileRoundingDifference(tonnage, withProportions);
            Assert.AreEqual(0, tonnage - reconciled.TotalRamTonnage());
            Assert.AreEqual(withProportions with { AmberMedicalTonnage = withProportions.AmberMedicalTonnage + roundingDiff }, reconciled);
        }

        [TestMethod]
        public void SumProducerGroupTonnages_CorrectlyTotalsProportionForHC()
        {
            var expRamH2Proportions = new RAMProportions { Red = 50, Amber = 50, Green = 0, RedMedical = 0, AmberMedical = 0, GreenMedical = 0 };
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
                            HouseholdRAMTonnage = new RAMTonnage { RedTonnage = 100, AmberTonnage = 0, GreenTonnage = 0, RedMedicalTonnage = 0, AmberMedicalTonnage = 0, GreenMedicalTonnage = 0 },
                            PublicBinTonnage = 200,
                            PublicBinRAMTonnage = new RAMTonnage { RedTonnage = 0, AmberTonnage = 100, GreenTonnage = 0, RedMedicalTonnage = 0, AmberMedicalTonnage = 0, GreenMedicalTonnage = 0 },
                            HouseholdTonnageWithoutRAM = 0,
                            PublicBinTonnageWithoutRAM = 100,
                            H2RamProportions = expRamH2Proportions,
                            ProjectedHouseholdTonnage = 100,
                            ProjectedHouseholdRAMTonnage = new RAMTonnage { RedTonnage = 100, AmberTonnage = 0, GreenTonnage = 0, RedMedicalTonnage = 0, AmberMedicalTonnage = 0, GreenMedicalTonnage = 0 },
                            ProjectedPublicBinTonnage = 200,
                            ProjectedPublicBinRAMTonnage = new RAMTonnage { RedTonnage = 50, AmberTonnage = 150, GreenTonnage = 0, RedMedicalTonnage = 0, AmberMedicalTonnage = 0, GreenMedicalTonnage = 0 }
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
                            HouseholdRAMTonnage = new RAMTonnage { RedTonnage = 100, AmberTonnage = 0, GreenTonnage = 0, RedMedicalTonnage = 0, AmberMedicalTonnage = 0, GreenMedicalTonnage = 0 },
                            PublicBinTonnage = 200,
                            PublicBinRAMTonnage = new RAMTonnage { RedTonnage = 0, AmberTonnage = 100, GreenTonnage = 0, RedMedicalTonnage = 0, AmberMedicalTonnage = 0, GreenMedicalTonnage = 0 },
                            HouseholdTonnageWithoutRAM = 0,
                            PublicBinTonnageWithoutRAM = 100,
                            H2RamProportions = expRamH2Proportions,
                            ProjectedHouseholdTonnage = 100,
                            ProjectedHouseholdRAMTonnage = new RAMTonnage { RedTonnage = 100, AmberTonnage = 0, GreenTonnage = 0, RedMedicalTonnage = 0, AmberMedicalTonnage = 0, GreenMedicalTonnage = 0 },
                            ProjectedPublicBinTonnage = 200,
                            ProjectedPublicBinRAMTonnage = new RAMTonnage { RedTonnage = 50, AmberTonnage = 150, GreenTonnage = 0, RedMedicalTonnage = 0, AmberMedicalTonnage = 0, GreenMedicalTonnage = 0 }
                        }
                    }
                },
            };

            var expSummedAlm = new CalcResultH1ProjectedProducerMaterialTonnage() {
                HouseholdTonnage = 200,
                HouseholdRAMTonnage = new RAMTonnage { RedTonnage = 200, AmberTonnage = 0, GreenTonnage = 0, RedMedicalTonnage = 0, AmberMedicalTonnage = 0, GreenMedicalTonnage = 0 },
                PublicBinTonnage = 400,
                PublicBinRAMTonnage = new RAMTonnage { RedTonnage = 0, AmberTonnage = 200, GreenTonnage = 0, RedMedicalTonnage = 0, AmberMedicalTonnage = 0, GreenMedicalTonnage = 0 },
                HouseholdTonnageWithoutRAM = 0,
                PublicBinTonnageWithoutRAM = 200,
                H2RamProportions = expRamH2Proportions,
                ProjectedHouseholdTonnage = 200,
                ProjectedHouseholdRAMTonnage = new RAMTonnage { RedTonnage = 200, AmberTonnage = 0, GreenTonnage = 0, RedMedicalTonnage = 0, AmberMedicalTonnage = 0, GreenMedicalTonnage = 0 },
                ProjectedPublicBinTonnage = 400,
                ProjectedPublicBinRAMTonnage = new RAMTonnage { RedTonnage = 100, AmberTonnage = 300, GreenTonnage = 0, RedMedicalTonnage = 0, AmberMedicalTonnage = 0, GreenMedicalTonnage = 0 }
            };

            var result = H1ProjectedProducersBuilderUtils.SumProducerGroupTonnages(prodGroup);

            Assert.IsTrue(result.IsSubtotal);
            Assert.AreEqual("1", result.Level);
            Assert.AreEqual(expSummedAlm, result.H1ProjectedTonnageByMaterial["AL"]);
        }

        private decimal To3DP(decimal value)
        {
            return Math.Round(value, 3);
        }

        private decimal To6DP(decimal value)
        {
            return Math.Round(value, 6);
        }

        private static void AssertWithin(decimal expected, decimal actual, decimal tolerance = 0.001m)
        {
            Assert.IsTrue(Math.Abs(expected - actual) <= tolerance);
        }

        private RAMTonnage ProducerReportedMaterialToRAMTonnage(ProducerReportedMaterial material)
        {
            return new RAMTonnage
            {
                RedTonnage = material.PackagingTonnageRed ?? 0,
                AmberTonnage = material.PackagingTonnageAmber ?? 0,
                GreenTonnage = material.PackagingTonnageGreen ?? 0,
                RedMedicalTonnage = material.PackagingTonnageRedMedical ?? 0,
                AmberMedicalTonnage = material.PackagingTonnageAmberMedical ?? 0,
                GreenMedicalTonnage = material.PackagingTonnageGreenMedical ?? 0
            };
        }

        private decimal CalcWithoutRam(ProducerReportedMaterial material)
        {
            return material.PackagingTonnage - (material.PackagingTonnageRed ?? 0) - (material.PackagingTonnageAmber ?? 0) - (material.PackagingTonnageGreen ?? 0) - (material.PackagingTonnageRedMedical ?? 0) - (material.PackagingTonnageAmberMedical ?? 0) - (material.PackagingTonnageGreenMedical ?? 0);
        }

        private (ProducerReportedMaterial, ProducerReportedMaterial, List<ProducerDetail>) GetH1NoRamReportedMaterials()
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

        private (ProducerReportedMaterial, ProducerReportedMaterial, ProducerReportedMaterial, ProducerReportedMaterial, List<ProducerDetail>) GetH1NoRamReportedMaterialsWithSubsidiary()
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

        private (ProducerReportedMaterial, ProducerReportedMaterial, List<ProducerDetail>) GetH1PartialRamReportedMaterials()
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

        private (ProducerReportedMaterial, ProducerReportedMaterial, List<ProducerDetail>) GetH1FullRamReportedMaterials()
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

        private (CalcResultH2ProjectedProducerMaterialTonnage, CalcResultH2ProjectedProducerMaterialTonnage, List<CalcResultH2ProjectedProducer>) GetH2WithHCSubtotal()
        {
            var alm = new CalcResultH2ProjectedProducerMaterialTonnage
                        {
                            HouseholdTonnage = 75,
                            HouseholdRAMTonnage = new RAMTonnage { RedTonnage = 0, AmberTonnage = 11, GreenTonnage = 12, RedMedicalTonnage = 13, AmberMedicalTonnage = 14, GreenMedicalTonnage = 15 },
                            PublicBinTonnage = 135,
                            PublicBinRAMTonnage = new RAMTonnage { RedTonnage = 0, AmberTonnage = 21, GreenTonnage = 22, RedMedicalTonnage = 23, AmberMedicalTonnage = 24, GreenMedicalTonnage = 25 },
                            HouseholdTonnageWithoutRAM = 10,
                            PublicBinTonnageWithoutRAM = 20,
                            ProjectedHouseholdTonnage = 75,
                            ProjectedHouseholdRAMTonnage = new RAMTonnage { RedTonnage = 10, AmberTonnage = 11, GreenTonnage = 12, RedMedicalTonnage = 13, AmberMedicalTonnage = 14, GreenMedicalTonnage = 15 },
                            ProjectedPublicBinTonnage = 135,
                            ProjectedPublicBinRAMTonnage = new RAMTonnage { RedTonnage = 20, AmberTonnage = 21, GreenTonnage = 22, RedMedicalTonnage = 23, AmberMedicalTonnage = 24, GreenMedicalTonnage = 25 },

                        };
            var glass = new CalcResultH2ProjectedProducerMaterialTonnage
                        {
                            HouseholdTonnage = 75,
                            HouseholdRAMTonnage = new RAMTonnage { RedTonnage = 10, AmberTonnage = 11, GreenTonnage = 5, RedMedicalTonnage = 0, AmberMedicalTonnage = 14, GreenMedicalTonnage = 0 },
                            PublicBinTonnage = 135,
                            PublicBinRAMTonnage = new RAMTonnage { RedTonnage = 10, AmberTonnage = 21, GreenTonnage = 22, RedMedicalTonnage = 23, AmberMedicalTonnage = 24, GreenMedicalTonnage = 25 },
                            HouseholdDrinksContainerTonnage = 195,
                            HouseholdDrinksContainerRAMTonnage = new RAMTonnage { RedTonnage = 30, AmberTonnage = 11, GreenTonnage = 32, RedMedicalTonnage = 33, AmberMedicalTonnage = 34, GreenMedicalTonnage = 35 },
                            HouseholdTonnageWithoutRAM = 35,
                            PublicBinTonnageWithoutRAM = 10,
                            HouseholdDrinksContainerTonnageWithoutRAM = 20,
                            ProjectedHouseholdTonnage = 75,
                            ProjectedHouseholdRAMTonnage = new RAMTonnage { RedTonnage = 45, AmberTonnage = 11, GreenTonnage = 5, RedMedicalTonnage = 0, AmberMedicalTonnage = 14, GreenMedicalTonnage = 0 },
                            ProjectedPublicBinTonnage = 135,
                            ProjectedPublicBinRAMTonnage = new RAMTonnage { RedTonnage = 20, AmberTonnage = 21, GreenTonnage = 22, RedMedicalTonnage = 23, AmberMedicalTonnage = 24, GreenMedicalTonnage = 25 },
                            ProjectedHouseholdDrinksContainerTonnage = 195,
                            ProjectedHouseholdDrinksContainerRAMTonnage = new RAMTonnage { RedTonnage = 50, AmberTonnage = 11, GreenTonnage = 32, RedMedicalTonnage = 33, AmberMedicalTonnage = 34, GreenMedicalTonnage = 35 },
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

        private (CalcResultH2ProjectedProducerMaterialTonnage, CalcResultH2ProjectedProducerMaterialTonnage, List<CalcResultH2ProjectedProducer>) GetH2FullRamProjectedProducers()
        {
            var alm = new CalcResultH2ProjectedProducerMaterialTonnage
                        {
                            HouseholdTonnage = 75,
                            HouseholdRAMTonnage = new RAMTonnage { RedTonnage = 10, AmberTonnage = 11, GreenTonnage = 12, RedMedicalTonnage = 13, AmberMedicalTonnage = 14, GreenMedicalTonnage = 15 },
                            PublicBinTonnage = 135,
                            PublicBinRAMTonnage = new RAMTonnage { RedTonnage = 20, AmberTonnage = 21, GreenTonnage = 22, RedMedicalTonnage = 23, AmberMedicalTonnage = 24, GreenMedicalTonnage = 25 },
                            HouseholdTonnageWithoutRAM = 10,
                            PublicBinTonnageWithoutRAM = 20,
                            ProjectedHouseholdTonnage = 75,
                            ProjectedHouseholdRAMTonnage = new RAMTonnage { RedTonnage = 10, AmberTonnage = 11, GreenTonnage = 12, RedMedicalTonnage = 13, AmberMedicalTonnage = 14, GreenMedicalTonnage = 15 },
                            ProjectedPublicBinTonnage = 135,
                            ProjectedPublicBinRAMTonnage = new RAMTonnage { RedTonnage = 20, AmberTonnage = 21, GreenTonnage = 22, RedMedicalTonnage = 23, AmberMedicalTonnage = 24, GreenMedicalTonnage = 25 },
                        };
            var glass = new CalcResultH2ProjectedProducerMaterialTonnage
                        {
                            HouseholdTonnage = 75,
                            HouseholdRAMTonnage = new RAMTonnage { RedTonnage = 10, AmberTonnage = 11, GreenTonnage = 12, RedMedicalTonnage = 13, AmberMedicalTonnage = 14, GreenMedicalTonnage = 15 },
                            PublicBinTonnage = 135,
                            PublicBinRAMTonnage = new RAMTonnage { RedTonnage = 20, AmberTonnage = 21, GreenTonnage = 22, RedMedicalTonnage = 23, AmberMedicalTonnage = 24, GreenMedicalTonnage = 25 },
                            HouseholdDrinksContainerTonnage = 195,
                            HouseholdDrinksContainerRAMTonnage = new RAMTonnage { RedTonnage = 30, AmberTonnage = 31, GreenTonnage = 32, RedMedicalTonnage = 33, AmberMedicalTonnage = 34, GreenMedicalTonnage = 35 },
                            HouseholdTonnageWithoutRAM = 35,
                            PublicBinTonnageWithoutRAM = 10,
                            HouseholdDrinksContainerTonnageWithoutRAM = 20,
                            ProjectedHouseholdTonnage = 75,
                            ProjectedHouseholdRAMTonnage = new RAMTonnage { RedTonnage = 10, AmberTonnage = 11, GreenTonnage = 12, RedMedicalTonnage = 13, AmberMedicalTonnage = 14, GreenMedicalTonnage = 15 },
                            ProjectedPublicBinTonnage = 135,
                            ProjectedPublicBinRAMTonnage = new RAMTonnage { RedTonnage = 20, AmberTonnage = 21, GreenTonnage = 22, RedMedicalTonnage = 23, AmberMedicalTonnage = 24, GreenMedicalTonnage = 25 },
                            ProjectedHouseholdDrinksContainerTonnage = 195,
                            ProjectedHouseholdDrinksContainerRAMTonnage = new RAMTonnage { RedTonnage = 30, AmberTonnage = 31, GreenTonnage = 32, RedMedicalTonnage = 33, AmberMedicalTonnage = 34, GreenMedicalTonnage = 35 },
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

        private (CalcResultH2ProjectedProducerMaterialTonnage, CalcResultH2ProjectedProducerMaterialTonnage, List<CalcResultH2ProjectedProducer>) GetH2PartialRamProjectedProducers()
        {
            var alm = new CalcResultH2ProjectedProducerMaterialTonnage
                        {
                            HouseholdTonnage = 75,
                            HouseholdRAMTonnage = new RAMTonnage { RedTonnage = 0, AmberTonnage = 11, GreenTonnage = 12, RedMedicalTonnage = 13, AmberMedicalTonnage = 14, GreenMedicalTonnage = 15 },
                            PublicBinTonnage = 135,
                            PublicBinRAMTonnage = new RAMTonnage { RedTonnage = 0, AmberTonnage = 21, GreenTonnage = 22, RedMedicalTonnage = 23, AmberMedicalTonnage = 24, GreenMedicalTonnage = 25 },
                            HouseholdTonnageWithoutRAM = 10,
                            PublicBinTonnageWithoutRAM = 20,
                            ProjectedHouseholdTonnage = 75,
                            ProjectedHouseholdRAMTonnage = new RAMTonnage { RedTonnage = 10, AmberTonnage = 11, GreenTonnage = 12, RedMedicalTonnage = 13, AmberMedicalTonnage = 14, GreenMedicalTonnage = 15 },
                            ProjectedPublicBinTonnage = 135,
                            ProjectedPublicBinRAMTonnage = new RAMTonnage { RedTonnage = 20, AmberTonnage = 21, GreenTonnage = 22, RedMedicalTonnage = 23, AmberMedicalTonnage = 24, GreenMedicalTonnage = 25 },

                        };
            var glass = new CalcResultH2ProjectedProducerMaterialTonnage
                        {
                            HouseholdTonnage = 75,
                            HouseholdRAMTonnage = new RAMTonnage { RedTonnage = 10, AmberTonnage = 11, GreenTonnage = 5, RedMedicalTonnage = 0, AmberMedicalTonnage = 14, GreenMedicalTonnage = 0 },
                            PublicBinTonnage = 135,
                            PublicBinRAMTonnage = new RAMTonnage { RedTonnage = 10, AmberTonnage = 21, GreenTonnage = 22, RedMedicalTonnage = 23, AmberMedicalTonnage = 24, GreenMedicalTonnage = 25 },
                            HouseholdDrinksContainerTonnage = 195,
                            HouseholdDrinksContainerRAMTonnage = new RAMTonnage { RedTonnage = 30, AmberTonnage = 11, GreenTonnage = 32, RedMedicalTonnage = 33, AmberMedicalTonnage = 34, GreenMedicalTonnage = 35 },
                            HouseholdTonnageWithoutRAM = 35,
                            PublicBinTonnageWithoutRAM = 10,
                            HouseholdDrinksContainerTonnageWithoutRAM = 20,
                            ProjectedHouseholdTonnage = 75,
                            ProjectedHouseholdRAMTonnage = new RAMTonnage { RedTonnage = 45, AmberTonnage = 11, GreenTonnage = 5, RedMedicalTonnage = 0, AmberMedicalTonnage = 14, GreenMedicalTonnage = 0 },
                            ProjectedPublicBinTonnage = 135,
                            ProjectedPublicBinRAMTonnage = new RAMTonnage { RedTonnage = 20, AmberTonnage = 21, GreenTonnage = 22, RedMedicalTonnage = 23, AmberMedicalTonnage = 24, GreenMedicalTonnage = 25 },
                            ProjectedHouseholdDrinksContainerTonnage = 195,
                            ProjectedHouseholdDrinksContainerRAMTonnage = new RAMTonnage { RedTonnage = 50, AmberTonnage = 11, GreenTonnage = 32, RedMedicalTonnage = 33, AmberMedicalTonnage = 34, GreenMedicalTonnage = 35 },
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

        private CalcResultH2ProjectedProducerMaterialTonnage GetEmptyH2MaterialTonnage()
        {
            return new CalcResultH2ProjectedProducerMaterialTonnage
            {
                HouseholdTonnage = 0,
                HouseholdRAMTonnage = EmptyRAMTonnage(),
                PublicBinTonnage = 0,
                PublicBinRAMTonnage = EmptyRAMTonnage(),
                HouseholdTonnageWithoutRAM = 0,
                PublicBinTonnageWithoutRAM = 0,
                ProjectedHouseholdTonnage = 0,
                ProjectedHouseholdRAMTonnage = EmptyRAMTonnage(),
                ProjectedPublicBinTonnage = 0,
                ProjectedPublicBinRAMTonnage = EmptyRAMTonnage(),
            };
        }

        private CalcResultH2ProjectedProducerMaterialTonnage GetEmptyH2MaterialTonnageWithHDC()
        {
            return new CalcResultH2ProjectedProducerMaterialTonnage
            {
                HouseholdTonnage = 0,
                HouseholdRAMTonnage = EmptyRAMTonnage(),
                PublicBinTonnage = 0,
                PublicBinRAMTonnage = EmptyRAMTonnage(),
                HouseholdDrinksContainerTonnage = 0,
                HouseholdDrinksContainerRAMTonnage = EmptyRAMTonnage(),
                HouseholdTonnageWithoutRAM = 0,
                PublicBinTonnageWithoutRAM = 0,
                ProjectedHouseholdTonnage = 0,
                ProjectedHouseholdRAMTonnage = EmptyRAMTonnage(),
                ProjectedPublicBinTonnage = 0,
                ProjectedPublicBinRAMTonnage = EmptyRAMTonnage(),
                ProjectedHouseholdDrinksContainerTonnage = 0,
                ProjectedHouseholdDrinksContainerRAMTonnage = EmptyRAMTonnage(),
            };
}

        private (CalcResultH2ProjectedProducerMaterialTonnage, CalcResultH2ProjectedProducerMaterialTonnage, List<CalcResultH2ProjectedProducer>) GetH2NoReportedMaterialProjectedProducers()
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
                                HouseholdRAMTonnage = new RAMTonnage { RedTonnage = 10, AmberTonnage = 11, GreenTonnage = 12, RedMedicalTonnage = 13, AmberMedicalTonnage = 14, GreenMedicalTonnage = 15 },
                                PublicBinTonnage = 135,
                                PublicBinRAMTonnage = new RAMTonnage { RedTonnage = 20, AmberTonnage = 21, GreenTonnage = 22, RedMedicalTonnage = 23, AmberMedicalTonnage = 24, GreenMedicalTonnage = 25 },
                                HouseholdTonnageWithoutRAM = 10,
                                PublicBinTonnageWithoutRAM = 20,
                                ProjectedHouseholdTonnage = 75,
                                ProjectedHouseholdRAMTonnage = new RAMTonnage { RedTonnage = 20, AmberTonnage = 11, GreenTonnage = 12, RedMedicalTonnage = 13, AmberMedicalTonnage = 14, GreenMedicalTonnage = 15 },
                                ProjectedPublicBinTonnage = 135,
                                ProjectedPublicBinRAMTonnage = new RAMTonnage { RedTonnage = 40, AmberTonnage = 21, GreenTonnage = 22, RedMedicalTonnage = 23, AmberMedicalTonnage = 24, GreenMedicalTonnage = 25 },

                            }
                        }
                    }
                }
            });
        }
    }
}
