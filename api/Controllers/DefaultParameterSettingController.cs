using api.Data;
using api.Dtos;
using api.Models;
using api.Validators;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace api.Controllers
{
    public class DefaultParameterSettingController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public DefaultParameterSettingController(ApplicationDBContext context)
        {
            this._context = context;
        }

        [HttpPost]
        [Route("api/CreateDefaultParameterSetting")]
        public IActionResult Create([FromBody] CreateDefaultParameterSettingDto createDefaultParameterDto)
        {
            var validator = new CreateDefaultParameterSettingDtoValidator();
            var validationResult = validator.Validate(createDefaultParameterDto);

            if (validationResult != null && validationResult.IsInvalid)
            {
                return BadRequest(validationResult.Errors);
            }
            var defaultParamSettingMaster = new DefaultParameterSettingMaster
            {
                CreatedAt = DateTime.Now,
                CreatedBy = "Testuser",
                EffectiveFrom = DateTime.Now,
                EffectiveTo = null,
                ParameterYear = createDefaultParameterDto.ParameterYear
            };
            this._context.DefaultParameterSettings.Add(defaultParamSettingMaster);
            foreach (var templateValue in createDefaultParameterDto.SchemeParameterTemplateValues)
            {
                this._context.DefaultParameterSettingDetail.Add(new DefaultParameterSettingDetail
                {
                    ParameterValue = templateValue.ParameterValue,
                    ParameterUniqueReferenceId = templateValue.ParameterUniqueReferenceId,
                    DefaultParameterSettingMaster = defaultParamSettingMaster
                });
            }
            this._context.SaveChanges();
            return new ObjectResult(null) { StatusCode = StatusCodes.Status201Created };
        }

        [HttpGet]
        [Route("api/GetDefaultParameterSetting")]
        public IActionResult Get()
        {
            return Ok(new {Test = "This is not implemented!"});
        }
    }
}
