using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.API.Services
{
    public static class TransposePomAndOrgDataService
    {
        public static void Transpose(ApplicationDBContext context, int runId)
        {
            var materials = context.Material.ToList();

            var pomDataMaster = context.CalculatorRunPomDataMaster.Single(pdm => pdm.Id == runId);

            var pomDataDetails = context.CalculatorRunPomDataDetails.Where(pdd => pdd.CalculatorRunPomDataMasterId == pomDataMaster.Id).ToList();

            var calculatorRun = context.CalculatorRuns.Single(cr => cr.Id == runId);

            var producerDetails = new List<ProducerDetail>();

            var producerReportedMaterials = new List<ProducerReportedMaterial>();

            using (var transaction = context.Database.BeginTransaction())
            {
                try
                {
                    foreach (var pom in pomDataDetails)
                    {
                        if (pom.OrganisationId != null)
                        {
                            var organisationDataMaster = context.CalculatorRunOrganisationDataMaster.Single(odm => odm.Id == runId);

                            var organisationDataDetails = context.CalculatorRunOrganisationDataDetails.Where(odd => odd.CalculatorRunOrganisationDataMasterId == organisationDataMaster.Id).ToList();

                            var producerName = organisationDataDetails.Single(odd => odd.OrganisationId == pom.OrganisationId).OrganisationName;

                            var producerDetail = new ProducerDetail
                            {
                                CalculatorRunId = runId,
                                ProducerId = pom.OrganisationId,
                                SubsidiaryId = pom.SubsidaryId,
                                ProducerName = producerName,
                                CalculatorRun = calculatorRun
                            };

                            producerDetails.Add(producerDetail);

                            context.ProducerDetail.Add(producerDetail);
                            context.SaveChanges();

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
