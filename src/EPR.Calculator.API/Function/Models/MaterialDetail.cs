namespace EPR.Calculator.Service.Function.Models
{
    public record MaterialDetail
    {
        public int Id { get; set; }

        public required string Code { get; set; }

        public required string Name { get; set; }

        public required string Description { get; set; }
    }
}
