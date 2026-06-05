using EPR.Calculator.API.Data;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures;

namespace EPR.Calculator.Service.Function.UnitTests.TestHelpers;

/// <summary>
///     Helper base class for unit tests that leverages AutoFixture to address cross-cutting concerns.
/// </summary>
/// <typeparam name="TTestSubject"></typeparam>
/// <remarks>
///     Override <see cref="TestInitialize" /> and use
///     <see cref="FixtureFreezer.Freeze{T}(IFixture)">fixture.Freeze()</see> to register test subject dependencies (i.e.
///     mocks). Freeze will ensure they are automatically injected into the test subject.
/// </remarks>
public abstract class TestsFor<TTestSubject>
    where TTestSubject : class
{
    protected IFixture fixture { get; } = TestFixtures.New();
    protected ApplicationDBContext dbContext { get; private set; } = null!;
    protected TTestSubject testSubject { get; private set; } = null!;

    [TestInitialize]
    public void BaseTestInitialize()
    {
        dbContext = fixture.Freeze<ApplicationDBContext>();
        TestInitialize();
        dbContext.SaveChanges();
        testSubject = fixture.Create<TTestSubject>();
    }

    [TestCleanup]
    public void BaseCleanup()
    {
        dbContext.Database.EnsureDeleted();
        dbContext.Dispose();
        TestCleanup();
    }

    /// <summary>
    ///     Override this method to perform additional setup.
    /// </summary>
    /// <remarks>
    ///     This is called after the DB context is created, but before the test  subject is created.
    /// </remarks>
    protected virtual void TestInitialize()
    {
    }

    /// <summary>
    ///     Override this method to perform additional cleanup.
    /// </summary>
    /// <remarks>
    ///     This is called after the DB context is disposed.
    /// </remarks>
    protected virtual void TestCleanup()
    {
    }
}
