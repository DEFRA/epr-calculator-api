using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures;

namespace EPR.Calculator.Service.Function.UnitTests.Services
{
    [TestClass]
    public class ErrorReportServiceTests
    {
        private ApplicationDBContext _dbContext = null!;
        private Mock<IInvoicedProducerService> _invoicedProducerService = null!;
        private ErrorReportService _sut = null!;

        [TestInitialize]
        public void Setup()
        {
            var fixture = TestFixtures.New();
            _dbContext = fixture.Freeze<ApplicationDBContext>();
            _invoicedProducerService = fixture.Freeze<Mock<IInvoicedProducerService>>();

            _sut = fixture.Create<ErrorReportService>();
        }

        [TestMethod]
        public void HandleMissingRegistrationData_InsertsCorrectErrorReports_WhenUnmatchedExists()
        {
            // Arrange
            var runId = 123;
            var createdBy = "test user";

            var pomDetails = new[]
            {
                new CalculatorRunPomDataDetail
                {
                    OrganisationId = 1,
                    SubsidiaryId = "11",
                    SubmissionPeriod = "2023-P2",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "July to December 2023"
                },
                new CalculatorRunPomDataDetail
                {
                    OrganisationId = 2,
                    SubsidiaryId = "22",
                    SubmissionPeriod = "2023-P2",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "July to December 2023"
                },
                new CalculatorRunPomDataDetail
                {
                    OrganisationId = 3,
                    SubsidiaryId = "33",
                    SubmissionPeriod = "2023-P2",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "July to December 2023"
                }
            };

            var orgDetails = new[]
            {
                new CalculatorRunOrganisationDataDetail
                {
                    OrganisationId = 1,
                    OrganisationName = "Test"
                }
            };

            // Act
            IEnumerable<ErrorReport> reportsList = ErrorReportService.HandleMissingRegistrationData(pomDetails, orgDetails, runId, createdBy);


            Assert.AreEqual(3, reportsList.Count(), "Expected 3 unmatched records to be returned.");

            foreach (var r in reportsList)
            {
                Assert.AreEqual(runId, r.CalculatorRunId);
                Assert.AreEqual(ErrorCodes.MissingRegistrationData, r.ErrorCode);
                Assert.AreEqual(createdBy, r.CreatedBy);
            }

            Assert.IsTrue(reportsList.Any(r => r.ProducerId == 1 && r.SubsidiaryId == "11"));
            Assert.IsTrue(reportsList.Any(r => r.ProducerId == 2 && r.SubsidiaryId == "22"));
            Assert.IsTrue(reportsList.Any(r => r.ProducerId == 3 && r.SubsidiaryId == "33"));
        }

        [TestMethod]
        public void HandleMissingRegistrationData_DeduplicatesMultiplePomsForSameOrgSub()
        {
            // Arrange
            var runId = 200;
            var createdBy = "dedup test";

            var pomDetails = new[]
            {
                new CalculatorRunPomDataDetail
                {
                    OrganisationId = 10,
                    SubsidiaryId = "101",
                    SubmissionPeriod = "2023-P2",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "July to December 2023"
                },
                new CalculatorRunPomDataDetail
                {
                    OrganisationId = 10,
                    SubsidiaryId = "101",
                    SubmissionPeriod = "2023-P2",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "July to December 2023"
                },
                new CalculatorRunPomDataDetail
                {
                    OrganisationId = 10,
                    SubsidiaryId = "102",
                    SubmissionPeriod = "2023-P2",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "July to December 2023"
                }
            };

            var orgDetails = Array.Empty<CalculatorRunOrganisationDataDetail>();

            // Act
            IEnumerable<ErrorReport> reportsList = ErrorReportService.HandleMissingRegistrationData(pomDetails, orgDetails, runId, createdBy);

            // Assert
            Assert.AreEqual(2, reportsList.Count(), "Expected 1 error per unique Org+Sub combination.");
            Assert.IsTrue(reportsList.Any(r => r.ProducerId == 10 && r.SubsidiaryId == "101"));
            Assert.IsTrue(reportsList.Any(r => r.ProducerId == 10 && r.SubsidiaryId == "102"));
        }

        [TestMethod]
        public void HandleMissingRegistrationData_DoesNotInsert_WhenAllMatched()
        {
            // Arrange
            var runId = 300;
            var createdBy = "no error";

            var pomDetails = new[]
            {
                new CalculatorRunPomDataDetail
                {
                    OrganisationId = 1,
                    SubsidiaryId = "101",
                    SubmissionPeriod = "2023-P2",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "July to December 2023"
                },
                new CalculatorRunPomDataDetail
                {
                    OrganisationId = 2,
                    SubsidiaryId = "102",
                     SubmissionPeriod = "2023-P2",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "July to December 2023"
                }
            };

            var orgDetails = new[]
            {
                new CalculatorRunOrganisationDataDetail
                {
                    OrganisationId = 1,
                    SubsidiaryId = "101",
                    OrganisationName = "Test"
                },
                new CalculatorRunOrganisationDataDetail
                {
                    OrganisationId = 2,
                    SubsidiaryId = "102",
                    OrganisationName = "Test1"
                }
            };

            // Act
            IEnumerable<ErrorReport> reportsList = ErrorReportService.HandleMissingRegistrationData(pomDetails, orgDetails, runId, createdBy);

            // Assert
            Assert.AreEqual(0, reportsList.Count(), "Expected no unmatched records to be returned.");
        }

