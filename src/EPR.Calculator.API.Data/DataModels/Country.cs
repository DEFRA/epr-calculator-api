namespace EPR.Calculator.API.Data.DataModels;

public class Country
{
    public int Id { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }

    #region EF navigational properties

    public virtual ICollection<CountryApportionment> CountryApportionments { get; } = [];

    #endregion
}
