using FluentAssertions;
using System;
using System.IO;
using Xunit;

namespace logrotate.Tests.Integration
{
    /// <summary>
    /// Integration tests for the minage directive functionality.
    /// The minage directive prevents rotation of log files that are younger than
    /// the specified number of days. This is useful to avoid rotating logs too frequently.
    /// </summary>
    public class MinAgeDirectiveTests : IntegrationTestBase
    {
        [Fact]
        public void RotateLog_WithMinAge1AndOldFile_ShouldRotate()
        {
            // Tests that minage allows rotation of files older than specified days

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Old log content\n");

            // Set file time to 2 days ago
            File.SetLastWriteTime(logFile, DateTime.Now.AddDays(-2));

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    minage 1
    rotate 3
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act - Force rotation
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - File should be rotated (it's older than 1 day)
                File.Exists($"{logFile}.1").Should().BeTrue("file older than minage should be rotated");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithMinAge1AndNewFile_ShouldNotRotate()
        {
            // Tests that minage prevents rotation of files younger than specified days

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Fresh log content\n");

            // File is just created (age = 0 days, less than minage 1)

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    minage 1
    rotate 3
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act - Force rotation (but minage should prevent it)
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - File should NOT be rotated (it's younger than 1 day)
                File.Exists($"{logFile}.1").Should().BeFalse("file younger than minage should not be rotated");
                File.Exists(logFile).Should().BeTrue("original file should remain");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithMinAge7AndWeekOldFile_ShouldRotate()
        {
            // Tests minage with 7 days (1 week)

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Week old log\n");

            // Set file time to 8 days ago
            File.SetLastWriteTime(logFile, DateTime.Now.AddDays(-8));

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    minage 7
    rotate 3
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - File should be rotated (older than 7 days)
                File.Exists($"{logFile}.1").Should().BeTrue("file older than 7 days should be rotated");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithMinAge7AndRecentFile_ShouldNotRotate()
        {
            // Tests minage prevents rotation of files younger than 7 days

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Recent log\n");

            // Set file time to 5 days ago (younger than minage 7)
            File.SetLastWriteTime(logFile, DateTime.Now.AddDays(-5));

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    minage 7
    rotate 3
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - File should NOT be rotated (younger than 7 days)
                File.Exists($"{logFile}.1").Should().BeFalse("file younger than 7 days should not be rotated");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithMinAgeAndSize_ShouldRespectBothConditions()
        {
            // Tests that minage works together with size-based rotation

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, new string('X', 10000)); // Large file

            // File is fresh (just created)

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    minage 1
    size 1k
    rotate 3
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act - File is large enough, but too young
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Should NOT rotate despite size (minage prevents it)
                File.Exists($"{logFile}.1").Should().BeFalse("file should not rotate when younger than minage, even if size is exceeded");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithMinAgeAndDaily_ShouldWorkTogether()
        {
            // Tests that minage works with time-based rotation (daily)

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Log content\n");

            // Set file to just under minage (12 hours old)
            File.SetLastWriteTime(logFile, DateTime.Now.AddHours(-12));

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    minage 1
    daily
    rotate 3
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act - Force rotation
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Should NOT rotate (file is younger than 1 day)
                File.Exists($"{logFile}.1").Should().BeFalse("file younger than minage should not rotate even with force and daily");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithMinAge30AndMultipleFiles_ShouldFilterCorrectly()
        {
            // Tests that minage correctly filters multiple files

            // Arrange
            string oldLog = Path.Combine(TestDir, "old.log");
            string newLog = Path.Combine(TestDir, "new.log");

            File.WriteAllText(oldLog, "Old log\n");
            File.WriteAllText(newLog, "New log\n");

            // Set old log to 35 days ago
            File.SetLastWriteTime(oldLog, DateTime.Now.AddDays(-35));
            // New log is fresh (just created)

            string stateFile = Path.Combine(TestDir, "state.txt");
            string wildcard = Path.Combine(TestDir, "*.log");
            string configContent = $@"
{wildcard} {{
    minage 30
    rotate 3
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Only old log should be rotated
                File.Exists($"{oldLog}.1").Should().BeTrue("old log (>30 days) should be rotated");
                File.Exists($"{newLog}.1").Should().BeFalse("new log (<30 days) should not be rotated");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithMinAgeExactlyAtBoundary_ShouldNotRotate()
        {
            // Tests edge case: file exactly at minage boundary should not rotate

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Boundary test\n");

            // Set file to exactly 2 days ago (but slightly less, e.g., 2 days minus 1 hour)
            File.SetLastWriteTime(logFile, DateTime.Now.AddDays(-2).AddHours(1));

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    minage 2
    rotate 3
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Should NOT rotate (file is slightly younger than 2 full days)
                File.Exists($"{logFile}.1").Should().BeFalse("file at boundary (not quite minage days) should not rotate");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void ParseConfig_WithMinAgeDirective_ShouldParseSuccessfully()
        {
            // Tests that the minage directive is properly parsed

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "test");

            string configContent = $@"
{logFile} {{
    minage 5
    rotate 1
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                var exitCode = RunLogRotate(configFile);

                // Assert
                exitCode.Should().Be(0, "config with 'minage 5' should parse successfully");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithMinAgeAndCompress_ShouldWorkTogether()
        {
            // Tests that minage works with compression

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Log content for compression\n");

            // Set file to 3 days ago
            File.SetLastWriteTime(logFile, DateTime.Now.AddDays(-3));

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    minage 2
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

                // Assert - File should be rotated and compressed
                File.Exists($"{logFile}.1.gz").Should().BeTrue("old file should be rotated and compressed");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }
    }
}
