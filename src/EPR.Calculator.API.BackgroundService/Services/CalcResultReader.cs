using EPR.Calculator.API.BackgroundService.Constants;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.ScaledupProducers;
using EPR.Calculator.API.BackgroundService.Models;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Utils;
using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using EPR.Calculator.API.BackgroundService.Builder.ScaledupProducers;

namespace EPR.Calculator.API.BackgroundService.Services
{
    public interface ICalcResultReader
    {
        Task<ImmutableList<CalcResultH1ProjectedProducer>> ReadH1ProjectedData(int runId, CancellationToken cancellationToken);
        Task<ImmutableList<CalcResultH2ProjectedProducer>> ReadH2ProjectedData(int runId, CancellationToken cancellationToken);
        Task<ImmutableList<CalcResultScaledupProducer>> ReadScaledData(int runId, CancellationToken cancellationToken);
        Task<ImmutableList<CalcResultPartialObligation>> ReadPartialData(int runId, CancellationToken cancellationToken);
        Task<FeeDetail> ReadProducerFeesTotal(int runId, CancellationToken cancellationToken);

        // Deferred - the query runs as the caller enumerates, so a downstream consumer streaming
        // straight to a CSV/JSON writer never holds every producer's fee graph in memory at once.
        IEnumerable<FeeDetail> StreamProducerFeeDetails(int runId);
        Task<IReadOnlyDictionary<string, SelfManagedConsumerWasteData>> ReadSmcwByMaterial(int runId, CancellationToken cancellationToken);
        Task<ModulationResult> ReadModulationResult(int runId, CancellationToken cancellationToken);
        Task<CalcResultLapcapData> ReadLapcapData(int runId, CancellationToken cancellationToken);
        Task<CalcResultCommsCost> ReadCommsCost(int runId, CancellationToken cancellationToken);
        Task<CalcResultLateReportingTonnage> ReadLateReportingTonnage(int runId, CancellationToken cancellationToken);
        Task<CalcResultParameterOtherCost> ReadParameterOtherCost(int runId, CancellationToken cancellationToken);
        Task<CalcResultOnePlusFourApportionment> ReadOnePlusFourApportionment(int runId, CancellationToken cancellationToken);
        Task<CalcResultLaDisposalCostData> ReadLaDisposalCostData(int runId, CancellationToken cancellationToken);
        Task<ImmutableList<CalcResultCancelledProducer>> ReadCancelledProducers(int runId, CancellationToken cancellationToken);
    }

    public class CalcResultReader(ApplicationDBContext dbContext, IMaterialService materialService) : ICalcResultReader
    {
        [ActivityTrace]
        public async Task<ImmutableList<CalcResultH1ProjectedProducer>> ReadH1ProjectedData(int runId, CancellationToken cancellationToken)
        {
            // Group in memory, not SQL: EF's GroupBy+ToList() per group can misattribute rows under some query plans.
            var rows = await dbContext.TransformProjectedH1
                        .AsNoTracking()
                        .Where(p => p.CalculatorRunId == runId)
                        .ToListAsync(cancellationToken);

            return rows
                        .GroupBy(p => new { p.ProducerId, p.SubsidiaryId, p.SubmissionPeriodCode, p.Level })
                        .Select(g => new CalcResultH1ProjectedProducer
                        {
                            ProducerId = g.Key.ProducerId,
                            SubsidiaryId = g.Key.SubsidiaryId,
                            Level = g.Key.Level,
                            SubmissionPeriodCode = g.Key.SubmissionPeriodCode,
                            H1ProjectedTonnageByMaterial = MapToH1MaterialTonnages(g.ToList())
                        })
                        .OrderBy(p => p.ProducerId)
                        .ThenBy(p => p.Level)
                        .ThenBy(p => p.SubsidiaryId)
                        .ToImmutableList();
        }

