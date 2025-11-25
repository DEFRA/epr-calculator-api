namespace EPR.Calculator.API.UnitTests
{
    using AutoFixture;
    using EPR.Calculator.API.Data;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    /// Tests methods of <see cref="ApplicationDBContext"/>>.
    /// </summary>
    [TestClass]
    public class ApplicationDBContextTests
    {
        public ApplicationDBContextTests()
        {
            Options = new Mock<DbContextOptions<ApplicationDBContext>>().Object;
            Options = new DbContextOptionsBuilder<ApplicationDBContext>().Options;
            TestClass = new TestApplicationDBContext(Options);
        }

        private TestApplicationDBContext TestClass { get; init; }

        private DbContextOptions Options { get; init; }

        [TestMethod]
        public void CanCallOnConfiguring()
        {
            // Arrange
            var fixture = new Fixture();
            var optionsBuilder = fixture.Create<DbContextOptionsBuilder>();

            // Act
            TestClass.PublicOnConfiguring(optionsBuilder);

            // Assert
            Assert.IsTrue(optionsBuilder.IsConfigured);
        }

        /// <summary>
        /// Test class to expose the protected "OnConfiguring" method for testing.
        /// </summary>
        private class TestApplicationDBContext(DbContextOptions options) : ApplicationDBContext((DbContextOptions<ApplicationDBContext>)options)
        {
            public void PublicOnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                OnConfiguring(optionsBuilder);
            }
        }
    }
}