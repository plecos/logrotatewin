using System;
using System.IO;
using FluentAssertions;
using Xunit;

namespace logrotate.Tests.Integration
{
    [Trait("Category", "Integration")]
    public class SizeBasedIntegrationTests : IntegrationTestBase
    {
        [Fact]
        public void RotateLog_WithSize_ShouldRotateWhenExceedingSize()
        {
            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            TestHelpers.CreateTempLogFile(logFile, 2048); // 2KB file

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    size 1k
    rotate 2
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert
                File.Exists($"{logFile}.1").Should().BeTrue("file should be rotated when exceeding size");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithSize_ShouldNotRotateWhenBelowSize()
        {
            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            TestHelpers.CreateTempLogFile(logFile, 512); // 512 bytes

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    size 1k
    rotate 2
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act - Don't use -f flag, as force overrides size checks
                RunLogRotate("-s", stateFile, configFile);

                // Assert
                File.Exists($"{logFile}.1").Should().BeFalse("file should not be rotated when below size threshold");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithSizeAndCompress_ShouldRotateAndCompress()
        {
            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            TestHelpers.CreateTempLogFile(logFile, 2048); // 2KB file

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    size 1k
    compress
    rotate 2
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert
                File.Exists($"{logFile}.1.gz").Should().BeTrue("file should be rotated and compressed");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }
    }
}
