using EPR.Calculator.API.Mappers;
using EPR.Calculator.API.Validators;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.Controllers
{
    [Route("v1")]
    public class DefaultParameterSettingController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        private readonly ICreateDefaultParameterDataValidator validator;

        public DefaultParameterSettingController(ApplicationDBContext context,
                ICreateDefaultParameterDataValidator validator)
        {
            this._context = context;
            this.validator = validator;
        }

        [HttpPost]
        [Route("defaultParameterSetting")]
        public async Task<IActionResult> Create([FromBody] CreateDefaultParameterSettingDto request)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ModelState.Values.SelectMany(x => x.Errors));
            }
            var validationResult = validator.Validate(request);
            if (validationResult != null && validationResult.IsInvalid)
            {
                return BadRequest(validationResult.Errors);
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var oldDefaultSettings = await _context.DefaultParameterSettings.Where(x => x.EffectiveTo == null).ToListAsync();
                    oldDefaultSettings.ForEach(x => { x.EffectiveTo = DateTime.Now; });

                    var defaultParamSettingMaster = new DefaultParameterSettingMaster
                    {
                        CreatedAt = DateTime.Now,
                        CreatedBy = "Testuser",
                        EffectiveFrom = DateTime.Now,
                        EffectiveTo = null,
                        ParameterYear = request.ParameterYear,
                        ParameterFileName = request.ParameterFileName
                    };
                    await _context.DefaultParameterSettings.AddAsync(defaultParamSettingMaster);

                    var defaultParameterSettingDetails = request.SchemeParameterTemplateValues
                    .Select(templateValue => new DefaultParameterSettingDetail
                    {
                        ParameterValue = decimal.Parse(templateValue.ParameterValue.TrimEnd('%').Replace("£", string.Empty)),
                        ParameterUniqueReferenceId = templateValue.ParameterUniqueReferenceId,
                        DefaultParameterSettingMaster = defaultParamSettingMaster
                    })
                    .ToList();

                    await _context.DefaultParameterSettingDetail.AddRangeAsync(defaultParameterSettingDetails);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception exception)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(StatusCodes.Status500InternalServerError, exception);
                }
            }

            return new ObjectResult(null) { StatusCode = StatusCodes.Status201Created };
        }

        [HttpGet]
        [Route("defaultParameterSetting/{parameterYear}")]
        public async Task<IActionResult> Get([FromRoute] string parameterYear)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, ModelState.Values.SelectMany(x => x.Errors));
                }

                var currentDefaultSetting = await _context.DefaultParameterSettings
                    .SingleOrDefaultAsync(x => x.EffectiveTo == null && x.ParameterYear == parameterYear);

                if (currentDefaultSetting == null)
                {
                    return new ObjectResult("No data available for the specified year. Please check the year and try again.") { StatusCode = StatusCodes.Status404NotFound };
                }
            
                var _pramSettingDetails = await _context.DefaultParameterSettingDetail
                    .Where(x => x.DefaultParameterSettingMasterId == currentDefaultSetting.Id)
                    .ToListAsync();

                var _templateDetails = await _context.DefaultParameterTemplateMasterList.ToListAsync();

                var schemeParameters = CreateDefaultParameterSettingMapper.Map(currentDefaultSetting, _templateDetails);
                return new ObjectResult(schemeParameters) { StatusCode = StatusCodes.Status200OK };
            }
            catch (Exception exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, exception);
            }
        }
    }
}