        [ActivityTrace]
        public async Task<ImmutableList<CalcResultH2ProjectedProducer>> ReadH2ProjectedData(int runId, CancellationToken cancellationToken)
        {
            // Group in memory, not SQL: EF's GroupBy+ToList() per group can misattribute rows under some query plans.
            var rows = await dbContext.TransformProjectedH2
                        .AsNoTracking()
                        .Where(p => p.CalculatorRunId == runId)
                        .ToListAsync(cancellationToken);

            return rows
                        .GroupBy(p => new { p.ProducerId, p.SubsidiaryId, p.SubmissionPeriodCode, p.Level })
                        .Select(g => new CalcResultH2ProjectedProducer
                        {
                            ProducerId = g.Key.ProducerId,
                            SubsidiaryId = g.Key.SubsidiaryId,
                            Level = g.Key.Level,
                            SubmissionPeriodCode = g.Key.SubmissionPeriodCode,
                            H2ProjectedTonnageByMaterial = MapToH2MaterialTonnages(g.ToList())
                        })
                        .OrderBy(p => p.ProducerId)
                        .ThenBy(p => p.Level)
                        .ThenBy(p => p.SubsidiaryId)
                        .ToImmutableList();
        }

        [ActivityTrace]
        public async Task<ImmutableList<CalcResultScaledupProducer>> ReadScaledData(int runId, CancellationToken cancellationToken)
        {
            // Group in memory, not SQL: EF's GroupBy+ToList() per group can misattribute rows under some query plans.
            var rows = await dbContext.TransformScaled
                        .AsNoTracking()
                        .Where(p => p.CalculatorRunId == runId)
                        .ToListAsync(cancellationToken);

            var scaledupProducers = rows
                        .GroupBy(p => new { p.ProducerId, p.SubsidiaryId, p.ProducerName, p.TradingName, p.SubmissionPeriodCode, p.Level, p.IsSubTotal, p.DaysInSubmissionPeriod, p.DaysInWholePeriod, p.ScaleupFactor })
                        .Select(g =>
                            new CalcResultScaledupProducer
                            {
                                ProducerId = g.Key.ProducerId,
                                SubsidiaryId = g.Key.SubsidiaryId,
                                ProducerName = g.Key.ProducerName,
                                TradingName = g.Key.TradingName,
                                Level = g.Key.Level,
                                IsSubtotalRow = g.Key.IsSubTotal,
                                SubmissionPeriodCode = g.Key.SubmissionPeriodCode,
                                DaysInSubmissionPeriod = g.Key.DaysInSubmissionPeriod,
                                DaysInWholePeriod = g.Key.DaysInWholePeriod,
                                ScaleupFactor = g.Key.ScaleupFactor,
                                PomData = MapToScaled(g.ToList())
                            }
                        )
                        .OrderBy(p => p.ProducerId)
                        .ThenBy(p => p.SubmissionPeriodCode)
                        .ThenBy(p => p.Level)
                        .ThenBy(p => p.SubsidiaryId)
                        .ThenBy(p => p.SubmissionPeriodCode)
                        .ToImmutableList();

            var materials = await materialService.GetMaterials();
            foreach (var producer in scaledupProducers)
                producer.ScaledupProducerTonnageByMaterial = CalcResultScaledupProducersBuilder.GetTonnages(producer.PomData, materials);

            return scaledupProducers;
        }

