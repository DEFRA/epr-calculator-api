using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.DataTypes;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Enums;
using EPR.Calculator.API.Mappers;
using EPR.Calculator.API.Models;
using EPR.Calculator.API.Services;
using EPR.Calculator.API.Validators;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;

namespace EPR.Calculator.API.Controllers;

[Route("v1")]
[SuppressMessage("Major Code Smell", "S6960:Controllers should not have mixed responsibilities", Justification = "Legacy tech debt to be addressed later.")]
[SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "Legacy tech debt to be addressed later.")]
public class CalculatorController(
    ApplicationDBContext dbContext,
    IConfiguration configuration,
    IStorageService storageService,
    IServiceBusService serviceBusService,
    ICalculatorRunStatusDataValidator runStatusValidator,
    ICalcRelativeYearRequestDtoDataValidator validator,
    IAvailableClassificationsService availableClassificationsService,
    ICalculationRunService calculationRunService)
    : ControllerBase
{
    [HttpPost]
    [Route("calculatorRun")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status424FailedDependency)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] CreateCalculatorRunDto request)
    {
        var claim = User.Claims.FirstOrDefault(x => x.Type == "name");
        if (claim == null)
            return new ObjectResult(CommonResources.NoClaimInRequest) { StatusCode = StatusCodes.Status401Unauthorized };

        var userName = claim.Value;

        // Return bad request if the model is invalid
        if (!ModelState.IsValid)
        {
            return StatusCode(
                StatusCodes.Status400BadRequest,
                ModelState.Values.SelectMany(x => x.Errors));
        }

        var isCalcAlreadyRunning = await dbContext.CalculatorRuns.AnyAsync(run => run.CalculatorRunClassificationId == (int)RunClassification.RUNNING);
        if (isCalcAlreadyRunning)
        {
            return new ObjectResult(new { Message = CommonResources.CalculationAlreadyRunning })
            {
                StatusCode = StatusCodes.Status422UnprocessableEntity
            };
        }

        var relativeYear = await dbContext.FindRelativeYearAsync(request.RelativeYear.Value);

        if (relativeYear is null)
        {
            return new ObjectResult(new { Message = CommonResources.InvalidRelativeYear })
            {
                StatusCode = StatusCodes.Status400BadRequest
            };
        }

        // Return failed dependency error if at least one of the dependent data not available for the relative year
        var dataPreCheckMessage = DataPreChecksBeforeInitialisingCalculatorRun(relativeYear.Value);
        if (!string.IsNullOrWhiteSpace(dataPreCheckMessage))
            return new ObjectResult(dataPreCheckMessage) { StatusCode = StatusCodes.Status424FailedDependency };

        if (!ValidateCalculatorRunName(request.CalculatorRunName, out var errorMessage))
        {
            return new ObjectResult(errorMessage)
            {
                StatusCode = StatusCodes.Status400BadRequest
            };
        }

        // Read configuration items: service bus connection string and queue name
        var serviceBusConnectionString = configuration.GetSection("ServiceBus").GetSection("ConnectionString").Value;
        var serviceBusQueueName = configuration.GetSection("ServiceBus").GetSection("QueueName").Value;

        if (string.IsNullOrWhiteSpace(serviceBusConnectionString))
            throw new ConfigurationErrorsException(CommonResources.ServiceBusConnectionStringMissing);

        if (string.IsNullOrWhiteSpace(serviceBusQueueName))
            throw new ConfigurationErrorsException(CommonResources.ServiceBusQueueNameMissing);

        // Get active default parameter settings master
        var activeDefaultParameterSettingsMaster = await dbContext.DefaultParameterSettings
            .SingleAsync(x => x.EffectiveTo == null && x.RelativeYear == relativeYear.Value);

        // Get active lapcap data master
        var activeLapcapDataMaster = await dbContext.LapcapDataMaster
            .SingleAsync(data => data.RelativeYear == relativeYear.Value && data.EffectiveTo == null);

        // Setup calculator run details
        var calculatorRun = new CalculatorRun
        {
            Name = request.CalculatorRunName,
            RelativeYear = relativeYear.Value,
            CreatedBy = userName,
            CreatedAt = DateTime.UtcNow,
            CalculatorRunClassificationId = (int)RunClassification.RUNNING,
            DefaultParameterSettingMasterId = activeDefaultParameterSettingsMaster.Id,
            LapcapDataMasterId = activeLapcapDataMaster.Id,
            BillingRunStatus = BillingRunStatus.None
        };

        using (var transaction = await dbContext.Database.BeginTransactionAsync())
        {
            try
            {
                // Save calculator run details to the database
                await dbContext.CalculatorRuns.AddAsync(calculatorRun);
                await dbContext.SaveChangesAsync();

                // Setup message
                var calculatorRunMessage = new CalculatorRunMessage
                {
                    CalculatorRunId = calculatorRun.Id,
                    CreatedBy = User.Identity?.Name ?? userName
                };

                // Send message
                await serviceBusService.SendMessage(serviceBusQueueName, calculatorRunMessage);

                // All good, commit transaction
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                // Error, rollback transaction
                await transaction.RollbackAsync();
                throw;
            }
        }

        // Return accepted status code: Accepted
        return new ObjectResult(null) { StatusCode = StatusCodes.Status202Accepted };
    }

    [HttpPost]
    [Route("calculatorRuns")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCalculatorRuns([FromBody] CalculatorRunsParamsDto request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return StatusCode(StatusCodes.Status400BadRequest, ModelState.Values.SelectMany(x => x.Errors));

        var runDtos = await dbContext.CalculatorRuns
            .Where(run => run.RelativeYear == request.RelativeYear)
            .Select(CalcRunMapper.ToDto)
            .OrderByDescending(run => run.CreatedAt)
            .ToListAsync(cancellationToken);

        return new ObjectResult(runDtos) { StatusCode = StatusCodes.Status200OK };
    }

    [HttpGet]
    [Route("calculatorRuns/{runIdOrName}")]
    [ProducesResponseType(typeof(CalculatorRunDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCalculatorRun(string runIdOrName, CancellationToken cancellationToken)
    {
        IQueryable<CalculatorRun> query = dbContext.CalculatorRuns;

        query = int.TryParse(runIdOrName, out var runId)
            ? query.Where(run => run.Id == runId)
            : query.Where(run => EF.Functions.Like(run.Name, runIdOrName));

        var runDto = await query
            .Select(CalcRunMapper.ToDto)
            .SingleOrDefaultAsync(cancellationToken);

        if (runDto == null)
            return new NotFoundObjectResult(string.Format(CommonResources.UnableToFindRun, runIdOrName));

        return new ObjectResult(runDto);
    }

    [HttpGet]
    [Route("DownloadResult/{runId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> DownloadResultFile(int runId)
    {
        if (!ModelState.IsValid)
        {
            var badRequest = Results.BadRequest(ModelState.Values.SelectMany(x => x.Errors));
            return badRequest;
        }

        var csvFileMetadata = await dbContext.CalculatorRunCsvFileMetadata.SingleOrDefaultAsync(metadata => metadata.CalculatorRunId == runId && metadata.FileName != null && metadata.FileName.Contains("_Results"));
        if (csvFileMetadata == null)
            return Results.NotFound(string.Format(CommonResources.NoCSVFileFound, runId));

        return await storageService.DownloadFile(csvFileMetadata.FileName, csvFileMetadata.BlobUri);
    }

    [HttpGet]
    [Route("RelativeYears")]
    [ProducesResponseType(typeof(IEnumerable<RelativeYearDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RelativeYears()
    {
        var relativeYears = await dbContext.CalculatorRunRelativeYears
            .Select(y => y.Value)
            .ToListAsync();

        return Ok(relativeYears);
    }

    [HttpGet]
    [Route("ClassificationByRelativeYear")]
    [ProducesResponseType(typeof(RelativeYearClassificationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ClassificationByRelativeYear([FromQuery] CalcRelativeYearRequestDto request)
    {
        if (!ModelState.IsValid)
            return StatusCode(StatusCodes.Status400BadRequest, ModelState.Values.SelectMany(x => x.Errors));

        var validationResult = await validator.Validate(request);
        if (validationResult.IsInvalid)
            return BadRequest(validationResult.Errors);

        var relativeYear = new RelativeYear(request.RelativeYearValue);
        var classifications = await availableClassificationsService.GetAvailableClassificationsForRelativeYearAsync(request);
        if (classifications.Count == 0)
            return NotFound(CommonResources.NoClassificationsFound);

        var runs = await calculationRunService.GetDesignatedRunsByFinanialYear(relativeYear);

        var runDto = RelativeYearClassificationsMapper.Map(relativeYear, classifications, runs);

        return Ok(runDto);
    }

    [HttpDelete]
    [Route("calculatorRuns/{runId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteCalculatorRun(int runId, CancellationToken cancellationToken)
    {
        var run = await dbContext.CalculatorRuns
            .SingleOrDefaultAsync(run => run.Id == runId, cancellationToken);

        if (run == null || run.CalculatorRunClassificationId == (int)RunClassification.DELETED)
            return NoContent();

        var request = new CalculatorRunStatusUpdateDto
        {
            RunId = runId,
            ClassificationId = (int) RunClassification.DELETED
        };

        // Perform basic validation on classification status
        var validationResult = runStatusValidator.Validate(run, request);

        if (validationResult.IsInvalid)
            return new ObjectResult(validationResult.Errors) { StatusCode = StatusCodes.Status422UnprocessableEntity };

        // Perform validation to check other designated runs are not in progress and not already completed for the same relative year
        var designatedRuns = await calculationRunService.GetDesignatedRunsByFinanialYear(run.RelativeYear, cancellationToken);

        validationResult = runStatusValidator.Validate(designatedRuns, run, request);

        if (validationResult.IsInvalid)
            return new ObjectResult(validationResult.Errors) { StatusCode = StatusCodes.Status422UnprocessableEntity };

        run.CalculatorRunClassificationId = (int) RunClassification.DELETED;
        run.UpdatedAt = DateTime.UtcNow;
        run.UpdatedBy = User.GetDisplayName();

        dbContext.CalculatorRuns.Update(run);
        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private string DataPreChecksBeforeInitialisingCalculatorRun(RelativeYear relativeYear)
    {
        // Get active default parameter settings for the given relative year
        var activeDefaultParameterSettings = dbContext.DefaultParameterSettings
            .SingleOrDefault(x => x.EffectiveTo == null && x.RelativeYear == relativeYear);

        // Get active Lapcap data for the given relative year
        var activeLapcapData = dbContext.LapcapDataMaster
            .SingleOrDefault(data => data.RelativeYear == relativeYear && data.EffectiveTo == null);

        // Return no active default paramater settings and lapcap data message
        if (activeDefaultParameterSettings == null && activeLapcapData == null)
            return string.Format(CommonResources.DataNotAvaialbleForRelativeYear, relativeYear);

        // Return no active default parameter settings found message
        if (activeDefaultParameterSettings == null)
            return string.Format(CommonResources.DefaultParameterNotAvailable, relativeYear);

        // Return no active lapcap data found message
        if (activeLapcapData == null)
            return string.Format(CommonResources.LapcapDataNotAvailable, relativeYear);

        // All good, return empty string
        return string.Empty;
    }

    private bool ValidateCalculatorRunName(string runName, [NotNullWhen(false)] out string? errorMessage)
    {
        errorMessage = null;

        if(int.TryParse(runName, out _))
            errorMessage = string.Format(CommonResources.CalculatorRunNameNotNumber);

        if (dbContext.CalculatorRuns.Any(run => EF.Functions.Like(run.Name, runName)))
            errorMessage = string.Format(CommonResources.CalculatorRunNameExists, runName);

        return errorMessage == null;
    }
}
