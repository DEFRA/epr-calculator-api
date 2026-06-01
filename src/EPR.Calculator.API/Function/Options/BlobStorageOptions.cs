using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.Service.Function.Services;

namespace EPR.Calculator.Service.Function.Options;

/// <summary>
///     Configuration options for <see cref="BlobStorageService" />.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
[ExcludeFromCodeCoverage]
public record BlobStorageOptions
{
    public const string SectionKey = "BlobStorage";

    [Required(AllowEmptyStrings = false)] public string ConnectionString { get; init; } = null!;

    [Required(AllowEmptyStrings = false)] public string ResultFileCsvContainer { get; init; } = null!;

    [Required(AllowEmptyStrings = false)] public string BillingFileCsvContainer { get; init; } = null!;

    [Required(AllowEmptyStrings = false)] public string BillingFileJsonContainer { get; init; } = null!;
}