        [ActivityTrace]
        public async Task<ImmutableList<CalcResultPartialObligation>> ReadPartialData(int runId, CancellationToken cancellationToken){
            // Group in memory, not SQL: EF's GroupBy+ToList() per group can misattribute rows under some query plans.
            var rows = await dbContext.TransformPartial
                        .AsNoTracking()
                        .Where(p => p.CalculatorRunId == runId)
                        .ToListAsync(cancellationToken);

            return rows
                        .GroupBy(p => new { p.ProducerId, p.SubsidiaryId, p.ProducerName, p.TradingName, p.SubmissionYear, p.Level, p.DaysInSubmissionYear, p.JoiningDate, p.DaysObligated, p.ObligatedFactor })
                        .Select(g =>
                            new CalcResultPartialObligation
                            {
                                ProducerId = g.Key.ProducerId,
                                SubsidiaryId = g.Key.SubsidiaryId,
                                ProducerName = g.Key.ProducerName,
                                TradingName = g.Key.TradingName,
                                Level = g.Key.Level,
                                SubmissionYear = g.Key.SubmissionYear,
                                DaysInSubmissionYear = g.Key.DaysInSubmissionYear,
                                JoiningDate = g.Key.JoiningDate,
                                DaysObligated = g.Key.DaysObligated,
                                ObligatedFactor = g.Key.ObligatedFactor,
                                PartialObligationTonnageByMaterial = MapToPartial(g.ToList())
                            }
                        )
                        .OrderBy(p => p.ProducerId)
                        .ThenBy(p => p.Level)
                        .ThenBy(p => p.SubsidiaryId)
                        .ToImmutableList();
        }

        [ActivityTrace]
        public async Task<FeeDetail> ReadProducerFeesTotal(int runId, CancellationToken cancellationToken) =>
            await dbContext.ProducerDisposalFee
                        .AsNoTracking()
                        .Where(p => p.CalculatorRunId == runId)
                        .Select(p => p.Total)
                        .SingleAsync(cancellationToken);

        // EF materialising each owned-JSON document was most of a download's cost, so read the stored UTF-8 bytes and deserialise them directly.
        // This is a raw command, not an EF query, so AsNoTracking() can't be applied; nothing is tracked.
        public IEnumerable<FeeDetail> StreamProducerFeeDetails(int runId)
        {
            dbContext.Database.OpenConnection();
            try
            {
                var detail = JsonColumnName<ProducerFeeDetail>(nameof(ProducerFeeDetail.FeeDetail));

                using var command = dbContext.Database.GetDbConnection().CreateCommand();
                // Reading the JSON as raw bytes is faster than as text, but SQL Server only allows that for binary columns, hence the cast.
                // The cast is SQL Server only, so the integration tests cover it; unit tests require the cast to be removed.
                #pragma warning disable S2077 // Table and column names come from EF Core metadata, not user input
                command.CommandText = $"""
                    SELECT {(dbContext.Database.IsSqlServer() ? $"CAST({detail} AS varbinary(max))" : detail)}
                    FROM {TableName<ProducerFeeDetail>()}
                    WHERE {ColumnName<ProducerFeeDetail>(nameof(ProducerFeeDetail.ProducerFeesId))} IN (
                        SELECT {ColumnName<ProducerFees>(nameof(ProducerFees.Id))}
                        FROM {TableName<ProducerFees>()}
                        WHERE {ColumnName<ProducerFees>(nameof(ProducerFees.CalculatorRunId))} = @runId)
                    ORDER BY {ColumnName<ProducerFeeDetail>(nameof(ProducerFeeDetail.Id))}
                    """;
                #pragma warning restore S2077
                command.CommandTimeout = dbContext.Database.GetCommandTimeout() ?? command.CommandTimeout;

                var runIdParameter = command.CreateParameter();
                runIdParameter.ParameterName = "@runId";
                runIdParameter.Value = runId;
                command.Parameters.Add(runIdParameter);

                using var reader = command.ExecuteReader(CommandBehavior.SequentialAccess);
                while (reader.Read())
                {
                    using var json = reader.GetStream(0);
                    yield return JsonSerializer.Deserialize<FeeDetail>(json, FeeDetailJsonOptions)!;
                }
            }
            finally
            {
                dbContext.Database.CloseConnection();
            }
        }

        private IEntityType EntityType<T>() => dbContext.Model.FindEntityType(typeof(T))!;

        private string TableName<T>() => EntityType<T>().GetTableName()!;

        private string ColumnName<T>(string property)
        {
            var entityType = EntityType<T>();
            return entityType.FindProperty(property)!.GetColumnName(StoreObjectIdentifier.Table(entityType.GetTableName()!, entityType.GetSchema()))!;
        }

