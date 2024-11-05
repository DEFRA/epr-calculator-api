using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Wrapper;

namespace EPR.Calculator.API.Services
{

    public class TransposePomAndOrgDataService : ITransposePomAndOrgDataService
    {
        private readonly ApplicationDBContext context;
        private readonly IOrgAndPomWrapper wrapper;

        public TransposePomAndOrgDataService(ApplicationDBContext context, IOrgAndPomWrapper wrapper)
        {
            this.context = context;
            this.wrapper = wrapper;
        }

        public void Transpose(int runId)
        {
            var materials = this.context.Material.ToList();

            var calculatorRun = this.context.CalculatorRuns.Single(cr => cr.Id == runId);

            if (calculatorRun.CalculatorRunPomDataMasterId != null)
            {
                using (var transaction = this.context.Database.BeginTransaction())
                {
                    try
                    {
                        var producerDetails = new List<ProducerDetail>();
                        var producerReportedMaterials = new List<ProducerReportedMaterial>();

                        var allPomDataDetails = this.wrapper.GetPomData();

                        //John To add
                        var pomDataDetails = allPomDataDetails;

                        foreach (var pom in pomDataDetails)
                        {
                            if (pom.OrganisationId.HasValue)
                            {
                                var organisationDataMaster = context.CalculatorRunOrganisationDataMaster.Single(odm => odm.Id == calculatorRun.CalculatorRunOrganisationDataMasterId);

                                var organisationDataDetails = context.CalculatorRunOrganisationDataDetails.Where
                                    (
                                        odd => odd.CalculatorRunOrganisationDataMasterId == organisationDataMaster.Id
                                    ).ToList();

                                var producerName = organisationDataDetails.Single(odd => odd.OrganisationId == pom.OrganisationId)?.OrganisationName;

                                if (producerName != null)
                                {
                                    var producerDetail = new ProducerDetail
                                    {
                                        CalculatorRunId = runId,
                                        ProducerId = pom.OrganisationId.Value,
                                        SubsidiaryId = pom.SubsidaryId,
                                        ProducerName = producerName,
                                        CalculatorRun = calculatorRun
                                    };

                                    producerDetails.Add(producerDetail);

                                    context.ProducerDetail.Add(producerDetail);

                                    var material = materials.Single(m => m.Code == pom.PackagingMaterial);

                                    var producerReportedMaterial = new ProducerReportedMaterial
                                    {
                                        MaterialId = material.Id,
                                        Material = material,
                                        ProducerDetailId = producerDetail.Id,
                                        ProducerDetail = producerDetail,
                                        PackagingType = pom.PackagingType,
                                        PackagingTonnage = (decimal)pom.PackagingMaterialWeight / 1000,
                                    };

                                    producerReportedMaterials.Add(producerReportedMaterial);
                                }
                            }
                        }

                        context.ProducerReportedMaterial.AddRange(producerReportedMaterials);
                        context.SaveChanges();

                        transaction.Commit();
                    }
                    catch (Exception exception)
                    {
                        // Error, rollback transaction
                        transaction.Rollback();
                    }
                }
            }
        }
    }
}
