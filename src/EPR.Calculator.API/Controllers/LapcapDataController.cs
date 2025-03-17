﻿namespace EPR.Calculator.API.Controllers
{
    using EPR.Calculator.API.Data;
    using EPR.Calculator.API.Data.DataModels;
    using EPR.Calculator.API.Dtos;
    using EPR.Calculator.API.Mappers;
    using EPR.Calculator.API.Validators;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    [Route("v1")]
    public class LapcapDataController : ControllerBase
    {
        private readonly ApplicationDBContext context;
        private readonly ILapcapDataValidator validator;

        public LapcapDataController(ApplicationDBContext context, ILapcapDataValidator validator)
        {
            this.context = context;
            this.validator = validator;
        }

        [HttpPost]
        [Route("lapcapData")]
        [Authorize()]
        public async Task<IActionResult> Create([FromBody] CreateLapcapDataDto request)
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
            if (validationResult.IsInvalid)
            {
                return BadRequest(validationResult.Errors);
            }
            var templateMaster = context.LapcapDataTemplateMaster.ToList();
            using (var transaction = await context.Database.BeginTransactionAsync())
            {
                try
                {
                    var oldLapcapData = this.context.LapcapDataMaster.Where(x => x.EffectiveTo == null).ToList();
                    oldLapcapData.ForEach(x => { x.EffectiveTo = DateTime.Now; });

                    var lapcapDataMaster = new LapcapDataMaster
                    {
                        CreatedAt = DateTime.Now,
                        CreatedBy = userName,
                        EffectiveFrom = DateTime.Now,
                        EffectiveTo = null,
                        LapcapFileName = request.LapcapFileName,
                        ProjectionYear = request.ParameterYear
                    };
                    await this.context.LapcapDataMaster.AddAsync(lapcapDataMaster);

                    foreach (var templateValue in request.LapcapDataTemplateValues)
                    {
                        var uniqueReference = templateMaster.Single(x =>
                            x.Material == templateValue.Material && x.Country == templateValue.CountryName).UniqueReference;

                        await this.context.LapcapDataDetail.AddAsync(new LapcapDataDetail
                        {
                            TotalCost = decimal.Parse(templateValue.TotalCost.Replace("£", string.Empty)),
                            UniqueReference = uniqueReference,
                            LapcapDataMaster = lapcapDataMaster
                        });
                    }
                    await this.context.SaveChangesAsync();
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

        /// <summary>
        /// Retrieves LAPCAP data for a specified year.
        /// </summary>
        /// <param name="parameterYear">The year for which to retrieve LAPCAP data.</param>
        /// <returns>
        /// An IActionResult containing the LAPCAP data for the specified year, or an appropriate error message:
        /// - 400 Bad Request if the model state is invalid.
        /// - 404 Not Found if no data is available for the specified year.
        /// - 500 Internal Server Error if an exception occurs during data retrieval.
        /// </returns>
        /// <response code="200">Returns the LAPCAP data for the specified year.</response>
        /// <response code="400">If the model state is invalid.</response>
        /// <response code="404">If no data is available for the specified year.</response>
        /// <response code="500">If an internal server error occurs.</response>
        [HttpGet]
        [Route("lapcapData/{parameterYear}")]
        [Authorize()]
        public async Task<IActionResult> Get([FromRoute] string parameterYear)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ModelState.Values.SelectMany(x => x.Errors));
            }

            var lapcapDataMaster = await context.LapcapDataMaster
              .Include(m => m.Details)
              .SingleOrDefaultAsync(m => m.EffectiveTo == null && m.ProjectionYear == parameterYear);

            if (lapcapDataMaster == null)
            {
                return new ObjectResult("No data available for the specified year. Please check the year and try again.") { StatusCode = StatusCodes.Status404NotFound };
            }

            try
            {
                var _lapcaptemplateDetails = await this.context.LapcapDataTemplateMaster.ToListAsync();
                var lapcapdatavalues = LapcapDataParameterSettingMapper.Map(lapcapDataMaster, _lapcaptemplateDetails);
                return new ObjectResult(lapcapdatavalues) { StatusCode = StatusCodes.Status200OK };
            }
            catch (Exception exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, exception);
            }
        }
    }
}