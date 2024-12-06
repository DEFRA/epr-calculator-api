using EPR.Calculator.API.Builder;
using EPR.Calculator.API.Controllers;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Exporter;
using EPR.Calculator.API.Models;
using EPR.Calculator.API.Services;
using EPR.Calculator.API.Validators;
using EPR.Calculator.API.Wrapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EPR.Calculator.API.UnitTests
{
    [TestClass]
    public class PrepareCalcResultsValidationTest
    {
        private CalculatorInternalController controller;
        private ApplicationDBContext _context;
        private Mock<IRpdStatusDataValidator> _rpdStatusDataValidator;
        private Mock<IOrgAndPomWrapper> _wrapper;
        private Mock<ICalcResultBuilder> _builder;
        private Mock<ICalcResultsExporter<CalcResult>> _exporter;
        private Mock<ITransposePomAndOrgDataService> _transposePomAndOrgDataService;

        [TestInitialize]
        public void SetUp()
        {
            _context = new ApplicationDBContext();
            _rpdStatusDataValidator = new Mock<IRpdStatusDataValidator>();
            _wrapper = new Mock<IOrgAndPomWrapper>();
            _builder = new Mock<ICalcResultBuilder>();
            _exporter = new Mock<ICalcResultsExporter<CalcResult>>();
            _transposePomAndOrgDataService = new Mock<ITransposePomAndOrgDataService>();
            controller = new CalculatorInternalController(_context, _rpdStatusDataValidator.Object, _wrapper.Object, _builder.Object, _exporter.Object, _transposePomAndOrgDataService.Object);
        }

        [TestMethod]
        public void PrepareCalcResults_Invalid_RunId()
        {
            controller.ModelState.AddModelError("InvalidRunId", CalcResultsRequestDtoValidator.ErrorMessage);
            var result = controller.PrepareCalcResults(new CalcResultsRequestDto { RunId = 0 }) as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
            var errors = result.Value as IEnumerable<ModelError>;
            Assert.IsNotNull(errors);
            Assert.IsTrue(errors.Any(x => x.ErrorMessage == CalcResultsRequestDtoValidator.ErrorMessage));
        }
    }
}
