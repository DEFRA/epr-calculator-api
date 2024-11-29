namespace EPR.Calculator.API.Dtos
{
    public class CalculatorRunDto
    {
        public int RunId {get; set; }
        public DateTime CreatedAt { get; set; }
        public string RunName { get; set; }
        public string FileExtension { get; set; }
        public DateTime? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int RunClassificationId { get; set; }
        public string RunClassificationStatus { get; set; }
    }
}
