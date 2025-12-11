using FluentAssertions;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace logrotate.Tests.Integration
{
    /// <summary>
    /// Integration tests for the dateformat directive functionality.
    /// The dateformat directive specifies a custom date format for use with dateext.
    /// Supports %Y (year), %m (month), %d (day) placeholders.
    /// </summary>
    public class DateFormatDirectiveTests : IntegrationTestBase
    {
        [Fact]
        public void RotateLog_WithDateFormatYearMonthDay_ShouldUseCustomFormat()
        {
            // dateformat directive allows customizing the date format used with dateext
            // Default format is -%Y%m%d, this tests a custom format

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Original log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    rotate 2
    dateext
    dateformat -%Y-%m-%d
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - File should have custom date format with dashes
                DateTime now = DateTime.Now;
                string expectedDateSuffix = $"-{now.Year}-{now.Month:D2}-{now.Day:D2}";
                string expectedRotatedFile = $"{logFile}{expectedDateSuffix}";

                File.Exists(expectedRotatedFile).Should().BeTrue($"rotated file should exist with format {expectedDateSuffix}");
                File.Exists(logFile).Should().BeTrue("original log file should be recreated");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithDateFormatYearMonth_ShouldOmitDay()
        {
            // Tests dateformat with only year and month

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Original log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    rotate 2
    dateext
    dateformat -%Y%m
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - File should only have year and month
                DateTime now = DateTime.Now;
                string expectedDateSuffix = $"-{now.Year}{now.Month:D2}";
                string expectedRotatedFile = $"{logFile}{expectedDateSuffix}";

                File.Exists(expectedRotatedFile).Should().BeTrue($"rotated file should exist with format {expectedDateSuffix}");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithDateFormatCustomSeparator_ShouldUseUnderscores()
        {
            // Tests dateformat with custom separator (underscores instead of dashes)

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Original log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    rotate 2
    dateext
    dateformat _%Y_%m_%d
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - File should have underscores as separators
                DateTime now = DateTime.Now;
                string expectedDateSuffix = $"_{now.Year}_{now.Month:D2}_{now.Day:D2}";
                string expectedRotatedFile = $"{logFile}{expectedDateSuffix}";

                File.Exists(expectedRotatedFile).Should().BeTrue($"rotated file should exist with format {expectedDateSuffix}");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithDateFormatAndCompress_ShouldAppendGzExtension()
        {
            // Tests that dateformat works correctly with compression
            // The .gz extension should be added after the date

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Original log content that is long enough to be worth compressing\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    rotate 2
    dateext
    dateformat -%Y%m%d
    compress
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - File should have date format followed by .gz
                DateTime now = DateTime.Now;
                string expectedDateSuffix = $"-{now.Year}{now.Month:D2}{now.Day:D2}";
                string expectedRotatedFile = $"{logFile}{expectedDateSuffix}.gz";

                File.Exists(expectedRotatedFile).Should().BeTrue($"compressed rotated file should exist with format {expectedDateSuffix}.gz");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithoutDateExt_ShouldIgnoreDateFormat()
        {
            // dateformat directive only applies when dateext is enabled
            // Without dateext, files should use numeric extensions

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Original log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    rotate 2
    dateformat -%Y%m%d
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Without dateext, should use .1 not date format
                File.Exists($"{logFile}.1").Should().BeTrue("without dateext, should use numeric extension .1");

                // Verify no date-formatted file was created
                DateTime now = DateTime.Now;
                string unexpectedDateSuffix = $"-{now.Year}{now.Month:D2}{now.Day:D2}";
                string unexpectedRotatedFile = $"{logFile}{unexpectedDateSuffix}";
                File.Exists(unexpectedRotatedFile).Should().BeFalse("date format should not be used without dateext");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithDateFormatYearOnly_ShouldUseYearOnly()
        {
            // Tests dateformat with only year placeholder

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Original log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    rotate 2
    dateext
    dateformat -%Y
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - File should only have year
                DateTime now = DateTime.Now;
                string expectedDateSuffix = $"-{now.Year}";
                string expectedRotatedFile = $"{logFile}{expectedDateSuffix}";

                File.Exists(expectedRotatedFile).Should().BeTrue($"rotated file should exist with format {expectedDateSuffix}");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }
    }
}
