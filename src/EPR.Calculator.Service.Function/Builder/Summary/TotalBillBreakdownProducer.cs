using EPR.Calculator.API.Data.DataModels;
﻿using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder.Summary;

public static class TotalBillBreakdownProducer
{
    public static void SetValues(ProducerFees producerFees)
    {
        foreach (var fee in producerFees.Details.Select(d => d.FeeDetail).Append(producerFees.Total))
        {
            fee.TotalBillBreakdown =
                fee.LADisposalCostsSection1
                + fee.CommsCostsSection2a
                + fee.CommsCostsSection2b
                + fee.CommsCostsSection2c
                + fee.SaOperatingCostsSection3
                + fee.LaDataPrepSection4
                + fee.SaSetupCostsSection5;
        }
    }
}
