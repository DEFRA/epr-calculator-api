namespace EPR.Calculator.API.UnitTests.Builder
{
    using System;
    using EPR.Calculator.API.Builder.Detail;
    using EPR.Calculator.API.Data;
    using EPR.Calculator.API.Dtos;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CalcResultDetailBuilderTests
    {
        private CalcResultDetailBuilder _testClass;
        private ApplicationDBContext _context;

        [TestInitialize]
        public void SetUp()
        {
            _context = new ApplicationDBContext();
            _testClass = new CalcResultDetailBuilder(_context);
        }

        [TestMethod]
        public void CanConstruct()
        {
            // Act
            var instance = new CalcResultDetailBuilder(_context);

            // Assert
            Assert.IsNotNull(instance);
        }
    }
}