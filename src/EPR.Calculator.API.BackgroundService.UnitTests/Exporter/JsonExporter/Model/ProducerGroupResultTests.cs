using EPR.Calculator.API.BackgroundService.Exporter.JsonExporter.Model;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.BackgroundService.UnitTests.TestHelpers.TestData;

namespace EPR.Calculator.API.BackgroundService.UnitTests.Exporter.JsonExporter.Model;

/// <summary>
/// Tests for the 2026-schema grouping of the flat per-row producer summary into one
/// <see cref="ProducerGroupResult"/> (with <see cref="ProducerMemberResult"/> members) per producerID.
/// </summary>
[TestClass]
public class ProducerGroupResultTests
{
    private static readonly IImmutableList<MaterialDetail> Materials = TestDataHelper.GetMaterialDetails();

    private static FeeDetail Row(int producerId, string subsidiaryId, string producerName, string level) =>
        new()
        {
            ProducerId   = producerId,
            SubsidiaryId = subsidiaryId,
            ProducerName = producerName,
            Level        = level,
        };

    [TestMethod]
    public void From_SingleOrganisationProducer_HasOneMemberWithNullSubsidiaryId()
    {
        var row = Row(producerId: 1, subsidiaryId: string.Empty, producerName: "Solo Ltd", level: "1");

        var group = ProducerGroupResult.From(row, [row], Materials, applyModulation: false);

        Assert.AreEqual("1", group.ProducerID);
        var member = group.Members.Single();
        Assert.IsNull(member.SubsidiaryID);
        Assert.AreEqual("Solo Ltd", member.ProducerName);
    }

    [TestMethod]
    public void From_CompositeProducer_MembersCarryTheirOwnSubsidiaryIdAndName()
    {
        var aggregateRow = Row(producerId: 100001, subsidiaryId: string.Empty, producerName: string.Empty, level: "1");
        var selfMember    = Row(producerId: 100001, subsidiaryId: "100001", producerName: "Good L1 Ltd", level: "2");
        var subsidiary    = Row(producerId: 100001, subsidiaryId: "100002", producerName: "Good L2 Ltd", level: "2");

        var group = ProducerGroupResult.From(aggregateRow, [selfMember, subsidiary], Materials, applyModulation: false);

        Assert.AreEqual("100001", group.ProducerID);
        Assert.AreEqual(2, group.Members.Count());
        Assert.AreEqual("100001", group.Members.Single(m => m.ProducerName == "Good L1 Ltd").SubsidiaryID);
        Assert.AreEqual("100002", group.Members.Single(m => m.ProducerName == "Good L2 Ltd").SubsidiaryID);
    }
}
