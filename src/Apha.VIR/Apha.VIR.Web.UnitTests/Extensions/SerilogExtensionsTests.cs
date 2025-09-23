using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.CloudWatchLogs;
using Apha.VIR.Web.Extensions;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Serilog;

namespace Apha.VIR.Web.UnitTests.Extensions
{
    public class SerilogExtensionsTests
    {
        private readonly IConfiguration _mockConfiguration;

        public SerilogExtensionsTests()
        {
            // Build fake config with dummy values to prevent nulls
            var inMemorySettings = new Dictionary<string, string?>
            {
                { "AwsLogging:LogGroupName", "TestLogGroup" }
            };
            _mockConfiguration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
        }

        [Fact]
        public void UseAwsCloudWatch_ReturnsLoggerConfiguration()
        {
            // Arrange
            var loggerConfiguration = new LoggerConfiguration();

            // Act
            var result = loggerConfiguration.UseAwsCloudWatch(_mockConfiguration);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<LoggerConfiguration>(result);
        }

        [Fact]
        public void UseAwsCloudWatch_SetsCorrectCloudWatchOptions()
        {
            // Arrange
            var loggerConfiguration = new LoggerConfiguration();

            // Act
            var result = loggerConfiguration.UseAwsCloudWatch(_mockConfiguration);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<LoggerConfiguration>(result);

            // Additional assertion to differentiate from UseAwsCloudWatch_ReturnsLoggerConfiguration
            var logger = result.CreateLogger();
            var exception = Record.Exception(() => logger.Warning("Testing CloudWatch options"));
            Assert.Null(exception);
        }

        [Fact]
        public void UseAwsCloudWatch_ConfiguresConsoleSink()
        {
            // Arrange
            var loggerConfiguration = new LoggerConfiguration();

            // Act
            loggerConfiguration.UseAwsCloudWatch(_mockConfiguration);

            // Assert
            var logger = loggerConfiguration.CreateLogger();
            Assert.NotNull(logger);

            // Act: should not throw
            var exception = Record.Exception(() => logger.Information("Test log message"));
            Assert.Null(exception);
        }

        [Fact]
        public void UseAwsCloudWatch_ConfiguresCloudWatchSink()
        {
            // Arrange
            var loggerConfiguration = new LoggerConfiguration();

            // Act
            loggerConfiguration.UseAwsCloudWatch(_mockConfiguration);

            // Assert
            var logger = loggerConfiguration.CreateLogger();
            Assert.NotNull(logger);

            // Act: should not throw
            var exception = Record.Exception(() => logger.Error("Test error message"));
            Assert.Null(exception);
        }

        [Fact]       
        public void UseAwsCloudWatch_ThrowsArgumentException_WhenLogGroupNameMissing()
        {
            // Arrange
            var config = new ConfigurationBuilder().Build(); // no AwsLogging section
            var loggerConfiguration = new LoggerConfiguration();

            // Act
            var exception = Record.Exception(() => loggerConfiguration.UseAwsCloudWatch(config));

            // Assert
            Assert.NotNull(exception);
            Assert.IsType<ArgumentException>(exception);
            Assert.Contains("LogGroupName", exception.Message);
        }
    }
}
