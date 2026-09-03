using EPR.CommonDataService.DataApi.Alignment;

namespace EPR.Calculator.API.DataApi.UnitTests.Alignment;

[TestClass]
public class ProducerErrorDetectorTests
{
    [TestMethod]
    public void HandleMissingRegistrationData_InsertsCorrectErrorReports_WhenUnmatchedExists()
    {
        var poms = new[]
        {
            CreatePom(1, "11", "2023-P2"),
            CreatePom(2, "22", "2023-P2"),
            CreatePom(3, "33", "2023-P2")
        };

        var orgs = new[] { CreateOrg(1, null, "Test") };

        var result = ProducerErrorDetector.HandleMissingRegistrationData(poms, orgs);

        Assert.AreEqual(3, result.Count, "Expected 3 unmatched records to be returned.");

        foreach (var r in result)
        {
            Assert.AreEqual(ProducerErrorCodes.MissingRegistrationData, r.ErrorCode);
            Assert.IsFalse(r.IsWarning);
        }

        Assert.IsTrue(result.Any(r => r.OrganisationId == 1 && r.SubsidiaryId == "11"));
        Assert.IsTrue(result.Any(r => r.OrganisationId == 2 && r.SubsidiaryId == "22"));
        Assert.IsTrue(result.Any(r => r.OrganisationId == 3 && r.SubsidiaryId == "33"));
    }

    [TestMethod]
    public void HandleMissingRegistrationData_DeduplicatesMultiplePomsForSameOrgSub()
    {
        var poms = new[]
        {
            CreatePom(10, "101", "2023-P2"),
            CreatePom(10, "101", "2023-P2"),
            CreatePom(10, "102", "2023-P2")
        };

        var orgs = Array.Empty<AlignmentOrganisation>();

        var result = ProducerErrorDetector.HandleMissingRegistrationData(poms, orgs);

        Assert.AreEqual(2, result.Count, "Expected 1 error per unique Org+Sub combination.");
        Assert.IsTrue(result.Any(r => r.OrganisationId == 10 && r.SubsidiaryId == "101"));
        Assert.IsTrue(result.Any(r => r.OrganisationId == 10 && r.SubsidiaryId == "102"));
    }

    [TestMethod]
    public void HandleMissingRegistrationData_DoesNotInsert_WhenAllMatched()
    {
        var poms = new[]
        {
            CreatePom(1, "101", "2023-P2"),
            CreatePom(2, "102", "2023-P2")
        };

        var orgs = new[]
        {
            CreateOrg(1, "101", "Test"),
            CreateOrg(2, "102", "Test1")
        };

        var result = ProducerErrorDetector.HandleMissingRegistrationData(poms, orgs);

        Assert.AreEqual(0, result.Count, "Expected no unmatched records to be returned.");
    }

    [TestMethod]
    public void HandleMissingRegistrationData_DeduplicatesMultiplePomsForSameOrgSub_Issue3()
    {
        // Simulate 41 POMs for the same Org/Sub which does NOT exist in org table
        var poms = Enumerable.Range(1, 41)
            .Select(_ => CreatePom(100, "200", "2023-P2"))
            .ToArray();

        var orgs = Array.Empty<AlignmentOrganisation>();

        var result = ProducerErrorDetector.HandleMissingRegistrationData(poms, orgs);

        Assert.AreEqual(1, result.Count, "Expected only 1 error for the unique Org/Sub combination.");
        var report = result.First();
        Assert.AreEqual(100, report.OrganisationId);
        Assert.AreEqual("200", report.SubsidiaryId);
        Assert.AreEqual(ProducerErrorCodes.MissingRegistrationData, report.ErrorCode);
    }

