﻿using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Mappers;
using EPR.Calculator.API.Validators;
using FluentValidation;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Abstractions;
using System;

namespace EPR.Calculator.API.Controllers
{
    [Route("v1")]
    public class DefaultParameterSettingController : ControllerBase
    {
        private readonly ApplicationDBContext context;
        private readonly ICreateDefaultParameterDataValidator validator;
        private readonly TelemetryClient _telemetryClient;

        public DefaultParameterSettingController(
            ApplicationDBContext context,
            ICreateDefaultParameterDataValidator validator,
            TelemetryClient telemetryClient)
        {
            this.context = context;
            this.validator = validator;
            this._telemetryClient = telemetryClient;
        }

        [HttpPost]
        [Route("defaultParameterSetting")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateDefaultParameterSettingDto request)
        {
            this._telemetryClient.TrackTrace($"1.Parameter File Name in DefaultParameter API :{request.ParameterFileName}");
            var claim = this.User.Claims.FirstOrDefault(x => x.Type == "name");
            if (claim == null)
            {
                return new ObjectResult("No claims in the request") { StatusCode = StatusCodes.Status401Unauthorized };
            }

            var userName = claim.Value;
            if (!this.ModelState.IsValid)
            {
                return this.StatusCode(StatusCodes.Status400BadRequest, this.ModelState.Values.SelectMany(x => x.Errors));
            }

            var validationResult = this.validator.Validate(request);
            if (validationResult != null && validationResult.IsInvalid)
            {
                this._telemetryClient.TrackTrace($"2.Parameter File Name in API :{request.ParameterFileName}");
                this._telemetryClient.TrackTrace($"3.Validation errors :{validationResult.Errors}");
                return this.BadRequest(validationResult.Errors);
            }

            using (var transaction = await this.context.Database.BeginTransactionAsync())
            {
                try
                {
                    var oldDefaultSettings = await this.context.DefaultParameterSettings.Where(x => x.EffectiveTo == null && x.ParameterYear.Name == request.ParameterYear).ToListAsync();
                    oldDefaultSettings.ForEach(x => { x.EffectiveTo = DateTime.Now; });

                    var financialYear = await this.context.FinancialYears.Where(
                        x => x.Name == request.ParameterYear).SingleAsync();

                    var defaultParamSettingMaster = new DefaultParameterSettingMaster
                    {
                        CreatedAt = DateTime.Now,
                        CreatedBy = userName,
                        EffectiveFrom = DateTime.Now,
                        EffectiveTo = null,
                        ParameterYear = financialYear,
                        ParameterFileName = request.ParameterFileName,
                    };
                    await this.context.DefaultParameterSettings.AddAsync(defaultParamSettingMaster);

                    var defaultParameterSettingDetails = request.SchemeParameterTemplateValues
                    .Select(templateValue => new DefaultParameterSettingDetail
                    {
                        ParameterValue = decimal.Parse(templateValue.ParameterValue.TrimEnd('%').Replace("£", string.Empty)),
                        ParameterUniqueReferenceId = templateValue.ParameterUniqueReferenceId,
                        DefaultParameterSettingMaster = defaultParamSettingMaster,
                    })
                    .ToList();

                    await this.context.DefaultParameterSettingDetail.AddRangeAsync(defaultParameterSettingDetails);

                    await this.context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception exception)
                {
                    await transaction.RollbackAsync();
                    this._telemetryClient.TrackTrace($"4.500InternalServerError Exception :{exception}");
                    return this.StatusCode(StatusCodes.Status500InternalServerError, exception);
                }
            }

            return new ObjectResult(null) { StatusCode = StatusCodes.Status201Created };
        }

        [HttpGet]
        [Route("defaultParameterSetting/{parameterYear}")]
        [ProducesResponseType(typeof(List<DefaultSchemeParametersDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get([FromRoute] string parameterYear)
        {
            if (!this.ModelState.IsValid)
            {
                return this.StatusCode(StatusCodes.Status400BadRequest, this.ModelState.Values.SelectMany(x => x.Errors));
            }

            try
            {
                var financialYear = await this.context.FinancialYears.Where(x => x.Name == parameterYear).SingleOrDefaultAsync();
                if (financialYear == null)
                {
                    return new ObjectResult("No data available for the specified year. Please check the year and try again.") { StatusCode = StatusCodes.Status400BadRequest };
                }

                var currentDefaultSetting = await this.context.DefaultParameterSettings
                    .SingleOrDefaultAsync(x => x.EffectiveTo == null && x.ParameterYear.Name == financialYear.Name);

                if (currentDefaultSetting == null)
                {
                    return new ObjectResult("No data available for the specified year. Please check the year and try again.") { StatusCode = StatusCodes.Status404NotFound };
                }

                var pramSettingDetails = await this.context.DefaultParameterSettingDetail
                    .Where(x => x.DefaultParameterSettingMasterId == currentDefaultSetting.Id)
                    .ToListAsync();

                var templateDetails = await this.context.DefaultParameterTemplateMasterList.ToListAsync();

                var schemeParameters = CreateDefaultParameterSettingMapper.Map(currentDefaultSetting, templateDetails);
                return new ObjectResult(schemeParameters) { StatusCode = StatusCodes.Status200OK };
            }
            catch (Exception exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, exception);
            }
        }
    }
}