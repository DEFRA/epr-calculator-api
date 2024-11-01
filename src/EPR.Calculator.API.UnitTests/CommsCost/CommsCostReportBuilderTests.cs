using AutoFixture;
using EPR.Calculator.API.CommsCost;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.API.UnitTests.CommsCost
{
    [TestClass]
    public class CommsCostReportBuilderTests
    {
        public CommsCostReportBuilderTests() 
        {
            this.Fixture = new Fixture();
            this.Context = CreateMockDatabase();
            this.TestClass = new CommsCostReportBuilder(this.Context.Object);
        }

        private Mock<ApplicationDBContext> Context { get; }

        private Fixture Fixture { get; }

        private CommsCostReportBuilder TestClass { get; }

        [TestMethod]
        public void PreliminaryTesting()
        {
            var report = this.TestClass.BuildReport();
            File.WriteAllText("C:\\Users\\a898212\\OneDrive - Eviden\\Documents\\CommsCost\\CommsCostReport.csv", report);
        }

        private Mock<ApplicationDBContext> CreateMockDatabase()
        {
            var mockDb = new Mock<ApplicationDBContext>();

            var materialData = new[]
            {
                new Material{Id = 0, Code = "123", Name = "Aluminium"},
                //new Material{Id = 1, Code = "456", Name = "Fibre composite"},
                //new Material{Id = 2, Code = "789", Name = "Glass"},
            };
            mockDb.Setup(db => db.Material).Returns(CreateMockMaterialsTable(materialData.AsQueryable()));

            var countryData = new[]
            {
                new Country{Id = 0, Name = "England", Code = "123"},
                new Country{Id = 1, Name = "Wales", Code = "456"},
                new Country{Id = 2, Name = "Scotland", Code = "789"},
                new Country{Id = 3, Name = "Northern Ireland", Code = "789"},
            };
            mockDb.Setup(db => db.Country).Returns(CreateMockMaterialsTable(countryData.AsQueryable()));

            var countryApportmentData = new[]
            {
                Fixture.Build<CountryApportionment>()
                    .With(ca => ca.CountryId, 0)
                    .With(ca => ca.Apportionment, 52.49321928M)
                    .Create(),
                Fixture.Build<CountryApportionment>()
                    .With(ca => ca.CountryId, 1)
                    .With(ca => ca.Apportionment, 13.24848738M)
                    .Create(),
                Fixture.Build<CountryApportionment>()
                    .With(ca => ca.CountryId, 2)
                    .With(ca => ca.Apportionment, 24.32714375M)
                    .Create(),
                Fixture.Build<CountryApportionment>()
                    .With(ca => ca.CountryId, 3)
                    .With(ca => ca.Apportionment, 9.93114959M)
                    .Create(),
            };
            mockDb.Setup(db => db.CountryApportionment)
                .Returns(CreateMockMaterialsTable(countryApportmentData.AsQueryable()));


            return mockDb;
        }

        private static DbSet<TData> CreateMockMaterialsTable<TData>(IQueryable<TData> data) where TData : class
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
