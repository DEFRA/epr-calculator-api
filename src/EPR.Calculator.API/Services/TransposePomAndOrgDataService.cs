using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using Microsoft.AspNetCore.Routing.Constraints;

namespace EPR.Calculator.API.Services
{

    public class TransposePomAndOrgDataService : ITransposePomAndOrgDataService
    {
        private readonly ApplicationDBContext context;

        private const string PeriodSeparator = "-P";

        public class OrganisationDetails
        {
            public int? OrganisationId { get; set; }
            public required string OrganisationName { get; set; }
            public string? SubmissionPeriod { get; set; }
            public string? SubmissionPeriodDescription { get; set; }

            public string? SubsidaryId { get; set; }

        }

        internal class SubmissionDetails
        {
            public string? SubmissionPeriod { get; set; }
            public string? SubmissionPeriodDesc { get; set; }
        }


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
                // Get the calculator run organisation data master record based on the CalculatorRunOrganisationDataMasterId
                // from the calculator run table
                var organisationDataMaster = context.CalculatorRunOrganisationDataMaster.Single(odm => odm.Id == calculatorRun.CalculatorRunOrganisationDataMasterId);

              var SubmissionPeriodDetails = (from s in context.CalculatorRunPomDataDetails
                                           where s.CalculatorRunPomDataMasterId == calculatorRun.CalculatorRunPomDataMasterId
                                           select new SubmissionDetails
                                           {
                                               SubmissionPeriod = s.SubmissionPeriod,
                                               SubmissionPeriodDesc = s.SubmissionPeriodDesc
                                           }
                                        ).Distinct().ToList();
               
             var OrganisationsList  = GetAllOrganisationsBasedonRunId(resultsRequestDto.RunId);

              var  OrganisationsBySubmissionPeriod = GetOrganisationDetailsBySubmissionPeriod(OrganisationsList, SubmissionPeriodDetails).ToList();

                // Get the calculator run organisation data details as we need the organisation name
                var organisationDataDetails = context.CalculatorRunOrganisationDataDetails
                    .Where(odd => odd.CalculatorRunOrganisationDataMasterId == organisationDataMaster.Id && odd.OrganisationName != null && odd.OrganisationName !="")
                    .OrderBy(odd => odd.OrganisationName)
                    .GroupBy(odd => new { odd.OrganisationId, odd.SubsidaryId })
                    .Select(odd => odd.First())
                    .ToList();




                // Get the calculator run pom data master record based on the CalculatorRunPomDataMasterId
                var pomDataMaster = context.CalculatorRunPomDataMaster.Single(pdm => pdm.Id == calculatorRun.CalculatorRunPomDataMasterId);

