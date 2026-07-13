using System.ComponentModel.DataAnnotations;

namespace EPR.Calculator.API.Options;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public record ServiceBusOptions
{
    public const string SectionKey = "ServiceBus";

    [Required(AllowEmptyStrings = false)] public string ConnectionString { get; init; } = null!;
    [Required(AllowEmptyStrings = false)] public string QueueName { get; init; } = null!;

    [Required]
    [Range(typeof(TimeSpan), "00:00:01", "00:01:00")]
    public TimeSpan RetryDelay { get; init; } = TimeSpan.FromSeconds(3);

    [Range(1, 100)] public int MaxRetries { get; init; } = 2;
}
