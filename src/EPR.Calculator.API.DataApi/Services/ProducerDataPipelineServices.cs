using EPR.Calculator.Api.DataApi.AcceptedFileSelection;
using EPR.Calculator.Api.DataApi.Alignment;
using EPR.Calculator.Api.DataApi.ObligationDetermination;
using EPR.Calculator.Api.DataApi.PomEligibility;

namespace EPR.Calculator.Api.DataApi.Services;

/// <summary>
///     The business-rule stages <see cref="ProducerDataService" /> runs the streamed organisation/POM
///     data through, grouped into one constructor parameter rather than six.
/// </summary>
internal sealed class ProducerDataPipelineServices(
    IAcceptedFileSelector acceptedFileSelector,
    IProducerObligationDeterminer obligationDeterminer,
    IPomEligibilityFilter pomEligibilityFilter,
    IOrganisationPeriodFlagsCalculator organisationPeriodFlagsCalculator,
    IProducerErrorDetector errorDetector,
    IProducerPomAligner aligner)
{
    public IAcceptedFileSelector AcceptedFileSelector { get; } = acceptedFileSelector;

    public IProducerObligationDeterminer ObligationDeterminer { get; } = obligationDeterminer;

    public IPomEligibilityFilter PomEligibilityFilter { get; } = pomEligibilityFilter;

    public IOrganisationPeriodFlagsCalculator OrganisationPeriodFlagsCalculator { get; } = organisationPeriodFlagsCalculator;

    public IProducerErrorDetector ErrorDetector { get; } = errorDetector;

    public IProducerPomAligner Aligner { get; } = aligner;
}
