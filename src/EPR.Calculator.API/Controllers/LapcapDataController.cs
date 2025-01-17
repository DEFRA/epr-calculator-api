﻿using EPR.Calculator.API.Validators;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Mappers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.Controllers
{
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
        public async Task<IActionResult> Create([FromBody] CreateLapcapDataDto request)
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
                        CreatedBy = "Testuser",
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
        public async Task<IActionResult> Get([FromRoute] string parameterYear)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ModelState.Values.SelectMany(x => x.Errors));
            }

            try
            {
                var currentDefaultSetting = await this.context.LapcapDataMaster
                    .SingleOrDefaultAsync(setting => setting.EffectiveTo == null && setting.ProjectionYear == parameterYear);

                if (currentDefaultSetting == null)
                {
                    return new ObjectResult("No data available for the specified year. Please check the year and try again.") { StatusCode = StatusCodes.Status404NotFound };
                }
            
                var lapcapTemplateDetails = await this.context.LapcapDataTemplateMaster.ToListAsync();
                var lapcapDataValues = LapcapDataParameterSettingMapper.Map(currentDefaultSetting, lapcapTemplateDetails);
                return new ObjectResult(lapcapDataValues) { StatusCode = StatusCodes.Status200OK };
            }
            catch (Exception exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, exception);
            }
        }
    }
}