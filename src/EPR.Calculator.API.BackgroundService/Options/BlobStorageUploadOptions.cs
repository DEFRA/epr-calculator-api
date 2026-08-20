using System.ComponentModel.DataAnnotations;

namespace EPR.Calculator.API.BackgroundService.Options;

/// <summary>
///     Configuration options for <see cref="BlobStorageService" />.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public record BlobStorageUploadOptions
{
    public const string SectionKey = "BlobStorage";

    [Required(AllowEmptyStrings = false)] public string ConnectionString { get; init; } = null!;

    [Required(AllowEmptyStrings = false)] public string ResultFileCsvContainer { get; init; } = null!;

    [Required(AllowEmptyStrings = false)] public string BillingFileCsvContainer { get; init; } = null!;

    [Required(AllowEmptyStrings = false)] public string BillingFileJsonContainer { get; init; } = null!;
}