        private string JsonColumnName<T>(string ownedNavigation) =>
            EntityType<T>().FindNavigation(ownedNavigation)!.TargetEntityType.GetContainerColumnName()!;

        // EF writes decimals in owned JSON as numbers on SQL Server but as strings on SQLite (the unit tests), and the liability directions as names (HasConversion<string>).
        private static readonly JsonSerializerOptions FeeDetailJsonOptions = new()
        {
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            Converters = { new JsonStringEnumConverter() }
        };

        [ActivityTrace]
        public async Task<IReadOnlyDictionary<string, SelfManagedConsumerWasteData>> ReadSmcwByMaterial(int runId, CancellationToken cancellationToken)
        {
            var smcw = await dbContext.SelfManagedConsumerWaste
                    .AsNoTracking()
                    .Where(p => p.CalculatorRunId == runId)
                    .SingleAsync(cancellationToken);

            return smcw.TotalByMaterial;
        }

        [ActivityTrace]
        public async Task<ModulationResult> ReadModulationResult(int runId, CancellationToken cancellationToken) =>
            await dbContext.ModulationResult
                    .AsNoTracking()
                    .Where(p => p.CalculatorRunId == runId)
                    .SingleAsync(cancellationToken);

        [ActivityTrace]
        public async Task<CalcResultLapcapData> ReadLapcapData(int runId, CancellationToken cancellationToken) =>
            await dbContext.LapcapData
                    .AsNoTracking()
                    .Where(p => p.CalculatorRunId == runId)
                    .Select(x => x.LapcapData)
                    .SingleAsync(cancellationToken);

        [ActivityTrace]
        public async Task<CalcResultCommsCost> ReadCommsCost(int runId, CancellationToken cancellationToken) =>
            await dbContext.CommCost
                    .AsNoTracking()
                    .Where(p => p.CalculatorRunId == runId)
                    .Select(x => x.CommsCost)
                    .SingleAsync(cancellationToken);

        [ActivityTrace]
        public async Task<CalcResultLateReportingTonnage> ReadLateReportingTonnage(int runId, CancellationToken cancellationToken) =>
            await dbContext.LateReportingTonnage
                    .AsNoTracking()
                    .Where(p => p.CalculatorRunId == runId)
                    .Select(x => x.LateReportingTonnage)
                    .SingleAsync(cancellationToken);

        [ActivityTrace]
        public async Task<CalcResultParameterOtherCost> ReadParameterOtherCost(int runId, CancellationToken cancellationToken) =>
            await dbContext.ParameterOtherCost
                    .AsNoTracking()
                    .Where(p => p.CalculatorRunId == runId)
                    .Select(x => x.ParameterOtherCost)
                    .SingleAsync(cancellationToken);

        [ActivityTrace]
        public async Task<CalcResultOnePlusFourApportionment> ReadOnePlusFourApportionment(int runId, CancellationToken cancellationToken) =>
            await dbContext.OnePlusFourApportionment
                    .AsNoTracking()
                    .Where(p => p.CalculatorRunId == runId)
                    .Select(x => x.OnePlusFourApportionment)
                    .SingleAsync(cancellationToken);

        [ActivityTrace]
        public async Task<CalcResultLaDisposalCostData> ReadLaDisposalCostData(int runId, CancellationToken cancellationToken) =>
            await dbContext.LaDisposalCostData
                    .AsNoTracking()
                    .Where(p => p.CalculatorRunId == runId)
                    .Select(x => x.LaDisposalCost)
                    .SingleAsync(cancellationToken);

        [ActivityTrace]
        public async Task<ImmutableList<CalcResultCancelledProducer>> ReadCancelledProducers(int runId, CancellationToken cancellationToken) =>
            await dbContext.CancelledProducers
                    .AsNoTracking()
                    .Where(p => p.CalculatorRunId == runId)
                    .Select(x => x.CancelledProducer)
                    .ToImmutableListAsync(cancellationToken);

