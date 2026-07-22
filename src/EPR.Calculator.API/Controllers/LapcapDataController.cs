using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Extensions;
using EPR.Calculator.API.Mappers;
using EPR.Calculator.API.Validators;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;

namespace EPR.Calculator.API.Controllers;

[ApiController]
[Produces("application/json")]
[Route("v1")]
public class LapcapDataController (
    ApplicationDBContext context,
    ILapcapDataValidator validator,
    ILogger<LapcapDataController> logger
) : ControllerBase
{
    [HttpPost]
    [Route("lapcapData")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] CreateLapcapDataDto request)
    {
        logger.LogDebug("Requested LAPCAP filename: {LapcapFilename}", request.LapcapFileName);

        var validationResult = validator.Validate(request);
        if (validationResult.IsInvalid)
            return BadRequest(validationResult.Errors);

        var templateMaster = await context.LapcapDataTemplateMaster.ToListAsync();
        using (var transaction = await context.Database.BeginTransactionAsync())
        {
            try
            {
                var relativeYear = await context.FindRelativeYearAsync(request.RelativeYear.Value);
                if (relativeYear == null)
                    return new ObjectResult(CommonResources.NoDataForSpecifiedYear) { StatusCode = StatusCodes.Status400BadRequest };

                var oldLapcapData = await context.LapcapDataMaster
                    .Where(x => x.EffectiveTo == null && x.RelativeYear == request.RelativeYear)
                    .ToListAsync();

                oldLapcapData.ForEach(x => { x.EffectiveTo = DateTime.UtcNow; }); // Side effecting db update

                var lapcapDataMaster = new LapcapDataMaster
                {
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = User.GetName(),
                    EffectiveFrom = DateTime.UtcNow,
                    EffectiveTo = null,
                    LapcapFileName = request.LapcapFileName,
                    RelativeYear = request.RelativeYear
                };
                await context.LapcapDataMaster.AddAsync(lapcapDataMaster);

                foreach (var templateValue in request.LapcapDataTemplateValues)
                {
                    var uniqueReference = templateMaster.Single(x =>
                        x.Material == templateValue.Material && x.Country == templateValue.CountryName).UniqueReference;

                    await context.LapcapDataDetail.AddAsync(new LapcapDataDetail
                    {
                        TotalCost = decimal.Parse(templateValue.TotalCost.Replace("£", string.Empty)),
                        UniqueReference = uniqueReference,
                        LapcapDataMaster = lapcapDataMaster
                    });
                }

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

    /// <summary>
    ///     Retrieves LAPCAP data for a specified year.
    /// </summary>
    /// <param name="relativeYearValue">The year for which to retrieve LAPCAP data.</param>
    /// <returns>
    ///     An IActionResult containing the LAPCAP data for the specified year, or an appropriate error message:
    ///     - 400 Bad Request if the model state is invalid.
    ///     - 404 Not Found if no data is available for the specified year.
    ///     - 500 Internal Server Error if an exception occurs during data retrieval.
    /// </returns>
    /// <response code="200">Returns the LAPCAP data for the specified year.</response>
    /// <response code="400">If the model state is invalid.</response>
    /// <response code="404">If no data is available for the specified year.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpGet]
    [Route("lapcapData/{relativeYearValue}")]
    [ProducesResponseType(typeof(List<LapCapParameterDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Get([FromRoute] int relativeYearValue)
    {
        var relativeYear = await context.FindRelativeYearAsync(relativeYearValue);
        if (relativeYear == null)
            return new ObjectResult(CommonResources.NoDataForSpecifiedYear) { StatusCode = StatusCodes.Status404NotFound };

        var lapcapDataMaster = await context.LapcapDataMaster
            .Include(m => m.Details)
            .SingleOrDefaultAsync(m => m.EffectiveTo == null && m.RelativeYear == relativeYear);

        if (lapcapDataMaster == null)
            return new ObjectResult(CommonResources.NoDataForSpecifiedYear) { StatusCode = StatusCodes.Status404NotFound };

        var lapcaptemplateDetails = await context.LapcapDataTemplateMaster.ToListAsync();
        var lapcapdatavalues = LapcapDataParameterSettingMapper.Map(lapcapDataMaster, lapcaptemplateDetails);
        return new ObjectResult(lapcapdatavalues) { StatusCode = StatusCodes.Status200OK };
    }
}
