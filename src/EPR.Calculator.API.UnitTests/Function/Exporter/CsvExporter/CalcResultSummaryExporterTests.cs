using System.Text;
using EPR.Calculator.Service.Function.Exporter.CsvExporter;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.UnitTests.Builder;

namespace EPR.Calculator.Service.Function.UnitTests.Exporter.CsvExporter
{
    [TestClass]
    public class CalcResultSummaryExporterTests
    {
        private CalcResultSummaryExporter _testClass;

        public CalcResultSummaryExporterTests()
        {
            _testClass = new CalcResultSummaryExporter();
        }

        [TestMethod]
        public void CanCallWriteColumnHeaders()
        {
            // Arrange
            var resultSummary = new CalcResultSummary
            {
                ColumnHeaders = new List<CalcResultSummaryHeader>
                {
                    new CalcResultSummaryHeader
                    {
                        ColumnIndex = 0,
                        Name = "Column 0",
                    },
                    new CalcResultSummaryHeader
                    {
                        ColumnIndex = 1,
                        Name = "Column 2",
                    },
                    new CalcResultSummaryHeader
                    {
                        ColumnIndex = 5,
                        Name = "Column 6",
                    },
                    new CalcResultSummaryHeader
                    {
                        ColumnIndex = 10,
                        Name = "Column 11",
                    },
                    new CalcResultSummaryHeader
                    {
                        ColumnIndex = 20,
                        Name = "Column 21",
                    },
                },
            };
            var csvContent = new StringBuilder();

            // Act
            _testClass.WriteColumnHeaders(resultSummary, csvContent);

            // Assert
            Assert.AreEqual("\"Column 0\",\"Column 2\",\"Column 6\",\"Column 11\",\"Column 21\",", csvContent.ToString());
        }

        [TestMethod]
        public void CanCallWriteSecondaryHeaders()
        {
            var columnHeaders = new List<CalcResultSummaryHeader>
            {
                new CalcResultSummaryHeader
                {
                    ColumnIndex = 1,
                    Name = "Column 0",
                },
                new CalcResultSummaryHeader
                {
                    ColumnIndex = 2,
                    Name = "Column 2",
                },
                new CalcResultSummaryHeader
                {
                    ColumnIndex = 6,
                    Name = "Column 6",
                },
                new CalcResultSummaryHeader
                {
                    ColumnIndex = 11,
                    Name = "Column 11",
                },
                new CalcResultSummaryHeader
                {
                    ColumnIndex = 21,
                    Name = "Column 21",
                },
            };
            var csvContent = new StringBuilder();
            _testClass.WriteSecondaryHeaders(csvContent, columnHeaders);

            var rowContents = csvContent.ToString().Split(",");
            Assert.AreEqual("\"Column 0\"", rowContents[0]);
            Assert.AreEqual("\"Column 2\"", rowContents[1]);
            Assert.AreEqual("\"Column 6\"", rowContents[5]);
            Assert.AreEqual("\"Column 11\"", rowContents[10]);
            Assert.AreEqual("\"Column 21\"", rowContents[20].Trim());
        }

        [TestMethod]
        public void CanCallAddNewRow()
        {
            // Arrange
            var csvContent = new StringBuilder();
            var producer = TestDataHelper.GetProducerDisposalFees()[0];

            // Act
            _testClass.AddNewRow(csvContent, producer, applyModulation: false);
            var results = csvContent.ToString().Split(",");

            // Assert
            Assert.AreEqual(306, results.Length);
        }

        [TestMethod]
        public void CanCallAddNewRow_IsOverAllTotalTrue()
        {
            // Arrange
            var csvContent = new StringBuilder();
            var producer = TestDataHelper.GetProducerDisposalFeesForOverAllTotal()[0];

            // Act
            _testClass.AddNewRow(csvContent, producer, applyModulation: false);
            var results = csvContent.ToString().Split(",");

            // Assert
            Assert.AreEqual(306, results.Length);
        }

        [TestMethod]
        public void CanCallAddNewRow_TonnageValueNull()
        {
            // Arrange
            var csvContent = new StringBuilder();
            var producer = TestDataHelper.GetProducerDisposalFeesTonnageValueNull()[0];

            // Act
            _testClass.AddNewRow(csvContent, producer, applyModulation: false);
            var results = csvContent.ToString().Split(",");

            // Assert
            Assert.AreEqual(306, results.Length);
        }

        [TestMethod]
        public void CanCallAddNewRow_WithModulations()
        {
            // Arrange
            var csvContent = new StringBuilder();
            var producer = TestDataHelper.GetProducerDisposalFees(applyModulation: true)[0];

            // Act
            _testClass.AddNewRow(csvContent, producer, applyModulation: true);
            var results = csvContent.ToString().Split(",");

            // Assert
            Assert.AreEqual(482, results.Length);
        }

        [TestMethod]
        public void CanCallExport()
        {
            // Arrange

            var resultSummary = new CalcResultSummary
            {
                ProducerDisposalFeesHeaders = [new CalcResultSummaryHeader { ColumnIndex = 1, Name = "Column 1"}],
                MaterialBreakdownHeaders = [new CalcResultSummaryHeader { ColumnIndex = 1, Name = "Column 1"}],
                ColumnHeaders = [new CalcResultSummaryHeader { ColumnIndex = 1, Name = "Column 1"}],
                ProducerDisposalFees = TestDataHelper.GetProducerDisposalFees()
            };

            var csvContent = new StringBuilder();

            // Act
            _testClass.Export(resultSummary, csvContent, applyModulation: false);

            // Assert
            Assert.IsNotNull(csvContent.ToString());
        }
    }
}
