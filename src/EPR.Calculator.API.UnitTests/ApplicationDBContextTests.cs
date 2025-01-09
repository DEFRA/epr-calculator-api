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
        private TestApplicationDBContext TestClass { get; init; }

        private DbContextOptions Options { get; init; }

        public ApplicationDBContextTests()
        {
            Options = new Mock<DbContextOptions>().Object;
            Options = new DbContextOptionsBuilder().Options;
            TestClass = new TestApplicationDBContext(Options);
        }

        [TestMethod]
        public void CanCallOnConfiguring()
        {
            // Arrange
            var fixture = new Fixture();
            var optionsBuilder = fixture.Create<DbContextOptionsBuilder>();

            // Act
            TestClass.PublicOnConfiguring(optionsBuilder);

            // Assert
            Assert.AreEqual(true, optionsBuilder.IsConfigured);
        }

        /// <summary>
        /// Test class to expose the protected "OnConfiguring" method for testing.
        /// </summary>
        private class TestApplicationDBContext(DbContextOptions options) : ApplicationDBContext(options)
        {
            public void PublicOnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                base.OnConfiguring(optionsBuilder);
            }
        }
    }

}