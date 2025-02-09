using AutoFixture;
using EPR.Calculator.API.Builder;
using EPR.Calculator.API.Controllers;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Enums;
using EPR.Calculator.API.Exporter;
using EPR.Calculator.API.Models;
using EPR.Calculator.API.Services;
using EPR.Calculator.API.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Net;
using static EPR.Calculator.API.Services.TransposePomAndOrgDataService;

namespace EPR.Calculator.API.UnitTests.Services
{
    [TestClass]
    public class TransposePomAndOrgDataServiceTests
    {
        private readonly ApplicationDBContext _context;

        private readonly DbContextOptions<ApplicationDBContext> _dbContextOptions;

        public TransposePomAndOrgDataServiceTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

            _context = new ApplicationDBContext(_dbContextOptions);

            SeedDatabase();
        }

        public Fixture Fixture { get; init; } = new Fixture();

        [TestCleanup]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        private void SeedDatabase()
        {
            
            _context.CalculatorRunOrganisationDataMaster.AddRange(GetCalculatorRunOrganisationDataMaster());
            _context.CalculatorRunOrganisationDataDetails.AddRange(GetCalculatorRunOrganisationDataDetails());

            _context.CalculatorRunPomDataMaster.AddRange(GetCalculatorRunPomDataMaster());
            _context.CalculatorRunPomDataDetails.AddRange(GetCalculatorRunPomDataDetails());


            _context.CalculatorRuns.AddRange(GetCalculatorRuns());
            _context.Material.AddRange(GetMaterials());

            _context.SaveChanges();
        }


        [TestMethod]
        public void Transpose_Should_Return_Correct_Producer_Detail()
        {   
            var expectedResult = new ProducerDetail
            {
                Id = 1,
                ProducerId = 1,
                ProducerName = "UPU LIMITED",
                CalculatorRunId = 1,
                CalculatorRun = Fixture.Create<CalculatorRun>(),
            };

#pragma warning disable CS8604 // Possible null reference argument.
            var service = new TransposePomAndOrgDataService(_context);
#pragma warning restore CS8604 // Possible null reference argument.

            var resultsRequestDto = new CalcResultsRequestDto { RunId = 3 };
            service.Transpose(resultsRequestDto, CancellationToken.None);

            var producerDetail = _context.ProducerDetail.FirstOrDefault();
            Assert.IsNotNull(producerDetail);
            Assert.AreEqual(expectedResult.ProducerId, producerDetail.ProducerId);
            Assert.AreEqual(expectedResult.ProducerName, producerDetail.ProducerName);           
        }

        [TestMethod]
        public void Transpose_Should_Return_Correct_Producer_Reported_Material()
        {
            var expectedResult = new ProducerReportedMaterial
            {
                Id = 1,
                MaterialId = 4,
                ProducerDetailId = 1,
                PackagingType = "CW",
                PackagingTonnage = 1,
                Material = new Material
                {
                    Id = 4,
                    Code = "PC",
                    Name = "Paper or card",
                    Description = "Paper or card"
                },
                ProducerDetail = new ProducerDetail
                {
                    Id = 1,
                    ProducerId = 1,
                    SubsidiaryId = "1",
                    ProducerName = "UPU LIMITED",
                    CalculatorRunId = 1,
                    CalculatorRun = Fixture.Create<CalculatorRun>(),
                }
            };

#pragma warning disable CS8604 // Possible null reference argument.
            var service = new TransposePomAndOrgDataService(_context);
#pragma warning restore CS8604 // Possible null reference argument.

            var resultsRequestDto = new CalcResultsRequestDto { RunId = 3 };
            service.Transpose(resultsRequestDto, CancellationToken.None);

            var producerReportedMaterial = _context.ProducerReportedMaterial.FirstOrDefault();
            Assert.IsNotNull(producerReportedMaterial);
            Assert.AreEqual(expectedResult.Material.Code, producerReportedMaterial.Material!.Code);
            Assert.AreEqual(expectedResult.Material.Name, producerReportedMaterial.Material.Name);
            Assert.AreEqual(expectedResult.ProducerDetail.ProducerId, producerReportedMaterial.ProducerDetail!.ProducerId);
            Assert.AreEqual(expectedResult.ProducerDetail.ProducerName, producerReportedMaterial.ProducerDetail.ProducerName);
        }