        [TestMethod]
        public void HandleMissingRegistrationData_DeduplicatesMultiplePomsForSameOrgSub_Issue3()
        {
            // Arrange
            var runId = 400;
            var createdBy = "dedup issue3";

            // Simulate 41 POMs for the same Org/Sub which does NOT exist in org table
            var pomDetails = Enumerable.Range(1, 41)
                .Select(i => new CalculatorRunPomDataDetail
                {
                    OrganisationId = 100,
                    SubsidiaryId = "200",
                    SubmissionPeriod = "2023-P2",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "July to December 2023"
                })
                .ToArray();

            // Org table is empty → all POMs are unmatched
            var orgDetails = Array.Empty<CalculatorRunOrganisationDataDetail>();

            // Act
            IEnumerable<ErrorReport> reportsList = ErrorReportService.HandleMissingRegistrationData(pomDetails, orgDetails, runId, createdBy);

            // Assert
            // Should insert ONLY 1 error for the unique Org/Sub combination
            Assert.AreEqual(1, reportsList.Count(), "Expected only 1 error for the unique Org/Sub combination.");

            var report = reportsList.First();
            Assert.AreEqual(100, report.ProducerId);
            Assert.AreEqual("200", report.SubsidiaryId);
            Assert.AreEqual(runId, report.CalculatorRunId);
            Assert.AreEqual(ErrorCodes.MissingRegistrationData, report.ErrorCode);
            Assert.AreEqual(createdBy, report.CreatedBy);
        }

        [TestMethod]
        public void HandleMissingRegistrationData_DoesNotInsert_WhenNoUnmatched()
        {
            // Arrange
            var runId = 500;
            var createdBy = "no unmatched test";

            var pomDetails = new[]
            {
                new CalculatorRunPomDataDetail
                {
                    OrganisationId = 1,
                    SubsidiaryId = "101",
                    SubmissionPeriod = "2023-P2",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "July to December 2023"
                },
                new CalculatorRunPomDataDetail
                {
                    OrganisationId = 2,
                    SubsidiaryId = "202",
                    SubmissionPeriod = "2023-P2",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "July to December 2023"
                }
            };

            var orgDetails = new[]
            {
                new CalculatorRunOrganisationDataDetail
                {
                    OrganisationId = 1,
                    SubsidiaryId = "101",
                    OrganisationName = "Test"
                },
                new CalculatorRunOrganisationDataDetail
                {
                    OrganisationId = 2,
                    SubsidiaryId = "202",
                    OrganisationName = "Test1"
                }
            };

            // Act
            IEnumerable<ErrorReport> reportsList = ErrorReportService.HandleMissingRegistrationData(pomDetails, orgDetails, runId, createdBy);

            // Assert
            Assert.AreEqual(0, reportsList.Count(), "Expected no unmatched records to be returned.");
        }

        [TestMethod]
        public void HandleMissingRegistrationData_WhenMissing_ErrorAllInOrganisation()
        {
            // Arrange
            var runId = 500;
            var createdBy = "no unmatched test";
            var timestamp = DateTime.UtcNow;

            var pomDetails = new[]
            {
                new CalculatorRunPomDataDetail
                {
                    OrganisationId = 1,
                    SubsidiaryId = null,
                    SubmissionPeriod = "2023-P2",
                    LoadTimeStamp = timestamp,
                    SubmissionPeriodDesc = "July to December 2023"
                },
                new CalculatorRunPomDataDetail
                {
                    OrganisationId = 1,
                    SubsidiaryId = "101",
                    SubmissionPeriod = "2023-P2",
                    LoadTimeStamp = timestamp,
                    SubmissionPeriodDesc = "July to December 2023"
                },
                new CalculatorRunPomDataDetail
                {
                    OrganisationId = 1,
                    SubsidiaryId = "202",
                    SubmissionPeriod = "2023-P2",
                    LoadTimeStamp = timestamp,
                    SubmissionPeriodDesc = "July to December 2023"
                },
                new CalculatorRunPomDataDetail
                {
                    OrganisationId = 2,
                    SubsidiaryId = "303",
                    SubmissionPeriod = "2023-P2",
                    LoadTimeStamp = timestamp,
                    SubmissionPeriodDesc = "July to December 2023"
                }
            };

            var orgDetails = new[]
            {
                new CalculatorRunOrganisationDataDetail
                {
                    OrganisationId = 1,
                    SubsidiaryId = "101",
                    OrganisationName = "Test"
                },
                new CalculatorRunOrganisationDataDetail
                {
                    OrganisationId = 2,
                    SubsidiaryId = "303",
                    OrganisationName = "Test1"
                }
            };

            // Act
            IEnumerable<ErrorReport> reportsList = ErrorReportService.HandleMissingRegistrationData(pomDetails, orgDetails, runId, createdBy);

            // Assert
            Assert.AreEqual(3, reportsList.Count(), "Expected 3 error messages as Org 1 SubsidiaryId 202 is missing Reg data - so errors applies to all in Org 1");
            CollectionAssert.AreEquivalent(new[]
                {
                    (ProducerId: 1, SubsidiaryId: null , ErrorCode: ErrorCodes.MissingRegistrationData, leaverCode: ""),
                    (ProducerId: 1, SubsidiaryId: "101", ErrorCode: ErrorCodes.MissingRegistrationData, leaverCode: ""),
                    (ProducerId: 1, SubsidiaryId: "202", ErrorCode: ErrorCodes.MissingRegistrationData, leaverCode: "")
                },
                reportsList
                    .Select(r => (r.ProducerId, r.SubsidiaryId, r.ErrorCode, r.LeaverCode))
                    .ToList()
            );

        }

