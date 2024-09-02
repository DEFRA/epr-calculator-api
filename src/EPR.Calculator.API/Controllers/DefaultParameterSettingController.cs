using api.Mappers;
using api.Validators;
using EPR.Calculator.API.CommandHandlers;
using EPR.Calculator.API.Commands;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System;

namespace EPR.Calculator.API.Controllers
{
    public class DefaultParameterSettingController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        private readonly ICreateDefaultParameterCommandHandler commandHandler;

        public DefaultParameterSettingController(ApplicationDBContext context, ICreateDefaultParameterCommandHandler commandHandler)
        {
            this._context = context;
            this.commandHandler = commandHandler;
        }

        [HttpPost]
        [Route("api/defaultParameterSetting")]
        public IActionResult Create([FromBody] CreateDefaultParameterSettingDto request)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ModelState.Values.SelectMany(x => x.Errors));
            }

            var command = new CreateDefaultParameterCommand
            {
                ParameterYear = request.ParameterYear,
                SchemeParameterTemplateValues = request.SchemeParameterTemplateValues
            };
            try
            {
                this.commandHandler.Handle(command);
            }
            catch (Exception exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, exception);
            }

            return command.IsInvalid ? BadRequest(command.ValidationErrors) :
                new ObjectResult(null) { StatusCode = StatusCodes.Status201Created };
        }

        [HttpGet]
        [Route("api/defaultParameterSetting/{parameterYear}")]
        public IActionResult Get([FromRoute] string parameterYear)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ModelState.Values.SelectMany(x => x.Errors));
            }

            var currentDefaultSetting = _context.DefaultParameterSettings
                .SingleOrDefault(x => x.EffectiveTo == null && x.ParameterYear == parameterYear);

            if (currentDefaultSetting == null)
            {
                return new ObjectResult("No data available for the specified year. Please check the year and try again.") { StatusCode = StatusCodes.Status404NotFound };
            }

            try
            {
                var _pramSettingDetails = _context.DefaultParameterSettingDetail.ToList();
                var _templateDetails = _context.DefaultParameterTemplateMasterList;
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