    [TestMethod]
    public void HandleMissingRegistrationData_WhenMissing_ErrorAllInOrganisation()
    {
        var poms = new[]
        {
            CreatePom(1, null, "2023-P2"),
            CreatePom(1, "101", "2023-P2"),
            CreatePom(1, "202", "2023-P2"),
            CreatePom(2, "303", "2023-P2")
        };

        var orgs = new[]
        {
            CreateOrg(1, "101", "Test"),
            CreateOrg(2, "303", "Test1")
        };

        var result = ProducerErrorDetector.HandleMissingRegistrationData(poms, orgs);

        Assert.AreEqual(3, result.Count, "Expected 3 error messages as Org 1 SubsidiaryId 202 is missing Reg data - so errors applies to all in Org 1");
        CollectionAssert.AreEquivalent(new[]
            {
                (OrganisationId: 1, SubsidiaryId: (string?)null, ErrorCode: ProducerErrorCodes.MissingRegistrationData, LeaverCode: ""),
                (OrganisationId: 1, SubsidiaryId: "101", ErrorCode: ProducerErrorCodes.MissingRegistrationData, LeaverCode: ""),
                (OrganisationId: 1, SubsidiaryId: "202", ErrorCode: ProducerErrorCodes.MissingRegistrationData, LeaverCode: "")
            },
            result.Select(r => (r.OrganisationId, r.SubsidiaryId, r.ErrorCode, r.LeaverCode)).ToList());
    }

    [TestMethod]
    public void HandleMissingRegistrationData_Throws_WhenPomsNull()
    {
        var orgs = new[] { CreateOrg(1, null, "Test1") };

        Should.Throw<ArgumentNullException>(() =>
            ProducerErrorDetector.HandleMissingRegistrationData(null!, orgs));
    }

    [TestMethod]
    public void HandleMissingRegistrationData_Throws_WhenOrganisationsNull()
    {
        var poms = new[] { CreatePom(1, "11", "2023-P2") };

        Should.Throw<ArgumentNullException>(() =>
            ProducerErrorDetector.HandleMissingRegistrationData(poms, null!));
    }

    [TestMethod]
    public void HandleMissingRegistrationData_DoesNotInsert_WhenSubmitterIdsMatched()
    {
        var submitterId1 = Guid.NewGuid();
        var submitterId2 = Guid.NewGuid();

        var poms = new[]
        {
            CreatePom(1, "101", "2023-P2", submitterId1),
            CreatePom(2, "102", "2023-P2", submitterId2)
        };

        var orgs = new[]
        {
            CreateOrg(1, "101", "Test", submitterId1),
            CreateOrg(2, "102", "Test1", submitterId2)
        };

        var result = ProducerErrorDetector.HandleMissingRegistrationData(poms, orgs);

        Assert.AreEqual(0, result.Count, "Expected no unmatched records to be returned.");
    }

    [TestMethod]
    public void HandleMissingRegistrationData_Inserts_WhenSubmitterIdsDoNotMatch()
    {
        var pom1SubmitterId = Guid.NewGuid();
        var pom2SubmitterId = Guid.NewGuid();

        var poms = new[]
        {
            CreatePom(1, "101", "2023-P2", pom1SubmitterId),
            CreatePom(2, "102", "2023-P2", pom1SubmitterId)
        };

        var orgs = new[]
        {
            CreateOrg(1, "101", "Test", pom1SubmitterId),
            CreateOrg(2, "102", "Test1", pom2SubmitterId)
        };

        var result = ProducerErrorDetector.HandleMissingRegistrationData(poms, orgs);

        Assert.AreEqual(1, result.Count, "Expected 1 unmatched records to be returned.");
        var error = result.First();
        Assert.AreEqual(ProducerErrorCodes.MissingRegistrationData, error.ErrorCode, "Incorrect Error Type");
        Assert.AreEqual(2, error.OrganisationId, "Incorrect Organisation Id");
    }

