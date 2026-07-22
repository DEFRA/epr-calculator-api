using System.ComponentModel.DataAnnotations;
using EPR.Calculator.API.Services;

namespace EPR.Calculator.API.Options;

/// <summary>
///     Configuration options for <see cref="BlobStorageService" />.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public record BlobStorageOptions
{
    public const string SectionKey = "BlobStorage";

    [Required(AllowEmptyStrings = false)] public string ConnectionString { get; init; } = null!;
    [Required(AllowEmptyStrings = false)] public string ResultFileCsvContainer { get; init; } = null!;
    [Required(AllowEmptyStrings = false)] public string BillingFileCsvContainer { get; init; } = null!;
    [Required(AllowEmptyStrings = false)] public string BillingFileJsonContainer { get; init; } = null!;
    [Required(AllowEmptyStrings = false)] public string FssContainer { get; init; } = null!;
}
