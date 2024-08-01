using EPR.Calculator.API.Dtos;
using api.Validators;
using Microsoft.AspNetCore.Mvc;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using FluentValidation;

namespace EPR.Calculator.API.Controllers
{
    public class DefaultParameterSettingController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public DefaultParameterSettingController(ApplicationDBContext context)
        {
            this._context = context;
        }

        [HttpPost]
        [Route("api/defaultParameterSetting")]
        public IActionResult Create([FromBody] CreateDefaultParameterSettingDto request)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ModelState.Values.SelectMany(x => x.Errors));
            }
            var customValidator = new CreateDefaultParameterDataValidator(this._context);
            var validationResult = customValidator.Validate(request);
            if (validationResult != null && validationResult.IsInvalid)
            {
                return BadRequest(validationResult.Errors);
            }

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var oldDefaultSettings = this._context.DefaultParameterSettings.Where(x => x.EffectiveTo == null).ToList();
                    oldDefaultSettings.ForEach(x => { x.EffectiveTo = DateTime.Now; });

                    var defaultParamSettingMaster = new DefaultParameterSettingMaster
                    {
                        CreatedAt = DateTime.Now,
                        CreatedBy = "Testuser",
                        EffectiveFrom = DateTime.Now,
                        EffectiveTo = null,
                        ParameterYear = request.ParameterYear
                    };
                    this._context.DefaultParameterSettings.Add(defaultParamSettingMaster);

                    foreach (var templateValue in request.SchemeParameterTemplateValues)
                    {
                        this._context.DefaultParameterSettingDetail.Add(new DefaultParameterSettingDetail
                        {
                            ParameterValue = templateValue.ParameterValue,
                            ParameterUniqueReferenceId = templateValue.ParameterUniqueReferenceId,
                            DefaultParameterSettingMaster = defaultParamSettingMaster
                        });
                    }
                    this._context.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception exception)
                {
                    transaction.Rollback();
                    return StatusCode(StatusCodes.Status500InternalServerError, exception);
                }
            }
                
            return new ObjectResult(null) { StatusCode = StatusCodes.Status201Created };
        }

        [HttpGet]
        [Route("api/defaultParameterSetting")]
        public IActionResult Get()
        {
            return Ok(new {Test = "This is not implemented!"});
        }
    }
}
