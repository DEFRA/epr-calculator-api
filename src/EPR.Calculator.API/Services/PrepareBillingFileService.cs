using System.Net;
using EPR.Calculator.API.Constants;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Services.Abstractions;
using EPR.Calculator.API.Utils;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.Services;

public class PrepareBillingFileService(ApplicationDBContext applicationDBContext) : IPrepareBillingFileService
{
    public async Task<ServiceProcessResponseDto> PrepareBillingFileAsync(int calculatorRunId)
    {
        var calculatorRun = await applicationDBContext.CalculatorRuns
        .SingleOrDefaultAsync(x => x.Id == calculatorRunId && Util.AcceptableRunStatusForBillingInstructions().Contains(x.CalculatorRunClassificationId));

        if (calculatorRun is null)
        {
            return new ServiceProcessResponseDto
            {
                StatusCode = HttpStatusCode.UnprocessableContent,
                Message = ErrorMessages.InvalidRunId,
            };
        }

        var rows = await applicationDBContext.ProducerResultFileSuggestedBillingInstruction
        .Where(x => x.CalculatorRunId == calculatorRunId)
        .ToListAsync();

        if (rows.Count == 0)
        {
            return new ServiceProcessResponseDto
            {
                StatusCode = HttpStatusCode.UnprocessableContent,
                Message = ErrorMessages.InvalidOrganisationId,
            };
        }


        return new ServiceProcessResponseDto
        {
            StatusCode = HttpStatusCode.OK,
            Message = "Billing file prepared successfully (stub)."
        };

        // TODO
        //return await _calcResultsFileBuilder.BuildBillingFileAsync(calculatorRunId, rows, CancellationToken.None)
        //    .ContinueWith(t =>
        //    {
        //        if (t.IsFaulted)
        //        {
        //            return new ServiceProcessResponseDto
        //            {
        //                StatusCode = HttpStatusCode.InternalServerError,
        //                Message = t.Exception?.Message ?? "An error occurred while preparing the billing file.",
        //            };
        //        }

        //        return new ServiceProcessResponseDto
        //        {
        //            StatusCode = HttpStatusCode.OK,
        //            Message = "Billing file prepared successfully.",
        //        };
        //    });
    }
}
