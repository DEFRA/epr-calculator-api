using System.Reflection;
using System.Text;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Exporter.CsvExporter;
using Moq.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.UnitTests.Exporter.CsvExporter
{
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
            Fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => Fixture.Behaviors.Remove(b));
            Fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            RunId = Fixture.Create<int>();
            TimeStamp = Fixture.Create<DateTime>();
        }

        /// <summary>
        /// Check that an exception is thrown when trying to construct the file name,
        /// but a blank run name is used.
        /// </summary>
        [TestMethod]
        public void CanCreateBillingCsvFileName()
        {
            var billingFileCsvName = new CalcResultsAndBillingFileName(10223, "RunName", new DateTime(2025, 10, 1, 9, 25, 0), true);
            Assert.AreEqual("10223-RunName_Billing File_202510010925.csv", billingFileCsvName);
        }

        /// <summary>
        /// Check that an exception is thrown when trying to construct the file name,
        /// but a blank run name is used.
        /// </summary>
        [TestMethod]
        public void CanCreateResultsCsvFileName()
        {
            var resultsFileCsvName = new CalcResultsAndBillingFileName(10223, "RunName", new DateTime(2025, 10, 1, 9, 25, 0));
            Assert.AreEqual("10223-RunName_Results File_20251001.csv", resultsFileCsvName);
        }

        /// <summary>
        /// Check that an exception is thrown when trying to construct the file name,
        /// but a blank run name is used.
        /// </summary>
        [TestMethod]
        public void CanCreateBillingJsonFileName()
        {
            var billingFileJsonName = new CalcResultsAndBillingFileName(10223, true, true);
            Assert.AreEqual("10223billing.json", billingFileJsonName);
        }

        /// <summary>
        /// Check that an exception is thrown when trying to construct the file name,
        /// but a blank run name is used.
        /// </summary>
        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("   ")]
        public void CannotConstructBillingFileWithInvalidRunName(string value)
        {
            // Arrange
            Exception? exception = null;

            // Act
            try
            {
                _ = new CalcResultsAndBillingFileName(RunId, value, TimeStamp, true);
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
            // Arrange
            var runName = GetRandomString(nameLength);

            char[] delimiters = ['-', '_', '.'];
            var expectedRunNameLength = Math.Min(nameLength, CalcResultsAndBillingFileName.MaxRunNameLength);
            var expectedRunName = runName.Substring(0, expectedRunNameLength);
            var expectedTimeStamp = TimeStamp.ToString("yyyyMMdd");

            // Act
            var testClass = new CalcResultsAndBillingFileName(RunId, runName, TimeStamp);

            // Assert
            var components = testClass.ToString().Split(delimiters);

            Assert.AreEqual(RunId, int.Parse(components[0]));
            Assert.IsTrue(components[1].Length <= 30);
            Assert.AreEqual(expectedRunName, components[1]);
            Assert.AreEqual(expectedTimeStamp, components[3]);
            Assert.AreEqual(CalcResultsAndBillingFileName.CsvFileExtension, components[4]);
            Assert.AreEqual(
                testClass,
                $"{RunId}-{expectedRunName}_Results File_{expectedTimeStamp}.csv");
        }

        /// <summary>
        /// Checks generating a file name using values retrieved from the database.
        /// </summary>
        [TestMethod]
        public void CanCallFromDatabase()
        {
            // Arrange
            var mockRun = Fixture.Build<CalculatorRun>().Create();
            mockRun.Name = Fixture.Create<string>();
            var context = new Mock<ApplicationDBContext>();
            context.Setup(c => c.CalculatorRuns).ReturnsDbSet([mockRun]);
            var expectedFileName = $"{mockRun.Id}" +
                $"-{mockRun.Name[..30]}" +
                $"_Results File" +
                $"_{mockRun.CreatedAt:yyyyMMdd}" +
                $".csv";

            // Act
            var result = CalcResultsAndBillingFileName.FromDatabase(context.Object, mockRun.Id);

            // Assert
            Assert.AreEqual(expectedFileName, (string)result);
        }

        /// <summary>
        /// Tests the AppendFileInfo method with an invalid file path.
        /// </summary>
        [TestMethod]
        public void AppendFileInfo_InvalidFilePath_DoesNotAppend()
        {
            // Arrange
            var csvContent = new StringBuilder();
            string label = "TestLabel";
            string filePath = "fileName.csv,2025-02-14"; // Missing user part

            // Act
            InvokeAppendFileInfo(csvContent, label, filePath);

            // Assert
            Assert.AreEqual(string.Empty, csvContent.ToString());
        }

        /// <summary>
        /// Tests the AppendFileInfo method with an empty file path.
        /// </summary>
        [TestMethod]
        public void AppendFileInfo_EmptyFilePath_DoesNotAppend()
        {
            // Arrange
            var csvContent = new StringBuilder();
            string label = "TestLabel";
            string filePath = string.Empty;

            // Act
            InvokeAppendFileInfo(csvContent, label, filePath);

            // Assert
            Assert.AreEqual(string.Empty, csvContent.ToString());
        }

        /// <summary>
        /// Checks generating a file name using values retrieved from the database.
        /// </summary>
        private static void InvokeAppendFileInfo(StringBuilder csvContent, string label, string filePath)
        {
            // Get the type of the class containing the method
            Type type = typeof(CalcResultsExporter);

            // Get the method info using reflection
            MethodInfo? methodInfo = type.GetMethod("AppendFileInfo", BindingFlags.NonPublic | BindingFlags.Static);

            // Check if methodInfo is not null before invoking
            if (methodInfo != null)
            {
                // Invoke the method
                methodInfo.Invoke(null, new object[] { csvContent, label, filePath });
            }
        }

        private static string GetRandomString(int length)
            => string.Join(string.Empty, new char[length].Select(c => GetRandomChar()));

        private static char GetRandomChar()
            => (char)('a' + Random.Shared.Next(0, 26));
    }
}
