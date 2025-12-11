using System;
using System.IO;
using FluentAssertions;
using Xunit;

namespace logrotate.Tests.Integration
{
    [Trait("Category", "Integration")]
    public class BasicRotationTests : IntegrationTestBase
    {
        [Fact]
        public void RotateLog_WithRotateCount_ShouldCreateRotatedFiles()
        {
            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Original log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    rotate 3
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act - First rotation
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - After first rotation
                File.Exists($"{logFile}.1").Should().BeTrue("first rotation should create .1 file");
                File.Exists(logFile).Should().BeTrue("original log should be recreated with 'create' directive");

                // Act - Second rotation
                File.WriteAllText(logFile, "Second log content\n");
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - After second rotation
                File.Exists($"{logFile}.1").Should().BeTrue(".1 file should exist");
                File.Exists($"{logFile}.2").Should().BeTrue(".2 file should exist after second rotation");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_ExceedingRotateCount_ShouldDeleteOldestFile()
        {
            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    rotate 2
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act - Rotate 3 times (exceeds rotate count of 2)
                for (int i = 0; i < 3; i++)
                {
                    File.WriteAllText(logFile, $"Log content {i}\n");
                    RunLogRotate("-s", stateFile, "-f", configFile);
                }

                // Assert - Should only have .1 and .2, not .3
                File.Exists($"{logFile}.1").Should().BeTrue();
                File.Exists($"{logFile}.2").Should().BeTrue();
                File.Exists($"{logFile}.3").Should().BeFalse("oldest file should be deleted when exceeding rotate count");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithMissingOk_ShouldNotFailOnMissingFile()
        {
            // Arrange
            string logFile = Path.Combine(TestDir, "nonexistent.log");
            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    missingok
    rotate 2
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act & Assert - Should not return error code
                int exitCode = RunLogRotate("-s", stateFile, "-f", configFile);
                exitCode.Should().Be(0, "missingok should allow missing files");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithNotIfEmpty_ShouldSkipEmptyFiles()
        {
            // Arrange
            string logFile = Path.Combine(TestDir, "empty.log");
            File.WriteAllText(logFile, ""); // Create empty file

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    notifempty
    rotate 2
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert
                File.Exists($"{logFile}.1").Should().BeFalse("empty file should not be rotated with notifempty");
                File.Exists(logFile).Should().BeTrue("original empty file should still exist");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithWildcard_ShouldRotateAllMatchingFiles()
        {
            // Arrange
            string log1 = Path.Combine(TestDir, "app1.log");
            string log2 = Path.Combine(TestDir, "app2.log");
            File.WriteAllText(log1, "App 1 log\n");
            File.WriteAllText(log2, "App 2 log\n");

            string wildcardPattern = Path.Combine(TestDir, "*.log");
            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{wildcardPattern} {{
    rotate 2
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert
                File.Exists($"{log1}.1").Should().BeTrue("app1.log should be rotated");
                File.Exists($"{log2}.1").Should().BeTrue("app2.log should be rotated");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }
    }
}
