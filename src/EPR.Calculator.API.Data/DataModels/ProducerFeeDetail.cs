namespace EPR.Calculator.API.Data.DataModels;

public class ProducerFeeDetail
{
    public int Id { get; set; }

    public int ProducerFeesId { get; set; }

    public required FeeDetail FeeDetail { get; set; }
}
