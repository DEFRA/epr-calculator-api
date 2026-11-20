using System.Globalization;
using System.Text.RegularExpressions;
using EPR.Calculator.API.BackgroundService.Features.BillingRuns.Constants;
using EPR.Calculator.API.BackgroundService.Models;
using EPR.Calculator.API.BackgroundService.Services;
using EPR.Calculator.API.BackgroundService.Utils;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.DataTypes;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.Services.RunImport;

/// <summary>A run's results and billing CSVs, with the names they had.</summary>
public sealed record RunImportFiles(string ResultsFileName, TextReader ResultsCsv, string BillingFileName, TextReader BillingCsv);

public interface IRunImporter
{
    Task<int> ImportAsync(RunImportFiles files, RunImportDetails details, CancellationToken cancellationToken);
}

/// <summary>
///     Stores a run from its results and billing CSVs, in the same shape a calculator run stores it, so its files can be
///     downloaded again. The results CSV holds the run as calculated, with every producer; the billing CSV says which
///     producers were billed and which rejected.
/// </summary>
public partial class RunImporter(ApplicationDBContext dbContext, ICalcResultWriter calcResultWriter, IMaterialService materialService, IInvoiceDetailsService invoiceDetailsService) : IRunImporter
{
    public async Task<int> ImportAsync(RunImportFiles files, RunImportDetails details, CancellationToken cancellationToken)
    {
        var file = BillingFileName.Parse(files.BillingFileName);
        var results = BillingCsv.Read(files.ResultsCsv);
        var billing = BillingCsv.Read(files.BillingCsv);
        var header = RunSectionsParser.ReadHeader(results);

        if (ResultsRunId(files.ResultsFileName) != header.RunId)
            throw new FormatException($"The results file name is for run {ResultsRunId(files.ResultsFileName)} but the file is for run {header.RunId}");
        if (file.RunId != header.RunId || RunSectionsParser.ReadHeader(billing).RunId != header.RunId)
            throw new FormatException($"The billing file isn't for run {header.RunId}");

        var lapcap = RunSectionsParser.ReadLapcapData(results);
        var lateReporting = RunSectionsParser.ReadLateReportingTonnage(results);
        var otherCost = RunSectionsParser.ReadParameterOtherCost(results, header);
        var onePlusFour = RunSectionsParser.ReadOnePlusFourApportionment(results);
        var comms = RunSectionsParser.ReadCommsCost(results);
        var laDisposal = RunSectionsParser.ReadLaDisposalCostData(results);
        var cancelled = ProducerSectionsParser.ReadCancelledProducers(results);
        var scaledUp = ProducerSectionsParser.ReadScaledupProducers(results, RunSectionsParser.MaterialCodes);
        var partial = ProducerSectionsParser.ReadPartialObligations(results, RunSectionsParser.MaterialCodes);
        var fees = FeeTableParser.Read(results, RunSectionsParser.MaterialCodes);
        var errors = ProducerSectionsParser.ReadErrorReport(results);
        var modulation = RunSectionsParser.ReadModulation(results, header.RunId, await materialService.GetMaterials());
        var projected = ProjectedProducersParser.Read(results, RunSectionsParser.MaterialCodes);

        var billed = FeeTableParser.Read(billing, RunSectionsParser.MaterialCodes);
        var rejected = ProducerSectionsParser.ReadRejectedProducers(billing);

        if (fees.Warnings.Count > 0 || billed.Warnings.Count > 0)
            throw new FormatException(string.Join(Environment.NewLine, fees.Warnings.Concat(billed.Warnings)));

        var producerKeys = fees.Producers.Select(p => (p.ProducerId, p.SubsidiaryId, p.Level)).ToHashSet();
        if (billed.Producers.FirstOrDefault(p => !producerKeys.Contains((p.ProducerId, p.SubsidiaryId, p.Level))) is { } notInResults)
            throw new FormatException($"Producer {notInResults.ProducerId} is in the billing file but not the results file");

        ApplyNames(details.ProducerNames, fees.Producers, scaledUp, partial, cancelled);

        // A check of a whole zip runs every run's import in one transaction it then rolls back.
        await using var transaction = dbContext.Database.CurrentTransaction is null ? await dbContext.Database.BeginTransactionAsync(cancellationToken) : null;

        await StoreRunAsync(header, file, details, lapcap, DefaultParameters(header, lateReporting, otherCost, comms, modulation, details), cancellationToken);

        await calcResultWriter.StoreLapcapData(header.RunId, lapcap, cancellationToken);
        await calcResultWriter.StoreLateReportingTonnage(header.RunId, lateReporting, cancellationToken);
        await calcResultWriter.StoreParameterOtherCost(header.RunId, otherCost, cancellationToken);
        await calcResultWriter.StoreOnePlusFourApportionment(header.RunId, onePlusFour, cancellationToken);
        await calcResultWriter.StoreCommsCost(header.RunId, comms, cancellationToken);
        await calcResultWriter.StoreLaDisposalCostData(header.RunId, laDisposal, cancellationToken);
        await calcResultWriter.StoreCancelledProducers(header.RunId, cancelled, cancellationToken);

        if (scaledUp is not null)
            await calcResultWriter.StoreScaledData(header.RunId, scaledUp, cancellationToken);

        if (partial is not null)
            await calcResultWriter.StorePartialData(header.RunId, partial, cancellationToken);

        if (projected is not null)
        {
            await calcResultWriter.StoreProjectedH1Data(header.RunId, projected.H1ProjectedProducers, cancellationToken);
            await calcResultWriter.StoreProjectedH2Data(header.RunId, projected.H2ProjectedProducers, cancellationToken);
        }

        if (modulation is not null)
            await calcResultWriter.StoreModulationResult(header.RunId, modulation, cancellationToken);

        await calcResultWriter.StoreProducerFees(header.RunId, fees.Total, fees.Producers, cancellationToken);
        await StoreProducersAsync(header.RunId, fees.Producers, errors, details.ProducerNames, details.Organisations, cancellationToken);
        await StoreErrorReportAsync(header, details, errors, cancellationToken);
        await StoreBillingInstructionsAsync(header, file, details, fees.Producers, cancelled, new BillingFileContents(billed.Producers, rejected), cancellationToken);
        await StoreInvoicedAsync(header.RunId, fees.Producers, cancellationToken);

        if (transaction is not null)
            await transaction.CommitAsync(cancellationToken);
        return header.RunId;
    }