        [TestMethod]
        public void HandleMissingRegistrationData_Throws_WhenPomDetailsNull()
        {
            // Arrange
            var orgDetails = new[] { new CalculatorRunOrganisationDataDetail
                {
                    OrganisationId = 1,
                    OrganisationName="Test1"
                },
            };

            // Act & Assert
            Should.Throw<ArgumentNullException>(() =>
                ErrorReportService.HandleMissingRegistrationData(null!, orgDetails, 1, "u"));
        }

        [TestMethod]
        public void HandleMissingRegistrationData_Throws_WhenOrgDetailsNull()
        {
            // Arrange
            var pomDetails = new[] { new CalculatorRunPomDataDetail
                {
                    OrganisationId = 1,
                    SubsidiaryId = "11",
                    SubmissionPeriod = "2023-P2",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "July to December 2023"
                },
            };

            // Act & Assert
            Should.Throw<ArgumentNullException>(() =>
                ErrorReportService.HandleMissingRegistrationData(pomDetails, null!, 1, "u"));
        }

        [TestMethod]
        public void HandleMissingRegistrationData_DoesNotInsert_WhenSubmitterIdsMatched()
        {
            // Arrange
            var runId = 300;
            var createdBy = "no error";

            Guid pom1ID = Guid.NewGuid();
            Guid pom2ID = Guid.NewGuid();
            var pomDetails = new[] {
                CreatePomData(1, "2023-P2", pom1ID, "", "", 0, submissionPeriodDesc:"July to December 2023",subsidiaryId:"101"),
                CreatePomData(2, "2023-P2", pom2ID, "", "", 0, submissionPeriodDesc:"July to December 2023",subsidiaryId:"102")
             };

            var orgDetails = new[] {
                CreateOrganisationData(1,"101","Test",pom1ID, submissionPeriodDesc:"July to December 2023"),
                CreateOrganisationData(2,"102","Test1",pom2ID, submissionPeriodDesc:"July to December 2023")
            };

            // Act
            IEnumerable<ErrorReport> reportsList = ErrorReportService.HandleMissingRegistrationData(pomDetails, orgDetails, runId, createdBy);

            // Assert
            Assert.AreEqual(0, reportsList.Count(), "Expected no unmatched records to be returned.");
        }

        [TestMethod]
        public void HandleMissingRegistrationData_Inserts_WhenSubmitterIdsDoNotMatch()
        {
            // Arrange
            var runId = 300;
            var createdBy = "no error";

            Guid pom1SumbitterId = Guid.NewGuid();
            Guid pom2SubmitterId = Guid.NewGuid();

            var pomDetails = new[] {
                CreatePomData(1, "2023-P2", pom1SumbitterId, "", "", 0, submissionPeriodDesc:"July to December 2023",subsidiaryId:"101"),
                CreatePomData(2, "2023-P2", pom1SumbitterId, "", "", 0, submissionPeriodDesc:"July to December 2023",subsidiaryId:"102")
            };

            var orgDetails = new[] {
                CreateOrganisationData(1,"101","Test",pom1SumbitterId, submissionPeriodDesc:"July to December 2023"),
                CreateOrganisationData(2,"102","Test1",pom2SubmitterId, submissionPeriodDesc:"July to December 2023")
            };

            // Act
            IEnumerable<ErrorReport> capturedReports = ErrorReportService.HandleMissingRegistrationData(pomDetails, orgDetails, runId, createdBy);

            // Assert
            var reportsList = capturedReports.ToList();
            Assert.AreEqual(1, reportsList.Count, "Expected 1 unmatched records to be returned.");
            var error = reportsList.First();
            Assert.AreEqual(ErrorCodes.MissingRegistrationData, error.ErrorCode, "Incorrect Error Type");
            Assert.AreEqual(2, error.ProducerId, "Incorrect Producer Id");
        }

