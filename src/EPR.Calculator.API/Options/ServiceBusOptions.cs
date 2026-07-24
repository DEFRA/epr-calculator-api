using System.ComponentModel.DataAnnotations;

namespace EPR.Calculator.API.Options;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public record ServiceBusOptions
{
    public const string SectionKey = "ServiceBus";

    [Required(AllowEmptyStrings = false)] public string ConnectionString { get; init; } = null!;
    [Required(AllowEmptyStrings = false)] public string QueueName { get; init; } = null!;
}