    private static int ResultsRunId(string fileName) =>
        ResultsFilePattern().Match(Path.GetFileName(fileName)) is { Success: true } match
            ? int.Parse(match.Groups["runId"].Value, CultureInfo.InvariantCulture)
            : throw new FormatException($"'{fileName}' isn't a results file name");

    [GeneratedRegex(@"^(?<runId>\d+)-.*_Results File_\d{8}\.csv$")]
    private static partial Regex ResultsFilePattern();

    private static void ApplyNames(
        IReadOnlyDictionary<(int ProducerId, string? SubsidiaryId), ProducerNames> names,
        IReadOnlyList<FeeDetail> producers,
        CalcResultScaledupProducers? scaledUp,
        IReadOnlyList<CalcResultPartialObligation>? partial,
        IReadOnlyList<CalcResultCancelledProducer> cancelled)
    {
        if (names.Count == 0)
            return;

        // The names file can't tell an empty name from a missing one, so an empty name in it leaves the file's as it is.
        ProducerNames? Find(int producerId, string? subsidiaryId) => names.GetValueOrDefault((producerId, NullIfEmpty(subsidiaryId)));
        static string? Or(string? correction, string? current) => string.IsNullOrEmpty(correction) ? current : correction;

        foreach (var producer in producers)
            if (Find(producer.ProducerId, producer.SubsidiaryId) is { } name)
                (producer.ProducerName, producer.TradingName) = (Or(name.ProducerName, producer.ProducerName)!, Or(name.TradingName, producer.TradingName));

        foreach (var producer in scaledUp?.ScaledupProducers ?? [])
            if (Find(producer.ProducerId, producer.SubsidiaryId) is { } name)
                (producer.ProducerName, producer.TradingName) = (Or(name.ProducerName, producer.ProducerName), Or(name.TradingName, producer.TradingName));

        foreach (var producer in partial ?? [])
            if (Find(producer.ProducerId, producer.SubsidiaryId) is { } name)
                (producer.ProducerName, producer.TradingName) = (Or(name.ProducerName, producer.ProducerName), Or(name.TradingName, producer.TradingName));

        foreach (var producer in cancelled)
            if (Find(producer.ProducerId, producer.SubsidiaryId) is { } name)
                (producer.ProducerOrSubsidiaryName, producer.TradingName) = (Or(name.ProducerName, producer.ProducerOrSubsidiaryName), Or(name.TradingName, producer.TradingName));
    }

