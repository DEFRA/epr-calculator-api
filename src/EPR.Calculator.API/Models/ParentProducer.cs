namespace EPR.Calculator.API.Models
{
    public class ParentProducer
    {
        public int Id { get; set; }

        public int ProducerId { get; set; }

        public string ProducerName { get; set; } = string.Empty;
    }
}
