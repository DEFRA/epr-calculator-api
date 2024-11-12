using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;

namespace EPR.Calculator.API.Services
{

    public class TransposePomAndOrgDataService : ITransposePomAndOrgDataService
    {
        private readonly ApplicationDBContext context;

        public TransposePomAndOrgDataService(ApplicationDBContext context)
        {
            this.context = context;
        }

        public void Transpose(CalcResultsRequestDto resultsRequestDto)
        {
            var materials = this.context.Material.ToList();

            var calculatorRun = this.context.CalculatorRuns.Single(cr => cr.Id == resultsRequestDto.RunId);

            if (calculatorRun.CalculatorRunPomDataMasterId != null)
            {
                using (var transaction = this.context.Database.BeginTransaction())
                {
                    try
                    {
                        // Get the calculator run organisation data master record based on the CalculatorRunOrganisationDataMasterId
                        // from the calculator run table
                        var organisationDataMaster = context.CalculatorRunOrganisationDataMaster.Single(odm => odm.Id == calculatorRun.CalculatorRunOrganisationDataMasterId);

                        // Get the calculator run organisation data details as we need the organisation name
                        var organisationDataDetails = context.CalculatorRunOrganisationDataDetails.Where
                            (
                                odd => odd.CalculatorRunOrganisationDataMasterId == organisationDataMaster.Id
                            ).ToList();

                        // Get the calculator run pom data master record based on the CalculatorRunPomDataMasterId
                        var pomDataMaster = context.CalculatorRunPomDataMaster.Single(pdm => pdm.Id == calculatorRun.CalculatorRunPomDataMasterId);

                        foreach (var organisation in organisationDataDetails)
                        {
                            // Initialise the producerReportedMaterials
                            var producerReportedMaterials = new List<ProducerReportedMaterial>();

                            // Get the calculator run pom data details related to the calculator run pom data master
                            var calculatorRunPomDataDetails = context.CalculatorRunPomDataDetails.Where
                                (
                                    pdd => pdd.CalculatorRunPomDataMasterId == pomDataMaster.Id && pdd.OrganisationId == organisation.OrganisationId
                                ).ToList();

                            // Proceed further only if there is any pom data based on the pom data master id and organisation id
                            // TO DO: We have to record if there is no pom data in a separate table post Dec 2024
                            if (calculatorRunPomDataDetails.Count > 0)
                            {
                                var organisations = organisationDataDetails.Where(odd => odd.OrganisationName == organisation.OrganisationName).OrderByDescending(odd => odd.SubmissionPeriodDesc);

                                // Get the producer based on the latest submission period
                                var producer = organisations.FirstOrDefault();

                                // Proceed further only if the organisation is not null and organisation id not null
                                // TO DO: We have to record if the organisation name is null in a separate table post Dec 2024
                                if (producer != null && producer.OrganisationId != null)
                                {
                                    var producerDetail = new ProducerDetail
                                    {
                                        CalculatorRunId = resultsRequestDto.RunId,
                                        ProducerId = producer.OrganisationId.Value,
                                        SubsidiaryId = producer.SubsidaryId,
                                        ProducerName = producer.OrganisationName,
                                        CalculatorRun = calculatorRun
                                    };

                                    // Add producer detail record to the database context
                                    context.ProducerDetail.Add(producerDetail);

                                    // Loop through the calculator run pom data details and
                                    // populate the producerDetails and producerReportedMaterials
                                    foreach (var pom in calculatorRunPomDataDetails)
                                    {
                                        // Proceed further only if the organisation id has a value
                                        // TO DO: We have to record if the organisation id is null in a separate table post Dec 2024
                                        if (pom.OrganisationId.HasValue)
                                        {
                                            var material = materials.Single(m => m.Code == pom.PackagingMaterial);

                                            // Proceed further only if the packaging type and packaging material weight is not null
                                            // TO DO: We have to record if the packaging type or packaging material weight is null in a separate table post Dec 2024
                                            if (pom.PackagingType != null && pom.PackagingMaterialWeight != null)
                                            {
                                                var producerReportedMaterial = new ProducerReportedMaterial
                                                {
                                                    MaterialId = material.Id,
                                                    Material = material,
                                                    ProducerDetail = producerDetail,
                                                    PackagingType = pom.PackagingType,
                                                    PackagingTonnage = (decimal)pom.PackagingMaterialWeight / 1000,
                                                };

                                                // Populate the producer reported material list
                                                producerReportedMaterials.Add(producerReportedMaterial);
                                            }
                                        }
                                    }

                                    // Add the list of producer reported materials to the database context
                                    context.ProducerReportedMaterial.AddRange(producerReportedMaterials);
                                }
                            }
                        }

                        // Apply the database changes
                        context.SaveChanges();

                        // Success, commit transaction
                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        // Error, rollback transaction
                        transaction.Rollback();
                        // TO DO: Decide upon the exception later during the complete integration
                        throw;
                    }
                }
            }
        }
    }
}
