using System.Globalization;
using EPR.Calculator.API.BackgroundService.Features.BillingRuns.Constants;
using EPR.Calculator.API.BackgroundService.Services;
using EPR.Calculator.API.BackgroundService.Utils;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.DataTypes;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.BackgroundService.Features.RunImport;

public interface IRunImporter
{
    Task<int> ImportAsync(TextReader billingCsv, string fileName, RunImportDetails details, CancellationToken cancellationToken);
}

/// <summary>
///     Stores a run from its billing CSV, in the same shape a calculator run stores it, so its files can be downloaded again.
/// </summary>
public class RunImporter(ApplicationDBContext dbContext, ICalcResultWriter calcResultWriter, IMaterialService materialService) : IRunImporter
{
    public async Task<int> ImportAsync(TextReader billingCsv, string fileName, RunImportDetails details, CancellationToken cancellationToken)
    {
        var file = BillingFileName.Parse(fileName);
        var csv = BillingCsv.Read(billingCsv);
        var header = RunSectionsParser.ReadHeader(csv);

        if (file.RunId != header.RunId)
            throw new FormatException($"The file name is for run {file.RunId} but the file is for run {header.RunId}");

        var lapcap = RunSectionsParser.ReadLapcapData(csv);
        var lateReporting = RunSectionsParser.ReadLateReportingTonnage(csv);
        var otherCost = RunSectionsParser.ReadParameterOtherCost(csv, header);
        var onePlusFour = RunSectionsParser.ReadOnePlusFourApportionment(csv);
        var comms = RunSectionsParser.ReadCommsCost(csv);
        var laDisposal = RunSectionsParser.ReadLaDisposalCostData(csv);
        var cancelled = ProducerSectionsParser.ReadCancelledProducers(csv);
        var scaledUp = ProducerSectionsParser.ReadScaledupProducers(csv, RunSectionsParser.MaterialCodes);
        var partial = ProducerSectionsParser.ReadPartialObligations(csv, RunSectionsParser.MaterialCodes);
        var fees = FeeTableParser.Read(csv, RunSectionsParser.MaterialCodes);
        var rejected = ProducerSectionsParser.ReadRejectedProducers(csv);
        var modulation = RunSectionsParser.ReadModulation(csv, header.RunId, await materialService.GetMaterials());
        var projected = ProjectedProducersParser.Read(csv, RunSectionsParser.MaterialCodes);

        if (fees.Warnings.Count > 0)
            throw new FormatException(string.Join(Environment.NewLine, fees.Warnings));

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        await StoreRunAsync(header, file, details, DefaultParameters(header, lateReporting, otherCost, comms, modulation, details), cancellationToken);

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
        await StoreBillingInstructionsAsync(header.RunId, fees.Producers, file, details.BillingFileCreatedBy ?? header.RunBy, cancellationToken);
        await StoreRejectedProducersAsync(header.RunId, rejected, cancellationToken);
        await StoreOrganisationsAsync(header.RunId, fees.Producers, cancellationToken);

        await transaction.CommitAsync(cancellationToken);
        return header.RunId;
    }

    private async Task StoreRunAsync(RunSectionsParser.RunHeader header, BillingFileName file, RunImportDetails details, IReadOnlyDictionary<string, string> defaultParameters, CancellationToken cancellationToken)
    {
        var lapcapMaster = await LapcapMasterAsync(header, cancellationToken);
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
            BillingFileSharedAt             = file.IsAuthorised ? details.BillingFileSharedAt ?? file.CreatedAt : null,
            BillingFileSharedBy             = file.IsAuthorised ? details.BillingFileSharedBy ?? details.BillingFileCreatedBy ?? header.RunBy : null,
            CreatedBy                       = header.RunBy,
            CreatedAt                       = header.RunDate,
            OrgPomDataLoadedAt              = header.OrgPomDataLoadedAt,
            LapcapDataMasterId              = lapcapMaster.Id,
            DefaultParameterSettingMasterId = parameterMaster.Id
        };

        run.CalculatorRunBillingFileMetadata.Add(new CalculatorRunBillingFileMetadata
        {
            BillingCsvFileName     = file.CsvFileName,
            BillingJsonFileName    = file.JsonFileName,
            BillingFileCreatedDate = file.CreatedAt,
            BillingFileCreatedBy   = details.BillingFileCreatedBy ?? header.RunBy
        });