    private static string? NullIfEmpty(string? value) => string.IsNullOrEmpty(value) ? null : value;

    private async Task StoreRunAsync(RunSectionsParser.RunHeader header, BillingFileName file, RunImportDetails details, CalcResultLapcapData lapcap, IReadOnlyDictionary<string, string> defaultParameters, CancellationToken cancellationToken)
    {
        var lapcapMaster = await LapcapMasterAsync(header, lapcap, cancellationToken);
        var parameterMaster = await ParameterMasterAsync(header, defaultParameters, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        // The run keeps its original ID: it's part of each Billing Instruction ID that FSS holds.
        var run = new CalculatorRun
        {
            Id                              = header.RunId,
            Name                            = header.RunName,
            RelativeYear                    = header.RelativeYear,
            Classification                  = details.Classification ?? await ClassificationAsync(header, cancellationToken),
            CalculationRunStatus            = CalculationRunStatus.Completed,
            BillingRunStatus                = BillingRunStatus.Completed,
            IsBillingFileShared             = file.IsAuthorised,
            BillingFileSharedAt             = file.IsAuthorised ? details.BillingFileSharedAt ?? details.BillingFileCreatedAt ?? file.CreatedAt : null,
            BillingFileSharedBy             = file.IsAuthorised ? details.BillingFileSharedBy ?? details.BillingFileCreatedBy ?? header.RunBy : null,
            CreatedBy                       = header.RunBy,
            CreatedAt                       = details.RunDate ?? header.RunDate,
            UpdatedBy                       = details.UpdatedBy,
            UpdatedAt                       = details.UpdatedAt,
            BillingRunStartedAt             = details.BillingRunStartedAt is { } startedAt ? DateTime.SpecifyKind(startedAt, DateTimeKind.Utc) : null,
            OrgPomDataLoadedAt              = header.OrgPomDataLoadedAt,
            LapcapDataMasterId              = lapcapMaster.Id,
            DefaultParameterSettingMasterId = parameterMaster.Id
        };

        run.CalculatorRunBillingFileMetadata.Add(new CalculatorRunBillingFileMetadata
        {
            BillingCsvFileName     = file.CsvFileName,
            BillingJsonFileName    = file.JsonFileName,
            BillingFileCreatedDate = details.BillingFileCreatedAt ?? file.CreatedAt,
            BillingFileCreatedBy   = details.BillingFileCreatedBy ?? header.RunBy
        });

        dbContext.CalculatorRuns.Add(run);

        await dbContext.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT dbo.calculator_run ON", cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT dbo.calculator_run OFF", cancellationToken);
    }

