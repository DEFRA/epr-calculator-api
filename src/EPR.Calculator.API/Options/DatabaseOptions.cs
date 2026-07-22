using System.ComponentModel.DataAnnotations;
using EPR.Calculator.API.Data;

namespace EPR.Calculator.API.Options;

/// <summary>
///     Configuration options for <see cref="ApplicationDBContext" />.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public record DatabaseOptions
{
    public const string SectionKey = "Database";

    [Required(AllowEmptyStrings = false)] public string ConnectionString { get; init; } = null!;

    [Required]
    [Range(typeof(TimeSpan), "00:00:01", "01:00:00")]
    public TimeSpan CommandTimeout { get; init; } = TimeSpan.FromMinutes(15);
}
