using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.Service.Function.Services
{
    // TODO rename this - not just projected producers
    public interface IProjectedProducersService
    {
        Task StoreProjectedProducers(List<L1Producer> producerDetails);
    }

    public class ProjectedProducersService(
        ApplicationDBContext dbContext,
        IBulkOperations bulkOps)
        : IProjectedProducersService
    {
        public async Task StoreProjectedProducers(List<L1Producer> producerDetails)
        {
            var producerProjected =
                producerDetails
                    .SelectMany(p => p.Producers)
                    .SelectMany(p => p.ProducerReportedMaterials.Select(rm =>
                        new ProducerReportedMaterialProjected
                        {
                            ProducerDetailId             = rm.ProducerDetailId,
                            MaterialId                   = rm.MaterialId,
                            SubmissionPeriod             = rm.SubmissionPeriod,
                            PackagingType                = rm.PackagingType,
                            PackagingTonnage             = rm.PackagingTonnage,
                            PackagingTonnageRed          = rm.PackagingTonnageRed,
                            PackagingTonnageAmber        = rm.PackagingTonnageAmber,
                            PackagingTonnageGreen        = rm.PackagingTonnageGreen,
                            PackagingTonnageRedMedical   = rm.PackagingTonnageRedMedical,
                            PackagingTonnageAmberMedical = rm.PackagingTonnageAmberMedical,
                            PackagingTonnageGreenMedical = rm.PackagingTonnageGreenMedical
                        }
                    )
                ).ToList();

            await bulkOps.BulkInsertAsync(dbContext, producerProjected);
        }
    }
}
