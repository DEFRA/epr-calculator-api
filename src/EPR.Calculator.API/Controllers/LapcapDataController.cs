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
public class LapcapDataController (
    ApplicationDBContext dbContext
) : ControllerBase
{
    [HttpPut]
    [Route("lapcapData")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Set(SetLapcapDataRequest request, CancellationToken cancellationToken = default)
    {
        await using (var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken))
        {
            try
            {
                var oldMasters = await dbContext.LapcapDataMaster
                    .Where(x => x.EffectiveTo == null && x.RelativeYear == request.RelativeYear)
                    .ToListAsync(cancellationToken);

                oldMasters.ForEach(x => { x.EffectiveTo = DateTime.UtcNow; }); // Side effecting db update

                var newMaster = new LapcapDataMaster
                {
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = User.GetName(),
                    EffectiveFrom = DateTime.UtcNow,
                    EffectiveTo = null,
                    LapcapFileName = request.Filename!,
                    RelativeYear = request.RelativeYear!.Value
                };
                await dbContext.LapcapDataMaster.AddAsync(newMaster, cancellationToken);

                var masterTemplate = await dbContext.LapcapDataTemplateMaster
                    .ToImmutableDictionaryAsync(LapcapKeyHelper.KeyFor, cancellationToken);

                foreach (var value in request.Values!)
                {
                    var newDetail = new LapcapDataDetail
                    {
                        TotalCost = value.TotalCost!.Value,
                        UniqueReference = masterTemplate[LapcapKeyHelper.KeyFor(value)].UniqueReference,
                        LapcapDataMaster = newMaster
                    };

                    await dbContext.LapcapDataDetail.AddAsync(newDetail, cancellationToken);
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
        var relativeYear = await dbContext.FindRelativeYearAsync(relativeYearValue);
        if (relativeYear == null)
            return new ObjectResult(CommonResources.NoDataForSpecifiedYear) { StatusCode = StatusCodes.Status404NotFound };

        var lapcapDataMaster = await dbContext.LapcapDataMaster
            .Include(m => m.Details)
            .SingleOrDefaultAsync(m => m.EffectiveTo == null && m.RelativeYear == relativeYear);

        if (lapcapDataMaster == null)
            return new ObjectResult(CommonResources.NoDataForSpecifiedYear) { StatusCode = StatusCodes.Status404NotFound };

        var lapcaptemplateDetails = await dbContext.LapcapDataTemplateMaster.ToListAsync();
        var lapcapdatavalues = LapcapDataParameterSettingMapper.Map(lapcapDataMaster, lapcaptemplateDetails);
        return new ObjectResult(lapcapdatavalues) { StatusCode = StatusCodes.Status200OK };
    }
}
