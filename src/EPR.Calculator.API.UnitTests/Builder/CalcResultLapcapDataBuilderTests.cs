namespace EPR.Calculator.API.UnitTests.Builder
{
    using System;
    using EPR.Calculator.API.Builder;
    using EPR.Calculator.API.Data;
    using EPR.Calculator.API.Dtos;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CalcResultLapcapDataBuilderTests
    {
        private CalcResultLapcapDataBuilder _testClass;
        private ApplicationDBContext _context;

        [TestInitialize]
        public void SetUp()
        {
            _context = new ApplicationDBContext();
            _testClass = new CalcResultLapcapDataBuilder(_context);
        }

        [TestMethod]
        public void CanConstruct()
        {
            // Act
            var instance = new CalcResultLapcapDataBuilder(_context);

            // Assert
            Assert.IsNotNull(instance);
        }

    }
}