    [TestMethod]
    public void HandleMissingPomData_WherePreviousPomsExist()
    {
        var submitterId1 = Guid.NewGuid();
        var submitterId2 = Guid.NewGuid();

        var orgs = new[]
        {
            CreateOrg(100101, null, "ECOLTD", submitterId1, "N"),
            CreateOrg(200202, null, "Green holdings", submitterId2),
            CreateOrg(200202, "100500", "Pure leaf drinks", submitterId2),
            CreateOrg(200202, "100101", "ECOLTD", submitterId2, "O", "01", hasH1: false, hasH2: false),
            CreateOrg(200202, "100102", "ECOLTD", submitterId2, "O", "01", hasH1: false, hasH2: true),
            CreateOrg(200202, "100103", "ECOLTD", submitterId2, "O", "01", hasH1: true, hasH2: false)
        };

        var poms = new[]
        {
            CreatePom(100101, submitterId1, "2024-P1", "HH", "ST", 5000),
            CreatePom(100102, submitterId1, "2024-P1", "HH", "PL", 3000),
            CreatePom(100103, submitterId1, "2024-P4", "HH", "ST", 5000),
            CreatePom(100101, submitterId1, "2024-P4", "HH", "PL", 3000),
            CreatePom(200202, submitterId2, "2024-P1", "HH", "PL", 2000),
            CreatePom(200202, submitterId2, "2024-P1", "HH", "AL", 4500),
            CreatePom(200202, submitterId2, "2024-P4", "HH", "PL", 2000),
            CreatePom(200202, submitterId2, "2024-P1", "HH", "PL", 3500, "100500"),
            CreatePom(200202, submitterId2, "2024-P1", "HH", "PL", 4000, "100500"),
            CreatePom(200202, submitterId2, "2024-P4", "HH", "PL", 3000, "100500")
        };

        var result = ProducerErrorDetector.HandleMissingPomData(poms, orgs);

        Assert.AreEqual(3, result.Count, "Expected 3 unmatched records to be returned.");

        Assert.AreEqual(ProducerErrorCodes.MissingPOMData, result[0].ErrorCode);
        Assert.AreEqual(200202, result[0].OrganisationId);
        Assert.AreEqual("100101", result[0].SubsidiaryId);
        Assert.AreEqual("01", result[0].LeaverCode);

        Assert.AreEqual(ProducerErrorCodes.MissingPOMData, result[1].ErrorCode);
        Assert.AreEqual(200202, result[1].OrganisationId);
        Assert.AreEqual("100102", result[1].SubsidiaryId);
        Assert.AreEqual("01", result[1].LeaverCode);

        Assert.AreEqual(ProducerErrorCodes.MissingPOMData, result[2].ErrorCode);
        Assert.AreEqual(200202, result[2].OrganisationId);
        Assert.AreEqual("100103", result[2].SubsidiaryId);
        Assert.AreEqual("01", result[2].LeaverCode);
    }

    [TestMethod]
    public void HandleMissingPomData_WhereNoPreviousPomsExist()
    {
        var submitterId1 = Guid.NewGuid();
        var submitterId2 = Guid.NewGuid();

        var orgs = new[]
        {
            CreateOrg(100101, null, "ECOLTD", submitterId1, "N"),
            CreateOrg(200202, null, "Green holdings", submitterId2),
            CreateOrg(200202, "100500", "Pure leaf drinks", submitterId2),
            CreateOrg(200202, "100101", "ECOLTD", submitterId2, "O", "01")
        };

        var poms = new[]
        {
            CreatePom(200202, submitterId2, "2024-P1", "HH", "PL", 2000),
            CreatePom(200202, submitterId2, "2024-P1", "HH", "AL", 4500),
            CreatePom(200202, submitterId2, "2024-P4", "HH", "PL", 2000),
            CreatePom(200202, submitterId2, "2024-P1", "HH", "PL", 3500, "100500"),
            CreatePom(200202, submitterId2, "2024-P1", "HH", "PL", 4000, "100500"),
            CreatePom(200202, submitterId2, "2024-P4", "HH", "PL", 3000, "100500")
        };

        var result = ProducerErrorDetector.HandleMissingPomData(poms, orgs);

        Assert.IsFalse(result.Any());
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

        var orgs = new[]
        {
            CreateOrg(producer1, null, "ECOLTD", submitterId1, "E", errorCode: error1),
            CreateOrg(producer2, null, "Green holdings", submitterId2),
            CreateOrg(producer2, "100500", "Pure leaf drinks", submitterId2, "E", errorCode: error2, statusCode: "some status code"),
            CreateOrg(producer2, "100101", "ECOLTD", submitterId2, "E", errorCode: null)
        };

        var poms = new[]
        {
            CreatePom(producer1, submitterId1, "2024-P1", "HH", "ST", 5000),
            CreatePom(producer2, submitterId2, "2024-P1", "HH", "PL", 2000),
            CreatePom(producer2, submitterId2, "2024-P1", "HH", "PL", 3500, "100500"),
            CreatePom(producer2, submitterId2, "2024-P1", "HH", "PL", 3500, "100101")
        };

        var result = ProducerErrorDetector.HandleObligatedErrors(poms, orgs);

        Assert.AreEqual(3, result.Count, "Expected 3 unmatched records to be returned.");
        Assert.IsTrue(result.Any(p => p.OrganisationId == 100101 && p.SubsidiaryId == null && p.ErrorCode == error1 && p.LeaverCode == ""));
        Assert.IsTrue(result.Any(p => p.OrganisationId == 200202 && p.SubsidiaryId == "100500" && p.ErrorCode == error2 && p.LeaverCode == "some status code"));
        Assert.IsTrue(result.Any(p => p.OrganisationId == 200202 && p.SubsidiaryId == "100101" && p.ErrorCode == ProducerErrorCodes.Empty && p.LeaverCode == ""));
        Assert.IsTrue(result.All(r => !r.IsWarning));
        Assert.IsTrue(result.All(r => r.HasPomMatch), "Every organisation here has a matching POM.");
    }