        [TestMethod]
        public void Transpose_Should_Return_Correct_Producer_Subsidary_Detail()
        {
            var expectedResult = new ProducerDetail
            {
                Id = 1,
                ProducerId = 2,
                SubsidiaryId ="1",                
                ProducerName = "Subsid2",
                CalculatorRunId = 1,
                CalculatorRun = Fixture.Create<CalculatorRun>(),
            };

#pragma warning disable CS8604 // Possible null reference argument.
            var service = new TransposePomAndOrgDataService(_context);
#pragma warning restore CS8604 // Possible null reference argument.

            var resultsRequestDto = new CalcResultsRequestDto { RunId = 1 };
            service.Transpose(resultsRequestDto, CancellationToken.None);

            var producerDetail = _context.ProducerDetail.FirstOrDefault(t=>t.SubsidiaryId != null);
            Assert.IsNotNull(producerDetail);
            Assert.AreEqual(expectedResult.ProducerId, producerDetail.ProducerId);
            Assert.AreEqual(expectedResult.ProducerName, producerDetail.ProducerName);
        }


        [TestMethod]
        public void Transpose_Should_Return_Correct_Producer_Detail_When_Submission_Period_Not_Exists()
        {
            var expectedResult = new ProducerDetail
            {
                Id = 1,
                ProducerId = 2,
                ProducerName = "Subsid2",
                CalculatorRunId = 1,
                CalculatorRun = Fixture.Create<CalculatorRun>(),
            };

#pragma warning disable CS8604 // Possible null reference argument.
            var service = new TransposePomAndOrgDataService(_context);
#pragma warning restore CS8604 // Possible null reference argument.

            var resultsRequestDto = new CalcResultsRequestDto { RunId = 1 };
            service.Transpose(resultsRequestDto, CancellationToken.None);

            var producerDetail = _context.ProducerDetail.FirstOrDefault();
            Assert.IsNotNull(producerDetail);
            Assert.AreEqual(expectedResult.ProducerId, producerDetail.ProducerId);
            Assert.AreEqual(expectedResult.ProducerName, producerDetail.ProducerName);
        }

        [TestMethod]
        public void Transpose_Should_Return_Latest_Organisation_Name()
        {
            var mockContext = new Mock<ApplicationDBContext>();
            var service = new TransposePomAndOrgDataService(mockContext.Object);

            var organisationDetails = new List<CalculatorRunOrganisationDataDetail>
            {
                new CalculatorRunOrganisationDataDetail
                {
                    OrganisationId = 1,
                    OrganisationName = "Test1",
                    SubsidaryId = "sub1",
                    SubmissionPeriodDesc = "January to June 2023"
                },
                new CalculatorRunOrganisationDataDetail
                {
                    OrganisationId = 2,
                    OrganisationName = "Test2",
                    SubsidaryId = "sub2",
                    SubmissionPeriodDesc = "January to June 2023"
                }
            };

            var orgDetails = service.GetAllOrganisationsBasedonRunId(organisationDetails);

            var orgSubDetails = new List<OrganisationDetails>()
            {
                new OrganisationDetails()
                {
                     OrganisationId = 1,
                     OrganisationName = "Test1",
                     SubsidaryId = "sub1",
                     SubmissionPeriodDescription = "January to June 2023"
                },
                 new OrganisationDetails()
                {
                     OrganisationId = 2,
                     OrganisationName = "Test2",
                     SubsidaryId = "sub2",
                     SubmissionPeriodDescription = "January to June 2024"
                 }
            };

            var output = service.GetLatestOrganisationName(1, orgSubDetails, orgDetails);
            Assert.IsNotNull(output);
            Assert.AreEqual("Test1", output);
        }


