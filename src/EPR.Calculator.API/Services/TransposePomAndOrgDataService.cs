using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using Microsoft.EntityFrameworkCore;
using System.Data;

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

        public async Task<bool> Transpose(CalcResultsRequestDto resultsRequestDto, CancellationToken cancellationToken)
        {
            context.ChangeTracker.AutoDetectChangesEnabled = false;
            var newProducerDetails = new List<ProducerDetail>();
            var newProducerReportedMaterials = new List<ProducerReportedMaterial>();

            var result = false;
            var materials = await this.context.Material.ToListAsync(cancellationToken);

            var calculatorRun = await context.CalculatorRuns
                .Where(x => x.Id == resultsRequestDto.RunId)
                .SingleAsync(cancellationToken);
            var calculatorRunPomDataDetails = await context.CalculatorRunPomDataDetails
                .Where(x => x.CalculatorRunPomDataMasterId == calculatorRun.CalculatorRunPomDataMasterId)
                .OrderBy(x => x.SubmissionPeriodDesc)
                .ToListAsync(cancellationToken);
            var calculatorRunOrgDataDetails = await context.CalculatorRunOrganisationDataDetails
                .Where(x => x.CalculatorRunOrganisationDataMasterId == calculatorRun.CalculatorRunOrganisationDataMasterId)
                .OrderBy(x => x.SubmissionPeriodDesc)
                .ToListAsync(cancellationToken);

            if (calculatorRun.CalculatorRunPomDataMasterId != null)
            {
                var organisationDataMaster = await context.CalculatorRunOrganisationDataMaster
                    .SingleAsync(x => x.Id == calculatorRun.CalculatorRunOrganisationDataMasterId, cancellationToken);

                var SubmissionPeriodDetails = (from s in calculatorRunPomDataDetails
                                               where s.CalculatorRunPomDataMasterId == calculatorRun.CalculatorRunPomDataMasterId
                                               select new SubmissionDetails
                                               {
                                                   SubmissionPeriod = s.SubmissionPeriod,
                                                   SubmissionPeriodDesc = s.SubmissionPeriodDesc
                                               }
                                        ).Distinct().ToList();

                var OrganisationsList = GetAllOrganisationsBasedonRunId(calculatorRunOrgDataDetails);

                var OrganisationsBySubmissionPeriod = GetOrganisationDetailsBySubmissionPeriod(OrganisationsList, SubmissionPeriodDetails).ToList();

                var organisationDataDetails = calculatorRunOrgDataDetails
                    .Where(odd => odd.CalculatorRunOrganisationDataMasterId == organisationDataMaster.Id && odd.OrganisationName != null && odd.OrganisationName != "")
                    .OrderBy(odd => odd.OrganisationName)
                    .GroupBy(odd => new { odd.OrganisationId, odd.SubsidaryId })
                    .Select(odd => odd.First())
                    .ToList();

                // Get the calculator run pom data master record based on the CalculatorRunPomDataMasterId
                var pomDataMaster = await context.CalculatorRunPomDataMaster
                    .SingleAsync(x => x.Id == calculatorRun.CalculatorRunPomDataMasterId, cancellationToken);


                foreach (var organisation in organisationDataDetails.Where(t => !string.IsNullOrWhiteSpace(t.OrganisationName)))
                {
                    // Initialise the producerReportedMaterials
                    var producerReportedMaterials = new List<ProducerReportedMaterial>();

                    // Get the calculator run pom data details related to the calculator run pom data master
                    var runPomDataDetailsForSubsidaryId = calculatorRunPomDataDetails.Where
                        (
                            pdd => pdd.CalculatorRunPomDataMasterId == pomDataMaster.Id &&
                            pdd.OrganisationId == organisation.OrganisationId &&
                            pdd.SubsidaryId == organisation.SubsidaryId
                        ).ToList();

                    // Proceed further only if there is any pom data based on the pom data master id and organisation id
                    // TO DO: We have to record if there is no pom data in a separate table post Dec 2024
                    if (runPomDataDetailsForSubsidaryId.Count > 0)
                    {
                        var organisations = organisationDataDetails.Where(odd => odd.OrganisationName == organisation.OrganisationName && odd.SubsidaryId == organisation.SubsidaryId).OrderByDescending(odd => odd.SubmissionPeriodDesc);

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
                            newProducerDetails.Add(producerDetail);

                            foreach (var material in materials)
                            {
                                var pomDataDetailsByMaterial = runPomDataDetailsForSubsidaryId.Where(pdd => pdd.PackagingMaterial == material.Code).GroupBy(pdd => pdd.PackagingType);

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
                            newProducerReportedMaterials.AddRange(producerReportedMaterials);
                        }
                    }
                }

                result = await BulkCopyAsync(newProducerDetails, newProducerReportedMaterials, resultsRequestDto.RunId);
            }
            return result;
        }

        public async Task<bool> BulkCopyAsync(
            IEnumerable<ProducerDetail> newProducerDetails,
            IEnumerable<ProducerReportedMaterial> newProducerReportedMaterials,
            int calculatorRunId)
        {
            var connectionString = this.context.Database.GetConnectionString();
            var producerDetailTable = MakeproducerDetailTable(newProducerDetails, calculatorRunId);

            using (var bulkCopy = new Microsoft.Data.SqlClient.SqlBulkCopy(connectionString,
                Microsoft.Data.SqlClient.SqlBulkCopyOptions.Default))
            {
                bulkCopy.DestinationTableName = "dbo.producer_detail";

                try
                {
                    await bulkCopy.WriteToServerAsync(producerDetailTable, DataRowState.Unchanged);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return false;
                }
            }

            var producerReportedMaterialsTable = MakeproducerReportedTable(newProducerReportedMaterials);
            using (var bulkCopy = new Microsoft.Data.SqlClient.SqlBulkCopy(
                connectionString,
                Microsoft.Data.SqlClient.SqlBulkCopyOptions.Default))
            {
                bulkCopy.DestinationTableName = "dbo.producer_reported_material";

                try
                {
                    await bulkCopy.WriteToServerAsync(producerReportedMaterialsTable, DataRowState.Unchanged);
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return false;
                }
            }
        }

        private DataTable MakeproducerReportedTable(IEnumerable<ProducerReportedMaterial> producerReportedMaterials)
        {
            DataTable producerDetails = new DataTable("producer_detail");

            DataColumn id = new DataColumn();
            id.DataType = Type.GetType("System.Int32");
            id.ColumnName = "id";
            id.AutoIncrement = true;
            producerDetails.Columns.Add(id);

            DataColumn materialId = new DataColumn();
            materialId.DataType = Type.GetType("System.Int32");
            materialId.ColumnName = "material_id";
            producerDetails.Columns.Add(materialId);

            DataColumn producerDetailId = new DataColumn();
            producerDetailId.DataType = Type.GetType("System.Int32");
            producerDetailId.ColumnName = "producer_detail_id";
            producerDetails.Columns.Add(producerDetailId);

            DataColumn packagingType = new DataColumn();
            packagingType.DataType = Type.GetType("System.String");
            packagingType.ColumnName = "packaging_type";
            producerDetails.Columns.Add(packagingType);

            DataColumn packagingTonnage = new DataColumn();
            packagingTonnage.DataType = Type.GetType("System.Decimal");
            packagingTonnage.ColumnName = "packaging_tonnage";
            producerDetails.Columns.Add(packagingTonnage);

            foreach (var producerDetail in producerReportedMaterials)
            {
                DataRow row = producerDetails.NewRow();
                row["id"] = producerDetail.Id;
                row["material_id"] = producerDetail.MaterialId;
                row["producer_detail_id"] = producerDetail.ProducerDetailId;
                row["packaging_type"] = producerDetail.PackagingType;
                row["packaging_tonnage"] = producerDetail.PackagingTonnage;
                producerDetails.Rows.Add(row);
            }

            producerDetails.AcceptChanges();

            // Return the new DataTable.
            return producerDetails;
        }

        private DataTable MakeproducerDetailTable(IEnumerable<ProducerDetail> newProducerDetails, int calculatorRunId)
        {
            DataTable producerDetails = new DataTable("producer_detail");

            DataColumn id = new DataColumn();
            id.DataType = Type.GetType("System.Int32");
            id.ColumnName = "id";
            id.AutoIncrement = true;
            producerDetails.Columns.Add(id);

            DataColumn producerId = new DataColumn();
            producerId.DataType = Type.GetType("System.Int32");
            producerId.ColumnName = "producer_id";
            producerDetails.Columns.Add(producerId);

            DataColumn subsidaryId = new DataColumn();
            subsidaryId.DataType = Type.GetType("System.String");
            subsidaryId.ColumnName = "subsidiary_id";
            producerDetails.Columns.Add(subsidaryId);

            DataColumn producerName = new DataColumn();
            producerName.DataType = Type.GetType("System.String");
            producerName.ColumnName = "producer_name";
            producerDetails.Columns.Add(producerName);

            DataColumn calcRunId = new DataColumn();
            calcRunId.DataType = Type.GetType("System.Int32");
            calcRunId.ColumnName = "calculator_run_id";
            producerDetails.Columns.Add(calcRunId);

            foreach (var producerDetail in newProducerDetails)
            {
                DataRow row = producerDetails.NewRow();
                row["id"] = producerDetail.Id;
                row["producer_id"] = producerDetail.ProducerId;
                row["subsidiary_id"] = producerDetail.SubsidiaryId;
                row["producer_name"] = producerDetail.ProducerName;
                row["calculator_run_id"] = calculatorRunId;
                producerDetails.Rows.Add(row);
            }

            producerDetails.AcceptChanges();

            // Return the new DataTable.
            return producerDetails;
        }

        private static List<OrganisationDetails> GetOrganisationDetailsBySubmissionPeriod(
            IEnumerable<OrganisationDetails> organisationsList,
            IEnumerable<SubmissionDetails> submissionPeriodDetails)
        {
            var list = new List<OrganisationDetails>();
            foreach (var org in organisationsList)
            {
                if (submissionPeriodDetails.Any(x => x.SubmissionPeriodDesc == org.SubmissionPeriodDescription))
                {
                    var sub = submissionPeriodDetails.First(x => x.SubmissionPeriodDesc == org.SubmissionPeriodDescription);
                    list.Add(new OrganisationDetails
                    {
                        OrganisationId = org.OrganisationId,
                        OrganisationName = org.OrganisationName,
                        SubmissionPeriodDescription = org.SubmissionPeriodDescription,
                        SubmissionPeriod = sub.SubmissionPeriod,
                        SubsidaryId = org.SubsidaryId
                    });
                }
            }
            return list;
        }

        public IEnumerable<OrganisationDetails> GetAllOrganisationsBasedonRunId(
            IEnumerable<CalculatorRunOrganisationDataDetail> calculatorRunOrganisationDataDetails)
        {
            return calculatorRunOrganisationDataDetails.Where(org => org.OrganisationName != null)
                .Select(org =>
                    new OrganisationDetails
                    {
                        OrganisationId = org.OrganisationId,
                        OrganisationName = org.OrganisationName,
                        SubmissionPeriodDescription = org.SubmissionPeriodDesc,
                        SubsidaryId = org.SubsidaryId
                    }).Distinct();
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
