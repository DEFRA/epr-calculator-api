namespace EPR.Calculator.API.UnitTests.Services
{
    using System;
    using AutoFixture;
    using EPR.Calculator.API.Data;
    using EPR.Calculator.API.Services;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using Microsoft.Extensions.Configuration;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class CommandTimeoutServiceTests
    {
        public CommandTimeoutServiceTests()
        {
            this.Fixture = new Fixture();
            this.Configuration = new Mock<IConfiguration>();
            this.TestClass = new CommandTimeoutService(this.Configuration.Object);
            this.Database = new DatabaseFacade(new ApplicationDBContext());
        }

        private CommandTimeoutService TestClass { get; init; }

        private IFixture Fixture { get; init; }

        private Mock<IConfiguration> Configuration { get; init; }

        private DatabaseFacade Database { get; init; }

        [TestMethod]
        public void CanConstruct()
        {
            // Act
            var instance = new CommandTimeoutService();

            // Assert
            Assert.IsNotNull(instance);

            // Act
            instance = new CommandTimeoutService(this.Configuration.Object);

            // Assert
            Assert.IsNotNull(instance);
        }

        /// <summary>
        /// Checks that <see cref="CommandTimeoutService.SetCommandTimeout(DatabaseFacade, string)"/> sets
        /// the database's command timeout to the expected value.
        /// </summary>
        /// <param name="value">
        /// The timeout value (in minutes) that is to be read from the (mock) config file.
        /// </param>
        /// <param name="expectedValue">
        /// The value (in seconds) that the database timeout is expected to be set to.
        /// </param>
        [TestMethod]
        [DataRow(0, null)]
        [DataRow(1, 60)]
        [DataRow(2, 120)]
        public void CanCallSetCommandTimeout(double value, int? expectedValue)
        {
            // Arrange
            var configValueKey = this.Fixture.Create<string>();

            var mockValueSection = new Mock<IConfigurationSection>();
            mockValueSection.Setup(s => s.Value).Returns(value.ToString());

            var mockTimeoutSection = new Mock<IConfigurationSection>();
            mockTimeoutSection.Setup(x => x.GetSection(configValueKey))
                .Returns(mockValueSection.Object);

            this.Configuration.Setup(c => c.GetSection("Timeouts"))
                .Returns(mockTimeoutSection.Object);

            // Act
            this.TestClass.SetCommandTimeout(this.Database, configValueKey);
            var result = this.Database.GetCommandTimeout();

            // Assert
            Assert.AreEqual(expectedValue, result);
        }

        /// <summary>
        /// Checks that <see cref="CommandTimeoutService.SetCommandTimeout(DatabaseFacade, string)"/>
        /// throws an exception when an empty key is passed.
        /// </summary>
        /// <param name="value">The key value to test.</param>
        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("   ")]
        public void CannotCallSetCommandTimeoutWithInvalidKey(string value)
        {
            // Arrange
            Exception? result = null;

            // Act
            try
            {
                this.TestClass.SetCommandTimeout(this.Database, value);
            }
            catch (Exception ex)
            {
                result = ex;
            }

            // Assert
            Assert.IsInstanceOfType<ArgumentException>(result);
        }
    }
}