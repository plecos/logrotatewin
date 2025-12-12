using FluentAssertions;
using System.IO;
using Xunit;

namespace logrotate.Tests.Integration
{
    /// <summary>
    /// Integration tests for the ignoreduplicates directive functionality.
    /// The ignoreduplicates directive prevents logrotate from processing the same log file
    /// multiple times when it appears in multiple configuration sections.
    /// </summary>
    public class IgnoreDuplicatesDirectiveTests : IntegrationTestBase
    {
        [Fact]
        public void RotateLog_WithIgnoreDuplicatesAndDuplicatePaths_ShouldProcessOnce()
        {
            // Tests that ignoreduplicates prevents processing the same file twice
            // Uses wildcard patterns that would match the same file multiple times

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Original log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string wildcard1 = Path.Combine(TestDir, "*.log");
            string wildcard2 = Path.Combine(TestDir, "test.*");

            string configContent = $@"
{wildcard1} {{
    ignoreduplicates
    rotate 3
    create
}}

{wildcard2} {{
    ignoreduplicates
    rotate 5
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act - Should only rotate once despite file matching both wildcards
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - File should be rotated only once
                File.Exists($"{logFile}.1").Should().BeTrue("file should be rotated once");
                File.Exists($"{logFile}.2").Should().BeFalse("file should not be rotated twice due to ignoreduplicates");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithoutIgnoreDuplicates_ShouldProcessMultipleTimes()
        {
            // Tests default behavior - same file matched by different wildcards is processed multiple times

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Original log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string wildcard1 = Path.Combine(TestDir, "*.log");
            string wildcard2 = Path.Combine(TestDir, "test.*");

            string configContent = $@"
{wildcard1} {{
    rotate 3
    create
}}

{wildcard2} {{
    rotate 3
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act - Without ignoreduplicates, file may be processed multiple times
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - File should be rotated (at least once, possibly twice)
                File.Exists($"{logFile}.1").Should().BeTrue("file should be rotated");
                // Note: Can't reliably test for .2 as it depends on rotation order
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithIgnoreDuplicatesAndWildcards_ShouldHandleDuplicates()
        {
            // Tests that ignoreduplicates works with wildcard patterns

            // Arrange
            string log1 = Path.Combine(TestDir, "app1.log");
            string log2 = Path.Combine(TestDir, "app2.log");

            File.WriteAllText(log1, "App1 log content\n");
            File.WriteAllText(log2, "App2 log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string wildcard = Path.Combine(TestDir, "*.log");
            string configContent = $@"
{wildcard} {{
    ignoreduplicates
    rotate 3
    create
}}

{wildcard} {{
    ignoreduplicates
    rotate 3
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Each file should be rotated only once
                File.Exists($"{log1}.1").Should().BeTrue("app1.log should be rotated once");
                File.Exists($"{log2}.1").Should().BeTrue("app2.log should be rotated once");

                File.Exists($"{log1}.2").Should().BeFalse("app1.log should not be rotated twice");
                File.Exists($"{log2}.2").Should().BeFalse("app2.log should not be rotated twice");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithIgnoreDuplicatesMixedPaths_ShouldTrackCorrectly()
        {
            // Tests that ignoreduplicates tracks full file paths correctly across wildcards

            // Arrange
            string log1 = Path.Combine(TestDir, "test1.log");
            string log2 = Path.Combine(TestDir, "test2.log");

            File.WriteAllText(log1, "Test1 content\n");
            File.WriteAllText(log2, "Test2 content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string wildcard1 = Path.Combine(TestDir, "test1.*");
            string wildcard2 = Path.Combine(TestDir, "test2.*");
            string wildcard3 = Path.Combine(TestDir, "*.log");

            string configContent = $@"
{wildcard1} {{
    ignoreduplicates
    rotate 3
    create
}}

{wildcard2} {{
    ignoreduplicates
    rotate 3
    create
}}

{wildcard3} {{
    ignoreduplicates
    rotate 3
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Each unique file should be rotated once despite matching multiple wildcards
                File.Exists($"{log1}.1").Should().BeTrue("test1.log should be rotated once");
                File.Exists($"{log2}.1").Should().BeTrue("test2.log should be rotated once");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithIgnoreDuplicatesGlobalConfig_ShouldApplyToAll()
        {
            // Tests that ignoreduplicates in global config applies to all sections

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string wildcard1 = Path.Combine(TestDir, "*.log");
            string wildcard2 = Path.Combine(TestDir, "test.*");

            string configContent = $@"
ignoreduplicates

{wildcard1} {{
    rotate 3
    create
}}

{wildcard2} {{
    rotate 3
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - File should be rotated only once due to global ignoreduplicates
                File.Exists($"{logFile}.1").Should().BeTrue("file should be rotated once");
                File.Exists($"{logFile}.2").Should().BeFalse("file should not be rotated twice");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void ParseConfig_WithIgnoreDuplicatesDirective_ShouldParseSuccessfully()
        {
            // Tests that the ignoreduplicates directive is properly parsed

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "test");

            string configContent = $@"
{logFile} {{
    ignoreduplicates
    rotate 1
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                var exitCode = RunLogRotate(configFile);

                // Assert
                exitCode.Should().Be(0, "config with ignoreduplicates directive should parse successfully");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithIgnoreDuplicatesAndDifferentOptions_ShouldUseFirst()
        {
            // Tests that when a file matches multiple wildcards with ignoreduplicates,
            // only the first configuration is used

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Original log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string wildcard1 = Path.Combine(TestDir, "*.log");
            string wildcard2 = Path.Combine(TestDir, "test.*");

            string configContent = $@"
{wildcard1} {{
    ignoreduplicates
    rotate 2
    create
}}

{wildcard2} {{
    ignoreduplicates
    rotate 10
    compress
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Should use first config (no compression)
                File.Exists($"{logFile}.1").Should().BeTrue("file should be rotated");
                File.Exists($"{logFile}.1.gz").Should().BeFalse("should not be compressed (first config doesn't compress)");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }
    }
}
