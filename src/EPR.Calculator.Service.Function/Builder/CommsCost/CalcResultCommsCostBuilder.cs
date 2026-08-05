using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Utils;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Features.Common;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Utils;
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

public class CalcResultCommsCostBuilder(ApplicationDBContext context)
    : ICalcResultCommsCostBuilder
{
    public const string TwoCCommsCostByCountry = "2c Comms Costs - by Country";
    public const string TwoBCommsCostUkWide = "2b Comms Costs - UK wide";

    public async Task<CalcResultCommsCost> ConstructAsync(
        RunContext runContext,
        IImmutableList<MaterialDetail> materialDetails,
        CalcResultOnePlusFourApportionment apportionment,
        CalcResultLateReportingTonnage calcResultLateReportingTonnage
    )
    {
        var apportionmentDetail = apportionment.OnePlusFourApportionment;

        var producerReportedMaterials = await GetProducerReportedMaterials(runContext);
        var commsCostByMaterial = materialDetails.Select(material =>
        {
            var hhTonnage  = producerReportedMaterials.Where(x => x.MaterialId == material.Id && x.PackagingType == PackagingTypes.Household                ).Sum(x => x.PackagingTonnage);
            var pbTonnage  = producerReportedMaterials.Where(p => p.MaterialId == material.Id && p.PackagingType == PackagingTypes.PublicBin                ).Sum(p => p.PackagingTonnage);
            var hdcTonnage = producerReportedMaterials.Where(p => p.MaterialId == material.Id && p.PackagingType == PackagingTypes.HouseholdDrinksContainers).Sum(p => p.PackagingTonnage);

            var commsMatCost = MathUtils.RoundAwayFromZero(runContext.DefaultParameters.CommunicationCosts.ByMaterialCode[material.Code], 2);
            var commsCost    = new CalcResultCommsCostCommsCostByMaterial
            {
                Cost = new ByCountryCost
                {
                    England         = commsMatCost * apportionmentDetail.England         / 100,
                    Wales           = commsMatCost * apportionmentDetail.Wales           / 100,
                    Scotland        = commsMatCost * apportionmentDetail.Scotland        / 100,
                    NorthernIreland = commsMatCost * apportionmentDetail.NorthernIreland / 100
                },
                TotalCost                        = commsMatCost,
                HouseholdPackagingWasteTonnage   = hhTonnage,
                PublicBinTonnage                 = pbTonnage,
                HouseholdDrinksContainersTonnage = hdcTonnage,
                LateReportingTonnage             = calcResultLateReportingTonnage.ByMaterial[material.Code].Total
            };

            return (material.Code, commsCost);
        }).ToDictionary();

        var uk     = runContext.DefaultParameters.CommunicationCosts.ByCountry.UnitedKingdom;
        var ukCost = new ByCountryCost
        {
            England         = uk * apportionmentDetail.England         / 100,
            Wales           = uk * apportionmentDetail.Wales           / 100,
            Scotland        = uk * apportionmentDetail.Scotland        / 100,
            NorthernIreland = uk * apportionmentDetail.NorthernIreland / 100
        };

        return new CalcResultCommsCost()
        {
            OnePlusFourApportionment = apportionmentDetail,
            ByMaterial               = commsCostByMaterial,
            CommsCostUkWide          = ukCost,
            CommsCostByCountry       = runContext.DefaultParameters.CommunicationCosts.ByCountry
        };
    }

    public async Task<List<ProducerMaterialPackaging>> GetProducerReportedMaterials(RunContext runContext)
    {
        return await (
            from run in context.CalculatorRuns
            join pd in context.ProducerDetail on run.Id equals pd.CalculatorRunId
            join mat in context.ProducerMaterialPackaging on pd.Id equals mat.ProducerDetailId
            where run.Id == runContext.RunId &&
                  mat.PackagingType != PackagingTypes.ConsumerWaste
            select mat
        ).Distinct().ToListAsync();
    }
}
