using EPR.Calculator.API.Mappers;
using EPR.Calculator.API.Validators;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.ApplicationInsights;
using Microsoft.IdentityModel.Abstractions;

namespace EPR.Calculator.API.Controllers
{
    [Route("v1")]
    public class DefaultParameterSettingController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        private readonly ICreateDefaultParameterDataValidator validator;
        private readonly TelemetryClient _telemetryClient;

        public DefaultParameterSettingController(ApplicationDBContext context,
                ICreateDefaultParameterDataValidator validator, TelemetryClient telemetryClient)
        {
            this._context = context;
            this.validator = validator;
            this._telemetryClient = telemetryClient;
        }

        [HttpPost]
        [Route("defaultParameterSetting")]
        [Authorize()]
        public async Task<IActionResult> Create([FromBody] CreateDefaultParameterSettingDto request)
        {
            this._telemetryClient.TrackTrace($"1.Parameter File Name in DefaultParameter API :{request.ParameterFileName}");

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
                this._telemetryClient.TrackTrace($"2.Parameter File Name in API :{request.ParameterFileName}");
                this._telemetryClient.TrackTrace($"3.Validation errors :{validationResult.Errors}");
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
                        CreatedBy = userName,
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
                    this._telemetryClient.TrackTrace($"4.500InternalServerError Exception :{exception}");
                    return StatusCode(StatusCodes.Status500InternalServerError, exception);
                }
            }

            return new ObjectResult(null) { StatusCode = StatusCodes.Status201Created };
        }

        [HttpGet]
        [Route("defaultParameterSetting/{parameterYear}")]
        [Authorize()]
        public async Task<IActionResult> Get([FromRoute] string parameterYear)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ModelState.Values.SelectMany(x => x.Errors));
            }
            
            try
            {
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