            protected static IEnumerable<CalculatorRunOrganisationDataMaster> GetCalculatorRunOrganisationDataMaster()
        {
            var list = new List<CalculatorRunOrganisationDataMaster>
            {
                new() {
                    Id = 1,
                    CalendarYear = "2024-25",
                    EffectiveFrom = DateTime.Now,
                    CreatedBy = "Test user",
                    CreatedAt = DateTime.Now
                },
                new() {
                    Id = 2,
                    CalendarYear = "2024-25",
                    EffectiveFrom = DateTime.Now,
                    CreatedBy = "Test user",
                    CreatedAt = DateTime.Now
                }
            };
            return list;
        }

        protected static IEnumerable<CalculatorRunOrganisationDataDetail> GetCalculatorRunOrganisationDataDetails()
        {
            var list = new List<CalculatorRunOrganisationDataDetail>();
            list.AddRange(new List<CalculatorRunOrganisationDataDetail>()
            {
                new() {
                    Id = 1,
                    OrganisationId = 1,
                    OrganisationName = "UPU LIMITED",
                    SubsidaryId ="1",
                    LoadTimeStamp = DateTime.Now,
                    CalculatorRunOrganisationDataMasterId = 1,
                    SubmissionPeriodDesc = "January to June 2023"
                },
                new() {
                    Id = 2,
                    OrganisationId = 1,
                    OrganisationName = "Test LIMITED",
                    LoadTimeStamp = DateTime.Now,
                    CalculatorRunOrganisationDataMasterId = 1,
                    SubmissionPeriodDesc = "July to December 2023"
                },
                 new() {
                    Id = 3,
                    OrganisationId = 2,
                    SubsidaryId = "1",
                    OrganisationName = "Subsid2",
                    LoadTimeStamp = DateTime.Now,
                    CalculatorRunOrganisationDataMasterId = 2,
                    SubmissionPeriodDesc = "July to December 2023"
                },
            });
            return list;
        }

        protected static IEnumerable<Material> GetMaterials()
        {
            var list = new List<Material>
            {
                new() {
                    Id = 1,
                    Code = "AL",
                    Name = "Aluminium",
                    Description = "Aluminium"
                },
                new() {
                    Id = 2,
                    Code = "FC",
                    Name = "Fibre composite",
                    Description = "Fibre composite"
                },
                new() {
                    Id = 3,
                    Code = "GL",
                    Name = "Glass",
                    Description = "Glass"
                },
                new() {
                    Id = 4,
                    Code = "PC",
                    Name = "Paper or card",
                    Description = "Paper or card"
                },
                new() {
                    Id = 5,
                    Code = "PL",
                    Name = "Plastic",
                    Description = "Plastic"
                },
                new() {
                    Id = 6,
                    Code = "ST",
                    Name = "Steel",
                    Description = "Steel"
                },
                new() {
                    Id = 7,
                    Code = "WD",
                    Name = "Wood",
                    Description = "Wood"
                },
                new() {
                    Id = 8,
                    Code = "OT",
                    Name = "Other materials",
                    Description = "Other materials"
                }
            };
            return list;
        }

        protected static IEnumerable<CalculatorRunPomDataMaster> GetCalculatorRunPomDataMaster()
        {
            var list = new List<CalculatorRunPomDataMaster>
            {
                new() {
                    Id = 1,
                    CalendarYear = "2024-25",
                    EffectiveFrom = DateTime.Now,
                    CreatedBy = "Test user",
                    CreatedAt = DateTime.Now
                },
                 new() {
                    Id = 2,
                    CalendarYear = "2024-25",
                    EffectiveFrom = DateTime.Now,
                    CreatedBy = "Test user",
                    CreatedAt = DateTime.Now
                }
            };
            return list;
        }