    [TestMethod]
    public void HandleObligatedErrors_NoErrorsExistInRegData()
    {
        var producer1 = 100101;
        var producer2 = 200202;
        var submitterId1 = Guid.NewGuid();
        var submitterId2 = Guid.NewGuid();

        var orgs = new[]
        {
            CreateOrg(producer1, null, "ECOLTD", submitterId1, "N"),
            CreateOrg(producer2, null, "Green holdings", submitterId2),
            CreateOrg(producer2, "100500", "Pure leaf drinks", submitterId2),
            CreateOrg(producer2, "100101", "ECOLTD", submitterId2, "O", "01")
        };

        var poms = new[]
        {
            CreatePom(producer1, submitterId1, "2024-P1", "HH", "ST", 5000),
            CreatePom(producer2, submitterId2, "2024-P1", "HH", "PL", 2000),
            CreatePom(producer2, submitterId2, "2024-P1", "HH", "PL", 3500, "100500"),
            CreatePom(producer2, submitterId2, "2024-P1", "HH", "PL", 3500, "100101")
        };

        var result = ProducerErrorDetector.HandleObligatedErrors(poms, orgs);

        Assert.AreEqual(0, result.Count, "Expected 0 unmatched records to be returned.");
    }

    [TestMethod]
    public void HandleObligatedErrors_AlwaysIncludesEveryErrorStatusOrganisation_WithAccurateHasPomMatch()
    {
        // DataApi has no visibility into billing history, so it no longer decides whether a no-POM-match
        // error is worth surfacing - it always includes every "E"-status organisation and flags whether
        // it found a matching POM, leaving the "should this still show" call to the caller.
        var producer1 = 100101; // No pom
        var producer2 = 200202; // Has pom
        var producer3 = 300303; // Has pom
        var producer4 = 400404; // No pom
        var submitterId1 = Guid.NewGuid();
        var submitterId2 = Guid.NewGuid();
        var error1 = "Some warning";
        var error2 = "Some other warning";

        var orgs = new[]
        {
            CreateOrg(producer1, null, "ECOLTD", submitterId1, "E", errorCode: error1),
            CreateOrg(producer2, null, "Green holdings", submitterId2),
            CreateOrg(producer2, "100500", "Pure leaf drinks", submitterId2, "E", errorCode: error2, statusCode: "some status code"),
            CreateOrg(producer2, "100101", "ECOLTD", submitterId2, "E", errorCode: null),
            CreateOrg(producer3, null, "Pear", submitterId1, "E", errorCode: error1),
            CreateOrg(producer4, null, "Apple", submitterId1, "E", errorCode: error1)
        };

        var poms = new[]
        {
            CreatePom(producer2, submitterId2, "2024-P1", "HH", "PL", 2000),
            CreatePom(producer2, submitterId2, "2024-P1", "HH", "PL", 3500, "100500"),
            CreatePom(producer2, submitterId2, "2024-P1", "HH", "PL", 3500, "100101"),
            CreatePom(producer3, submitterId1, "2024-P1", "HH", "PL", 5000)
        };

        var result = ProducerErrorDetector.HandleObligatedErrors(poms, orgs);

        Assert.AreEqual(5, result.Count, "Expected all 5 \"E\"-status organisation rows, regardless of POM match.");
        Assert.IsTrue(result.Any(p => p.OrganisationId == producer1 && p.SubsidiaryId == null && p.ErrorCode == error1 && !p.HasPomMatch));
        Assert.IsTrue(result.Any(p => p.OrganisationId == producer2 && p.SubsidiaryId == "100500" && p.ErrorCode == error2 && p.LeaverCode == "some status code" && p.HasPomMatch));
        Assert.IsTrue(result.Any(p => p.OrganisationId == producer2 && p.SubsidiaryId == "100101" && p.ErrorCode == ProducerErrorCodes.Empty && p.LeaverCode == "" && p.HasPomMatch));
        Assert.IsTrue(result.Any(p => p.OrganisationId == producer3 && p.SubsidiaryId == null && p.ErrorCode == error1 && p.HasPomMatch));
        Assert.IsTrue(result.Any(p => p.OrganisationId == producer4 && p.SubsidiaryId == null && p.ErrorCode == error1 && !p.HasPomMatch));
    }

