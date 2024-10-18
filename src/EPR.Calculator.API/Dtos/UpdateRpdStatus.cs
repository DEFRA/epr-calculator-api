using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.API.Dtos
{
    [ExcludeFromCodeCoverage]
    public class UpdateRpdStatus
    {
        public required int RunId { get; set; }

        public required string UpdateBy { get; set; }

        public required bool isSuccessful { get; set; }
    }
}