        protected static IEnumerable<CalculatorRunPomDataDetail> GetCalculatorRunPomDataDetails()
        {
            var list = new List<CalculatorRunPomDataDetail>
            {
                new() {
                    Id = 1,
                    OrganisationId = 1,
                    SubsidaryId = "1",
                    SubmissionPeriod = "2023-P2",
                    PackagingActivity = null,
                    PackagingType = "CW",
                    PackagingClass = "O1",
                    PackagingMaterial = "PC",
                    PackagingMaterialWeight = 1000,
                    LoadTimeStamp = DateTime.Now,
                    CalculatorRunPomDataMasterId = 1,
                    SubmissionPeriodDesc = "July to December 2023"
                },
                new() {
                    Id = 2,
                    OrganisationId = 1,
                    SubmissionPeriod = "2023-P2",
                    PackagingActivity = null,
                    PackagingType = "CW",
                    PackagingClass = "O1",
                    PackagingMaterial = "PC",
                    PackagingMaterialWeight = 1000,
                    LoadTimeStamp = DateTime.Now,
                    CalculatorRunPomDataMasterId = 1,
                    SubmissionPeriodDesc = "July to December 2023"
                },
                new() {
                    Id = 3,
                    OrganisationId = 1,
                    SubsidaryId = "1",
                    SubmissionPeriod = "2023-P1",
                    PackagingActivity = null,
                    PackagingType = "CW",
                    PackagingClass = "O1",
                    PackagingMaterial = "PC",
                    PackagingMaterialWeight = 1000,
                    LoadTimeStamp = DateTime.Now,
                    CalculatorRunPomDataMasterId = 1,
                    SubmissionPeriodDesc = "January to June 2023"
                },
                 new() {
                    Id = 4,
                    OrganisationId = 2,
                    SubsidaryId = "1",
                    SubmissionPeriod = "2024-P1",
                    PackagingActivity = null,
                    PackagingType = "CW",
                    PackagingClass = "O1",
                    PackagingMaterial = "PC",
                    PackagingMaterialWeight = 1000,
                    LoadTimeStamp = DateTime.Now,
                    CalculatorRunPomDataMasterId = 2,
                    SubmissionPeriodDesc = "January to June 2024"
                },
            };
            return list;
        }



        protected static IEnumerable<CalculatorRun> GetCalculatorRuns()
        {
            var list = new List<CalculatorRun>
            {
                new() {
                    CalculatorRunClassificationId = (int)RunClassification.RUNNING,
                    Name = "Test Run",
                    Financial_Year = "2024-25",
                    CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc),
                    CreatedBy = "Test User",
                    CalculatorRunOrganisationDataMasterId = 2,
                    CalculatorRunPomDataMasterId = 2,
                },
                new() {
                    CalculatorRunClassificationId = (int)RunClassification.RUNNING,
                    Name = "Test Calculated Result",
                    Financial_Year = "2024-25",
                    CreatedAt = new DateTime(2024, 8, 21, 14, 16, 27, DateTimeKind.Utc),
                    CreatedBy = "Test User"
                },
                new() {
                    CalculatorRunClassificationId = (int)RunClassification.RUNNING,
                    Name = "Test Run",
                    Financial_Year = "2024-25",
                    CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc),
                    CreatedBy = "Test User",
                    CalculatorRunOrganisationDataMasterId = 1,
                    CalculatorRunPomDataMasterId = 1,
                },
                new() {
                    CalculatorRunClassificationId = (int)RunClassification.RUNNING,
                    Name = "Test Calculated Result",
                    Financial_Year = "2024-25",
                    CreatedAt = new DateTime(2024, 8, 21, 14, 16, 27, DateTimeKind.Utc),
                    CreatedBy = "Test User",
                    CalculatorRunOrganisationDataMasterId = 2,
                    CalculatorRunPomDataMasterId = 2,
                }
            };
            return list;
        }
    }
}
