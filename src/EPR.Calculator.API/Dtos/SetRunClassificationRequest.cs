using EPR.Calculator.API.Data.DataTypes;
using EPR.Calculator.API.Data.Enums;

namespace EPR.Calculator.API.Dtos;

public class SetRunClassificationRequest
{
    public required int RunId { get; set; }

    public required RunClassification Classification { get; set; }
}
