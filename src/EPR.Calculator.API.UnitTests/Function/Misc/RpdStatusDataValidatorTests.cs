using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Wrapper;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Misc;
using Microsoft.AspNetCore.Http;

namespace EPR.Calculator.Service.Function.UnitTests.Misc
{
    [TestClass]
    public class RpdStatusDataValidatorTests
    {
        private RpdStatusDataValidator TestClass { get; init; }

        private IFixture Fixture { get; init; }

        private Mock<IOrgAndPomWrapper> Wrapper { get; set; }

        private IEnumerable<CalculatorRunClassification> CalculatorRunClassifications { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RpdStatusDataValidatorTests"/> class.
        /// </summary>
        public RpdStatusDataValidatorTests()
        {
            Fixture = new Fixture();
            Wrapper = new Mock<IOrgAndPomWrapper>();
            TestClass = new RpdStatusDataValidator(Wrapper.Object);

            CalculatorRunClassifications = new List<CalculatorRunClassification>
            {
                new CalculatorRunClassification
                {
                    Id = (int)RunClassification.INTHEQUEUE,
                    Status = "IN THE QUEUE",
                },
                new CalculatorRunClassification
                {
                    Id = (int) RunClassification.RUNNING,
                    Status = RunClassification.RUNNING.ToString(),
                },
            };
        }

        [TestMethod]
        public void CanConstruct()
        {
            // Act
            var instance = new RpdStatusDataValidator(Wrapper.Object);

            // Assert
            Assert.IsNotNull(instance);
        }

        [TestMethod]
        [DataRow(RunClassification.INTHEQUEUE)]
        [DataRow(RunClassification.RUNNING)]
        public void IsValidRun_Success(RunClassification runClassification)
        {
            // Arrange
            var runId = Fixture.Create<int>();
            var run = Fixture.Create<CalculatorRun>();
            run.Id = runId;
            run.CalculatorRunOrganisationDataMasterId = null;
            run.CalculatorRunPomDataMasterId = null;
            run.CalculatorRunClassificationId = (int)runClassification;

            // Act
            var result = TestClass.IsValidRun(
                run,
                runId,
                CalculatorRunClassifications);

            // Assert
            Assert.IsTrue(result.isValid);
            Assert.IsNull(result.ErrorMessage);
        }

        [TestMethod]
        public void IsValidRun_With_CalcRun_Missing()
        {
            // Arrange
            var runId = Fixture.Create<int>();

            // Act
            var result = TestClass.IsValidRun(
                null,
                runId,
                Fixture.CreateMany<CalculatorRunClassification>());

            // Assert
            Assert.IsFalse(result.isValid);
            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
            Assert.AreEqual($"Calculator Run {runId} is missing", result.ErrorMessage);
        }

        [TestMethod]
        public void IsValidRun_With_RunId_Having_OrganisationDataMasterId()
        {
            // Arrange
            var runId = Fixture.Create<int>();
            var run = Fixture.Create<CalculatorRun>();
            run.Id = runId;

            // Act
            var result = TestClass.IsValidRun(
                run,
                runId,
                Fixture.CreateMany<CalculatorRunClassification>());

            // Assert
            Assert.IsFalse(result.isValid);
            Assert.AreEqual(StatusCodes.Status422UnprocessableEntity, result.StatusCode);
            Assert.AreEqual($"Calculator Run {runId} already has OrganisationDataMasterId associated with it", result.ErrorMessage);
        }

        [TestMethod]
        public void IsValidRun_With_RunId_Having_PomDataMasterId()
        {
            // Arrange
            var runId = Fixture.Create<int>();
            var run = Fixture.Create<CalculatorRun>();
            run.Id = runId;
            run.CalculatorRunOrganisationDataMasterId = null;

            // Act
            var result = TestClass.IsValidRun(
                run,
                runId,
                Fixture.CreateMany<CalculatorRunClassification>());

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status422UnprocessableEntity, result.StatusCode);
            Assert.AreEqual($"Calculator Run {runId} already has PomDataMasterId associated with it", result.ErrorMessage);
        }

        [TestMethod]
        public void IsValidRun_With_RunId_With_Incorrect_Classification()
        {
            // Arrange
            var runId = Fixture.Create<int>();
            var run = Fixture.Create<CalculatorRun>();
            run.Id = runId;
            run.CalculatorRunOrganisationDataMasterId = null;
            run.CalculatorRunPomDataMasterId = null;

            var classification = new CalculatorRunClassification
            {
                Id = runId,
                Status = RunClassification.ERROR.ToString(),
            };

            // Act
            var result = TestClass.IsValidRun(run, runId, [classification]);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status422UnprocessableEntity, result.StatusCode);
            Assert.AreEqual($"Calculator Run {runId} classification should be RUNNING or IN THE QUEUE", result.ErrorMessage);
        }

        [TestMethod]
        public void IsValidSuccessfullRun_Success()
        {
            // Arrange
            var runId = Fixture.Create<int>();
            var run = Fixture.Create<CalculatorRun>();
            run.Id = runId;
            run.CalculatorRunOrganisationDataMasterId = null;
            run.CalculatorRunPomDataMasterId = null;

            Wrapper.Setup(w => w.AnyPomData()).Returns(true);
            Wrapper.Setup(w => w.AnyOrganisationData()).Returns(true);

            // Act
            var result = TestClass.IsValidSuccessfulRun(runId);

            Assert.IsTrue(result.isValid);
            Assert.IsNull(result.ErrorMessage);
        }

        [TestMethod]
        [DataRow(false, true)]
        [DataRow(true, false)]
        [DataRow(false, false)]
        public void IsValidSuccessfullRun_With_RunId_Having_Pom_Data_Missing(bool pomDataExists, bool orgDataExists)
        {
            // Arrange
            var runId = Fixture.Create<int>();
            var run = Fixture.Create<CalculatorRun>();
            run.Id = runId;
            run.CalculatorRunOrganisationDataMasterId = null;
            run.CalculatorRunPomDataMasterId = null;

            Wrapper.Setup(w => w.AnyPomData()).Returns(pomDataExists);
            Wrapper.Setup(w => w.AnyOrganisationData()).Returns(orgDataExists);

            // Act
            var result = TestClass.IsValidSuccessfulRun(runId);

            Assert.AreEqual(StatusCodes.Status422UnprocessableEntity, result.StatusCode);
            Assert.AreEqual("PomData or Organisation Data is missing", result.ErrorMessage);
        }
    }
}
