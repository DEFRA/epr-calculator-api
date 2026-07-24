using System.Globalization;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Extensions;
using EPR.Calculator.API.Mappers;
using EPR.Calculator.API.Validators;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.Controllers;

[ApiController]
[Produces("application/json")]
[Route("v1")]
public class DefaultParameterSettingController (
    ApplicationDBContext context,
    ICreateDefaultParameterDataValidator validator,
    ILogger<DefaultParameterSettingController> logger
) : ControllerBase
{
    [HttpPost]
    [Route("defaultParameterSetting")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] CreateDefaultParameterSettingDto request)
    {
        logger.LogDebug("Requested Parameter filename: {ParameterFilename}", request.ParameterFileName);

        var validationResult = validator.Validate(request);

        if (validationResult.IsInvalid)
            return BadRequest(validationResult.Errors);

        using (var transaction = await context.Database.BeginTransactionAsync())
        {
            try
            {
                var relativeYear = await context.FindRelativeYearAsync(request.RelativeYear.Value);
                if (relativeYear == null)
                    return new ObjectResult(CommonResources.NoDataForSpecifiedYear) { StatusCode = StatusCodes.Status400BadRequest };

                var oldDefaultSettings = await context.DefaultParameterSettings
                    .Where(x => x.EffectiveTo == null && x.RelativeYear == request.RelativeYear)
                    .ToListAsync();

                oldDefaultSettings.ForEach(x => { x.EffectiveTo = DateTime.UtcNow; }); // side effecting db update

                var defaultParamSettingMaster = new DefaultParameterSettingMaster
                {
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = User.GetName(),
                    EffectiveFrom = DateTime.UtcNow,
                    EffectiveTo = null,
                    RelativeYear = request.RelativeYear,
                    ParameterFileName = request.ParameterFileName
                };
                await context.DefaultParameterSettings.AddAsync(defaultParamSettingMaster);

                var defaultParameterSettingDetails = request.SchemeParameterTemplateValues
                    .Select(templateValue =>
                    {
                        var parameterValue = templateValue.ParameterValue
                            .TrimEnd('%')
                            .Replace("£", "")
                            .Replace(",", "")
                            .Trim();

                        if (decimal.TryParse(parameterValue, NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
                        {
                            parameterValue = Math.Round(value, 3).ToString("F3", CultureInfo.InvariantCulture);
                        }

                        return new DefaultParameterSettingDetail
                        {
                            ParameterValue                = parameterValue,
                            ParameterUniqueReferenceId    = templateValue.ParameterUniqueReferenceId,
                            DefaultParameterSettingMaster = defaultParamSettingMaster
                        };
                    })
                    .ToList();

                await context.DefaultParameterSettingDetail.AddRangeAsync(defaultParameterSettingDetails);

                await context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        return new ObjectResult(null) { StatusCode = StatusCodes.Status201Created };
    }

    [HttpGet]
    [Route("defaultParameterSetting/{relativeYearValue}")]
    [ProducesResponseType(typeof(List<DefaultSchemeParametersDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Get([FromRoute] int relativeYearValue)
    {
        var relativeYear = await context.FindRelativeYearAsync(relativeYearValue);
        if (relativeYear == null)
            return new ObjectResult(CommonResources.NoDataForSpecifiedYear) { StatusCode = StatusCodes.Status400BadRequest };

        var currentDefaultSetting = await context.DefaultParameterSettings
            .Include(x => x.Details)
            .SingleOrDefaultAsync(x => x.EffectiveTo == null && x.RelativeYear == relativeYearValue);

        if (currentDefaultSetting == null)
            return new ObjectResult(CommonResources.NoDataForSpecifiedYear) { StatusCode = StatusCodes.Status404NotFound };

        var templateDetails = await context.DefaultParameterTemplateMasterList.ToListAsync();

        var schemeParameters = CreateDefaultParameterSettingMapper.Map(currentDefaultSetting, templateDetails);
        return new ObjectResult(schemeParameters) { StatusCode = StatusCodes.Status200OK };
    }
}
