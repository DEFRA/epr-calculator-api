namespace EPR.Calculator.API.Options;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public record FeatureFlagOptions
{
    public const string SectionKey = "FeatureFlags";

    public bool UploadFssBillingFileToBlobStorage { get; init; }
}
