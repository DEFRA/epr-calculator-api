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

namespace EPR.Calculator.API.UnitTests.Controllers
{
    [TestClass]
    public class PrepareCalcResultsValidationTest
    {
        private readonly CalculatorInternalController controller;
        private readonly ApplicationDBContext _context;
        private readonly Mock<IRpdStatusDataValidator> _rpdStatusDataValidator;
        private readonly Mock<IOrgAndPomWrapper> _wrapper;
        private readonly Mock<ICalcResultBuilder> _builder;
        private readonly Mock<ICalcResultsExporter<CalcResult>> _exporter;
        private readonly Mock<ITransposePomAndOrgDataService> _transposePomAndOrgDataService;
        private readonly CalculatorRunValidator _runValidator;

        public PrepareCalcResultsValidationTest()
        {
            _context = new ApplicationDBContext();
            _rpdStatusDataValidator = new Mock<IRpdStatusDataValidator>();
            _wrapper = new Mock<IOrgAndPomWrapper>();
            _builder = new Mock<ICalcResultBuilder>();
            _exporter = new Mock<ICalcResultsExporter<CalcResult>>();
            _transposePomAndOrgDataService = new Mock<ITransposePomAndOrgDataService>();
            _runValidator = new CalculatorRunValidator();
            var mockStorageService = new Mock<IStorageService>();
            controller = new CalculatorInternalController(_context, _rpdStatusDataValidator.Object, _wrapper.Object,
                            _builder.Object, _exporter.Object, _transposePomAndOrgDataService.Object, mockStorageService.Object, _runValidator);
        }

        [TestMethod]
        public void PrepareCalcResults_Invalid_RunId()
        {
            controller.ModelState.AddModelError("InvalidRunId", CalcResultsRequestDtoValidator.ErrorMessage);
            var task = controller.PrepareCalcResults(new CalcResultsRequestDto { RunId = 0 });
            task.Wait();
            var result = task.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
            var errors = result.Value as IEnumerable<ModelError>;
            Assert.IsNotNull(errors);
            Assert.IsTrue(errors.Any(x => x.ErrorMessage == CalcResultsRequestDtoValidator.ErrorMessage));
        }
    }
}