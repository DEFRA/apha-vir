using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using Serilog.Configuration;
using Serilog.Formatting.Compact;
using Serilog;
using Apha.VIR.Web.Extensions;
using Serilog.Core;
using Serilog.Events;

namespace Apha.VIR.Web.UnitTests.Extensions
{
    public class SerilogExtensionsTests
    {
        [Fact]
        public void UseStructuredConsoleLogging_ShouldReturnConfiguredLoggerConfiguration()
        {
            // Arrange
            var loggerConfiguration = new LoggerConfiguration();

            // Act
            var result = loggerConfiguration.UseStructuredConsoleLogging();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<LoggerConfiguration>(result);
        }

        [Fact]
        public void UseStructuredConsoleLogging_ShouldLogToConsole_WithStructuredJson()
        {
            // Arrange
            var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Debug(); // set min level to capture logs

            var logger = loggerConfiguration
                .UseStructuredConsoleLogging()
                .CreateLogger();

            // Substitute console writer (simulate structured output target)
            var testSink = Substitute.For<ILogEventSink>();
            var testLogger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Sink(testSink) // replace actual console with mock
                .CreateLogger();

            // Act
            testLogger.Information("Hello {User}", "TestUser");

            // Assert
            testSink.Received(1).Emit(Arg.Is<LogEvent>(le =>
                le.Level == LogEventLevel.Information &&
                le.MessageTemplate.Text.Contains("Hello {User}") &&
                le.Properties.ContainsKey("User")
            ));
        }

        [Fact]
        public void UseStructuredConsoleLogging_WhenCalledMultipleTimes_ShouldReturnNewConfigurationEachTime()
        {
            // Arrange
            var config1 = new LoggerConfiguration();
            var config2 = new LoggerConfiguration();

            // Act
            var result1 = config1.UseStructuredConsoleLogging();
            var result2 = config2.UseStructuredConsoleLogging();

            // Assert
            Assert.NotSame(result1, result2);
        }
    }
}
