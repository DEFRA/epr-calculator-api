using EPR.Calculator.API.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EPR.Calculator.API.UnitTests.DataModels;

[TestClass]
public class JsonColumnTypeTests
{
    // The database stores these columns as UTF-8 varchar, but EF must keep sending values as nvarchar:
    // a varchar parameter is encoded with the database's default code page and loses non-Latin characters.
    [TestMethod]
    public void OwnedJsonColumns_AreModelledAsNvarcharMax()
    {
        using var context = new ApplicationDBContext();
        var model = context.GetService<IDesignTimeModel>().Model;
        var relationalModel = model.GetRelationalModel();

        var jsonColumns = model.GetEntityTypes()
            .Where(e => e.GetContainerColumnName() is not null && e.FindOwnership()?.PrincipalEntityType.IsMappedToJson() == false)
            .Select(e => relationalModel.FindTable(e.GetTableName()!, e.GetSchema())!.FindColumn(e.GetContainerColumnName()!)!)
            .ToList();

        jsonColumns.Count.ShouldBe(12);
        jsonColumns.ShouldAllBe(c => c.StoreType == "nvarchar(max)");
    }
}
