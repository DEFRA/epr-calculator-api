namespace EPR.Calculator.API.Dtos;

public class GenericValidationResultDto
{
    public List<string> Errors { get; init; } = new();

    public bool IsInvalid => Errors.Count > 0;
}