    [TestMethod]
    public void HandleObligatedWarnings_WarningsExistInRegData()
    {
        var producer1 = 100101;
        var producer2 = 200202;
        var submitterId1 = Guid.NewGuid();
        var submitterId2 = Guid.NewGuid();
        var error1 = "Some error";
        var error2 = "Some other error";

        var orgs = new[]
        {
            CreateOrg(producer1, null, "ECOLTD", submitterId1, errorCode: error1, statusCode: "some status code"),
            CreateOrg(producer2, null, "Green holdings", submitterId2),
            CreateOrg(producer2, "100500", "Pure leaf drinks", submitterId2, errorCode: error2),
            CreateOrg(producer2, "100101", "ECOLTD", submitterId2)
        };

        var poms = new[]
        {
            CreatePom(producer1, submitterId1, "2024-P1", "HH", "ST", 5000),
            CreatePom(producer2, submitterId2, "2024-P1", "HH", "PL", 2000),
            CreatePom(producer2, submitterId2, "2024-P1", "HH", "PL", 3500, "100500"),
            CreatePom(producer2, submitterId2, "2024-P1", "HH", "PL", 3500, "100101")
        };

        var result = ProducerErrorDetector.HandleObligatedWarnings(poms, orgs);

        Assert.AreEqual(2, result.Count, "Expected 2 unmatched records to be returned.");
        Assert.IsTrue(result.Any(p => p.OrganisationId == producer1 && p.SubsidiaryId == null && p.ErrorCode == error1 && p.LeaverCode == "some status code"));
        Assert.IsTrue(result.Any(p => p.OrganisationId == producer2 && p.SubsidiaryId == "100500" && p.ErrorCode == error2 && p.LeaverCode == ""));
        Assert.IsTrue(result.All(r => r.IsWarning));
        Assert.IsTrue(result.All(r => r.HasPomMatch), "Every organisation here has a matching POM.");
    }

