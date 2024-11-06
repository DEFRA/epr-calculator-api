namespace EPR.Calculator.API.UnitTests.CommsCost
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
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
            var totalValues = new Dictionary<int, decimal>
            {
                {0, 2870.00M},
                {1, 7600.00M},
                {2, 4800.00M},
            };

            // Act
            var result = this.TestClass.BuildReport(this.RunId, totalValues);

            // Assert
            Assert.AreEqual(Resources.ExpectedCommsCostReportExample, result);

            // Write the report out so we can examine it in Excel.
            File.WriteAllText("C:\\Users\\a898212\\OneDrive - Eviden\\Documents\\CommsCost\\CommsCostReport.csv", result);
        }

        /// <summary>
        /// Create mocked data tables for the report to retrieve data from.
        /// </summary>
        private Mock<ApplicationDBContext> CreateMockDatabase(int runId)
        {
            var mockDb = new Mock<ApplicationDBContext>();

            // material table.
            var materialData = new[]
            {
                new Material{Id = 0, Code = "AL", Name = "Aluminium"},
                new Material{Id = 1, Code = "FC", Name = "Fibre composite"},
                new Material{Id = 2, Code = "GL", Name = "Glass"},
            };
            mockDb.Setup(db => db.Material)
                .Returns(CreateMockTable(materialData.AsQueryable()));

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
                .With(record => record.ParameterValue, 8000.000M)
                .With(record => record.ParameterUniqueReferenceId, "LRET-AL")
                .With(record => record.DefaultParameterSettingMasterId, parameterMasterId)
                .Create(),
                this.Fixture.Build<DefaultParameterSettingDetail>()
                .With(record => record.Id, 1)
                .With(record => record.ParameterValue, 7000.000M)
                .With(record => record.ParameterUniqueReferenceId, "LRET-FC")
                .With(record => record.DefaultParameterSettingMasterId, parameterMasterId)
                .Create(),
                this.Fixture.Build<DefaultParameterSettingDetail>()
                .With(record => record.Id, 2)
                .With(record => record.ParameterValue, 6000.000M)
                .With(record => record.ParameterUniqueReferenceId, "LRET-GL")
                .With(record => record.DefaultParameterSettingMasterId, parameterMasterId)
                .Create(),
            }.ToArray();
            mockDb.Setup(db => db.DefaultParameterSettingDetail)
                .Returns(CreateMockTable(parameterDetails.AsQueryable()));

            // default_parameter_setting_master table.
            var pmRecord = new Mock<DefaultParameterSettingMaster>();
            pmRecord.Setup(pm => pm.Id).Returns(parameterMasterId);
            pmRecord.Setup(pm => pm.Details).Returns(parameterDetails);
            var parametersMaster = new[] { pmRecord.Object };
            mockDb.Setup(db => db.DefaultParameterSettings)
                .Returns(CreateMockTable(parametersMaster.AsQueryable()));

            //// calculator_run table.
            var caclulatorRun = new Mock<CalculatorRun>();
            caclulatorRun.Setup(cr => cr.Id).Returns(runId);
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
