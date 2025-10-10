namespace EPR.Calculator.API.Dtos;

public class ClassifiedCalculatorRunDto
{
    public int RunId { get; init; }

    public DateTime CreatedAt { get; init; }

    public required string RunName { get; init; }

    public int RunClassificationId { get; init; }

    public required string RunClassificationStatus { get; init; }

    public DateTime? UpdatedAt { get; init; }

    public string? BillingFileAuthorisedBy { get; init; }

    public DateTime? BillingFileAuthorisedDate { get; init; }
}