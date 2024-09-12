using api.Validators;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using Microsoft.AspNetCore.Mvc;

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
        public IActionResult Create([FromBody] CreateLapcapDataDto request)
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
                        var uniqueReference = templateMaster.Single(x =>
                            x.Material == templateValue.Material && x.Country == templateValue.CountryName).UniqueReference;

                        this.context.LapcapDataDetail.Add(new LapcapDataDetail
                        {
                            TotalCost = decimal.Parse(templateValue.TotalCost.Replace("£", string.Empty)),
                            UniqueReference = uniqueReference,
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
    }
}
