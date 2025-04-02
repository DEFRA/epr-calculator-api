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
            this.Options = new Mock<DbContextOptions>().Object;
            this.Options = new DbContextOptionsBuilder().Options;
            this.TestClass = new TestApplicationDBContext(this.Options);
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
            this.TestClass.PublicOnConfiguring(optionsBuilder);

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
                this.OnConfiguring(optionsBuilder);
            }
        }
    }
}