    // Only one LAPCAP and one parameter file is active per year, so a new upload closes the previous one, as the app does.
    // Runs in a year that used the same file share it.
    private async Task<LapcapDataMaster> LapcapMasterAsync(RunSectionsParser.RunHeader header, CalcResultLapcapData lapcap, CancellationToken cancellationToken)
    {
        var file = header.LapcapFile;
        var existing = await dbContext.LapcapDataMaster.SingleOrDefaultAsync(m =>
            m.RelativeYear == header.RelativeYear && m.LapcapFileName == file.FileName && m.CreatedAt == file.UploadedAt, cancellationToken);
        if (existing is not null)
            return existing;

        await dbContext.LapcapDataMaster
            .Where(m => m.RelativeYear == header.RelativeYear && m.EffectiveTo == null)
            .ExecuteUpdateAsync(s => s.SetProperty(m => m.EffectiveTo, file.UploadedAt), cancellationToken);

        var master = new LapcapDataMaster
        {
            RelativeYear   = header.RelativeYear,
            EffectiveFrom  = file.UploadedAt,
            CreatedBy      = file.UploadedBy,
            CreatedAt      = file.UploadedAt,
            LapcapFileName = file.FileName
        };

        // The LAPCAP section shows the uploaded cost for each country and material.
        foreach (var (material, cost) in lapcap.ByMaterial)
            foreach (var (country, value) in new[] { ("ENG", cost.England), ("WLS", cost.Wales), ("SCT", cost.Scotland), ("NI", cost.NorthernIreland) })
                master.Details.Add(new LapcapDataDetail { UniqueReference = $"{country}-{material}", TotalCost = value });


        dbContext.Add(master);
        return master;
    }

    private async Task<DefaultParameterSettingMaster> ParameterMasterAsync(RunSectionsParser.RunHeader header, IReadOnlyDictionary<string, string> defaultParameters, CancellationToken cancellationToken)
    {
        var file = header.ParametersFile;
        var existing = await dbContext.DefaultParameterSettings.SingleOrDefaultAsync(m =>
            m.RelativeYear == header.RelativeYear && m.ParameterFileName == file.FileName && m.CreatedAt == file.UploadedAt, cancellationToken);
        if (existing is not null)
            return existing;

        await dbContext.DefaultParameterSettings
            .Where(m => m.RelativeYear == header.RelativeYear && m.EffectiveTo == null)
            .ExecuteUpdateAsync(s => s.SetProperty(m => m.EffectiveTo, file.UploadedAt), cancellationToken);

        var master = new DefaultParameterSettingMaster
        {
            RelativeYear      = header.RelativeYear,
            EffectiveFrom     = file.UploadedAt,
            CreatedBy         = file.UploadedBy,
            CreatedAt         = file.UploadedAt,
            ParameterFileName = file.FileName
        };

        foreach (var (reference, value) in defaultParameters)
            master.Details.Add(new DefaultParameterSettingDetail { ParameterUniqueReferenceId = reference, ParameterValue = value });

        dbContext.Add(master);
        return master;
    }

    private async Task<RunClassification> ClassificationAsync(RunSectionsParser.RunHeader header, CancellationToken cancellationToken) =>
        await dbContext.CalculatorRuns.AnyAsync(r => r.RelativeYear == header.RelativeYear && r.Id < header.RunId
                                                     && (r.Classification == RunClassification.Initial || r.Classification == RunClassification.Recalculation), cancellationToken)
            ? RunClassification.Recalculation
            : RunClassification.Initial;