        [TestMethod]
        public void HandleMissingPOMErrors_WherePreviousPOMsExists()
        {
            var submitterId1 = Guid.NewGuid();
            var submitterId2 = Guid.NewGuid();

            var orgDetails = new[] {
                CreateOrganisationData(100101,null,"ECOLTD",submitterId1, "N"),
                CreateOrganisationData(200202,null,"Green holdings",submitterId2),
                CreateOrganisationData(200202,"100500","Pure leaf drinks",submitterId2),
                CreateOrganisationData(200202,"100101","ECOLTD",submitterId2, "O", "01", hasH1: false, hasH2: false),
                CreateOrganisationData(200202,"100102","ECOLTD",submitterId2, "O", "01", hasH1: false, hasH2: true),
                CreateOrganisationData(200202,"100103","ECOLTD",submitterId2, "O", "01", hasH1: true, hasH2: false)
            };

            var pomDetails = new[] {
                CreatePomData(100101, "2024-P1",submitterId1,"HH","ST",5000),
                CreatePomData(100102, "2024-P1",submitterId1,"HH","PL",3000),
                CreatePomData(100103, "2024-P4",submitterId1,"HH","ST",5000),
                CreatePomData(100101, "2024-P4",submitterId1,"HH","PL",3000),
                CreatePomData(200202, "2024-P1",submitterId2,"HH","PL",2000),
                CreatePomData(200202, "2024-P1",submitterId2,"HH","AL",4500),
                CreatePomData(200202, "2024-P4",submitterId2,"HH","PL",2000),
                CreatePomData(200202, "2024-P1",submitterId2,"HH","PL",3500,subsidiaryId:"100500"),
                CreatePomData(200202, "2024-P1",submitterId2,"HH","PL",4000,subsidiaryId:"100500"),
                CreatePomData(200202, "2024-P4",submitterId2,"HH","PL",3000,subsidiaryId:"100500")
            };

            // Arrange
            var runId = 300;
            var createdBy = "no error";

            // Act
            List<ErrorReport> reportsList = ErrorReportService.HandleMissingPomData(pomDetails, orgDetails, runId, createdBy);

            // Assert
            Assert.AreEqual(3, reportsList.Count(), "Expected 3 unmatched records to be returned.");

            Assert.AreEqual(ErrorCodes.MissingPOMData, reportsList[0].ErrorCode, "Incorrect Error Code");
            Assert.AreEqual(200202, reportsList[0].ProducerId, "Incorrect Producer Id");
            Assert.AreEqual("100101", reportsList[0].SubsidiaryId, "Incorrect Subsidiary Id");
            Assert.AreEqual("01", reportsList[0].LeaverCode, "Incorrect Leaver Code");

            Assert.AreEqual(ErrorCodes.MissingPOMData, reportsList[1].ErrorCode, "Incorrect Error Code");
            Assert.AreEqual(200202, reportsList[1].ProducerId, "Incorrect Producer Id");
            Assert.AreEqual("100102", reportsList[1].SubsidiaryId, "Incorrect Subsidiary Id");
            Assert.AreEqual("01", reportsList[1].LeaverCode, "Incorrect Leaver Code");

            Assert.AreEqual(ErrorCodes.MissingPOMData, reportsList[2].ErrorCode, "Incorrect Error Code");
            Assert.AreEqual(200202, reportsList[2].ProducerId, "Incorrect Producer Id");
            Assert.AreEqual("100103", reportsList[2].SubsidiaryId, "Incorrect Subsidiary Id");
            Assert.AreEqual("01", reportsList[2].LeaverCode, "Incorrect Leaver Code");
        }

        [TestMethod]
        public void HandleMissingPOMErrors_WhereNoPreviousPOMsExists()
        {
            var submitterId1 = Guid.NewGuid();
            var submitterId2 = Guid.NewGuid();

            var orgDetails = new[] {
                CreateOrganisationData(100101,null,"ECOLTD",submitterId1, "N"),
                CreateOrganisationData(200202,null,"Green holdings",submitterId2),
                CreateOrganisationData(200202,"100500","Pure leaf drinks",submitterId2),
                CreateOrganisationData(200202,"100101","ECOLTD",submitterId2, "O", "01")
            };

            var pomDetails = new[] {
                CreatePomData(200202, "2024-P1",submitterId2,"HH","PL",2000),
                CreatePomData(200202, "2024-P1",submitterId2,"HH","AL",4500),
                CreatePomData(200202, "2024-P4",submitterId2,"HH","PL",2000),
                CreatePomData(200202, "2024-P1",submitterId2,"HH","PL",3500,subsidiaryId:"100500"),
                CreatePomData(200202, "2024-P1",submitterId2,"HH","PL",4000,subsidiaryId:"100500"),
                CreatePomData(200202, "2024-P4",submitterId2,"HH","PL",3000,subsidiaryId:"100500")
            };

            // Arrange
            var runId = 300;
            var createdBy = "no error";

            // Act
            IEnumerable<ErrorReport> capturedReports = ErrorReportService.HandleMissingPomData(pomDetails, orgDetails, runId, createdBy);

            // Assert
            Assert.IsFalse(capturedReports.Any());
        }

