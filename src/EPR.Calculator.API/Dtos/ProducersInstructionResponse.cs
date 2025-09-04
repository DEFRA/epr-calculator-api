namespace EPR.Calculator.API.Dtos;

public record ProducersInstructionResponse
{
    public ProducersInstructionSummary? ProducersInstructionSummary { get; set; }

    public List<ProducersInstructionDetail>? ProducersInstructionDetails { get; set; }
}