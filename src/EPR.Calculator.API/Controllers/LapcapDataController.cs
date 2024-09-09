using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Mappers;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.API.Controllers
{
    public class LapcapDataController : ControllerBase
    {
        private readonly ApplicationDBContext context;

        public LapcapDataController(ApplicationDBContext context)
        {
            this.context = context;
        }

        [HttpPost]
        [Route("api/lapcapData")]
        public IActionResult Create([FromBody] CreateLapcapDataDto request)
        {
            using (var transaction = context.Database.BeginTransaction())
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
                        Year = request.ParameterYear
                    };
                    this.context.LapcapDataMaster.Add(lapcapDataMaster);

                    foreach (var templateValue in request.LapcapDataTemplateValues)
                    {
                        this.context.LapcapDataDetail.Add(new LapcapDataDetail
                        {
                            TotalCost = decimal.Parse(templateValue.TotalCost.Replace("£", string.Empty)),
                            UniqueReference = templateValue.UniqueReference,
                            LapcapDataMaster = lapcapDataMaster
                        });
                    }
                    this.context.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception exception)
                {
                    transaction.Rollback();
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
        [Route("api/lapcapData/{parameterYear}")]
        public IActionResult Get([FromRoute] string parameterYear)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ModelState.Values.SelectMany(x => x.Errors));
            }

            var currentDefaultSetting = this.context.LapcapDataMaster
                .SingleOrDefault(x => x.EffectiveTo == null && x.Year == parameterYear);

            if (currentDefaultSetting == null)
            {
                return new ObjectResult("No data available for the specified year. Please check the year and try again.") { StatusCode = StatusCodes.Status404NotFound };
            }

            try
            {
                var _lapcappramSettingDetails = this.context.LapcapDataDetail.ToList();
                var _lapcaptemplateDetails = this.context.LapcapDataTemplateMaster;
                var lapcapdatavalues = LAPCAPParameterSettingMapper.Map(currentDefaultSetting, _lapcaptemplateDetails);
                return new ObjectResult(lapcapdatavalues) { StatusCode = StatusCodes.Status200OK };
            }
            catch (Exception exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, exception);
            }

        }
    }
}