        [TestMethod]
        public void HandleObligatedErrors_ErrorsExistInRegData()
        {
            var producer1 = 100101;
            var producer2 = 200202;
            var submitterId1 = Guid.NewGuid();
            var submitterId2 = Guid.NewGuid();
            var error1 = "Some warning";
            var error2 = "Some other warning";

            var orgDetails = new[] {
                CreateOrganisationData(producer1,null,"ECOLTD",submitterId1, "E", errorCode: error1),
                CreateOrganisationData(producer2,null,"Green holdings",submitterId2),
                CreateOrganisationData(producer2,"100500","Pure leaf drinks",submitterId2, "E", errorCode: error2, statusCode: "some status code"),
                CreateOrganisationData(producer2,"100101","ECOLTD",submitterId2, "E", errorCode: null)
            };

            var pomDetails = new[] {
                CreatePomData(producer1, "2024-P1",submitterId1,"HH","ST",5000),
                CreatePomData(producer2, "2024-P1",submitterId2,"HH","PL",2000),
                CreatePomData(producer2, "2024-P1",submitterId2,"HH","PL",3500,subsidiaryId:"100500"),
                CreatePomData(producer2, "2024-P1",submitterId2,"HH","PL",3500,subsidiaryId:"100101")
            };

            // Arrange
            var runId = 300;
            var createdBy = "no error";

            // Act
            IEnumerable<ErrorReport> reportsList = ErrorReportService.HandleObligatedErrors(pomDetails, orgDetails, [], runId, createdBy);
            // Assert
            Assert.AreEqual(3, reportsList.Count(), "Expected 3 unmatched records to be returned.");
            Assert.IsTrue(reportsList.Any(p => p.ProducerId == 100101 && p.SubsidiaryId == null && p.ErrorCode == error1 && p.LeaverCode == ""));
            Assert.IsTrue(reportsList.Any(p => p.ProducerId == 200202 && p.SubsidiaryId == "100500" && p.ErrorCode == error2 && p.LeaverCode == "some status code"));
            Assert.IsTrue(reportsList.Any(p => p.ProducerId == 200202 && p.SubsidiaryId == "100101" && p.ErrorCode == ErrorCodes.Empty && p.LeaverCode == ""));
        }

        [TestMethod]
        public void HandleObligatedErrors_NoErrorsExistInRegData()
        {
            var producer1 = 100101;
            var producer2 = 200202;
            var submitterId1 = Guid.NewGuid();
            var submitterId2 = Guid.NewGuid();

            var orgDetails = new[]
            {
                CreateOrganisationData(producer1,null,"ECOLTD",submitterId1, "N"),
                CreateOrganisationData(producer2,null,"Green holdings",submitterId2),
                CreateOrganisationData(producer2,"100500","Pure leaf drinks",submitterId2),
                CreateOrganisationData(producer2,"100101","ECOLTD",submitterId2, "O", "01")
            };

            var pomDetails = new[] {
                CreatePomData(producer1, "2024-P1",submitterId1,"HH","ST",5000),
                CreatePomData(producer2, "2024-P1",submitterId2,"HH","PL",2000),
                CreatePomData(producer2, "2024-P1",submitterId2,"HH","PL",3500,subsidiaryId:"100500"),
                CreatePomData(producer2, "2024-P1",submitterId2,"HH","PL",3500,subsidiaryId:"100101")
            };

            // Arrange
            var runId = 300;
            var createdBy = "no error";

            // Act
            IEnumerable<ErrorReport> reportsList = ErrorReportService.HandleObligatedErrors(pomDetails, orgDetails, [], runId, createdBy);

            // Assert
            Assert.AreEqual(0, reportsList.Count(), "Expected 0 unmatched records to be returned.");
        }

