using Microsoft.Extensions.Logging.Abstractions;
using EPR.Calculator.Service.Function.Services.CommonDataApi;
using EPR.Calculator.Service.Function.Services.DataLoading;

namespace EPR.Calculator.Service.Function.UnitTests.Services.DataLoading
{
    /// <summary>
    ///     Unit tests for the mapping and validation logic in <see cref="CommonDataApiLoaderMapper" />.
    ///     These tests exercise the internal mapper functions directly and do not require a database.
    /// </summary>
    [TestClass]
    public class CommonDataApiLoaderMapperTests
    {
        private static readonly DateTimeOffset FixedUtcNow = new(2024, 6, 15, 12, 0, 0, TimeSpan.Zero);
        private static readonly string ValidGuid = "11111111-1111-1111-1111-111111111111";

        // ───────────────────────── POM mapper – field mapping ─────────────────────────

        /// <summary>
        ///     Verifies that POM entity fields are mapped correctly from the API response.
        /// </summary>
        [TestMethod]
        public void PomMapper_WithValidResponse_MapsAllFieldsCorrectly()
        {
            // Arrange
            var response = new PomResponse
            {
                SubmissionPeriod = "2024-Q1",
                SubmissionPeriodDescription = "Quarter 1 2024",
                OrganisationId = 42,
                SubsidiaryId = "SUB-001",
                PackagingType = "HH",
                PackagingMaterial = "PL",
                PackagingMaterialSubtype = "ST-01",
                PackagingMaterialWeight = 250.75,
                PackagingClass = "P1",
                PackagingActivity = "PAC",
                RamRagRating = "G",
                SubmitterId = ValidGuid
            };

            var mapper = CommonDataApiLoaderMapper.MapPom(FixedUtcNow, NullLogger.Instance);

            // Act
            var pom = mapper(response);

            // Assert
            Assert.AreEqual("2024-Q1", pom.SubmissionPeriod);
            Assert.AreEqual("Quarter 1 2024", pom.SubmissionPeriodDesc);
            Assert.AreEqual(42, pom.OrganisationId);
            Assert.AreEqual("SUB-001", pom.SubsidiaryId);
            Assert.AreEqual("HH", pom.PackagingType);
            Assert.AreEqual("PL", pom.PackagingMaterial);
            Assert.AreEqual("ST-01", pom.PackagingMaterialSubtype);
            Assert.AreEqual(250.75, pom.PackagingMaterialWeight);
            Assert.AreEqual("P1", pom.PackagingClass);
            Assert.AreEqual("PAC", pom.PackagingActivity);
            Assert.AreEqual("G", pom.RamRagRating);
            Assert.AreEqual(Guid.Parse(ValidGuid), pom.SubmitterId);
            Assert.AreEqual(FixedUtcNow.DateTime, pom.LoadTimeStamp);
        }

        /// <summary>
        ///     Verifies that POM Rag Ratings are handled correctly.
        /// </summary>
        [TestMethod]
        [DataRow("G"  , "G")]
        [DataRow("G " , "G")]
        [DataRow(" G" , "G")]
        [DataRow(" "  , null)]
        [DataRow("\t" , null)]
        [DataRow(""   , null)]
        [DataRow("Bad", "R")]
        public void PomMapper_MapsRamRagRatingCorrectly(
            string input,
            string? expected)
        {
            // Arrange
            var response = new PomResponse
            {
                OrganisationId = 42,
                PackagingType = "HH",
                PackagingMaterial = "PL",
                PackagingMaterialWeight = 250.75,
                RamRagRating = input,
                SubmitterId = ValidGuid
            };

            var mapper = CommonDataApiLoaderMapper.MapPom(FixedUtcNow, NullLogger.Instance);

            // Act
            var pom = mapper(response);

            // Assert
            Assert.AreEqual(expected, pom.RamRagRating);
        }


        // ───────────────────────── POM mapper – validation ─────────────────────────

        /// <summary>
        ///     Verifies that an invalid POM SubmitterId causes a FormatException.
        /// </summary>
        [TestMethod]
        public void PomMapper_WithInvalidSubmitterId_ThrowsFormatException()
        {
            var response = new PomResponse
            {
                SubmissionPeriod = "2024-Q1",
                OrganisationId = 1,
                PackagingType = "HH",
                PackagingMaterial = "PL",
                PackagingMaterialWeight = 100.0,
                SubmitterId = "not-a-guid"
            };

            var mapper = CommonDataApiLoaderMapper.MapPom(FixedUtcNow, NullLogger.Instance);

            Should.Throw<FormatException>(() => mapper(response));
        }