                using (var transaction = this.context.Database.BeginTransaction())
                {
                    try
                    {
                        foreach (var organisation in organisationDataDetails.Where(t=>!string.IsNullOrWhiteSpace(t.OrganisationName)))
                        {
                            // Initialise the producerReportedMaterials
                            var producerReportedMaterials = new List<ProducerReportedMaterial>();

                            // Get the calculator run pom data details related to the calculator run pom data master
                            var calculatorRunPomDataDetails = context.CalculatorRunPomDataDetails.Where
                                (
                                    pdd => pdd.CalculatorRunPomDataMasterId == pomDataMaster.Id &&
                                    pdd.OrganisationId == organisation.OrganisationId &&
                                    pdd.SubsidaryId == organisation.SubsidaryId
                                ).ToList();

                            // Proceed further only if there is any pom data based on the pom data master id and organisation id
                            // TO DO: We have to record if there is no pom data in a separate table post Dec 2024
                            if (calculatorRunPomDataDetails.Count > 0)
                            {
                                var organisations = organisationDataDetails.Where(odd => odd.OrganisationName == organisation.OrganisationName  && odd.SubsidaryId == organisation.SubsidaryId).OrderByDescending(odd => odd.SubmissionPeriodDesc);

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
                                        ProducerName = string.IsNullOrWhiteSpace(producer.SubsidaryId) ? GetLatestOrganisationName(producer.OrganisationId.Value, OrganisationsBySubmissionPeriod, OrganisationsList) : GetLatestSubsidaryName(producer.OrganisationId.Value, producer.SubsidaryId, OrganisationsBySubmissionPeriod, OrganisationsList),
                                        CalculatorRun = calculatorRun
                                    };

                                    // Add producer detail record to the database context
                                    context.ProducerDetail.Add(producerDetail);

                                    foreach (var material in materials)
                                    {
                                        var pomDataDetailsByMaterial = calculatorRunPomDataDetails.Where(pdd => pdd.PackagingMaterial == material.Code).GroupBy(pdd => pdd.PackagingType);

                                        foreach (var pomData in pomDataDetailsByMaterial)
                                        {
                                            var pom = pomData.AsEnumerable();
                                            var packagingType = pom.FirstOrDefault()?.PackagingType;
                                            var totalPackagingMaterialWeight = pom.Sum(x => x.PackagingMaterialWeight);

                                            // Proceed further only if the packaging type and packaging material weight is not null
                                            // TO DO: We have to record if the packaging type or packaging material weight is null in a separate table post Dec 2024
                                            if (packagingType != null && totalPackagingMaterialWeight != null)
                                            {
                                                var producerReportedMaterial = new ProducerReportedMaterial
                                                {
                                                    MaterialId = material.Id,
                                                    Material = material,
                                                    ProducerDetail = producerDetail,
                                                    PackagingType = packagingType,
                                                    PackagingTonnage = Math.Round((decimal)(totalPackagingMaterialWeight) / 1000, 3),
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

        private static List<OrganisationDetails> GetOrganisationDetailsBySubmissionPeriod(IEnumerable<OrganisationDetails> organisationsList, IEnumerable<SubmissionDetails> submissionPeriodDetails)
        {
            return (from org in organisationsList
                    join sub in submissionPeriodDetails on
                    org.SubmissionPeriodDescription equals sub.SubmissionPeriodDesc
                    select new OrganisationDetails
                    {
                        OrganisationId = org.OrganisationId,
                        OrganisationName = org.OrganisationName,
                        SubmissionPeriodDescription = org.SubmissionPeriodDescription,
                        SubmissionPeriod = sub.SubmissionPeriod,
                        SubsidaryId = org.SubsidaryId
                    }).ToList();
        }

        public IEnumerable<OrganisationDetails> GetAllOrganisationsBasedonRunId(int runId)
        {
            return [.. (from run in context.CalculatorRuns join
                 org in context.CalculatorRunOrganisationDataDetails on run.CalculatorRunOrganisationDataMasterId equals org.CalculatorRunOrganisationDataMasterId
                    where (run.Id == runId && org.OrganisationName != null)
                    select new OrganisationDetails
                    {
                        OrganisationId = org.OrganisationId,
                        OrganisationName = org.OrganisationName,
                        SubmissionPeriodDescription = org.SubmissionPeriodDesc,
                        SubsidaryId = org.SubsidaryId
                    }).Distinct()];
        }


        public string? GetLatestOrganisationName(int orgId, List<OrganisationDetails> organisationsBySubmissionPeriod, IEnumerable<OrganisationDetails> organisationsList)
        {
            if (organisationsBySubmissionPeriod is null) return string.Empty;

            var organisations = organisationsBySubmissionPeriod.Where(t => t.OrganisationId == orgId && t.SubsidaryId == null).OrderByDescending(t => t.SubmissionPeriod?.Replace(PeriodSeparator, string.Empty)).ToList();

            var orgName = organisations?.FirstOrDefault(t => t.OrganisationId == orgId)?.OrganisationName;
            return string.IsNullOrWhiteSpace(orgName) ? organisationsList.FirstOrDefault(t => t.OrganisationId == orgId)?.OrganisationName : orgName;
        }

        public string? GetLatestSubsidaryName(int orgId, string? subsidaryId, List<OrganisationDetails> organisationsBySubmissionPeriod, IEnumerable<OrganisationDetails> organisationsList)
        {
            if (organisationsBySubmissionPeriod is null) return string.Empty;
            var subsidaries = organisationsBySubmissionPeriod.Where(t => t.OrganisationId == orgId && t.SubsidaryId == subsidaryId).OrderByDescending(t => t.SubmissionPeriod?.Replace(PeriodSeparator, string.Empty)).ToList();
            var subsidaryName = subsidaries?.FirstOrDefault(t => t.OrganisationId == orgId && t.SubsidaryId == subsidaryId)?.OrganisationName;
            return string.IsNullOrWhiteSpace(subsidaryName) ? organisationsList.FirstOrDefault(t => t.OrganisationId == orgId && t.SubsidaryId == subsidaryId)?.OrganisationName : subsidaryName;
        }
    }
}
