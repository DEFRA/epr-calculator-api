using System.Globalization;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Utils;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Extensions;
using EPR.Calculator.API.Mappers;
using EPR.Calculator.API.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.Controllers;

[ApiController]
[Produces("application/json")]
[Route("v1")]
public class DefaultParameterSettingController (
    ApplicationDBContext dbContext
) : ControllerBase
{
    [HttpPut]
    [Route("defaultParameterSetting")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Set(SetDefaultParametersRequest request, CancellationToken cancellationToken = default)
    {
        await using (var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken))
        {
            try
            {
                var oldMasters = await dbContext.DefaultParameterSettings
                    .Where(x => x.EffectiveTo == null && x.RelativeYear == request.RelativeYear)
                    .ToListAsync(cancellationToken);

                oldMasters.ForEach(x => { x.EffectiveTo = DateTime.UtcNow; }); // Side effecting db update

                var newMaster = new DefaultParameterSettingMaster
                {
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = User.GetName(),
                    EffectiveFrom = DateTime.UtcNow,
                    EffectiveTo = null,
                    ParameterFileName = request.Filename!,
                    RelativeYear = request.RelativeYear!.Value
                };
                await dbContext.DefaultParameterSettings.AddAsync(newMaster, cancellationToken);

                var masterTemplate = await dbContext.DefaultParameterTemplateMasterList
                    .ToImmutableDictionaryAsync(t => t.ParameterUniqueReferenceId, StringComparer.OrdinalIgnoreCase, cancellationToken);

                foreach (var parameter in request.Parameters!)
                {
                    var template = masterTemplate[parameter.Id!];

                    var newDetail = new DefaultParameterSettingDetail
                    {
                        DefaultParameterSettingMaster = newMaster,
                        ParameterUniqueReferenceId = template.ParameterUniqueReferenceId,
                        ParameterValue = SanitizeParameterValue(template.Unit, parameter.Value!)
                    };

                    await dbContext.DefaultParameterSettingDetail.AddAsync(newDetail, cancellationToken);
                }

                await dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(CancellationToken.None);
                throw;
            }
        }

        return new NoContentResult();

        static string SanitizeParameterValue(ParameterUnit unit, string value)
        {
            if (unit != ParameterUnit.Date)
            {
                value = RegexPatterns.NonDecimalChars().Replace(value, "");
                return decimal.Parse(value).ToString("F3");
            }

            value = value.Trim().ToUpperInvariant();

            return value != "NA"
                ? DateOnly.Parse(value, CultureInfo.CurrentCulture).ToShortDateString()
                : "NA";
        }
    }

    [HttpGet]
    [Route("defaultParameterSetting/{relativeYearValue}")]
    [ProducesResponseType(typeof(List<DefaultSchemeParametersDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Get([FromRoute] int relativeYearValue)
    {
        var relativeYear = await dbContext.FindRelativeYearAsync(relativeYearValue);
        if (relativeYear == null)
            return new ObjectResult(CommonResources.NoDataForSpecifiedYear) { StatusCode = StatusCodes.Status400BadRequest };

        var currentDefaultSetting = await dbContext.DefaultParameterSettings
            .Include(x => x.Details)
            .SingleOrDefaultAsync(x => x.EffectiveTo == null && x.RelativeYear == relativeYearValue);

        if (currentDefaultSetting == null)
            return new ObjectResult(CommonResources.NoDataForSpecifiedYear) { StatusCode = StatusCodes.Status404NotFound };

        var templateDetails = await dbContext.DefaultParameterTemplateMasterList.ToListAsync();

        var schemeParameters = CreateDefaultParameterSettingMapper.Map(currentDefaultSetting, templateDetails);
        return new ObjectResult(schemeParameters) { StatusCode = StatusCodes.Status200OK };
    }
}