        // ───────────────────── Organisation mapper – field mapping ─────────────────────

        /// <summary>
        ///     Verifies that Organisation entity fields are mapped correctly from the API response.
        /// </summary>
        [TestMethod]
        public void OrganisationMapper_WithValidResponse_MapsAllFieldsCorrectly()
        {
            // Arrange
            var response = new OrganisationResponse
            {
                OrganisationId = 99,
                SubsidiaryId = "SUB-002",
                OrganisationName = "Test Org Ltd",
                TradingName = "TestTrade",
                StatusCode = "Active",
                ErrorCode = "E001",
                JoinerDate = "2024-01-01",
                LeaverDate = "2024-12-31",
                ObligationStatus = "Full",
                NumDaysObligated = 365,
                SubmitterId = ValidGuid,
                HasH1 = true,
                HasH2 = false
            };

            var mapper = CommonDataApiLoaderMapper.MapOrganisation(FixedUtcNow);

            // Act
            var org = mapper(response);

            // Assert
            Assert.AreEqual(99, org.OrganisationId);
            Assert.AreEqual("SUB-002", org.SubsidiaryId);
            Assert.AreEqual("Test Org Ltd", org.OrganisationName);
            Assert.AreEqual("TestTrade", org.TradingName);
            Assert.AreEqual("Active", org.StatusCode);
            Assert.AreEqual("E001", org.ErrorCode);
            Assert.AreEqual("2024-01-01", org.JoinerDate);
            Assert.AreEqual("2024-12-31", org.LeaverDate);
            Assert.AreEqual("Full", org.ObligationStatus);
            Assert.AreEqual(365, org.DaysObligated);
            Assert.AreEqual(Guid.Parse(ValidGuid), org.SubmitterId);
            Assert.IsTrue(org.HasH1);
            Assert.IsFalse(org.HasH2);
            Assert.AreEqual(FixedUtcNow.DateTime, org.LoadTimestamp);
        }

        // ───────────────────── Organisation mapper – validation ─────────────────────

        /// <summary>
        ///     Verifies that an invalid Organisation SubmitterId causes a FormatException.
        /// </summary>
        [TestMethod]
        public void OrganisationMapper_WithInvalidSubmitterId_ThrowsFormatException()
        {
            var response = new OrganisationResponse
            {
                OrganisationId = 1,
                OrganisationName = "Test Org",
                ObligationStatus = "Full",
                SubmitterId = "not-a-guid"
            };

            var mapper = CommonDataApiLoaderMapper.MapOrganisation(FixedUtcNow);

            Should.Throw<FormatException>(() => mapper(response));
        }

        /// <summary>
        ///     Verifies that a null OrganisationName causes a FormatException.
        /// </summary>
        [TestMethod]
        public void OrganisationMapper_WithNullOrganisationName_ThrowsFormatException()
        {
            var response = new OrganisationResponse
            {
                OrganisationId = 1,
                OrganisationName = null,
                ObligationStatus = "Full",
                SubmitterId = ValidGuid
            };

            var mapper = CommonDataApiLoaderMapper.MapOrganisation(FixedUtcNow);

            Should.Throw<FormatException>(() => mapper(response));
        }

        /// <summary>
        ///     Verifies that a null OrganisationId causes a FormatException.
        /// </summary>
        [TestMethod]
        public void OrganisationMapper_WithNullOrganisationId_ThrowsFormatException()
        {
            var response = new OrganisationResponse
            {
                OrganisationId = null,
                OrganisationName = "Test Org",
                ObligationStatus = "Full",
                SubmitterId = ValidGuid
            };

            var mapper = CommonDataApiLoaderMapper.MapOrganisation(FixedUtcNow);

            Should.Throw<FormatException>(() => mapper(response));
        }

        /// <summary>
        ///     Verifies that a null ObligationStatus causes a FormatException.
        /// </summary>
        [TestMethod]
        public void OrganisationMapper_WithNullObligationStatus_ThrowsFormatException()
        {
            var response = new OrganisationResponse
            {
                OrganisationId = 1,
                OrganisationName = "Test Org",
                ObligationStatus = null,
                SubmitterId = ValidGuid
            };

            var mapper = CommonDataApiLoaderMapper.MapOrganisation(FixedUtcNow);

            Should.Throw<FormatException>(() => mapper(response));
        }
    }
}