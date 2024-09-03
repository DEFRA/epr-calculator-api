using api.Mappers;
using EPR.Calculator.API.CommandHandlers;
using EPR.Calculator.API.Commands;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Queries;
using EPR.Calculator.API.QueryHandlers;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.API.Controllers
{
    public class DefaultParameterSettingController : ControllerBase
    {
        private readonly ApplicationDBContext context;
        private readonly ICommandHandler<CreateDefaultParameterCommand> commandHandler;
        private readonly IQueryHandler<DefaultParameterSettingDetailQuery, IEnumerable<DefaultSchemeParametersDto>> queryHandler;

        public DefaultParameterSettingController(ApplicationDBContext context,
            ICommandHandler<CreateDefaultParameterCommand> commandHandler,
            IQueryHandler<DefaultParameterSettingDetailQuery, IEnumerable<DefaultSchemeParametersDto>> queryHandler)
        {
            this.context = context;
            this.commandHandler = commandHandler;
            this.queryHandler = queryHandler;
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

            try
            {
                var query = new DefaultParameterSettingDetailQuery(parameterYear);
                var schemeParameters = this.queryHandler.Query(query);
                return schemeParameters.Any() ?
                    new ObjectResult(schemeParameters) { StatusCode = StatusCodes.Status200OK } :
                    new ObjectResult("No data available for the specified year. Please check the year and try again.") { StatusCode = StatusCodes.Status404NotFound };
            }
            catch (Exception exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, exception);
            }
        }
    }
}