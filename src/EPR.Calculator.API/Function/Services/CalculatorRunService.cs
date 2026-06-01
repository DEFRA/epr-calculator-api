using System.Net.Mime;
using System.Text;
using System.Text.Json;
using EPR.Calculator.API.Models;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Services.DataLoading;

namespace EPR.Calculator.Service.Function.Services
{
    /// <summary>
    /// Interface for starting the calculator process.
    /// </summary>
    public interface ICalculatorRunService
    {
        /// <summary>
        /// Interface method to start the calculator process.
        /// </summary>
        /// <param name="calculatorRunParameter">The parameters required to run the calculator.</param>
        /// <param name="runName">The name of the calculator run.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating success or failure.</returns>
        Task<PreparedResult<string>> PrepareResultsFileAsync(CalculatorRunMessage calculatorRunParameter);
    }

    public class CalculatorRunService(
        IDataLoader dataLoader,
        IProducerDataTransposer producerDataTransposer,
        IPrepareCalcService prepareCalcService,
        IRpdStatusService statusService,
        IClassificationService classificationService,
        ILogger<CalculatorRunService> logger)
        : ICalculatorRunService
    {
        public async Task<PreparedResult<string>> PrepareResultsFileAsync(CalculatorRunMessage calculatorRunParameter)
        {
            try
            {
                await dataLoader.LoadData(calculatorRunParameter.RelativeYear);
                return await RunResultsFileCalculationAsync(calculatorRunParameter);
            }
            catch (OperationCanceledException ex)
            {
                logger.LogError(ex, "Prepare results file cancelled");
                return PreparedResult.Failure<string>();
            }
            catch (Exception ex)
            {
                await classificationService.UpdateRunClassification(calculatorRunParameter.CalculatorRunId, RunClassification.ERROR);
                logger.LogError(ex, "Prepare results file failed");
                return PreparedResult.Failure<string>();
            }
        }

        private async Task<PreparedResult<string>> RunResultsFileCalculationAsync(CalculatorRunMessage calculatorRunParameter)
        {
            var statusUpdateResponse = await statusService.UpdateRpdStatus(
                calculatorRunParameter.CalculatorRunId,
                calculatorRunParameter.CreatedBy,
                CancellationToken.None);

            if (statusUpdateResponse == RunClassification.RUNNING)
            {
                await producerDataTransposer.Transpose(calculatorRunParameter.CalculatorRunId, CancellationToken.None);

                return await prepareCalcService.PrepareCalcResultsAsync(
                    new CalcResultsRequestDto { RunId = calculatorRunParameter.CalculatorRunId, RelativeYear = calculatorRunParameter.RelativeYear },
                    calculatorRunParameter.RunName,
                    CancellationToken.None);
            }

            return PreparedResult.Failure<string>();
        }
    }
}
