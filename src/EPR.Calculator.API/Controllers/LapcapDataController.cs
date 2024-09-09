using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
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
    }
}