        dbContext.CalculatorRuns.Add(run);

        await dbContext.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT dbo.calculator_run ON", cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT dbo.calculator_run OFF", cancellationToken);
    }

    // Only one LAPCAP and one parameter file is active per year, so a new upload closes the previous one, as the app does.
    // Runs in a year that used the same file share it.
    private async Task<LapcapDataMaster> LapcapMasterAsync(RunSectionsParser.RunHeader header, CancellationToken cancellationToken)
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
    ///     A billing file only lists accepted producers, so each Level 1 row there was accepted. The file doesn't say who
    ///     accepted it or when, so that's recorded as the billing file's creation.
    /// </summary>
    private async Task StoreBillingInstructionsAsync(int runId, IReadOnlyList<FeeDetail> producers, BillingFileName file, string acceptedBy, CancellationToken cancellationToken)
    {
        foreach (var producer in producers.Where(p => p.Level == "1"))
        {
            var instruction = producer.BillingInstruction!;
            dbContext.ProducerResultFileSuggestedBillingInstruction.Add(new ProducerResultFileSuggestedBillingInstruction
            {
                CalculatorRunId                         = runId,
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
                BillingInstructionAcceptReject          = BillingConstants.Action.Accepted,
                LastModifiedAcceptRejectBy              = acceptedBy,
                LastModifiedAcceptReject                = file.CreatedAt
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    ///     The Rejected Report's producers, with their names where the rejected producers section looks them up.
    /// </summary>
    private async Task StoreRejectedProducersAsync(int runId, IReadOnlyList<ProducerSectionsParser.RejectedProducer> rejected, CancellationToken cancellationToken)
    {
        foreach (var producer in rejected)
        {
            var isCancel = producer.SuggestedBillingInstruction == BillingConstants.Suggestion.Cancel;

            dbContext.ProducerResultFileSuggestedBillingInstruction.Add(new ProducerResultFileSuggestedBillingInstruction
            {
                CalculatorRunId                = runId,
                ProducerId                     = producer.ProducerId,
                SuggestedBillingInstruction    = producer.SuggestedBillingInstruction,
                SuggestedInvoiceAmount         = isCancel ? null : producer.SuggestedInvoiceAmount,
                CurrentYearInvoiceTotalToDate  = isCancel ? producer.SuggestedInvoiceAmount : null,
                BillingInstructionAcceptReject = BillingConstants.Action.Rejected,
                ReasonForRejection             = producer.Reason,
                LastModifiedAcceptRejectBy     = producer.ConfirmedBy,
                LastModifiedAcceptReject       = producer.ConfirmedAt
            });

            // A rejected cancellation's name comes from the run that last invoiced it.
            if (!isCancel)
                dbContext.ProducerDetail.Add(new ProducerDetail
                {
                    CalculatorRunId = runId,
                    ProducerId      = producer.ProducerId,
                    ProducerName    = producer.ProducerName,
                    TradingName     = producer.TradingName
                });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    ///     Each Level 1 producer's name as it was in this run. Later runs look producers' names up here, for example for
    ///     a rejected cancellation.
    /// </summary>
    private async Task StoreOrganisationsAsync(int runId, IReadOnlyList<FeeDetail> producers, CancellationToken cancellationToken)
    {
        dbContext.CalculatorRunOrganisations.AddRange(producers
            .Where(p => p.Level == "1")
            .Select(p => new CalculatorRunOrganisation
            {
                CalculatorRunId  = runId,
                OrganisationId   = p.ProducerId,
                OrganisationName = p.ProducerName,
                TradingName      = p.TradingName,
                StatusCode       = p.StatusCode,
                JoinerDate       = p.JoinerDate,
                LeaverDate       = p.LeaverDate
            }));

        await dbContext.SaveChangesAsync(cancellationToken);
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

        // Runs before modulation have no red modulation factor.
        parameters["REDM-RF"] = (modulation?.RedFactor ?? 0).ToString(CultureInfo.InvariantCulture);

        foreach (var (reference, value) in details.DefaultParameters)
            parameters[reference] = value;

        return parameters;
    }
}
