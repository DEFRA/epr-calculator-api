using System.Globalization;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.Services;

public interface IParameterService
{
    public Task<DefaultParameters> GetDefaultParameters(int runId);
}

public class ParameterService(ApplicationDBContext dbContext) : IParameterService
{
    public async Task<DefaultParameters> GetDefaultParameters(int runId)
    {
        var values = await (
            from run in dbContext.CalculatorRuns.AsNoTracking()
            join defaultMaster in dbContext.DefaultParameterSettings.AsNoTracking()
                on run.DefaultParameterSettingMasterId equals defaultMaster.Id
            join defaultDetail in dbContext.DefaultParameterSettingDetail.AsNoTracking()
                on defaultMaster.Id equals defaultDetail.DefaultParameterSettingMasterId
            where run.Id == runId
            select new
            {
                Key = defaultDetail.ParameterUniqueReferenceId,
                Value = defaultDetail.ParameterValue
            })
            .ToDictionaryAsync(x => x.Key, x => x.Value);

        decimal D(string key) =>
            decimal.Parse(values[key]);

        DateTime? DT(string key) =>
            values[key].Equals("NA", StringComparison.OrdinalIgnoreCase)
                ? null
                : DateTime.ParseExact(values[key], "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None);

        return new DefaultParameters
        {
            CommunicationCosts = new CommunicationCosts
            {
                ByMaterialCode = new Dictionary<string, decimal>
                {
                    ["AL"] = D("COMC-AL"),
                    ["FC"] = D("COMC-FC"),
                    ["GL"] = D("COMC-GL"),
                    ["OT"] = D("COMC-OT"),
                    ["PC"] = D("COMC-PC"),
                    ["PL"] = D("COMC-PL"),
                    ["ST"] = D("COMC-ST"),
                    ["WD"] = D("COMC-WD")
                },
                ByCountry = new ByCountryCostWithUk
                {
                    UnitedKingdom   = D("COMC-UK"), // TODO can we remove this?
                    England         = D("COMC-ENG"),
                    Wales           = D("COMC-WLS"),
                    Scotland        = D("COMC-SCT"),
                    NorthernIreland = D("COMC-NIR")
                }
            },

            SchemeAdministratorOperatingCostsByCountry = new ByCountryCost
            {
                England         = D("SAOC-ENG"),
                Wales           = D("SAOC-WLS"),
                Scotland        = D("SAOC-SCT"),
                NorthernIreland = D("SAOC-NIR")
            },

            SchemeSetupCostsByCountry = new ByCountryCost
            {
                England         = D("SCSC-ENG"),
                Wales           = D("SCSC-WLS"),
                Scotland        = D("SCSC-SCT"),
                NorthernIreland = D("SCSC-NIR")
            },

            LocalAuthorityDataPreparationCostsByCountry = new ByCountryCost
            {
                England         = D("LAPC-ENG"),
                Wales           = D("LAPC-WLS"),
                Scotland        = D("LAPC-SCT"),
                NorthernIreland = D("LAPC-NIR")
            },

            LateReportingTonnageByMaterialCode = new Dictionary<string, RamTonnageGroup>
            {
                ["AL"] = Create(D("LRET-AL"), D("LRET-AL-G"), D("LRET-AL-R")),
                ["FC"] = Create(D("LRET-FC"), D("LRET-FC-G"), D("LRET-FC-R")),
                ["GL"] = Create(D("LRET-GL"), D("LRET-GL-G"), D("LRET-GL-R")),
                ["OT"] = Create(D("LRET-OT"), D("LRET-OT-G"), D("LRET-OT-R")),
                ["PC"] = Create(D("LRET-PC"), D("LRET-PC-G"), D("LRET-PC-R")),
                ["PL"] = Create(D("LRET-PL"), D("LRET-PL-G"), D("LRET-PL-R")),
                ["ST"] = Create(D("LRET-ST"), D("LRET-ST-G"), D("LRET-ST-R")),
                ["WD"] = Create(D("LRET-WD"), D("LRET-WD-G"), D("LRET-WD-R"))
            },

            MaterialityThreshold = new Threshold
            {
                AmountIncrease  = D("MATT-AI"),
                AmountDecrease  = D("MATT-AD"),
                PercentIncrease = D("MATT-PI"),
                PercentDecrease = D("MATT-PD")
            },

            TonnageChangeThreshold = new Threshold
            {
                AmountIncrease  = D("TONT-AI"),
                AmountDecrease  = D("TONT-AD"),
                PercentIncrease = D("TONT-PI"),
                PercentDecrease = D("TONT-PD")
            },

            BadDebtProvision    = D("BADEBT-P"),
            RedModulationFactor = D("REDM-RF"),
            CutOffDate          = DT("COFF-DT")
        };
    }

    public static RamTonnageGroup Create(decimal amber, decimal green, decimal red) =>
        new()
        {
            Amber = amber,
            Green = green,
            Red   = red,
            Total = amber + green + red
        };
}

public record DefaultParameters
{
    public required CommunicationCosts CommunicationCosts { get; init; }
    public required ByCountryCost SchemeAdministratorOperatingCostsByCountry { get; init; }
    public required ByCountryCost SchemeSetupCostsByCountry { get; init; }
    public required ByCountryCost LocalAuthorityDataPreparationCostsByCountry { get; init; }
    public required IReadOnlyDictionary<string, RamTonnageGroup> LateReportingTonnageByMaterialCode { get; init; }
    public required Threshold MaterialityThreshold { get; init; }
    public required Threshold TonnageChangeThreshold { get; init; }
    public required decimal BadDebtProvision { get; init; }
    public required decimal RedModulationFactor { get; init; }
    public DateTime? CutOffDate { get; init; }
}

public record CommunicationCosts
{
    public required IReadOnlyDictionary<string, decimal> ByMaterialCode { get; init; }
    public required ByCountryCostWithUk ByCountry { get; init; }
}

public record ByCountryCostWithUk : ByCountryCost
{
    public required decimal UnitedKingdom { get; init; }
}

public record Threshold
{
    public required decimal AmountIncrease { get; init; }
    public required decimal AmountDecrease { get; init; }
    public required decimal PercentIncrease { get; init; }
    public required decimal PercentDecrease { get; init; }
}
