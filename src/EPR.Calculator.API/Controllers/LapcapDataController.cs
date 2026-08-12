using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
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
    ApplicationDBContext context
) : ControllerBase
{
    [HttpPost]
    [Route("lapcapData")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create(CreateLapcapDataRequest request, CancellationToken cancellationToken = default)
    {
        await using (var transaction = await context.Database.BeginTransactionAsync(cancellationToken))
        {
            try
            {
                var master = await context.LapcapDataTemplateMaster
                    .ToDictionaryAsync(LapcapKeyHelper.KeyFor, cancellationToken);

                var oldLapcapData = await context.LapcapDataMaster
                    .Where(x => x.EffectiveTo == null && x.RelativeYear == request.RelativeYear)
                    .ToListAsync(cancellationToken);

                oldLapcapData.ForEach(x => { x.EffectiveTo = DateTime.UtcNow; }); // Side effecting db update

                var lapcapDataMaster = new LapcapDataMaster
                {
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = User.GetName(),
                    EffectiveFrom = DateTime.UtcNow,
                    EffectiveTo = null,
                    LapcapFileName = request.Filename!,
                    RelativeYear = request.RelativeYear!.Value
                };
                await context.LapcapDataMaster.AddAsync(lapcapDataMaster, cancellationToken);

                foreach (var value in request.Values!)
                {
                    await context.LapcapDataDetail.AddAsync(new LapcapDataDetail
                    {
                        TotalCost = value.TotalCost!.Value,
                        UniqueReference = master[LapcapKeyHelper.KeyFor(value)].UniqueReference,
                        LapcapDataMaster = lapcapDataMaster
                    }, cancellationToken);
                }

                await context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(CancellationToken.None);
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
