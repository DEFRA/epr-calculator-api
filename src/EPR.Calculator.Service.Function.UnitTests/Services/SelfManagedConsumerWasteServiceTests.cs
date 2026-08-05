using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.UnitTests.Services
{
    [TestClass]
    public class SelfManagedConsumerWasteServiceTests
    {
        private ApplicationDBContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDBContext(options);
        }

        private int SeedProducer(
            ApplicationDBContext context,
            decimal hh,
            decimal hhRed,
            decimal hhRedMedical,
            decimal hhAmber,
            decimal hhAmberMedical,
            decimal hhGreen,
            decimal hhGreenMedical,
            decimal smcw,
            int runId,
            string materialCode = MaterialCodes.Aluminium)
        {
            var material = new Material { Code = materialCode, Name = materialCode };
            context.Material.Add(material);

            var producer = new ProducerDetail
            {
                ProducerId = 1,
                SubsidiaryId = null,
                CalculatorRunId = runId
            };
            context.ProducerDetail.Add(producer);

            context.ProducerMaterialPackaging.AddRange(
                new ProducerMaterialPackaging
                {
                    ProducerDetailId             = producer.Id,
                    MaterialId                   = material.Id,
                    PackagingType                = PackagingTypes.Household,
                    PackagingTonnage             = hh,
                    PackagingTonnageRed          = hhRed,
                    PackagingTonnageRedMedical   = hhRedMedical,
                    PackagingTonnageAmber        = hhAmber,
                    PackagingTonnageAmberMedical = hhAmberMedical,
                    PackagingTonnageGreen        = hhGreen,
                    PackagingTonnageGreenMedical = hhGreenMedical,
                    SubmissionPeriod             = "2025-H1"
                },
                new ProducerMaterialPackaging
                {
                    ProducerDetailId = producer.Id,
                    MaterialId       = material.Id,
                    PackagingType    = PackagingTypes.ConsumerWaste,
                    PackagingTonnage = smcw,
                    SubmissionPeriod = "2025-H1"
                }
            );
            context.SaveChanges();
            return material.Id;
        }

        [TestMethod]
        public void Sum_Should_Handle_Nulls()
        {
            var items = new List<SelfManagedConsumerWasteData?>
            {
                null,
                new()
                {
                    SmcwTonnage = 10,
                    ActionedSmcwTonnage = new RamTonnageGroup { Total = 6, Red = 1, Amber = 2, Green = 3 },
                    ResidualSmcwTonnage = 5,
                    NetTonnage = new RamTonnageGroup { Total = 3, Red = 1, Amber = 1, Green = 1 }
                },
                null,
                new()
                {
                    SmcwTonnage = 20,
                    ActionedSmcwTonnage = new RamTonnageGroup { Total = null, Red = null, Amber = null, Green = null },
                    ResidualSmcwTonnage = -5,
                    NetTonnage = new RamTonnageGroup { Total = 2, Red = 2, Amber = 2, Green = 2 }
                }
            };

            var result = items.Sum();

            Assert.AreEqual(30, result.SmcwTonnage);
            Assert.AreEqual(6 , result.ActionedSmcwTonnage.Total);
            Assert.AreEqual(1 , result.ActionedSmcwTonnage.Red);
            Assert.AreEqual(2 , result.ActionedSmcwTonnage.Amber);
            Assert.AreEqual(3 , result.ActionedSmcwTonnage.Green);
            Assert.AreEqual(0 , result.ResidualSmcwTonnage);
            Assert.AreEqual(5 , result.NetTonnage.Total);
            Assert.AreEqual(3 , result.NetTonnage.Red);
            Assert.AreEqual(3 , result.NetTonnage.Amber);
            Assert.AreEqual(3 , result.NetTonnage.Green);
        }

        [TestMethod]
        public void Sum_Should_Return_Zero_For_Empty()
        {
            var result = new List<SelfManagedConsumerWasteData?>().Sum();

            Assert.AreEqual(0, result.SmcwTonnage);
            Assert.AreEqual(0, result.ActionedSmcwTonnage.Total);
            Assert.AreEqual(0, result.NetTonnage.Total);
        }

        [TestMethod]
        public async Task Calculate_Should_Aggregate_OverallTotals_Correctly()
        {
            var runContext = TestDataHelper.CalculatorRun2024;
            var context = CreateContext();

            var materialId = SeedProducer(
                context,
                hh: 100,
                hhRed: 25,
                hhRedMedical: 25,
                hhAmber: 20,
                hhAmberMedical: 20,
                hhGreen: 5,
                hhGreenMedical: 5,
                smcw: 40,
                runId: runContext.RunId
            );

            var service = new SelfManagedConsumerWasteService(context);

            var materials = new[]
            {
                new MaterialDetail { Id = materialId, Code = MaterialCodes.Aluminium, Name = MaterialNames.Aluminium }
            };

            var result = await service.Calculate(runContext, materials);

            var total = result.TotalByMaterial[MaterialCodes.Aluminium];

            Assert.AreEqual(40, total.SmcwTonnage);
            Assert.AreEqual( 0, total.ActionedSmcwTonnage.Total);
            Assert.AreEqual( 0, total.ActionedSmcwTonnage.Red);
            Assert.AreEqual( 0, total.ActionedSmcwTonnage.Amber);
            Assert.AreEqual( 0, total.ActionedSmcwTonnage.Green);
            Assert.AreEqual(60, total.NetTonnage.Total);
        }

        [TestMethod]
        public async Task Calculate_Should_Return_Zero_When_Material_Missing()
        {
            var runContext = TestDataHelper.CalculatorRun2024;
            var context = CreateContext();

            SeedProducer(
                context,
                hh: 100,
                hhRed: 25,
                hhRedMedical: 25,
                hhAmber: 20,
                hhAmberMedical: 20,
                hhGreen: 5,
                hhGreenMedical: 5,
                smcw: 40,
                runId: runContext.RunId
            );

            var service = new SelfManagedConsumerWasteService(context);

            var result = await service.Calculate(
                runContext,
                [new MaterialDetail { Id = 99, Code = "NOT_EXIST", Name = "" }]);

            var total = result.TotalByMaterial["NOT_EXIST"];

            Assert.AreEqual(0, total.SmcwTonnage, "SmcwTonnage mismatch");
            Assert.AreEqual(0, total.ActionedSmcwTonnage.Total, "ActionedSmcwTonnage mismatch");
            Assert.AreEqual(0, total.NetTonnage.Total, "NetTonnage total mismatch");
        }

        [TestMethod]
        public async Task Calculate_Should_Only_Include_Level1_In_OverallTotals()
        {
            var runContext = TestDataHelper.CalculatorRun2024;
            var context = CreateContext();

            var material = new Material { Id = 1, Code = MaterialCodes.Aluminium, Name = MaterialNames.Aluminium, Description = "" };

            var producer1 = new ProducerDetail
            {
                Id = 1,
                ProducerId = 1,
                CalculatorRunId = 1,
                SubsidiaryId = null
            };

            var producer2 = new ProducerDetail
            {
                Id = 2,
                ProducerId = 1,
                CalculatorRunId = 1,
                SubsidiaryId = "99" // forces Level 2
            };

            context.Material.Add(material);
            context.ProducerDetail.AddRange(producer1, producer2);

            context.SaveChanges();

            var service = new SelfManagedConsumerWasteService(context);

            var result = await service.Calculate(
                runContext,
                [new MaterialDetail { Id = 1, Code = MaterialCodes.Aluminium, Name = MaterialNames.Aluminium }]);

            var total = result.TotalByMaterial[MaterialCodes.Aluminium];

            // Level 2 should not contribute
            // TODO there is no ProducerMaterialPackaging created, so there's nothing to filter out
            Assert.AreEqual(0, total.SmcwTonnage);
            Assert.AreEqual(0, total.NetTonnage.Total);
        }

        public static IEnumerable<object[]> NetReportedTonnageCases => new List<object[]>
        {
            //             hh        , red     , redM, amber     , amberM, green   , greenM, cw  ,                           expected tuple   (total     , red.    , amber     , green)       // ECV-430
            new object[] { 942.362m  , 464.266m, 0m  , 278.096m  , 0m    , 200m    , 0m    , 100m, ((decimal?, decimal?, decimal?, decimal?)) (842.362m  , 464.266m, 178.096m  , 200m    ) }, // AC1
            new object[] { 27522.364m, 11000m  , 0m  , 15899.754m, 0m    , 622.610m, 0m    , 500m, ((decimal?, decimal?, decimal?, decimal?)) (27022.364m, 11000m  , 15399.754m, 622.610m) }, // AC2
            new object[] { 3287.503m , 2190.39m, 0m  , 300m      , 0m    , 797.113m, 0m    , 500m, ((decimal?, decimal?, decimal?, decimal?)) (2787.503m , 1990.39m, 0m        , 797.113m) }, // AC3
            new object[] { 220m      , 25m     , 0m  , 50m       , 0m    , 145m    , 0m    , 100m, ((decimal?, decimal?, decimal?, decimal?)) (120m      , 0m      , 0m        , 120m    ) }, // AC4
            new object[] { 0m        , 0m      , 0m  , 0m        , 0m    , 0m      , 0m    , 100m, ((decimal?, decimal?, decimal?, decimal?)) (0m        , 0m      , 0m        , 0m      ) }, // AC5
            new object[] { 300m      , 100m    , 0m  , 100m      , 0m    , 100m    , 0m    , 50m , ((decimal?, decimal?, decimal?, decimal?)) (250m      , 100m    , 50m       , 100m    ) },
            new object[] { 300m      , 100m    , 0m  , 100m      , 0m    , 100m    , 0m    , 100m, ((decimal?, decimal?, decimal?, decimal?)) (200m      , 100m    , 0m        , 100m    ) },
            new object[] { 300m      , 0m      , 100m, 0m        , 100m  , 0m      , 100m  , 150m, ((decimal?, decimal?, decimal?, decimal?)) (150m      , 50m     , 0m        , 100m    ) }, // RAG Medical
            new object[] { 300m      , 50m     , 50m , 50m       , 50m   , 50m     , 50m   , 150m, ((decimal?, decimal?, decimal?, decimal?)) (150m      , 50m     , 0m        , 100m    ) }, // RAG + RAG Medical
            new object[] { 300m      , 100m    , 0m  , 100m      , 0m    , 100m    , 0m    , 200m, ((decimal?, decimal?, decimal?, decimal?)) (100m      , 0m      , 0m        , 100m    ) },
            new object[] { 300m      , 100m    , 0m  , 100m      , 0m    , 100m    , 0m    , 250m, ((decimal?, decimal?, decimal?, decimal?)) (50m       , 0m      , 0m        , 50m     ) },
            new object[] { 300m      , 100m    , 0m  , 100m      , 0m    , 100m    , 0m    , 300m, ((decimal?, decimal?, decimal?, decimal?)) (0m        , 0m      , 0m        , 0m      ) },
            new object[] { 300m      , 100m    , 0m  , 100m      , 0m    , 100m    , 0m    , 350m, ((decimal?, decimal?, decimal?, decimal?)) (0m        , 0m      , 0m        , 0m      ) },
        };

        [TestMethod]
        [DynamicData(nameof(NetReportedTonnageCases))]
        public async Task CanGetNetReportedTonnage_WithModulations(
            decimal hh,
            decimal red,
            decimal redMedical,
            decimal amber,
            decimal amberMedical,
            decimal green,
            decimal greenMedical,
            decimal cw,
            (decimal? total, decimal? red, decimal? amber, decimal? green) expected)
        {
            var runContext = TestDataHelper.CalculatorRun2026;
            var context = CreateContext();

            var materialId = SeedProducer(
                context,
                hh,
                red,
                redMedical,
                amber,
                amberMedical,
                green,
                greenMedical,
                cw,
                runId: runContext.RunId
            );

            var service = new SelfManagedConsumerWasteService(context);

            var result = await service.Calculate(
                runContext,
                [new MaterialDetail { Id = materialId, Code = MaterialCodes.Aluminium, Name = MaterialNames.Aluminium }]);

            var x = result.ProducerTotals.First().SmcwByMaterial[MaterialCodes.Aluminium];

            Assert.AreEqual(expected.total  , x.NetTonnage.Total              , "Net Total mismatch");
            Assert.AreEqual(expected.red    , x.NetTonnage.Red                , "Net Red mismatch");
            Assert.AreEqual(expected.amber  , x.NetTonnage.Amber              , "Net Amber mismatch");
            Assert.AreEqual(expected.green  , x.NetTonnage.Green              , "Net Green mismatch");
            Assert.AreEqual(cw              , x.SmcwTonnage                   , "SmcwTonnage mismatch");
            Assert.AreEqual(Math.Min(hh, cw), x.ActionedSmcwTonnage.Total     , "ActionedSmcwTonnage mismatch");
        }
    }
}
