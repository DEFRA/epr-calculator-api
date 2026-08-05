using EPR.Calculator.API.Data.DataModels;
﻿using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder.Summary;

public static class OnePlus2A2B2CProducer
{
    public static void SetValues(ProducerFees producerFees)
    {
        var headerTotal = producerFees.Total.TotalOnePlus2A2B2CWithBadDebt();
        foreach (var fee in producerFees.Details.Select(fee => fee.FeeDetail))
        {
            fee.TotalOnePlus2A2B2CWithBadDebtPercentage = GetOverallProducerPercentage(fee, headerTotal);
        }
        producerFees.Total.TotalOnePlus2A2B2CWithBadDebtPercentage = GetOverallProducerPercentage(producerFees.Total, headerTotal);
    }

    private static decimal GetOverallProducerPercentage(FeeDetail fee, decimal totalOnePlus2A2B2CFeeWithBadDebtProvision)
    {
        return totalOnePlus2A2B2CFeeWithBadDebtProvision == 0
            ? 0
            : (fee.TotalOnePlus2A2B2CWithBadDebt() / totalOnePlus2A2B2CFeeWithBadDebtProvision) * 100;
    }
}
