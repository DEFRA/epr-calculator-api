using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Mappers;
using EPR.Calculator.API.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
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
        [Authorize(Roles = "SASuperUser")]
        public async Task<IActionResult> Create([FromBody] CreateDefaultParameterSettingDto request)
        {
            var claim = User?.Claims?.FirstOrDefault(x => x.Type == "name");
            if (claim == null)
            {
                return new ObjectResult("No claims in the request") { StatusCode = StatusCodes.Status401Unauthorized };
            }

            var userName = claim.Value;
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

                    var financialYear = await _context.FinancialYears.Where(
                        x => x.Name == request.ParameterYear).SingleAsync();

                    var defaultParamSettingMaster = new DefaultParameterSettingMaster
                    {
                        CreatedAt = DateTime.Now,
                        CreatedBy = userName,
                        EffectiveFrom = DateTime.Now,
                        EffectiveTo = null,
                        ParameterYear = financialYear,
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
        [Authorize(Roles = "SASuperUser")]
        public async Task<IActionResult> Get([FromRoute] string parameterYear)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ModelState.Values.SelectMany(x => x.Errors));
            }
            
            try
            {
                var financialYear = await _context.FinancialYears.Where(x => x.Name == parameterYear).FirstOrDefaultAsync();
                var currentDefaultSetting = await _context.DefaultParameterSettings
                    .SingleOrDefaultAsync(x => x.EffectiveTo == null && x.ParameterYear == financialYear);

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