    [TestMethod]
    public void HandleObligatedWarnings_AlwaysIncludesEveryQualifyingOrganisation_WithAccurateHasPomMatch()
    {
        var producer1 = 100101; // No pom
        var producer2 = 200202; // Has pom
        var producer3 = 300303; // Has pom
        var producer4 = 400404; // No pom
        var submitterId1 = Guid.NewGuid();
        var submitterId2 = Guid.NewGuid();
        var error1 = "Some error";
        var error2 = "Some other error";

        var orgs = new[]
        {
            CreateOrg(producer1, null, "ECOLTD", submitterId1, errorCode: error1, statusCode: "some status code"),
            CreateOrg(producer2, null, "Green holdings", submitterId2),
            CreateOrg(producer2, "100500", "Pure leaf drinks", submitterId2, errorCode: error2),
            CreateOrg(producer2, "100101", "ECOLTD", submitterId2),
            CreateOrg(producer3, null, "Pear", submitterId1, errorCode: error1),
            CreateOrg(producer4, null, "Apple", submitterId1, errorCode: error1)
        };

        var poms = new[]
        {
            CreatePom(producer2, submitterId2, "2024-P1", "HH", "PL", 2000),
            CreatePom(producer2, submitterId2, "2024-P1", "HH", "PL", 3500, "100500"),
            CreatePom(producer2, submitterId2, "2024-P1", "HH", "PL", 3500, "100101"),
            CreatePom(producer3, submitterId1, "2024-P1", "HH", "PL", 5000)
        };

        var result = ProducerErrorDetector.HandleObligatedWarnings(poms, orgs);

        Assert.AreEqual(4, result.Count, "Expected all 4 qualifying organisation rows, regardless of POM match.");
        Assert.IsTrue(result.Any(p => p.OrganisationId == producer1 && p.SubsidiaryId == null && p.ErrorCode == error1 && !p.HasPomMatch));
        Assert.IsTrue(result.Any(p => p.OrganisationId == producer2 && p.SubsidiaryId == "100500" && p.ErrorCode == error2 && p.LeaverCode == "" && p.HasPomMatch));
        Assert.IsTrue(result.Any(p => p.OrganisationId == producer3 && p.SubsidiaryId == null && p.ErrorCode == error1 && p.HasPomMatch));
        Assert.IsTrue(result.Any(p => p.OrganisationId == producer4 && p.SubsidiaryId == null && p.ErrorCode == error1 && !p.HasPomMatch));
    }

