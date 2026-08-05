using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Services;

namespace EPR.Calculator.Service.Function.UnitTests.TestHelpers.Helpers;

public static class DefaultParametersHelper
{
    public static DefaultParameters Empty()
    {
        var materials = MaterialHelper.GetMaterials();

        return new DefaultParameters
        {
            CommunicationCosts = new CommunicationCosts
            {
                ByMaterialCode = materials.ToDictionary(m => m.Code, _ => 0m),
                ByCountry = new ByCountryCostWithUk
                {
                    UnitedKingdom = 0,
                    England = 0,
                    Wales = 0,
                    Scotland = 0,
                    NorthernIreland = 0
                }
            },

            SchemeAdministratorOperatingCostsByCountry = new ByCountryCost
            {
                England = 0,
                Wales = 0,
                Scotland = 0,
                NorthernIreland = 0
            },

            SchemeSetupCostsByCountry = new ByCountryCost
            {
                England = 0,
                Wales = 0,
                Scotland = 0,
                NorthernIreland = 0
            },

            LocalAuthorityDataPreparationCostsByCountry = new ByCountryCost
            {
                England = 0,
                Wales = 0,
                Scotland = 0,
                NorthernIreland = 0
            },

            LateReportingTonnageByMaterialCode = materials.ToDictionary(
                m => m.Code,
                _ => RamTonnageGroup.Zero),

            MaterialityThreshold = new()
            {
                AmountIncrease = 0,
                AmountDecrease = 0,
                PercentIncrease = 0,
                PercentDecrease = 0
            },

            TonnageChangeThreshold = new()
            {
                AmountIncrease = 0,
                AmountDecrease = 0,
                PercentIncrease = 0,
                PercentDecrease = 0
            },

            BadDebtProvision = 0,
            RedModulationFactor = 0,
            CutOffDate = null
        };
    }

    public static IEnumerable<DefaultParameterSettingDetail> ToDetails(
        DefaultParameters parameters,
        int masterId)
    {
        foreach (var material in parameters.CommunicationCosts.ByMaterialCode)
        {
            yield return Detail(masterId, $"COMC-{material.Key}", material.Value);
        }

        yield return Detail(masterId, "COMC-UK", parameters.CommunicationCosts.ByCountry.UnitedKingdom);
        yield return Detail(masterId, "COMC-ENG", parameters.CommunicationCosts.ByCountry.England);
        yield return Detail(masterId, "COMC-WLS", parameters.CommunicationCosts.ByCountry.Wales);
        yield return Detail(masterId, "COMC-SCT", parameters.CommunicationCosts.ByCountry.Scotland);
        yield return Detail(masterId, "COMC-NIR", parameters.CommunicationCosts.ByCountry.NorthernIreland);

        yield return Detail(masterId, "BADEBT-P", parameters.BadDebtProvision);
        yield return Detail(masterId, "REDM-RF", parameters.RedModulationFactor);

        foreach (var material in parameters.LateReportingTonnageByMaterialCode)
        {
            yield return Detail(masterId, $"LRET-{material.Key}", material.Value.Amber ?? 0);
            yield return Detail(masterId, $"LRET-{material.Key}-G", material.Value.Green ?? 0);
            yield return Detail(masterId, $"LRET-{material.Key}-R", material.Value.Red ?? 0);
        }

        yield return Detail(masterId, "MATT-AI", parameters.MaterialityThreshold.AmountIncrease);
        yield return Detail(masterId, "MATT-AD", parameters.MaterialityThreshold.AmountDecrease);
        yield return Detail(masterId, "MATT-PI", parameters.MaterialityThreshold.PercentIncrease);
        yield return Detail(masterId, "MATT-PD", parameters.MaterialityThreshold.PercentDecrease);

        yield return Detail(masterId, "TONT-AI", parameters.TonnageChangeThreshold.AmountIncrease);
        yield return Detail(masterId, "TONT-AD", parameters.TonnageChangeThreshold.AmountDecrease);
        yield return Detail(masterId, "TONT-PI", parameters.TonnageChangeThreshold.PercentIncrease);
        yield return Detail(masterId, "TONT-PD", parameters.TonnageChangeThreshold.PercentDecrease);

        yield return Detail(masterId, "SAOC-ENG", parameters.SchemeAdministratorOperatingCostsByCountry.England);
        yield return Detail(masterId, "SAOC-WLS", parameters.SchemeAdministratorOperatingCostsByCountry.Wales);
        yield return Detail(masterId, "SAOC-SCT", parameters.SchemeAdministratorOperatingCostsByCountry.Scotland);
        yield return Detail(masterId, "SAOC-NIR", parameters.SchemeAdministratorOperatingCostsByCountry.NorthernIreland);

        yield return Detail(masterId, "SCSC-ENG", parameters.SchemeSetupCostsByCountry.England);
        yield return Detail(masterId, "SCSC-WLS", parameters.SchemeSetupCostsByCountry.Wales);
        yield return Detail(masterId, "SCSC-SCT", parameters.SchemeSetupCostsByCountry.Scotland);
        yield return Detail(masterId, "SCSC-NIR", parameters.SchemeSetupCostsByCountry.NorthernIreland);

        yield return Detail(masterId, "LAPC-ENG", parameters.LocalAuthorityDataPreparationCostsByCountry.England);
        yield return Detail(masterId, "LAPC-WLS", parameters.LocalAuthorityDataPreparationCostsByCountry.Wales);
        yield return Detail(masterId, "LAPC-SCT", parameters.LocalAuthorityDataPreparationCostsByCountry.Scotland);
        yield return Detail(masterId, "LAPC-NIR", parameters.LocalAuthorityDataPreparationCostsByCountry.NorthernIreland);

        yield return Detail(masterId, "COFF-DT", parameters.CutOffDate.HasValue
            ? parameters.CutOffDate.Value.ToString("dd/MM/yyyy")
            : "NA");
    }

    private static DefaultParameterSettingDetail Detail(
        int masterId,
        string reference,
        decimal value)
    {
        return new DefaultParameterSettingDetail
        {
            DefaultParameterSettingMasterId = masterId,
            ParameterUniqueReferenceId = reference,
            ParameterValue = value.ToString()
        };
    }

    private static DefaultParameterSettingDetail Detail(
        int masterId,
        string reference,
        string value)
    {
        return new DefaultParameterSettingDetail
        {
            DefaultParameterSettingMasterId = masterId,
            ParameterUniqueReferenceId = reference,
            ParameterValue = value
        };
    }
}
