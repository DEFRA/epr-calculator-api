namespace EPR.Calculator.API.Models
{
    public class MaterialDetail
    {
        public int Id { get; set; }

        public required string Code { get; set; }

        public required string Name { get; set; }

        public required string Description { get; set; }
    }
}
