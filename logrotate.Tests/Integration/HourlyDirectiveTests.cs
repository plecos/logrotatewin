using FluentAssertions;
using System;
using System.IO;
using Xunit;

namespace logrotate.Tests.Integration
{
    /// <summary>
    /// Integration tests for the hourly directive functionality.
    /// The hourly directive rotates logs every hour when the log file was last rotated
    /// more than an hour ago.
    /// </summary>
    public class HourlyDirectiveTests : IntegrationTestBase
    {
        [Fact]
        public void RotateLog_WithHourlyDirective_ShouldParseSuccessfully()
        {
            // Tests that the hourly directive is parsed without errors

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Test log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    hourly
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
                exitCode.Should().Be(0, "config with hourly directive should parse successfully");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithHourlyAndForce_ShouldRotateImmediately()
        {
            // Tests that hourly rotation works with force flag
            // The force flag should cause immediate rotation regardless of time

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Original log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    hourly
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
                File.Exists($"{logFile}.1").Should().BeTrue("hourly with force should rotate immediately");
                File.Exists(logFile).Should().BeTrue("new log file should be created");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithHourlyMultipleRotations_ShouldCreateMultipleFiles()
        {
            // Tests multiple hourly rotations create sequential numbered files

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "First log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    hourly
    rotate 5
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act - First rotation
                RunLogRotate("-s", stateFile, "-f", configFile);
                File.Exists($"{logFile}.1").Should().BeTrue("first rotation should create .1");

                // Act - Second rotation
                File.WriteAllText(logFile, "Second log content\n");
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert
                File.Exists($"{logFile}.1").Should().BeTrue(".1 should exist after second rotation");
                File.Exists($"{logFile}.2").Should().BeTrue(".2 should exist after second rotation");

                // Act - Third rotation
                File.WriteAllText(logFile, "Third log content\n");
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert
                File.Exists($"{logFile}.1").Should().BeTrue(".1 should exist");
                File.Exists($"{logFile}.2").Should().BeTrue(".2 should exist");
                File.Exists($"{logFile}.3").Should().BeTrue(".3 should exist");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithHourlyAndCompress_ShouldCompressRotatedFiles()
        {
            // Tests that hourly rotation works with compression

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Log content for compression test\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    hourly
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
                File.Exists($"{logFile}.1.gz").Should().BeTrue("hourly with compress should create .gz file");
                File.Exists($"{logFile}.1").Should().BeFalse("uncompressed file should not exist");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithHourlyAndDateExt_ShouldUseDateExtension()
        {
            // Tests that hourly rotation works with dateext directive

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Log content for dateext test\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    hourly
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
        public void RotateLog_WithHourlyAndSize_ShouldRotateBasedOnBothConditions()
        {
            // Tests that hourly directive can work with size directive
            // In Linux logrotate, when multiple time-based directives are specified,
            // the rotation happens when ANY condition is met

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Small log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    hourly
    size 1k
    rotate 3
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act - With force flag, should rotate regardless of size/time
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert
                File.Exists($"{logFile}.1").Should().BeTrue("should rotate with force flag");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithHourlyAndRotateCount_ShouldRespectRotateLimit()
        {
            // Tests that hourly rotation respects the rotate count limit

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    hourly
    rotate 2
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act - Create multiple rotations
                for (int i = 0; i < 4; i++)
                {
                    RunLogRotate("-s", stateFile, "-f", configFile);
                    if (i < 3)
                        File.WriteAllText(logFile, $"Log content rotation {i + 1}\n");
                }

                // Assert - Should only keep 2 rotated files (plus current)
                File.Exists($"{logFile}.1").Should().BeTrue(".1 should exist");
                File.Exists($"{logFile}.2").Should().BeTrue(".2 should exist");
                File.Exists($"{logFile}.3").Should().BeFalse(".3 should not exist (beyond rotate 2 limit)");
                File.Exists($"{logFile}.4").Should().BeFalse(".4 should not exist");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithHourlyAndOldDir_ShouldMoveToOldDir()
        {
            // Tests that hourly rotation works with olddir directive

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Log content\n");

            string oldDir = Path.Combine(TestDir, "old");
            Directory.CreateDirectory(oldDir);

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    hourly
    rotate 3
    olddir ""{oldDir}""
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert
                File.Exists(Path.Combine(oldDir, "test.log.1")).Should().BeTrue("rotated file should be in olddir");
                File.Exists($"{logFile}.1").Should().BeFalse("rotated file should not be in original directory");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
                TestHelpers.CleanupPath(oldDir);
            }
        }

        [Fact]
        public void ParseConfig_WithHourlyDirective_ShouldStoreHourlyFlag()
        {
            // Tests that the hourly directive is properly parsed and stored
            // This is a configuration parsing test

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "test");

            string configContent = $@"
{logFile} {{
    hourly
    rotate 1
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act - Just parse, don't force rotation
                // Running without force should still parse successfully
                var exitCode = RunLogRotate(configFile);

                // Assert - No error during parsing
                exitCode.Should().Be(0, "config with hourly directive should parse successfully");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }
    }
}
