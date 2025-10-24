namespace EPR.Calculator.API.Data.DataModels;

public class ErrorType
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }

    public ICollection<ErrorReport> ErrorReports { get; } = new List<ErrorReport>();
}