        [TestMethod]
        public void HandleObligatedErrors_DontShowErrorsWithNoPomAndNoNol()
        {
            var producer1 = 100101; //No pom and has NoL
            var producer2 = 200202; //Has pom and NoL
            var producer3 = 300303; //Has pom and no NoL
            var producer4 = 400404; //No pom or NoL
            var submitterId1 = Guid.NewGuid();
            var submitterId2 = Guid.NewGuid();
            var error1 = "Some warning";
            var error2 = "Some other warning";

            var orgDetails = new[] {
                CreateOrganisationData(producer1,null,"ECOLTD",submitterId1, "E", errorCode: error1),
                CreateOrganisationData(producer2,null,"Green holdings",submitterId2),
                CreateOrganisationData(producer2,"100500","Pure leaf drinks",submitterId2, "E", errorCode: error2, statusCode: "some status code"),
                CreateOrganisationData(producer2,"100101","ECOLTD",submitterId2, "E", errorCode: null),
                CreateOrganisationData(producer3,null,"Pear",submitterId1, "E", errorCode: error1),
                CreateOrganisationData(producer4,null,"Apple",submitterId1, "E", errorCode: error1)
            };

            var pomDetails = new[] {
                CreatePomData(producer2, "2024-P1",submitterId2,"HH","PL",2000),
                CreatePomData(producer2, "2024-P1",submitterId2,"HH","PL",3500,subsidiaryId:"100500"),
                CreatePomData(producer2, "2024-P1",submitterId2,"HH","PL",3500,subsidiaryId:"100101"),
                CreatePomData(producer3, "2024-P1",submitterId1,"HH","PL",5000)
            };

            List<InvoicedProducer> invoicedProducers =
            [
                new() { ProducerId = producer1, CalculatorRunId = 0, CalculatorName = "ignored", ProducerName = "ignored", TradingName = null, MaterialId = 0, BillingInstructionId = null, InvoicedNetTonnage = null, CurrentYearInvoicedTotalAfterThisRun = null },
                new() { ProducerId = producer2, CalculatorRunId = 0, CalculatorName = "ignored", ProducerName = "ignored", TradingName = null, MaterialId = 0, BillingInstructionId = null, InvoicedNetTonnage = null, CurrentYearInvoicedTotalAfterThisRun = null }
            ];

            // Arrange
            var runId = 300;
            var createdBy = "no error";

            // Act
            IEnumerable<ErrorReport> reportsList = ErrorReportService.HandleObligatedErrors(pomDetails, orgDetails, invoicedProducers, runId, createdBy);

            // Assert
            Assert.AreEqual(4, reportsList.Count(), "Expected 4 unmatched records to be returned.");
            Assert.IsTrue(reportsList.Any(p => p.ProducerId == producer1 && p.SubsidiaryId == null && p.ErrorCode == error1));
            Assert.IsTrue(reportsList.Any(p => p.ProducerId == producer2 && p.SubsidiaryId == "100500" && p.ErrorCode == error2 && p.LeaverCode == "some status code"));
            Assert.IsTrue(reportsList.Any(p => p.ProducerId == producer2 && p.SubsidiaryId == "100101" && p.ErrorCode == ErrorCodes.Empty && p.LeaverCode == ""));
            Assert.IsTrue(reportsList.Any(p => p.ProducerId == producer3 && p.SubsidiaryId == null && p.ErrorCode == error1));
        }

        [TestMethod]
        public void HandleWarningsErrors_WarningsExistInRegData()
        {
            var producer1 = 100101;
            var producer2 = 200202;
            var submitterId1 = Guid.NewGuid();
            var submitterId2 = Guid.NewGuid();
            var error1 = "Some error";
            var error2 = "Some other error";

            var orgDetails = new[] {
                CreateOrganisationData(producer1,null,"ECOLTD",submitterId1, errorCode: error1, statusCode: "some status code"),
                CreateOrganisationData(producer2,null,"Green holdings",submitterId2),
                CreateOrganisationData(producer2,"100500","Pure leaf drinks",submitterId2, errorCode: error2),
                CreateOrganisationData(producer2,"100101","ECOLTD",submitterId2)
            };

            var pomDetails = new[] {
                CreatePomData(producer1, "2024-P1",submitterId1,"HH","ST",5000),
                CreatePomData(producer2, "2024-P1",submitterId2,"HH","PL",2000),
                CreatePomData(producer2, "2024-P1",submitterId2,"HH","PL",3500,subsidiaryId:"100500"),
                CreatePomData(producer2, "2024-P1",submitterId2,"HH","PL",3500,subsidiaryId:"100101")
            };

            // Arrange
            var runId = 300;
            var createdBy = "no error";

            // Act
            IEnumerable<ErrorReport> reportsList = ErrorReportService.HandleObligatedWarnings(pomDetails, orgDetails, [], runId, createdBy);

            // Assert
            Assert.AreEqual(2, reportsList.Count(), "Expected 2 unmatched records to be returned.");
            Assert.IsTrue(reportsList.Any(p => p.ProducerId == producer1 && p.SubsidiaryId == null && p.ErrorCode == error1 && p.LeaverCode == "some status code"));
            Assert.IsTrue(reportsList.Any(p => p.ProducerId == producer2 && p.SubsidiaryId == "100500" && p.ErrorCode == error2 && p.LeaverCode == ""));
        }