        private static Dictionary<string, CalcResultH1ProjectedProducerMaterialTonnage> MapToH1MaterialTonnages(List<TransformProjectedH1> transformProjectedH1s)
        {
            return transformProjectedH1s.ToDictionary(
                t => t.MaterialCode,
                t => new CalcResultH1ProjectedProducerMaterialTonnage
                {
                    HouseholdTonnage = t.HouseholdTonnage,
                    HouseholdRAMTonnage = new RamTonnage
                    {
                        Red = t.HouseholdTonnageRed,
                        Amber = t.HouseholdTonnageAmber,
                        Green = t.HouseholdTonnageGreen,
                        RedMedical = t.HouseholdTonnageRedMedical,
                        AmberMedical = t.HouseholdTonnageAmberMedical,
                        GreenMedical = t.HouseholdTonnageGreenMedical
                    },
                    PublicBinTonnage = t.PublicBinTonnage,
                    PublicBinRAMTonnage = new RamTonnage
                    {
                        Red = t.PublicBinTonnageRed,
                        Amber = t.PublicBinTonnageAmber,
                        Green = t.PublicBinTonnageGreen,
                        RedMedical = t.PublicBinTonnageRedMedical,
                        AmberMedical = t.PublicBinTonnageAmberMedical,
                        GreenMedical = t.PublicBinTonnageGreenMedical
                    },
                    HouseholdDrinksContainerTonnage = t.HDCTonnage,
                    HouseholdDrinksContainerRAMTonnage = t.MaterialCode == MaterialCodes.Glass ? new RamTonnage
                    {
                        Red = t.HDCTonnageRed!.Value,
                        Amber = t.HDCTonnageAmber!.Value,
                        Green = t.HDCTonnageGreen!.Value,
                        RedMedical = t.HDCTonnageRedMedical!.Value,
                        AmberMedical = t.HDCTonnageAmberMedical!.Value,
                        GreenMedical = t.HDCTonnageGreenMedical!.Value
                    } : null,
                    HouseholdTonnageWithoutRAM = t.HouseholdTonnageWithoutRAM,
                    PublicBinTonnageWithoutRAM = t.PublicBinTonnageWithoutRAM,
                    HouseholdDrinksContainerTonnageWithoutRAM = t.HDCTonnageWithoutRAM,
                    ProjectedHouseholdTonnage = t.ProjectedHouseholdTonnage,
                    ProjectedHouseholdRAMTonnage = new RamTonnage
                    {
                        Red = t.ProjectedHouseholdTonnageRed,
                        Amber = t.ProjectedHouseholdTonnageAmber,
                        Green = t.ProjectedHouseholdTonnageGreen,
                        RedMedical = t.ProjectedHouseholdTonnageRedMedical,
                        AmberMedical = t.ProjectedHouseholdTonnageAmberMedical,
                        GreenMedical = t.ProjectedHouseholdTonnageGreenMedical
                    },
                    ProjectedPublicBinTonnage = t.ProjectedPublicBinTonnage,
                    ProjectedPublicBinRAMTonnage = new RamTonnage
                    {
                        Red = t.ProjectedPublicBinTonnageRed,
                        Amber = t.ProjectedPublicBinTonnageAmber,
                        Green = t.ProjectedPublicBinTonnageGreen,
                        RedMedical = t.ProjectedPublicBinTonnageRedMedical,
                        AmberMedical = t.ProjectedPublicBinTonnageAmberMedical,
                        GreenMedical = t.ProjectedPublicBinTonnageGreenMedical
                    },
                    ProjectedHouseholdDrinksContainerTonnage = t.ProjectedHDCTonnage,
                    ProjectedHouseholdDrinksContainerRAMTonnage = t.MaterialCode == MaterialCodes.Glass ? new RamTonnage
                    {
                        Red = t.ProjectedHDCTonnageRed!.Value,
                        Amber = t.ProjectedHDCTonnageAmber!.Value,
                        Green = t.ProjectedHDCTonnageGreen!.Value,
                        RedMedical = t.ProjectedHDCTonnageRedMedical!.Value,
                        AmberMedical = t.ProjectedHDCTonnageAmberMedical!.Value,
                        GreenMedical = t.ProjectedHDCTonnageGreenMedical!.Value
                    } : null,
                    H2RamProportions = new RAMProportions
                    {
                        Red = t.H2RamProportionsRed,
                        Amber = t.H2RamProportionsAmber,
                        Green = t.H2RamProportionsGreen,
                        RedMedical = t.H2RamProportionsRedMedical,
                        AmberMedical = t.H2RamProportionsAmberMedical,
                        GreenMedical = t.H2RamProportionsGreenMedical
                    }
                }
            );
        }

