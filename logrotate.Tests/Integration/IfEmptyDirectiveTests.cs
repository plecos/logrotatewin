using FluentAssertions;
using System.IO;
using Xunit;

namespace logrotate.Tests.Integration
{
    /// <summary>
    /// Integration tests for the ifempty/notifempty directive functionality.
    /// The ifempty directive (default) allows rotation of empty log files.
    /// The notifempty directive prevents rotation of empty log files.
    /// </summary>
    public class IfEmptyDirectiveTests : IntegrationTestBase
    {
        [Fact]
        public void RotateLog_WithIfEmpty_ShouldRotateEmptyFiles()
        {
            // 'ifempty' directive (or default behavior) allows empty files to be rotated
            // This is the default behavior

            // Arrange
            string logFile = Path.Combine(TestDir, "empty.log");
            File.WriteAllText(logFile, ""); // Create empty file

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    ifempty
    rotate 2
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Empty file SHOULD be rotated with ifempty
                File.Exists($"{logFile}.1").Should().BeTrue("empty file should be rotated with ifempty directive");
                File.Exists(logFile).Should().BeTrue("original log file should be recreated");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithDefaultBehavior_ShouldRotateEmptyFiles()
        {
            // Default behavior (no ifempty or notifempty specified) should rotate empty files
            // ifempty is true by default

            // Arrange
            string logFile = Path.Combine(TestDir, "empty.log");
            File.WriteAllText(logFile, ""); // Create empty file

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    rotate 2
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Empty file SHOULD be rotated by default
                File.Exists($"{logFile}.1").Should().BeTrue("empty file should be rotated by default");
                File.Exists(logFile).Should().BeTrue("original log file should be recreated");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithIfEmptyAndContent_ShouldStillRotate()
        {
            // ifempty should not prevent rotation of files with content
            // This verifies the directive works correctly for non-empty files too

            // Arrange
            string logFile = Path.Combine(TestDir, "full.log");
            File.WriteAllText(logFile, "Log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    ifempty
    rotate 2
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Non-empty file should also be rotated
                File.Exists($"{logFile}.1").Should().BeTrue("non-empty file should be rotated with ifempty");
                File.Exists(logFile).Should().BeTrue("original log file should be recreated");

                // Verify content was preserved
                string rotatedContent = File.ReadAllText($"{logFile}.1");
                rotatedContent.Should().Be("Log content\n", "rotated file should preserve original content");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithNotIfEmptyThenIfEmpty_ShouldOverride()
        {
            // When both notifempty and ifempty are specified, the last one should win
            // This tests directive precedence

            // Arrange
            string logFile = Path.Combine(TestDir, "empty.log");
            File.WriteAllText(logFile, ""); // Create empty file

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    notifempty
    ifempty
    rotate 2
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - ifempty (last directive) should win, so file should be rotated
                File.Exists($"{logFile}.1").Should().BeTrue("ifempty should override notifempty when specified last");
                File.Exists(logFile).Should().BeTrue("original log file should be recreated");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_CompareIfEmptyVsNotIfEmpty()
        {
            // Directly compare behavior of ifempty vs notifempty with same empty file
            // This demonstrates the clear difference between the two directives

            // Arrange
            string logFileIfEmpty = Path.Combine(TestDir, "empty_ifempty.log");
            string logFileNotIfEmpty = Path.Combine(TestDir, "empty_notifempty.log");
            File.WriteAllText(logFileIfEmpty, ""); // Create empty file
            File.WriteAllText(logFileNotIfEmpty, ""); // Create empty file

            string stateFile = Path.Combine(TestDir, "state.txt");

            // Config with ifempty
            string configIfEmpty = $@"
{logFileIfEmpty} {{
    ifempty
    rotate 2
    create
}}
";
            string configFileIfEmpty = TestHelpers.CreateTempConfigFile(configIfEmpty);

            // Config with notifempty
            string configNotIfEmpty = $@"
{logFileNotIfEmpty} {{
    notifempty
    rotate 2
    create
}}
";
            string configFileNotIfEmpty = TestHelpers.CreateTempConfigFile(configNotIfEmpty);

            try
            {
                // Act - Run both configs
                RunLogRotate("-s", stateFile, "-f", configFileIfEmpty);
                RunLogRotate("-s", stateFile, "-f", configFileNotIfEmpty);

                // Assert - ifempty should rotate, notifempty should not
                File.Exists($"{logFileIfEmpty}.1").Should().BeTrue("ifempty should allow rotation of empty files");
                File.Exists($"{logFileNotIfEmpty}.1").Should().BeFalse("notifempty should prevent rotation of empty files");
            }
            finally
            {
                TestHelpers.CleanupPath(configFileIfEmpty);
                TestHelpers.CleanupPath(configFileNotIfEmpty);
            }
        }
    }
}
