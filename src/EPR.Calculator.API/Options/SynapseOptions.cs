using System.ComponentModel.DataAnnotations;
using EPR.CommonDataService.DataApi.CommonDataApi.Infrastructure;

namespace EPR.Calculator.API.Options;

/// <summary>
///     Configuration options for <see cref="SynapseContext" />.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public record SynapseOptions
{
    public const string SectionKey = "Synapse";

    [Required(AllowEmptyStrings = false)] public string ConnectionString { get; init; } = null!;
}