        private static Dictionary<string, CalcResultH2ProjectedProducerMaterialTonnage> MapToH2MaterialTonnages(List<TransformProjectedH2> transformProjectedH2s)
        {
            return transformProjectedH2s.ToDictionary(
                t => t.MaterialCode,
                t => new CalcResultH2ProjectedProducerMaterialTonnage
                {
                    HouseholdTonnage = t.HouseholdTonnage,
                    HouseholdRAMTonnage = new RamTonnage
                    {
                        Red = t.HouseholdTonnageRed,
                        Amber = t.HouseholdTonnageAmber,
                        Green = t.HouseholdTonnageGreen,
                        RedMedical = t.HouseholdTonnageRedMedical,
                        AmberMedical = t.HouseholdTonnageAmberMedical,
                        GreenMedical = t.HouseholdTonnageGreenMedical
                    },
                    PublicBinTonnage = t.PublicBinTonnage,
                    PublicBinRAMTonnage = new RamTonnage
                    {
                        Red = t.PublicBinTonnageRed,
                        Amber = t.PublicBinTonnageAmber,
                        Green = t.PublicBinTonnageGreen,
                        RedMedical = t.PublicBinTonnageRedMedical,
                        AmberMedical = t.PublicBinTonnageAmberMedical,
                        GreenMedical = t.PublicBinTonnageGreenMedical
                    },
                    HouseholdDrinksContainerTonnage = t.HDCTonnage,
                    HouseholdDrinksContainerRAMTonnage = t.MaterialCode == MaterialCodes.Glass ? new RamTonnage
                    {
                        Red = t.HDCTonnageRed!.Value,
                        Amber = t.HDCTonnageAmber!.Value,
                        Green = t.HDCTonnageGreen!.Value,
                        RedMedical = t.HDCTonnageRedMedical!.Value,
                        AmberMedical = t.HDCTonnageAmberMedical!.Value,
                        GreenMedical = t.HDCTonnageGreenMedical!.Value
                    } : null,
                    HouseholdTonnageWithoutRAM = t.HouseholdTonnageWithoutRAM,
                    PublicBinTonnageWithoutRAM = t.PublicBinTonnageWithoutRAM,
                    HouseholdDrinksContainerTonnageWithoutRAM = t.HDCTonnageWithoutRAM,
                    ProjectedHouseholdTonnage = t.ProjectedHouseholdTonnage,
                    ProjectedHouseholdRAMTonnage = new RamTonnage
                    {
                        Red = t.ProjectedHouseholdTonnageRed,
                        Amber = t.ProjectedHouseholdTonnageAmber,
                        Green = t.ProjectedHouseholdTonnageGreen,
                        RedMedical = t.ProjectedHouseholdTonnageRedMedical,
                        AmberMedical = t.ProjectedHouseholdTonnageAmberMedical,
                        GreenMedical = t.ProjectedHouseholdTonnageGreenMedical
                    },
                    ProjectedPublicBinTonnage = t.ProjectedPublicBinTonnage,
                    ProjectedPublicBinRAMTonnage = new RamTonnage
                    {
                        Red = t.ProjectedPublicBinTonnageRed,
                        Amber = t.ProjectedPublicBinTonnageAmber,
                        Green = t.ProjectedPublicBinTonnageGreen,
                        RedMedical = t.ProjectedPublicBinTonnageRedMedical,
                        AmberMedical = t.ProjectedPublicBinTonnageAmberMedical,
                        GreenMedical = t.ProjectedPublicBinTonnageGreenMedical
                    },
                    ProjectedHouseholdDrinksContainerTonnage = t.ProjectedHDCTonnage,
                    ProjectedHouseholdDrinksContainerRAMTonnage = t.MaterialCode == MaterialCodes.Glass ? new RamTonnage
                    {
                        Red = t.ProjectedHDCTonnageRed!.Value,
                        Amber = t.ProjectedHDCTonnageAmber!.Value,
                        Green = t.ProjectedHDCTonnageGreen!.Value,
                        RedMedical = t.ProjectedHDCTonnageRedMedical!.Value,
                        AmberMedical = t.ProjectedHDCTonnageAmberMedical!.Value,
                        GreenMedical = t.ProjectedHDCTonnageGreenMedical!.Value
                    } : null
                }
            );
        }

