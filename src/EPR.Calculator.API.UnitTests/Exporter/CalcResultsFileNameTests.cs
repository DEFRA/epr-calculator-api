namespace EPR.Calculator.API.UnitTests.Exporter
{
    using System;
    using AutoFixture;
    using EPR.Calculator.API.Data;
    using EPR.Calculator.API.Data.DataModels;
    using EPR.Calculator.API.Exporter;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Moq.EntityFrameworkCore;

    /// <summary>
    /// Unit tests for <see cref="CalcResultsExporter"/>.
    /// </summary>
    [TestClass]
    public class CalcResultsFileNameTests
    {
        private Fixture Fixture { get; init; }

        private int RunId { get; init; }

        private DateTime TimeStamp { get; init; }

        public CalcResultsFileNameTests()
        {
            Fixture = new Fixture();
            RunId = Fixture.Create<int>();
            TimeStamp = Fixture.Create<DateTime>();
        }

        /// <summary>
        /// Check that an exception is thrown when trying to construct the file name,
        /// but a blank run name is used.
        /// </summary>
        /// <param name="value"></param>
        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("   ")]
        public void CannotConstructWithInvalidRunName(string value)
        {
            //Arrange
            Exception? exception = null;

            // Act
            try
            {
                new CalcResultsFileName(RunId, value, TimeStamp);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsInstanceOfType<ArgumentException>(exception);
        }

        /// <summary>
        /// Check that the format of the file name follows the expected pattern,
        /// and that the run ID, name and time-stamp have been inserted into it.
        /// </summary>
        /// <param name="nameLength">
        /// The length of the run name to use.
        /// Names containing more than 30 characters should end up being truncated in the output.
        /// </param>
        [TestMethod]
        [DataRow(1)]
        [DataRow(10)]
        [DataRow(30)]
        [DataRow(31)]
        [DataRow(1000)]
        public void StringFormatIsCorrect(int nameLength)
        {
            //Arrange
            var runName = GetRandomString(nameLength);

            char[] delimiters = ['-', '_', '.'];
            var expectedRunNameLength = Math.Min(nameLength, CalcResultsFileName.MaxRunNameLength);
            var expectedRunName = runName.Substring(0, expectedRunNameLength);
            var expectedTimeStamp = this.TimeStamp.ToString("yyyyMMdd");

            //Act
            var testClass = new CalcResultsFileName(RunId, runName, TimeStamp);

            //Assert
            var components = testClass.ToString().Split(delimiters);

            Assert.AreEqual(this.RunId, int.Parse(components[0]));
            Assert.IsTrue(components[1].Length <= 30);
            Assert.AreEqual(expectedRunName, components[1]);
            Assert.AreEqual(expectedTimeStamp, components[3]);
            Assert.AreEqual(CalcResultsFileName.FileExtension, components[4]);
            Assert.AreEqual(
                testClass,
                $"{this.RunId}-{expectedRunName}_Results File_{expectedTimeStamp}.csv");
        }

        /// <summary>
        /// Checks generating a file name using values retrieved from the database.
        /// </summary>
        [TestMethod]
        public void CanCallFromDatabase()
        {
            // Arrange
            var mockRun = Fixture.Create<CalculatorRun>();
            var context = new Mock<ApplicationDBContext>();
            context.Setup(c => c.CalculatorRuns).ReturnsDbSet([mockRun]);
            var expectedFileName = $"{mockRun.Id}" +
                $"-{mockRun.Name[0..30]}" +
                $"_Results File" +
                $"_{mockRun.CreatedAt:yyyyMMdd}" +
                $".csv";

            // Act
            var result = CalcResultsFileName.FromDatabase(context.Object, mockRun.Id);

            // Assert
            Assert.AreEqual(expectedFileName, (string)result);
        }

        private static string GetRandomString(int length)
            => string.Join(string.Empty, new char[length].Select(c => GetRandomChar()));

        private static char GetRandomChar()
            => (char)('a' + Random.Shared.Next(0, 26));
    }
}