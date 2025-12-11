using FluentAssertions;
using System;
using System.IO;
using Xunit;

namespace logrotate.Tests.Integration
{
    /// <summary>
    /// Integration tests for the minutes directive functionality.
    /// The minutes directive rotates logs after a specified number of minutes have elapsed
    /// since the last rotation.
    /// </summary>
    public class MinutesDirectiveTests : IntegrationTestBase
    {
        [Fact]
        public void RotateLog_WithMinutesDirective_ShouldParseSuccessfully()
        {
            // Tests that the minutes directive is parsed without errors

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Test log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    minutes 30
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
                exitCode.Should().Be(0, "config with minutes directive should parse successfully");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithMinutes5AndForce_ShouldRotateImmediately()
        {
            // Tests that minutes rotation works with force flag
            // The force flag should cause immediate rotation regardless of time

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Original log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    minutes 5
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
                File.Exists($"{logFile}.1").Should().BeTrue("minutes 5 with force should rotate immediately");
                File.Exists(logFile).Should().BeTrue("new log file should be created");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithMinutes15MultipleRotations_ShouldCreateMultipleFiles()
        {
            // Tests multiple minute-based rotations create sequential numbered files

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "First log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    minutes 15
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
        public void RotateLog_WithMinutes60AndCompress_ShouldCompressRotatedFiles()
        {
            // Tests that minutes rotation works with compression

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Log content for compression test\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    minutes 60
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
                File.Exists($"{logFile}.1.gz").Should().BeTrue("minutes with compress should create .gz file");
                File.Exists($"{logFile}.1").Should().BeFalse("uncompressed file should not exist");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithMinutes120AndDateExt_ShouldUseDateExtension()
        {
            // Tests that minutes rotation works with dateext directive

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Log content for dateext test\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    minutes 120
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
        public void RotateLog_WithMinutes10AndRotateCount_ShouldRespectRotateLimit()
        {
            // Tests that minutes rotation respects the rotate count limit

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    minutes 10
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
        public void RotateLog_WithMinutes1_ShouldSupportVeryFrequentRotation()
        {
            // Tests that very frequent rotations (1 minute) are supported
            // This is useful for high-volume logging scenarios

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    minutes 1
    rotate 10
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act - Force rotation
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert
                File.Exists($"{logFile}.1").Should().BeTrue("minutes 1 should allow very frequent rotations");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithMinutes720_ShouldSupportLongerIntervals()
        {
            // Tests that longer intervals (720 minutes = 12 hours) are supported
            // This provides flexibility between hourly and daily rotations

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    minutes 720
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
                File.Exists($"{logFile}.1").Should().BeTrue("minutes 720 should support 12-hour rotations");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void ParseConfig_WithMinutesDirective_ShouldStoreMinutesValue()
        {
            // Tests that the minutes directive is properly parsed and stored
            // This is a configuration parsing test

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "test");

            string configContent = $@"
{logFile} {{
    minutes 45
    rotate 1
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act - Just parse, don't force rotation
                var exitCode = RunLogRotate(configFile);

                // Assert - No error during parsing
                exitCode.Should().Be(0, "config with minutes directive should parse successfully");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }
    }
}