    /// <summary>
    ///     Each producer and subsidiary in the run, with their names as they were in it, as a run stores them from RPD.
    ///     The files look names up here, for example for the Rejected Report and the Error Table.
    /// </summary>
    private async Task StoreProducersAsync(
        int runId,
        IReadOnlyList<FeeDetail> producers,
        IReadOnlyList<ProducerSectionsParser.ErrorReportRow> errors,
        IReadOnlyDictionary<(int ProducerId, string? SubsidiaryId), ProducerNames> names,
        IReadOnlyList<RunOrganisation> runOrganisations,
        CancellationToken cancellationToken)
    {
        var organisations = new Dictionary<(int, string?), CalculatorRunOrganisation>();
        var producerDetails = AddProducers(runId, producers, organisations);
        AddErrorTableOrganisations(runId, errors, names, organisations);
        ApplyProductionOrganisations(runId, runOrganisations, organisations, producerDetails);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private Dictionary<(int, string?), ProducerDetail> AddProducers(
        int runId,
        IReadOnlyList<FeeDetail> producers,
        Dictionary<(int, string?), CalculatorRunOrganisation> organisations)
    {
        var producerDetails = new Dictionary<(int, string?), ProducerDetail>();
        foreach (var producer in producers.OrderBy(p => p.Level))
        {
            var key = (producer.ProducerId, NullIfEmpty(producer.SubsidiaryId));
            if (producerDetails.ContainsKey(key))
                continue;

            var detail = new ProducerDetail
            {
                CalculatorRunId = runId,
                ProducerId      = key.ProducerId,
                SubsidiaryId    = key.Item2,
                ProducerName    = producer.ProducerName,
                TradingName     = producer.TradingName,
                StatusCode      = producer.StatusCode,
                JoinerDate      = producer.JoinerDate,
                LeaverDate      = producer.LeaverDate
            };
            producerDetails[key] = detail;
            dbContext.ProducerDetail.Add(detail);

            var organisation = new CalculatorRunOrganisation
            {
                CalculatorRunId  = runId,
                OrganisationId   = key.ProducerId,
                SubsidiaryId     = key.Item2,
                OrganisationName = producer.ProducerName,
                TradingName      = producer.TradingName,
                StatusCode       = producer.StatusCode,
                JoinerDate       = producer.JoinerDate,
                LeaverDate       = producer.LeaverDate
            };
            organisations[key] = organisation;
            dbContext.CalculatorRunOrganisations.Add(organisation);
        }
        return producerDetails;
    }

    // The Error Table shows the organisations' own names, which can differ from the fee table's. Producers with errors
    // are often not in the fee table at all.
    private void AddErrorTableOrganisations(
        int runId,
        IReadOnlyList<ProducerSectionsParser.ErrorReportRow> errors,
        IReadOnlyDictionary<(int ProducerId, string? SubsidiaryId), ProducerNames> names,
        Dictionary<(int, string?), CalculatorRunOrganisation> organisations)
    {
        foreach (var error in errors.Where(e => e.ProducerName is not null))
        {
            if (organisations.TryGetValue((error.ProducerId, error.SubsidiaryId), out var existing))
            {
                if (AsTheFilesShowIt(existing.OrganisationName) != error.ProducerName)
                    (existing.OrganisationName, existing.TradingName) = (error.ProducerName!, error.TradingName);
                continue;
            }

            var name = names.GetValueOrDefault((error.ProducerId, error.SubsidiaryId));
            var organisation = new CalculatorRunOrganisation
            {
                CalculatorRunId  = runId,
                OrganisationId   = error.ProducerId,
                SubsidiaryId     = error.SubsidiaryId,
                OrganisationName = string.IsNullOrEmpty(name?.ProducerName) ? error.ProducerName! : name.ProducerName,
                TradingName      = string.IsNullOrEmpty(name?.TradingName) ? error.TradingName : name.TradingName
            };
            organisations[(error.ProducerId, error.SubsidiaryId)] = organisation;
            dbContext.CalculatorRunOrganisations.Add(organisation);
        }
    }

    /// <summary>
    ///     What production holds that the files don't: which organisations had errors, and organisations the files don't
    ///     show at all. A value the row leaves empty keeps what the files gave. An organisation listed again is another of
    ///     its rows, as production can hold several.
    /// </summary>
    private void ApplyProductionOrganisations(
        int runId,
        IReadOnlyList<RunOrganisation> runOrganisations,
        Dictionary<(int, string?), CalculatorRunOrganisation> organisations,
        Dictionary<(int, string?), ProducerDetail> producerDetails)
    {
        var applied = new HashSet<(int, string?)>();
        foreach (var source in runOrganisations)
        {
            var key = (source.OrganisationId, NullIfEmpty(source.SubsidiaryId));
            if (!applied.Add(key) || !organisations.TryGetValue(key, out var organisation))
            {
                organisation = new CalculatorRunOrganisation
                {
                    CalculatorRunId  = runId,
                    OrganisationId   = source.OrganisationId,
                    SubsidiaryId     = key.Item2,
                    OrganisationName = source.OrganisationName
                        ?? throw new FormatException($"Organisation {source.OrganisationId} {source.SubsidiaryId} isn't in the files, so {RunImportPackage.OrganisationsFile} needs its name")
                };
                organisations.TryAdd(key, organisation);
                dbContext.CalculatorRunOrganisations.Add(organisation);
            }

            organisation.IsError          = source.ObligationStatus == "E";
            organisation.OrganisationName = source.OrganisationName ?? organisation.OrganisationName;
            organisation.TradingName      = source.TradingName ?? organisation.TradingName;
            organisation.StatusCode       = source.StatusCode ?? organisation.StatusCode;
            organisation.JoinerDate       = source.JoinerDate ?? organisation.JoinerDate;
            organisation.LeaverDate       = source.LeaverDate ?? organisation.LeaverDate;
            organisation.DaysObligated    = source.DaysObligated ?? organisation.DaysObligated;

            if (source.ObligationStatus == "O" && producerDetails.TryGetValue(key, out var detail))
            {
                detail.StatusCode    = source.StatusCode ?? detail.StatusCode;
                detail.JoinerDate    = source.JoinerDate ?? detail.JoinerDate;
                detail.LeaverDate    = source.LeaverDate ?? detail.LeaverDate;
                detail.DaysObligated = source.DaysObligated ?? detail.DaysObligated;
            }
        }
    }

    // The CSV writer removes commas, tabs and line breaks and trims, so a corrected name shows like this in the files.
    private static string AsTheFilesShowIt(string? name) =>
        (name ?? string.Empty).Replace(Environment.NewLine, string.Empty).Replace("\t", string.Empty).Replace(",", string.Empty).Trim();

    private async Task StoreErrorReportAsync(RunSectionsParser.RunHeader header, RunImportDetails details, IReadOnlyList<ProducerSectionsParser.ErrorReportRow> errors, CancellationToken cancellationToken)
    {
        // The blank holding company rows aren't stored: the export adds them again.
        dbContext.ErrorReports.AddRange(errors.Where(e => e.ErrorCode is not null).Select(error => new ErrorReport
        {
            CalculatorRunId = header.RunId,
            ProducerId      = error.ProducerId,
            SubsidiaryId    = error.SubsidiaryId,
            LeaverCode      = error.LeaverCode,
            ErrorCode       = error.ErrorCode!,
            CreatedBy       = header.RunBy,
            CreatedAt       = details.RunDate ?? header.RunDate
        }));

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>The billing file's producers, and its Rejected Report's.</summary>
    private sealed record BillingFileContents(IReadOnlyList<FeeDetail> Billed, IReadOnlyList<ProducerSectionsParser.RejectedProducer> Rejected);

    /// <summary>
    ///     A suggested billing instruction for each Level 1 producer in the results, and each cancelled producer, with the
    ///     decision on it. Without <see cref="RunImportDetails.BillingDecisions" />, the billing file's producers count as
    ///     accepted, by whoever created it and when, and the Rejected Report's as rejected.
    /// </summary>
    private async Task StoreBillingInstructionsAsync(
        RunSectionsParser.RunHeader header,
        BillingFileName file,
        RunImportDetails details,
        IReadOnlyList<FeeDetail> producers,
        IReadOnlyList<CalcResultCancelledProducer> cancelled,
        BillingFileContents billing,
        CancellationToken cancellationToken)
    {
        var billedIds = billing.Billed.Where(p => p.Level == "1").Select(p => p.ProducerId).ToHashSet();
        var rejectedById = billing.Rejected.GroupBy(r => r.ProducerId).ToDictionary(g => g.Key, g => g.First());
        var acceptedBy = details.BillingFileCreatedBy ?? header.RunBy;

        BillingDecision? Decision(int producerId) =>
            details.BillingDecisions.Count > 0
                ? details.BillingDecisions.GetValueOrDefault(producerId)
                : billedIds.Contains(producerId)
                    ? new BillingDecision(BillingConstants.Action.Accepted, acceptedBy, details.BillingFileCreatedAt ?? file.CreatedAt)
                    : rejectedById.TryGetValue(producerId, out var r)
                        ? new BillingDecision(BillingConstants.Action.Rejected, r.ConfirmedBy, r.ConfirmedAt)
                        : null;

        var instructions = new List<ProducerResultFileSuggestedBillingInstruction>();

        foreach (var producer in producers.Where(p => p.Level == "1"))
        {
            var instruction = producer.BillingInstruction!;
            var decision = Decision(producer.ProducerId);
            instructions.Add(new ProducerResultFileSuggestedBillingInstruction
            {
                CalculatorRunId                         = header.RunId,
                ProducerId                              = producer.ProducerId,
                TotalProducerBillWithBadDebt            = producer.TotalBillBreakdown.ByCountry.Total,
                CurrentYearInvoiceTotalToDate           = instruction.CurrentYearInvoiceTotalToDate,
                TonnageChangeSinceLastInvoice           = instruction.TonnageChangeSinceLastInvoice,
                AmountLiabilityDifferenceCalcVsPrev     = instruction.LiabilityDifference,
                MaterialPoundThresholdBreached          = instruction.MaterialityLiabilityDirection.ToThresholdBreachedString(),
                TonnagePoundThresholdBreached           = instruction.TonnageAmountLiabilityDirection.ToThresholdBreachedString(),
                PercentageLiabilityDifferenceCalcVsPrev = instruction.PercentageLiabilityDifference,
                MaterialPercentageThresholdBreached     = instruction.MaterialityPercentageLiabilityDirection.ToThresholdBreachedString(),
                TonnagePercentageThresholdBreached      = instruction.TonnageAmountPercentageLiabilityDirection.ToThresholdBreachedString(),
                SuggestedBillingInstruction             = instruction.SuggestedBillingInstruction,
                SuggestedInvoiceAmount                  = instruction.SuggestedInvoiceAmount,
                BillingInstructionAcceptReject          = decision?.AcceptReject,
                ReasonForRejection                      = rejectedById.GetValueOrDefault(producer.ProducerId)?.Reason,
                LastModifiedAcceptRejectBy              = decision?.DecidedBy,
                LastModifiedAcceptReject                = decision?.DecidedAt
            });
        }

        // A cancelled producer has no fees in the run, just the total it was last invoiced.
        foreach (var producer in cancelled.GroupBy(p => p.ProducerId).Select(g => g.First()))
        {
            var decision = Decision(producer.ProducerId);
            instructions.Add(new ProducerResultFileSuggestedBillingInstruction
            {
                CalculatorRunId                = header.RunId,
                ProducerId                     = producer.ProducerId,
                SuggestedBillingInstruction    = BillingConstants.Suggestion.Cancel,
                CurrentYearInvoiceTotalToDate  = producer.LatestInvoice?.CurrentYearInvoicedTotalToDate,
                BillingInstructionAcceptReject = decision?.AcceptReject,
                ReasonForRejection             = rejectedById.GetValueOrDefault(producer.ProducerId)?.Reason,
                LastModifiedAcceptRejectBy     = decision?.DecidedBy,
                LastModifiedAcceptReject       = decision?.DecidedAt
            });
        }

        var instructed = instructions.Select(i => i.ProducerId).ToHashSet();
        if (details.BillingDecisions.Keys.FirstOrDefault(id => !instructed.Contains(id)) is var unknown and not 0)
            throw new FormatException($"Producer {unknown} has a billing decision but isn't in the run's results file");

        dbContext.ProducerResultFileSuggestedBillingInstruction.AddRange(instructions);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    ///     What was invoiced, as a run stores it: the net tonnages when the run finishes, and the invoice instructions when
    ///     its billing file is sent. Later runs read these for what's already been invoiced.
    /// </summary>
    private async Task StoreInvoicedAsync(int runId, IReadOnlyList<FeeDetail> producers, CancellationToken cancellationToken)
    {
        var materials = await materialService.GetMaterials();
        dbContext.ProducerInvoicedMaterialNetTonnage.AddRange(ProducerInvoiceNetTonnageService.GetInvoicedMaterialNetTonnage(runId, producers, materials));
        await dbContext.SaveChangesAsync(cancellationToken);

        var run = await dbContext.CalculatorRuns.AsNoTracking().SingleAsync(r => r.Id == runId, cancellationToken);
        if (run.IsBillingFileShared)
            await invoiceDetailsService.InsertInvoiceDetailsAtProducerLevel(runId, run.BillingFileSharedAt!.Value, run.BillingFileSharedBy!, cancellationToken);
    }

    /// <summary>
    ///     The run's default parameters, from the sections that show them; anything the file doesn't show comes from <paramref name="details" />.
    /// </summary>
    private static Dictionary<string, string> DefaultParameters(
        RunSectionsParser.RunHeader header,
        CalcResultLateReportingTonnage lateReporting,
        CalcResultParameterOtherCost otherCost,
        CalcResultCommsCost comms,
        ModulationResult? modulation,
        RunImportDetails details)
    {
        var values = new Dictionary<string, decimal>();

        void ByCountry(string prefix, ByCountryCost cost)
        {
            values[$"{prefix}-ENG"] = cost.England;
            values[$"{prefix}-WLS"] = cost.Wales;
            values[$"{prefix}-SCT"] = cost.Scotland;
            values[$"{prefix}-NIR"] = cost.NorthernIreland;
        }

        foreach (var (code, material) in comms.ByMaterial)
            values[$"COMC-{code}"] = material.TotalCost;

        ByCountry("COMC", comms.CommsCostByCountry);
        values["COMC-UK"] = comms.CommsCostUkWide.Total;
        ByCountry("SAOC", otherCost.SaOperatingCost);
        ByCountry("SCSC", otherCost.SchemeSetupCost);
        ByCountry("LAPC", otherCost.LaDataPrepCharge);

        // Before modulation the late reporting tonnage had no RAM split, so it's all held as amber.
        var hasRamSplit = lateReporting.Total.Red + lateReporting.Total.Amber + lateReporting.Total.Green != 0;
        foreach (var (code, tonnage) in lateReporting.ByMaterial)
        {
            values[$"LRET-{code}"]   = hasRamSplit ? tonnage.Amber : tonnage.Total;
            values[$"LRET-{code}-G"] = tonnage.Green;
            values[$"LRET-{code}-R"] = tonnage.Red;
        }

        values["MATT-AI"]  = otherCost.MaterialityIncrease.Amount;
        values["MATT-AD"]  = otherCost.MaterialityDecrease.Amount;
        values["MATT-PI"]  = otherCost.MaterialityIncrease.Percentage;
        values["MATT-PD"]  = otherCost.MaterialityDecrease.Percentage;
        values["TONT-AI"]  = otherCost.TonnageChangeIncrease.Amount;
        values["TONT-AD"]  = otherCost.TonnageChangeDecrease.Amount;
        values["TONT-PI"]  = otherCost.TonnageChangeIncrease.Percentage;
        values["TONT-PD"]  = otherCost.TonnageChangeDecrease.Percentage;
        values["BADEBT-P"] = otherCost.BadDebtValue;

        var parameters = values.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString(CultureInfo.InvariantCulture));
        parameters["COFF-DT"] = header.CutOffDate?.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) ?? "NA";

        // Files without modulation don't show the red modulation factor; uploads from before it were given 1.000.
        parameters["REDM-RF"] = (modulation?.RedFactor ?? 1.000m).ToString(CultureInfo.InvariantCulture);

        foreach (var (reference, value) in details.DefaultParameters)
            parameters[reference] = value;

        return parameters;
    }
}
