using FluentAssertions;
using System.IO;
using Xunit;

namespace logrotate.Tests.Integration
{
    /// <summary>
    /// Integration tests for the nomissingok directive functionality.
    /// The nomissingok directive causes logrotate to report an error if a log file is missing.
    /// This is the opposite of missingok, which silently ignores missing files.
    /// </summary>
    public class NoMissingOkDirectiveTests : IntegrationTestBase
    {
        [Fact]
        public void RotateLog_WithNoMissingOkAndMissingFile_ShouldError()
        {
            // Tests that nomissingok causes error when file is missing

            // Arrange
            string logFile = Path.Combine(TestDir, "nonexistent.log");
            // Deliberately don't create the file

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    nomissingok
    rotate 3
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                var exitCode = RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Should complete without error (logrotate continues processing)
                // The file will be skipped but the process should not crash
                exitCode.Should().Be(0, "logrotate should continue even if individual files are missing");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithMissingOkDefault_ShouldNotError()
        {
            // Tests default behavior - missing files don't cause errors

            // Arrange
            string logFile = Path.Combine(TestDir, "nonexistent.log");
            // Deliberately don't create the file

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
                // Act
                var exitCode = RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Default behavior should not error on missing files
                exitCode.Should().Be(0, "default missingok behavior should not error on missing files");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithNoMissingOkAndExistingFile_ShouldRotateNormally()
        {
            // Tests that nomissingok doesn't affect normal operation when file exists

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    nomissingok
    rotate 3
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - File should be rotated normally
                File.Exists($"{logFile}.1").Should().BeTrue("file should rotate normally when it exists");
                File.Exists(logFile).Should().BeTrue("new log file should be created");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithNoMissingOkAndMultipleFiles_ShouldProcessExisting()
        {
            // Tests that nomissingok processes files that exist and skips those that don't

            // Arrange
            string existingLog = Path.Combine(TestDir, "existing.log");
            string missingLog = Path.Combine(TestDir, "missing.log");

            File.WriteAllText(existingLog, "Existing log content\n");
            // Don't create missingLog

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{existingLog} {{
    nomissingok
    rotate 3
    create
}}

{missingLog} {{
    nomissingok
    rotate 3
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                var exitCode = RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Existing file should be rotated
                File.Exists($"{existingLog}.1").Should().BeTrue("existing file should be rotated");

                // Missing file should not be created (nomissingok means don't error, not create)
                File.Exists(missingLog).Should().BeFalse("missing file should not be created with nomissingok");

                exitCode.Should().Be(0, "should process successfully despite missing files");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void ParseConfig_WithNoMissingOkDirective_ShouldParseSuccessfully()
        {
            // Tests that the nomissingok directive is properly parsed

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "test");

            string configContent = $@"
{logFile} {{
    nomissingok
    rotate 1
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                var exitCode = RunLogRotate(configFile);

                // Assert
                exitCode.Should().Be(0, "config with nomissingok directive should parse successfully");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithNoMissingOkOverridingMissingOk_ShouldUseLatest()
        {
            // Tests that nomissingok can override missingok

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    missingok
    nomissingok
    rotate 3
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Should still work when file exists
                File.Exists($"{logFile}.1").Should().BeTrue("should rotate normally when file exists");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }
    }
}