    [TestMethod]
    public void Detect_AllTypes()
    {
        // Detect doesn't know about billing history and doesn't compute holding-company roll-ups
        // (both are the caller's job - see ErrorReportServiceTests) - it returns the flat set of
        // individual error/warning rows, unconditionally, each flagged with whether it found a POM match.
        var producer1 = 100101;
        var producer2 = 200202;
        var producer3 = 300303;
        var producer4 = 400404;
        var producer5 = 100200;
        var producer6 = 500505;
        var producer7 = 600606;
        var producer8 = 700707;
        var submitterId1 = Guid.NewGuid();
        var submitterId2 = Guid.NewGuid();
        var submitterId3 = Guid.NewGuid();

        var orgs = new[]
        {
            CreateOrg(producer1, null, "ECOLTD", submitterId1, "N"),
            CreateOrg(producer2, null, "Green holdings", submitterId2),
            CreateOrg(producer2, "100500", "Pure leaf drinks", submitterId2),
            CreateOrg(producer2, "100101", "ECOLTD", submitterId2, "O", "01", hasH1: false, hasH2: false),
            CreateOrg(producer3, null, "ECOLTD", submitterId3, "O", "01", errorCode: "some warning"),
            CreateOrg(producer4, "404", "Tea and cakes", submitterId3, "E", "01", errorCode: "some synapse error"),
            CreateOrg(producer6, null, "Pear", submitterId3, "E", "16", errorCode: "some synapse error"), // No pom - included, HasPomMatch false
            CreateOrg(producer7, null, "Kiwi", submitterId3, "O", "16", errorCode: "some warning"), // Has pom - included, HasPomMatch true
            CreateOrg(producer8, null, "Banana", submitterId3, "O", "16", errorCode: "some warning") // No pom - included, HasPomMatch false
        };

        var poms = new[]
        {
            CreatePom(producer1, submitterId1, "2024-P1", "HH", "ST", 5000),
            CreatePom(producer1, submitterId1, "2024-P1", "HH", "PL", 3000),
            CreatePom(producer1, submitterId1, "2024-P4", "HH", "ST", 5000),
            CreatePom(producer1, submitterId1, "2024-P4", "HH", "PL", 3000),
            CreatePom(producer2, submitterId2, "2024-P1", "HH", "PL", 2000),
            CreatePom(producer2, submitterId2, "2024-P1", "HH", "AL", 4500),
            CreatePom(producer2, submitterId2, "2024-P4", "HH", "PL", 2000),
            CreatePom(producer2, submitterId2, "2024-P1", "HH", "PL", 3500, "100500"),
            CreatePom(producer2, submitterId2, "2024-P1", "HH", "PL", 4000, "100500"),
            CreatePom(producer2, submitterId2, "2024-P4", "HH", "PL", 3000, "100500"),
            CreatePom(producer3, submitterId3, "2024-P1", "HH", "ST", 5000),
            CreatePom(producer3, submitterId3, "2024-P1", "HH", "ST", 5555),
            CreatePom(producer4, submitterId3, "2024-P1", "HH", "ST", 5666, "404"),
            CreatePom(producer5, submitterId1, "2024-P1", "HH", "ST", 5000),
            CreatePom(producer7, submitterId3, "2024-P1", "HH", "ST", 5000)
        };

        var detector = new ProducerErrorDetector();
        var result = detector.Detect(orgs, poms);

        Assert.AreEqual(7, result.Errors.Count, "Expected 7 individual error/warning rows - no holding roll-ups.");
        Assert.IsTrue(result.Errors.Any(p => p.OrganisationId == producer5 && p.SubsidiaryId == null && p.ErrorCode == ProducerErrorCodes.MissingRegistrationData && p.HasPomMatch));
        Assert.IsTrue(result.Errors.Any(p => p.OrganisationId == producer2 && p.SubsidiaryId == "100101" && p.ErrorCode == ProducerErrorCodes.MissingPOMData && p.LeaverCode == "01" && p.HasPomMatch));
        Assert.IsTrue(result.Errors.Any(p => p.OrganisationId == producer3 && p.SubsidiaryId == null && p.ErrorCode == "some warning" && p.IsWarning && p.HasPomMatch));
        Assert.IsTrue(result.Errors.Any(p => p.OrganisationId == producer4 && p.SubsidiaryId == "404" && p.ErrorCode == "some synapse error" && !p.IsWarning && p.HasPomMatch));
        Assert.IsTrue(result.Errors.Any(p => p.OrganisationId == producer6 && p.SubsidiaryId == null && p.ErrorCode == "some synapse error" && !p.IsWarning && !p.HasPomMatch));
        Assert.IsTrue(result.Errors.Any(p => p.OrganisationId == producer7 && p.SubsidiaryId == null && p.ErrorCode == "some warning" && p.IsWarning && p.HasPomMatch));
        Assert.IsTrue(result.Errors.Any(p => p.OrganisationId == producer8 && p.SubsidiaryId == null && p.ErrorCode == "some warning" && p.IsWarning && !p.HasPomMatch));
        Assert.IsFalse(result.Errors.Any(p => p.OrganisationId == producer2 && p.SubsidiaryId == null), "No holding roll-up - that's the caller's job now.");

        Assert.AreEqual(4, result.UnmatchedKeys.Count, "Expected 4 unmatched keys - warnings excluded.");
        Assert.IsTrue(result.UnmatchedKeys.Contains((producer5, null)));
        Assert.IsTrue(result.UnmatchedKeys.Contains((producer2, "100101")));
        Assert.IsTrue(result.UnmatchedKeys.Contains((producer4, "404")));
        Assert.IsTrue(result.UnmatchedKeys.Contains((producer6, null)));
    }

    private static AlignmentPom CreatePom(int orgId, string? subsidiaryId, string submissionPeriod, Guid? submitterId = null) =>
        new()
        {
            OrganisationId = orgId,
            SubsidiaryId = subsidiaryId,
            SubmissionPeriod = submissionPeriod,
            SubmitterId = submitterId
        };

    private static AlignmentPom CreatePom(int orgId, Guid submitterId, string submissionPeriod, string packagingType, string packagingMaterial, int packagingMaterialWeight, string? subsidiaryId = null) =>
        new()
        {
            OrganisationId = orgId,
            SubmissionPeriod = submissionPeriod,
            SubmitterId = submitterId,
            PackagingType = packagingType,
            PackagingMaterial = packagingMaterial,
            PackagingMaterialWeight = packagingMaterialWeight,
            SubsidiaryId = subsidiaryId
        };

    private static AlignmentOrganisation CreateOrg(
        int orgId,
        string? subId,
        string orgName,
        Guid? submitterId = null,
        string obligationStatus = "O",
        string statusCode = "",
        string? errorCode = null,
        bool hasH1 = true,
        bool hasH2 = true) =>
        new()
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
