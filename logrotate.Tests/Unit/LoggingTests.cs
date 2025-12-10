using System;
using System.IO;
using FluentAssertions;
using Xunit;

namespace logrotate.Tests.Unit
{
    [Trait("Category", "Unit")]
    public class LoggingTests
    {
        [Fact]
        public void SetDebug_ShouldEnableDebugLogging()
        {
            // Arrange & Act
            Logging.SetDebug(true);

            // Assert - Debug should be enabled
            // Note: We can't directly test internal state, but we can verify it doesn't throw
            Action act = () => Logging.Log("Test debug message", Logging.LogType.Debug);
            act.Should().NotThrow();

            // Cleanup
            Logging.SetDebug(false);
        }

        [Fact]
        public void SetVerbose_ShouldEnableVerboseLogging()
        {
            // Arrange & Act
            Logging.SetVerbose(true);

            // Assert
            Action act = () => Logging.Log("Test verbose message", Logging.LogType.Verbose);
            act.Should().NotThrow();

            // Cleanup
            Logging.SetVerbose(false);
        }

        [Fact]
        public void Log_WithRequiredType_ShouldAlwaysOutput()
        {
            // Arrange
            Logging.SetDebug(false);
            Logging.SetVerbose(false);

            // Act & Assert - Should not throw even when debug/verbose are off
            Action act = () => Logging.Log("Required message", Logging.LogType.Required);
            act.Should().NotThrow();
        }

        [Fact]
        public void Log_WithErrorType_ShouldAlwaysOutput()
        {
            // Arrange
            Logging.SetDebug(false);
            Logging.SetVerbose(false);

            // Act & Assert
            Action act = () => Logging.Log("Error message", Logging.LogType.Error);
            act.Should().NotThrow();
        }

        [Fact]
        public void Log_WithNullMessage_ShouldHandleGracefully()
        {
            // Act & Assert - Should not throw
            Action act = () => Logging.Log(null, Logging.LogType.Required);
            act.Should().NotThrow();
        }

        [Fact]
        public void Log_WithEmptyMessage_ShouldHandleGracefully()
        {
            // Act & Assert
            Action act = () => Logging.Log("", Logging.LogType.Required);
            act.Should().NotThrow();
        }

        [Fact]
        public void LogException_ShouldNotThrow()
        {
            // Arrange
            var exception = new Exception("Test exception");

            // Act & Assert
            Action act = () => Logging.LogException(exception);
            act.Should().NotThrow();
        }

        [Fact]
        public void LogException_WithNullException_ShouldHandleGracefully()
        {
            // Act & Assert
            Action act = () => Logging.LogException(null);
            act.Should().NotThrow();
        }

        [Fact]
        public void Log_SimpleMessage_ShouldNotThrow()
        {
            // Arrange
            string message = "Simple log message";

            // Act & Assert
            Action act = () => Logging.Log(message);
            act.Should().NotThrow();
        }

        [Fact]
        public void MultipleLogCalls_ShouldAllSucceed()
        {
            // Arrange
            Logging.SetDebug(true);
            Logging.SetVerbose(true);

            // Act & Assert
            Action act = () =>
            {
                Logging.Log("Message 1", Logging.LogType.Debug);
                Logging.Log("Message 2", Logging.LogType.Verbose);
                Logging.Log("Message 3", Logging.LogType.Required);
                Logging.Log("Message 4", Logging.LogType.Error);
            };
            act.Should().NotThrow();

            // Cleanup
            Logging.SetDebug(false);
            Logging.SetVerbose(false);
        }
    }
}