        [TestMethod]
        public void HandleWarningsErrors_DontShowWarningsWithNoPomAndNoNol()
        {
            var producer1 = 100101; //No pom and has NoL
            var producer2 = 200202; //Has pom and NoL
            var producer3 = 300303; //Has pom and no NoL
            var producer4 = 400404; //No pom or NoL
            var submitterId1 = Guid.NewGuid();
            var submitterId2 = Guid.NewGuid();
            var error1 = "Some error";
            var error2 = "Some other error";

            var orgDetails = new[] {
                CreateOrganisationData(producer1,null,"ECOLTD",submitterId1, errorCode: error1, statusCode: "some status code"),
                CreateOrganisationData(producer2,null,"Green holdings",submitterId2),
                CreateOrganisationData(producer2,"100500","Pure leaf drinks",submitterId2, errorCode: error2),
                CreateOrganisationData(producer2,"100101","ECOLTD",submitterId2),
                CreateOrganisationData(producer3,null,"Pear",submitterId1, errorCode: error1),
                CreateOrganisationData(producer4,null,"Apple",submitterId1, errorCode: error1)
            };

            var pomDetails = new[] {
                CreatePomData(producer2, "2024-P1",submitterId2,"HH","PL",2000),
                CreatePomData(producer2, "2024-P1",submitterId2,"HH","PL",3500,subsidiaryId:"100500"),
                CreatePomData(producer2, "2024-P1",submitterId2,"HH","PL",3500,subsidiaryId:"100101"),
                CreatePomData(producer3, "2024-P1",submitterId1,"HH","PL",5000)
            };

            List<InvoicedProducer> invoicedProducers =
            [
                new() { ProducerId = producer1, CalculatorRunId = 0, CalculatorName = "ignored", ProducerName = "ignored", TradingName = null, MaterialId = 0, BillingInstructionId = null, InvoicedNetTonnage = null, CurrentYearInvoicedTotalAfterThisRun = null },
                new() { ProducerId = producer2, CalculatorRunId = 0, CalculatorName = "ignored", ProducerName = "ignored", TradingName = null, MaterialId = 0, BillingInstructionId = null, InvoicedNetTonnage = null, CurrentYearInvoicedTotalAfterThisRun = null }
            ];

            // Arrange
            var runId = 300;
            var createdBy = "no error";

            // Act
            IEnumerable<ErrorReport> reportsList = ErrorReportService.HandleObligatedWarnings(pomDetails, orgDetails, invoicedProducers, runId, createdBy);

            // Assert
            Assert.AreEqual(3, reportsList.Count(), "Expected 3 unmatched records to be returned.");
            Assert.IsTrue(reportsList.Any(p => p.ProducerId == producer1 && p.SubsidiaryId == null && p.ErrorCode == error1 && p.LeaverCode == "some status code"));
            Assert.IsTrue(reportsList.Any(p => p.ProducerId == producer2 && p.SubsidiaryId == "100500" && p.ErrorCode == error2 && p.LeaverCode == ""));
            Assert.IsTrue(reportsList.Any(p => p.ProducerId == producer3 && p.SubsidiaryId == null && p.ErrorCode == error1));
        }

