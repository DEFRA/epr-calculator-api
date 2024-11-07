namespace EPR.Calculator.API.UnitTests.CommsCost
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoFixture;
    using EPR.Calculator.API.CommsCost;
    using EPR.Calculator.API.Data;
    using EPR.Calculator.API.Data.DataModels;
    using EPR.Calculator.API.UnitTests.Properties;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    /// Unit tests for the <see cref="CommsCostReportBuilder"/> class.
    /// </summary>
    [TestClass]
    public class CommsCostReportBuilderTests
    {
        public CommsCostReportBuilderTests()
        {
            this.Fixture = new Fixture();
            this.RunId = Fixture.Create<int>();
            this.Context = CreateMockDatabase(RunId);
            this.TestClass = new CommsCostReportBuilder(this.Context.Object);
        }

        private int RunId { get; } 

        private Mock<ApplicationDBContext> Context { get; }

        private Fixture Fixture { get; }

        private CommsCostReportBuilder TestClass { get; }

        /// <summary>
        /// Test building the ComsCost report.
        /// </summary>
        [TestMethod]
        public void BuildReport()
        {
            // Arrange

            // Act
            var result = this.TestClass.BuildReport(this.RunId);

            // Assert
            Assert.AreEqual(Resources.ExpectedCommsCostReportExample, result);

        }

        /// <summary>
        /// Create mocked data tables for the report to retrieve data from.
        /// </summary>
        private Mock<ApplicationDBContext> CreateMockDatabase(int runId)
        {
            var mockDb = new Mock<ApplicationDBContext>();

            // producer_reported_material table.
            var aluminiumData = new[]
            {
                new ProducerReportedMaterial
                {
                    Id = Fixture.Create<int>(),
                    Material = Fixture.Create<Material>(),
                    PackagingType = "HH",
                    PackagingTonnage = 5000,
                    ProducerDetail = Fixture.Create<ProducerDetail>(),
                },
                new ProducerReportedMaterial
                {
                    Id = Fixture.Create<int>(),
                    Material = Fixture.Create<Material>(),
                    PackagingType = "HH",
                    PackagingTonnage = 1980,
                    ProducerDetail = Fixture.Create<ProducerDetail>(),
                },
            };
            var fibreCompositeData = new[]
            {
                new ProducerReportedMaterial
                {
                    Id = Fixture.Create<int>(),
                    Material = Fixture.Create<Material>(),
                    PackagingType = "HH",
                    PackagingTonnage = 11850,
                    ProducerDetail = Fixture.Create<ProducerDetail>(),
                },
            };
            var glassData = new[]
            {
                new ProducerReportedMaterial
                {
                    Id = Fixture.Create<int>(),
                    Material = Fixture.Create<Material>(),
                    PackagingType = "HH",
                    PackagingTonnage = 2000,
                    ProducerDetail = Fixture.Create<ProducerDetail>(),
                },
                new ProducerReportedMaterial
                {
                    Id = Fixture.Create<int>(),
                    Material = Fixture.Create<Material>(),
                    PackagingType = "HH",
                    PackagingTonnage = 2000,
                    ProducerDetail = Fixture.Create<ProducerDetail>(),
                },
                new ProducerReportedMaterial
                {
                    Id = Fixture.Create<int>(),
                    Material = Fixture.Create<Material>(),
                    PackagingType = "HH",
                    PackagingTonnage = 900,
                    ProducerDetail = Fixture.Create<ProducerDetail>(),
                }
            };

            // material table.
            var aluminium = new Mock<Material>();
            aluminium.Object.Id = Fixture.Create<int>();
            aluminium.Object.Code = "AL";
            aluminium.Object.Name = "Aluminium";
            aluminium.Setup(a => a.ProducerReportedMaterials).Returns(aluminiumData);
            var fiberComposite = new Mock<Material>();
            fiberComposite.Object.Id = Fixture.Create<int>();
            fiberComposite.Object.Code = "FC";
            fiberComposite.Object.Name = "Fibre composite";
            fiberComposite.Setup(a => a.ProducerReportedMaterials).Returns(fibreCompositeData);
            var glass = new Mock<Material>();
            glass.Object.Id = Fixture.Create<int>();
            glass.Object.Code = "GL";
            glass.Object.Name = "Glass";
            glass.Setup(a => a.ProducerReportedMaterials).Returns(glassData);

            var materials = new[]{aluminium.Object, fiberComposite.Object, glass.Object};
            mockDb.Setup(db => db.Material)
                .Returns(CreateMockTable(materials.AsQueryable()));

            // country table.
            var countryData = new[]
            {
                new Country{Id = 0, Name = "England", Code = "123"},
                new Country{Id = 1, Name = "Wales", Code = "456"},
                new Country{Id = 2, Name = "Scotland", Code = "789"},
                new Country{Id = 3, Name = "Northern Ireland", Code = "789"},
            };
            mockDb.Setup(db => db.Country)
                .Returns(CreateMockTable(countryData.AsQueryable()));

            var parameterMasterId = this.Fixture.Create<int>();

            // default_parameter_setting_detail
            var parameterDetails = new[]
            {
                                this.Fixture.Build<DefaultParameterSettingDetail>()
                .With(record => record.Id, 0)
                .With(record => record.ParameterValue, 2870)
                .With(record => record.ParameterUniqueReferenceId, "COMC-AL")
                //.With(record => record.DefaultParameterSettingMasterId, parameterMasterId)
                .Create(),
                this.Fixture.Build<DefaultParameterSettingDetail>()
                .With(record => record.Id, 1)
                .With(record => record.ParameterValue, 7600)
                .With(record => record.ParameterUniqueReferenceId, "COMC-FC")
                //.With(record => record.DefaultParameterSettingMasterId, parameterMasterId)
                .Create(),
                this.Fixture.Build<DefaultParameterSettingDetail>()
                .With(record => record.Id, 2)
                .With(record => record.ParameterValue, 4800)
                .With(record => record.ParameterUniqueReferenceId, "COMC-GL")
                //.With(record => record.DefaultParameterSettingMasterId, parameterMasterId)
                .Create(),
                this.Fixture.Build<DefaultParameterSettingDetail>()
                .With(record => record.Id, 0)
                .With(record => record.ParameterValue, 8000)
                .With(record => record.ParameterUniqueReferenceId, "LRET-AL")
                //.With(record => record.DefaultParameterSettingMasterId, parameterMasterId)
                .Create(),
                this.Fixture.Build<DefaultParameterSettingDetail>()
                .With(record => record.Id, 1)
                .With(record => record.ParameterValue, 7000)
                .With(record => record.ParameterUniqueReferenceId, "LRET-FC")
                //.With(record => record.DefaultParameterSettingMasterId, parameterMasterId)
                .Create(),
                this.Fixture.Build<DefaultParameterSettingDetail>()
                .With(record => record.Id, 2)
                .With(record => record.ParameterValue, 6000)
                .With(record => record.ParameterUniqueReferenceId, "LRET-GL")
                //.With(record => record.DefaultParameterSettingMasterId, parameterMasterId)
                .Create(),
            }.ToArray();
            mockDb.Setup(db => db.DefaultParameterSettingDetail)
                .Returns(CreateMockTable(parameterDetails.AsQueryable()));

            // default_parameter_setting_master table.
            var pmRecord = new Mock<DefaultParameterSettingMaster>();
            pmRecord.Object.Id = parameterMasterId;
            pmRecord.Setup(pm => pm.Details).Returns(parameterDetails);
            var parametersMaster = new[] { pmRecord.Object };
            mockDb.Setup(db => db.DefaultParameterSettings)
                .Returns(CreateMockTable(parametersMaster.AsQueryable()));

            //// calculator_run table.
            var caclulatorRun = new Mock<CalculatorRun>();
            caclulatorRun.Object.Id = runId;
            caclulatorRun.Setup(cr => cr.DefaultParameterSettingMaster).Returns(pmRecord.Object);
            var calculatorRuns = new[] { caclulatorRun.Object };

            mockDb.Setup(db => db.CalculatorRuns)
                .Returns(CreateMockTable(calculatorRuns.AsQueryable()));        

            // country_apportionment table.
            var countryApportmentData = new[]
            {
                this.Fixture.Build<CountryApportionment>()
                    .With(record => record.CountryId, 0)
                    .With(record => record.Apportionment, 52.49321928M)
                    .Create(),
                this.Fixture.Build<CountryApportionment>()
                    .With(record => record.CountryId, 1)
                    .With(record => record.Apportionment, 13.24848738M)
                    .Create(),
                this.Fixture.Build<CountryApportionment>()
                    .With(record => record.CountryId, 2)
                    .With(record => record.Apportionment, 24.32714375M)
                    .Create(),
                this.Fixture.Build<CountryApportionment>()
                    .With(record => record.CountryId, 3)
                    .With(record => record.Apportionment, 9.93114959M)
                    .Create(),
            };
            mockDb.Setup(db => db.CountryApportionment)
                .Returns(CreateMockTable(countryApportmentData.AsQueryable()));

            return mockDb;
        }

        /// <summary>
        /// Generic helper method for converting a collection of objects into to a mock database table.
        /// </summary>
        private static DbSet<TData> CreateMockTable<TData>(IQueryable<TData> data) where TData : class
        {
            var mockSet = new Mock<DbSet<TData>>();
            mockSet.As<IQueryable<TData>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<TData>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<TData>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<TData>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator);
            return mockSet.Object;
        }
    }
}
