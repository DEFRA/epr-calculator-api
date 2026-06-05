using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Features.Common;
using EPR.Calculator.Service.Function.Models;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.Builder.CommsCost;

public interface ICalcResultCommsCostBuilder
{
    Task<CalcResultCommsCost> ConstructAsync(
        RunContext runContext,
        IImmutableList<MaterialDetail> materialDetails,
        CalcResultOnePlusFourApportionment apportionment,
        CalcResultLateReportingTonnage calcResultLateReportingTonnage
    );
}

[ExcludeFromCodeCoverage]
public class CalcResultCommsCostBuilder(ApplicationDBContext context)
    : ICalcResultCommsCostBuilder
{
    public const string CommunicationCostByMaterial = "Communication costs by material";
    public const string CommunicationCostByCountry = "Communication costs by country";
    public const string TwoCCommsCostByCountry = "2c Comms Costs - by Country";
    public const string Uk = "United Kingdom";
    public const string TwoBCommsCostUkWide = "2b Comms Costs - UK wide";
    public const string OnePlusFourApportionment = "1 + 4 Apportionment %s";

    public async Task<CalcResultCommsCost> ConstructAsync(
        RunContext runContext,
        IImmutableList<MaterialDetail> materialDetails,
        CalcResultOnePlusFourApportionment apportionment,
        CalcResultLateReportingTonnage calcResultLateReportingTonnage
    )
    {
        var apportionmentDetail = apportionment.OnePlusFourApportionment;

        var allDefaultResults = await (
            from run in context.CalculatorRuns
            join defaultMaster in context.DefaultParameterSettings on run.DefaultParameterSettingMasterId equals defaultMaster.Id
            join defaultDetail in context.DefaultParameterSettingDetail on defaultMaster.Id equals defaultDetail.DefaultParameterSettingMasterId
            join defaultTemplate in context.DefaultParameterTemplateMasterList on defaultDetail.ParameterUniqueReferenceId equals defaultTemplate.ParameterUniqueReferenceId
            where run.Id == runContext.RunId
            select new CalcCommsBuilderResult
            {
                ParameterValue = defaultDetail.ParameterValue,
                ParameterType = defaultTemplate.ParameterType,
                ParameterCategory = defaultTemplate.ParameterCategory
            }).Distinct().ToListAsync();

        var materialDefaults = allDefaultResults
            .Where(x => x.ParameterType == CommunicationCostByMaterial
                        && materialDetails.Select(m => m.Name).Contains(x.ParameterCategory))
            .ToImmutableArray();

        var producerReportedMaterials = await GetProducerReportedMaterials(runContext);

        var commsCostByMaterial = materialDetails.Select(material =>
        {
            var hhTonnage = producerReportedMaterials.Where(x => x.MaterialId == material.Id && x.PackagingType != PackagingTypes.PublicBin && x.PackagingType != PackagingTypes.HouseholdDrinksContainers)
                .Sum(x => x.PackagingTonnage);

            var lateReportingTonnage = calcResultLateReportingTonnage.ByMaterial[material.Code];
            var pbTonnage = producerReportedMaterials.Where(p => p.MaterialId == material.Id && p.PackagingType == PackagingTypes.PublicBin).Sum(p => p.PackagingTonnage);
            var hdcTonnage = producerReportedMaterials.Where(p => p.MaterialId == material.Id && p.PackagingType == PackagingTypes.HouseholdDrinksContainers).Sum(p => p.PackagingTonnage);

            var materialDefault = materialDefaults.Single(m => m.ParameterCategory == material.Name);
            var total = Math.Round(materialDefault.ParameterValue, 2);
            var commsCost = new CalcResultCommsCostCommsCostByMaterial{
                Cost = new ByCountryCost
                {
                    England         = total * apportionmentDetail.England         / 100,
                    Wales           = total * apportionmentDetail.Wales           / 100,
                    Scotland        = total * apportionmentDetail.Scotland        / 100,
                    NorthernIreland = total * apportionmentDetail.NorthernIreland / 100
                },
                TotalCost                      = total,
                HouseholdPackagingWasteTonnage = hhTonnage,
                PublicBinTonnage               = pbTonnage,
                HouseholdDrinksContainersTonnage = hdcTonnage,
                LateReportingTonnage           = lateReportingTonnage.Total
            };

            return (material.Code, commsCost);
        }).ToDictionary();

        var commsCostByUk =
            allDefaultResults.Single(x =>
                x.ParameterType == CommunicationCostByCountry && x.ParameterCategory == Uk);

        var ukCost = new ByCountryCost
        {
            England         = commsCostByUk.ParameterValue * apportionmentDetail.England         / 100,
            Wales           = commsCostByUk.ParameterValue * apportionmentDetail.Wales           / 100,
            Scotland        = commsCostByUk.ParameterValue * apportionmentDetail.Scotland        / 100,
            NorthernIreland = commsCostByUk.ParameterValue * apportionmentDetail.NorthernIreland / 100
        };

        var commsCostByCountryList = GetCommsCostByCountry(allDefaultResults);

        return new CalcResultCommsCost()
        {
            OnePlusFourApportionment = apportionmentDetail,
            ByMaterial               = commsCostByMaterial,
            CommsCostUkWide          = ukCost,
            CommsCostByCountry       = commsCostByCountryList
        };
    }

    public async Task<List<ProducerReportedMaterialProjected>> GetProducerReportedMaterials(RunContext runContext)
    {
        return await (
            from run in context.CalculatorRuns
            join pd in context.ProducerDetail on run.Id equals pd.CalculatorRunId
            join mat in context.ProducerReportedMaterialProjected on pd.Id equals mat.ProducerDetailId
            join material in context.Material on mat.MaterialId equals material.Id
            where run.Id == runContext.RunId &&
                  // TODO need the following filter?
                  mat.PackagingType != null &&
                  (mat.PackagingType == PackagingTypes.Household ||
                   mat.PackagingType == PackagingTypes.PublicBin ||
                   (mat.PackagingType == PackagingTypes.HouseholdDrinksContainers &&
                    material.Code == MaterialCodes.Glass))
            select mat
        ).Distinct().ToListAsync();
    }

    private static ByCountryCost GetCommsCostByCountry(
        IReadOnlyCollection<CalcCommsBuilderResult> allDefaultResults)
    {
        var englandValue =
            allDefaultResults.Single(x =>
                x.ParameterType == CommunicationCostByCountry &&
                x.ParameterCategory == "England").ParameterValue;
        var walesValue =
            allDefaultResults.Single(x =>
                x.ParameterType == CommunicationCostByCountry &&
                x.ParameterCategory == "Wales").ParameterValue;
        var niValue =
            allDefaultResults.Single(x =>
                x.ParameterType == CommunicationCostByCountry &&
                x.ParameterCategory == "Northern Ireland").ParameterValue;
        var scotlandValue =
            allDefaultResults.Single(x =>
                x.ParameterType == CommunicationCostByCountry &&
                x.ParameterCategory == "Scotland").ParameterValue;

        return new ByCountryCost
        {
            England         = englandValue,
            Wales           = walesValue,
            Scotland        = scotlandValue,
            NorthernIreland = niValue
        };
    }
}