        [TestMethod]
        public async Task HandleErrors_AllTypes()
        {
            var producer1 = 100101;
            var producer2 = 200202;
            var producer3 = 300303;
            var producer4 = 400404;
            var producer5 = 100200;
            var producer6 = 500505;
            var producer7 = 600606;
            var producer8 = 700707;
            var relativeYear = new RelativeYear(2025);
            var submitterId1 = Guid.NewGuid();
            var submitterId2 = Guid.NewGuid();
            var submitterId3 = Guid.NewGuid();

            var orgDetails = new[]
            {
                CreateOrganisationData(producer1,null,"ECOLTD",submitterId1, "N"),
                CreateOrganisationData(producer2,null,"Green holdings",submitterId2),
                CreateOrganisationData(producer2,"100500","Pure leaf drinks",submitterId2),
                CreateOrganisationData(producer2,"100101","ECOLTD",submitterId2, "O", "01", hasH1: false, hasH2: false),
                CreateOrganisationData(producer3,null,"ECOLTD",submitterId3, "O", "01", errorCode: "some warning"),
                CreateOrganisationData(producer4,"404","Tea and cakes",submitterId3, "E", "01", errorCode: "some synapse error"),
                CreateOrganisationData(producer6,null, "Pear", submitterId3, "E", "16", errorCode: "some synapse error"), //Has pom but no Nol - should show in error report
                CreateOrganisationData(producer7,null, "Kiwi", submitterId3, "O", "16", errorCode: "some warning"), //No pom but has Nol - should show in error report
                CreateOrganisationData(producer8,null, "Banana", submitterId3, "O", "16", errorCode: "some warning"), // No pom but no Nol - shouldn't show in error report
            };

            var pomDetails = new[] {
                CreatePomData(producer1, "2024-P1",submitterId1,"HH","ST",5000),
                CreatePomData(producer1, "2024-P1",submitterId1,"HH","PL",3000),
                CreatePomData(producer1, "2024-P4",submitterId1,"HH","ST",5000),
                CreatePomData(producer1, "2024-P4",submitterId1,"HH","PL",3000),
                CreatePomData(producer2, "2024-P1",submitterId2,"HH","PL",2000),
                CreatePomData(producer2, "2024-P1",submitterId2,"HH","AL",4500),
                CreatePomData(producer2, "2024-P4",submitterId2,"HH","PL",2000),
                CreatePomData(producer2, "2024-P1",submitterId2,"HH","PL",3500,subsidiaryId:"100500"),
                CreatePomData(producer2, "2024-P1",submitterId2,"HH","PL",4000,subsidiaryId:"100500"),
                CreatePomData(producer2, "2024-P4",submitterId2,"HH","PL",3000,subsidiaryId:"100500"),
                CreatePomData(producer3, "2024-P1",submitterId3,"HH","ST",5000),
                CreatePomData(producer3, "2024-P1",submitterId3,"HH","ST",5555),
                CreatePomData(producer4, "2024-P1",submitterId3,"HH","ST",5666, subsidiaryId:"404"),
                CreatePomData(producer5, "2024-P1",submitterId1,"HH","ST",5000),
                CreatePomData(producer7, "2024-P1",submitterId3,"HH","ST",5000),
            };

            // Arrange
            var runId = 300;
            var createdBy = "no error";

            _invoicedProducerService
            .Setup(m => m.GetInvoicedProducers(It.IsAny<RelativeYear>(), It.IsAny<ImmutableHashSet<int>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => [
                new() { ProducerId = producer6, CalculatorRunId = -1, CalculatorName = "ignored", ProducerName = "ignored", TradingName = null, MaterialId = 0, BillingInstructionId = null, InvoicedNetTonnage = null, CurrentYearInvoicedTotalAfterThisRun = null },
            ]);

            // Act
            var reportsList = await _sut.HandleErrors(pomDetails, orgDetails, runId, createdBy, relativeYear, CancellationToken.None);

            var errorReports = _dbContext.ErrorReports.ToList();

            Assert.AreEqual(8, errorReports.Count(), "Expected 8 unmatched records to be inserted.");
            Assert.IsTrue(errorReports.Any(p => p.ProducerId == producer5 && p.SubsidiaryId == null && p.ErrorCode == ErrorCodes.MissingRegistrationData));
            Assert.IsTrue(errorReports.Any(p => p.ProducerId == producer2 && p.SubsidiaryId == null && p.ErrorCode == ErrorCodes.Empty));
            Assert.IsTrue(errorReports.Any(p => p.ProducerId == producer2 && p.SubsidiaryId == "100101" && p.ErrorCode == ErrorCodes.MissingPOMData && p.LeaverCode == "01"));
            Assert.IsTrue(errorReports.Any(p => p.ProducerId == producer3 && p.SubsidiaryId == null && p.ErrorCode == "some warning"));
            Assert.IsTrue(errorReports.Any(p => p.ProducerId == producer4 && p.SubsidiaryId == null && p.ErrorCode == ErrorCodes.Empty));
            Assert.IsTrue(errorReports.Any(p => p.ProducerId == producer4 && p.SubsidiaryId == "404" && p.ErrorCode == "some synapse error"));
            Assert.IsTrue(errorReports.Any(p => p.ProducerId == producer6 && p.SubsidiaryId == null && p.ErrorCode == "some synapse error"));
            Assert.IsTrue(errorReports.Any(p => p.ProducerId == producer7 && p.SubsidiaryId == null && p.ErrorCode == "some warning"));
            Assert.IsFalse(errorReports.Any(p => p.ProducerId == producer8 && p.SubsidiaryId == null && p.ErrorCode == "some warning"));

            Assert.AreEqual(4, reportsList.Count(), "Expected 4 errors. Warnings and empty error parents should not be included.");
            Assert.IsTrue(reportsList.Any(p => p.OrgId == producer5 && p.SubId == null));
            Assert.IsTrue(reportsList.Any(p => p.OrgId == producer2 && p.SubId == "100101"));
            Assert.IsTrue(reportsList.Any(p => p.OrgId == producer4 && p.SubId == "404"));
            Assert.IsTrue(reportsList.Any(p => p.OrgId == producer6 && p.SubId == null));
        }

        private static CalculatorRunPomDataDetail CreatePomData(int orgId, string submissionPeriod, Guid submitterId, string packagingType, string packagingMaterial, int packagingMaterialWeight, string submissionPeriodDesc = "Jan to December 2025", string? subsidiaryId = null)
        {
            return new CalculatorRunPomDataDetail
            {
                OrganisationId = orgId,
                SubmissionPeriod = submissionPeriod,
                LoadTimeStamp = DateTime.UtcNow,
                SubmissionPeriodDesc = submissionPeriodDesc,
                SubmitterId = submitterId,
                PackagingType = packagingType,
                PackagingMaterial = packagingMaterial,
                PackagingMaterialWeight = packagingMaterialWeight,
                SubsidiaryId = subsidiaryId
            };
        }

        private CalculatorRunOrganisationDataDetail CreateOrganisationData(int orgId, string? subId, string orgName, Guid submitterId, string obligationStatus = "O", string statusCode = "", string submissionPeriodDesc = "Jan to December 2025", string? errorCode = null, bool hasH1 = true, bool hasH2 = true)
        {
            return new CalculatorRunOrganisationDataDetail
            {
                OrganisationId = orgId,
                SubsidiaryId = subId,
                OrganisationName = orgName,
                ObligationStatus = obligationStatus,
                StatusCode = statusCode,
                SubmitterId = submitterId,
                ErrorCode = errorCode,
                HasH1 = hasH1,
                HasH2 = hasH2
            };
        }
    }
}
