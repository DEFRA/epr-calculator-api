using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.API.Dtos
{
    [ExcludeFromCodeCoverage]
    public class UpdateRpdStatus
    {
        public required int RunId { get; set; }

        public required string UpdatedBy { get; set; }

        public required bool IsSuccessful { get; set; }
    }
}