        private static ImmutableList<ScaledupPomEntry> MapToScaled(List<TransformScaled> scaled)
        {
            return scaled.Select(s =>
                new ScaledupPomEntry(
                    MaterialId: s.MaterialId,
                    PackagingType: s.PackagingType,
                    Tonnage: s.Tonnage,
                    ScaledTonnage: s.ScaledTonnage
                )
            ).ToImmutableList();
        }

        private static Dictionary<string, CalcResultPartialObligationTonnage> MapToPartial(List<TransformPartial> partial)
        {
            RamTonnage? ToMaybeRamTonnage(
                decimal? red,
                decimal? amber,
                decimal? green,
                decimal? redMedical,
                decimal? amberMedical,
                decimal? greenMedical)
            {
                return red is null && amber is null && green is null && redMedical is null && amberMedical is null && greenMedical is null
                    ? null
                    : new RamTonnage
                    {
                        Red = red!.Value,
                        Amber = amber!.Value,
                        Green = green!.Value,
                        RedMedical = redMedical!.Value,
                        AmberMedical = amberMedical!.Value,
                        GreenMedical = greenMedical!.Value
                    };
            }

            return partial.ToDictionary(
                t => t.MaterialCode,
                t => new CalcResultPartialObligationTonnage
                {
                    ObligatedFactor = t.ObligatedFactor,
                    HouseholdTonnage = t.HouseholdTonnage,
                    HouseholdRAMTonnage = ToMaybeRamTonnage(
                        t.HouseholdTonnageRed,
                        t.HouseholdTonnageAmber,
                        t.HouseholdTonnageGreen,
                        t.HouseholdTonnageRedMedical,
                        t.HouseholdTonnageAmberMedical,
                        t.HouseholdTonnageGreenMedical
                    ),
                    PublicBinTonnage = t.PublicBinTonnage,
                    PublicBinRAMTonnage = ToMaybeRamTonnage(
                        t.PublicBinTonnageRed,
                        t.PublicBinTonnageAmber,
                        t.PublicBinTonnageGreen,
                        t.PublicBinTonnageRedMedical,
                        t.PublicBinTonnageAmberMedical,
                        t.PublicBinTonnageGreenMedical
                    ),
                    HouseholdDrinksContainersTonnage = t.HDCTonnage,
                    HouseholdDrinksContainersRAMTonnage = t.MaterialCode == MaterialCodes.Glass ? ToMaybeRamTonnage(
                        t.HDCTonnageRed,
                        t.HDCTonnageAmber,
                        t.HDCTonnageGreen,
                        t.HDCTonnageRedMedical,
                        t.HDCTonnageAmberMedical,
                        t.HDCTonnageGreenMedical
                    ) : null,
                    SelfManagedConsumerWasteTonnage = t.SMCWTonnage
                }
            );
        }
    }
}
