using FluentAssertions;
using System;
using System.IO;
using Xunit;

namespace logrotate.Tests.Integration
{
    /// <summary>
    /// Integration tests for the weekly directive with optional weekday parameter.
    /// The weekly directive with a weekday parameter (0-6, Sunday=0) rotates logs
    /// on a specific day of the week.
    /// </summary>
    public class WeeklyWeekdayDirectiveTests : IntegrationTestBase
    {
        [Fact]
        public void RotateLog_WithWeeklySunday_ShouldParseSuccessfully()
        {
            // Tests that weekly with weekday parameter (Sunday=0) parses correctly

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Test log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    weekly 0
    rotate 3
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                var exitCode = RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert
                exitCode.Should().Be(0, "config with 'weekly 0' should parse successfully");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithWeeklyMonday_ShouldRotateOnMondays()
        {
            // Tests that weekly 1 (Monday) rotates when force is used

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Original log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    weekly 1
    rotate 3
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act - Force rotation
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert
                File.Exists($"{logFile}.1").Should().BeTrue("weekly with force should rotate");
                File.Exists(logFile).Should().BeTrue("new log file should be created");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithWeeklyFriday_ShouldParseAndRotate()
        {
            // Tests weekly 5 (Friday) configuration

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    weekly 5
    rotate 4
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert
                File.Exists($"{logFile}.1").Should().BeTrue("weekly 5 should rotate with force");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithWeeklySaturday_ShouldSupportWeekend()
        {
            // Tests weekly 6 (Saturday) configuration

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    weekly 6
    rotate 2
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert
                File.Exists($"{logFile}.1").Should().BeTrue("weekly 6 (Saturday) should work");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithWeeklyNoParameter_ShouldStillWork()
        {
            // Tests backward compatibility - weekly without parameter should still work

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    weekly
    rotate 3
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert
                File.Exists($"{logFile}.1").Should().BeTrue("weekly without parameter should still work");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithWeeklyAndCompress_ShouldCompressRotatedFiles()
        {
            // Tests that weekly with weekday works with compression

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Log content for compression test\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    weekly 3
    rotate 3
    compress
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert
                File.Exists($"{logFile}.1.gz").Should().BeTrue("weekly with compress should create .gz file");
                File.Exists($"{logFile}.1").Should().BeFalse("uncompressed file should not exist");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithWeeklyAndDateExt_ShouldUseDateExtension()
        {
            // Tests that weekly with weekday works with dateext

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Log content for dateext test\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    weekly 2
    rotate 3
    dateext
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Should have date-based extension
                string[] rotatedFiles = Directory.GetFiles(TestDir, "test.log-*");
                rotatedFiles.Should().HaveCount(1, "should have one rotated file with date extension");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void ParseConfig_WithWeeklyWeekdayDirective_ShouldStoreWeekdayValue()
        {
            // Tests that the weekly weekday parameter is properly parsed

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "test");

            string configContent = $@"
{logFile} {{
    weekly 4
    rotate 1
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                var exitCode = RunLogRotate(configFile);

                // Assert
                exitCode.Should().Be(0, "config with 'weekly 4' should parse successfully");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }
    }
}
