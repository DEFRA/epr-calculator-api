using System.ComponentModel.DataAnnotations;

namespace EPR.Calculator.API.Options;

/// <summary>
///     Configuration options for the DataApi Synapse connection.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public record SynapseOptions
{
    public const string SectionKey = "Synapse";

    [Required(AllowEmptyStrings = false)] public string ConnectionString { get; init; } = null!;
}
