using FluentAssertions;
using System;
using System.IO;
using System.Text.RegularExpressions;
using Xunit;

namespace logrotate.Tests.Integration
{
    /// <summary>
    /// Integration tests for date-related directive functionality.
    ///
    /// Directives tested:
    /// - nodateext: Disable date extensions (opposite of dateext)
    /// - dateyesterday/nodateyesterday: Use yesterday's date instead of today
    /// - datehourago/nodatehourago: Use hour-ago timestamp instead of current time
    /// </summary>
    public class DateDirectiveTests : IntegrationTestBase
    {
        [Fact]
        public void RotateLog_WithNoDateExt_ShouldUseNumberedRotation()
        {
            // Tests that nodateext disables date extensions

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    rotate 3
    dateext
    nodateext
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Should use numbered rotation, not date extension
                File.Exists($"{logFile}.1").Should().BeTrue("nodateext should disable date extensions");

                // No date-based files should exist
                string[] dateFiles = Directory.GetFiles(TestDir, "test.log-*");
                dateFiles.Should().BeEmpty("nodateext should prevent date-based file names");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithDateExtOnly_ShouldUseDateRotation()
        {
            // Tests that dateext enables date extensions (baseline test)

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
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

                // Assert - Should have date-based rotation
                string[] dateFiles = Directory.GetFiles(TestDir, "test.log-*");
                dateFiles.Should().HaveCount(1, "dateext should create date-based file names");

                // Verify it's actually a date format
                string fileName = Path.GetFileName(dateFiles[0]);
                fileName.Should().MatchRegex(@"test\.log-\d{8}$", "should have YYYYMMDD format");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithDateYesterday_ShouldUseYesterdaysDate()
        {
            // Tests that dateyesterday uses yesterday's date in the timestamp

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    rotate 3
    dateext
    dateyesterday
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Should have yesterday's date
                DateTime yesterday = DateTime.Now.AddDays(-1);
                string expectedDateSuffix = yesterday.ToString("yyyyMMdd");

                File.Exists($"{logFile}-{expectedDateSuffix}").Should().BeTrue(
                    $"dateyesterday should use yesterday's date ({expectedDateSuffix})");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithNoDateYesterday_ShouldUseTodaysDate()
        {
            // Tests that nodateyesterday uses today's date (default behavior)

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    rotate 3
    dateext
    dateyesterday
    nodateyesterday
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Should use today's date (nodateyesterday overrides dateyesterday)
                DateTime today = DateTime.Now;
                string expectedDateSuffix = today.ToString("yyyyMMdd");

                File.Exists($"{logFile}-{expectedDateSuffix}").Should().BeTrue(
                    $"nodateyesterday should use today's date ({expectedDateSuffix})");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithDateHourAgo_ShouldUseHourAgoTimestamp()
        {
            // Tests that datehourago uses hour-ago timestamp

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    rotate 3
    dateext
    dateformat -%Y%m%d-%H
    datehourago
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Should have hour-ago timestamp
                DateTime hourAgo = DateTime.Now.AddHours(-1);
                string expectedDateSuffix = hourAgo.ToString("yyyyMMdd-HH");

                File.Exists($"{logFile}-{expectedDateSuffix}").Should().BeTrue(
                    $"datehourago should use hour-ago timestamp ({expectedDateSuffix})");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithNoDateHourAgo_ShouldUseCurrentHour()
        {
            // Tests that nodatehourago uses current hour (default behavior)

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    rotate 3
    dateext
    dateformat -%Y%m%d-%H
    datehourago
    nodatehourago
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Should use current hour (nodatehourago overrides datehourago)
                DateTime now = DateTime.Now;
                string expectedDateSuffix = now.ToString("yyyyMMdd-HH");

                File.Exists($"{logFile}-{expectedDateSuffix}").Should().BeTrue(
                    $"nodatehourago should use current hour ({expectedDateSuffix})");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithDateYesterdayAndDateFormat_ShouldWorkTogether()
        {
            // Tests that dateyesterday works with custom date formats

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    rotate 3
    dateext
    dateformat -%Y-%m-%d
    dateyesterday
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Should use yesterday's date with custom format
                DateTime yesterday = DateTime.Now.AddDays(-1);
                string expectedSuffix = yesterday.ToString("yyyy-MM-dd");

                File.Exists($"{logFile}-{expectedSuffix}").Should().BeTrue(
                    "dateyesterday should work with custom dateformat");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithDateHourAgoAndMinutes_ShouldIncludeMinutes()
        {
            // Tests that datehourago preserves minute precision

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    rotate 3
    dateext
    dateformat -%Y%m%d-%H%M
    datehourago
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Should have hour-ago timestamp with minutes
                DateTime hourAgo = DateTime.Now.AddHours(-1);
                string expectedSuffix = hourAgo.ToString("yyyyMMdd-HHmm");

                File.Exists($"{logFile}-{expectedSuffix}").Should().BeTrue(
                    "datehourago should work with minute precision");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithDateYesterdayAndCompress_ShouldWorkTogether()
        {
            // Tests that dateyesterday works with compression

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Log content for compression\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    rotate 3
    dateext
    dateyesterday
    compress
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Should have yesterday's date with .gz extension
                DateTime yesterday = DateTime.Now.AddDays(-1);
                string expectedDateSuffix = yesterday.ToString("yyyyMMdd");

                File.Exists($"{logFile}-{expectedDateSuffix}.gz").Should().BeTrue(
                    "dateyesterday should work with compression");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithDateYesterdayPrecedence_ShouldOverrideDateHourAgo()
        {
            // Tests that when both dateyesterday and datehourago are set, dateyesterday takes precedence

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    rotate 3
    dateext
    datehourago
    dateyesterday
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Should use yesterday's date (dateyesterday wins)
                DateTime yesterday = DateTime.Now.AddDays(-1);
                string expectedDateSuffix = yesterday.ToString("yyyyMMdd");

                File.Exists($"{logFile}-{expectedDateSuffix}").Should().BeTrue(
                    "dateyesterday should take precedence over datehourago");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void ParseConfig_WithNoDateExtDirective_ShouldParseSuccessfully()
        {
            // Tests that the nodateext directive is properly parsed

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "test");

            string configContent = $@"
{logFile} {{
    nodateext
    rotate 1
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                var exitCode = RunLogRotate(configFile);

                // Assert
                exitCode.Should().Be(0, "config with nodateext directive should parse successfully");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void ParseConfig_WithDateYesterdayDirective_ShouldParseSuccessfully()
        {
            // Tests that the dateyesterday directive is properly parsed

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "test");

            string configContent = $@"
{logFile} {{
    dateyesterday
    rotate 1
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                var exitCode = RunLogRotate(configFile);

                // Assert
                exitCode.Should().Be(0, "config with dateyesterday directive should parse successfully");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void ParseConfig_WithDateHourAgoDirective_ShouldParseSuccessfully()
        {
            // Tests that the datehourago directive is properly parsed

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "test");

            string configContent = $@"
{logFile} {{
    datehourago
    rotate 1
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                var exitCode = RunLogRotate(configFile);

                // Assert
                exitCode.Should().Be(0, "config with datehourago directive should parse successfully");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithDateYesterdayGlobal_ShouldApplyToAll()
        {
            // Tests that dateyesterday in global config applies to all sections

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
dateext
dateyesterday

{logFile} {{
    rotate 3
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Global dateyesterday should apply
                DateTime yesterday = DateTime.Now.AddDays(-1);
                string expectedDateSuffix = yesterday.ToString("yyyyMMdd");

                File.Exists($"{logFile}-{expectedDateSuffix}").Should().BeTrue(
                    "global dateyesterday should apply");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithDateHourAgoGlobal_ShouldApplyToAll()
        {
            // Tests that datehourago in global config applies to all sections

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
dateext
dateformat -%Y%m%d-%H
datehourago

{logFile} {{
    rotate 3
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Global datehourago should apply
                DateTime hourAgo = DateTime.Now.AddHours(-1);
                string expectedDateSuffix = hourAgo.ToString("yyyyMMdd-HH");

                File.Exists($"{logFile}-{expectedDateSuffix}").Should().BeTrue(
                    "global datehourago should apply");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }
    }
}
