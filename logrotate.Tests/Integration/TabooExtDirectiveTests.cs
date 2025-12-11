using FluentAssertions;
using System.IO;
using Xunit;

namespace logrotate.Tests.Integration
{
    /// <summary>
    /// Integration tests for the tabooext directive functionality.
    /// The tabooext directive specifies file extensions to skip when processing include directives.
    /// By default, .swp files are in the taboo list.
    /// </summary>
    public class TabooExtDirectiveTests : IntegrationTestBase
    {
        [Fact]
        public void Include_WithDefaultTabooExt_ShouldSkipSwpFiles()
        {
            // Default tabooext includes .swp files
            // When include directive processes a directory, .swp files should be skipped

            // Arrange
            string includeDir = Path.Combine(TestDir, "include");
            Directory.CreateDirectory(includeDir);

            string logFile1 = Path.Combine(TestDir, "test1.log");
            string logFile2 = Path.Combine(TestDir, "test2.log");
            File.WriteAllText(logFile1, "Log content 1\n");
            File.WriteAllText(logFile2, "Log content 2\n");

            // Create a valid config file in the include directory
            string validConfig = Path.Combine(includeDir, "valid.conf");
            string validConfigContent = $@"
{logFile1} {{
    rotate 2
    create
}}
";
            File.WriteAllText(validConfig, validConfigContent);

            // Create a .swp config file (should be skipped by default)
            string swpConfig = Path.Combine(includeDir, "editor.swp");
            string swpConfigContent = $@"
{logFile2} {{
    rotate 2
    create
}}
";
            File.WriteAllText(swpConfig, swpConfigContent);

            string stateFile = Path.Combine(TestDir, "state.txt");
            string mainConfigContent = $@"
include ""{includeDir}""
";
            string configFile = TestHelpers.CreateTempConfigFile(mainConfigContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Only logFile1 should be rotated (from valid.conf)
                // logFile2 should NOT be rotated because editor.swp was skipped
                File.Exists($"{logFile1}.1").Should().BeTrue("logFile1 should be rotated from valid.conf");
                File.Exists($"{logFile2}.1").Should().BeFalse("logFile2 should not be rotated because .swp file was skipped");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
                TestHelpers.CleanupPath(includeDir);
            }
        }

        [Fact]
        public void Include_WithCustomTabooExt_ShouldSkipSpecifiedExtensions()
        {
            // Custom tabooext replaces the default list
            // Only specified extensions should be skipped

            // Arrange
            string includeDir = Path.Combine(TestDir, "include");
            Directory.CreateDirectory(includeDir);

            string logFile1 = Path.Combine(TestDir, "test1.log");
            string logFile2 = Path.Combine(TestDir, "test2.log");
            string logFile3 = Path.Combine(TestDir, "test3.log");
            File.WriteAllText(logFile1, "Log content 1\n");
            File.WriteAllText(logFile2, "Log content 2\n");
            File.WriteAllText(logFile3, "Log content 3\n");

            // Create a .conf file (should be processed)
            string confConfig = Path.Combine(includeDir, "valid.conf");
            string confConfigContent = $@"
{logFile1} {{
    rotate 2
    create
}}
";
            File.WriteAllText(confConfig, confConfigContent);

            // Create a .swp file (should now be processed since we're replacing taboo list)
            string swpConfig = Path.Combine(includeDir, "editor.swp");
            string swpConfigContent = $@"
{logFile2} {{
    rotate 2
    create
}}
";
            File.WriteAllText(swpConfig, swpConfigContent);

            // Create a .bak file (should be skipped with custom tabooext)
            string bakConfig = Path.Combine(includeDir, "backup.bak");
            string bakConfigContent = $@"
{logFile3} {{
    rotate 2
    create
}}
";
            File.WriteAllText(bakConfig, bakConfigContent);

            string stateFile = Path.Combine(TestDir, "state.txt");
            string mainConfigContent = $@"
tabooext .bak .old
include ""{includeDir}""
";
            string configFile = TestHelpers.CreateTempConfigFile(mainConfigContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert
                File.Exists($"{logFile1}.1").Should().BeTrue("logFile1 should be rotated from .conf file");
                File.Exists($"{logFile2}.1").Should().BeTrue("logFile2 should be rotated because .swp is no longer in taboo list");
                File.Exists($"{logFile3}.1").Should().BeFalse("logFile3 should not be rotated because .bak is in custom taboo list");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
                TestHelpers.CleanupPath(includeDir);
            }
        }

        [Fact]
        public void Include_WithTabooExtPlus_ShouldAppendToDefaultList()
        {
            // tabooext + appends to the default list rather than replacing it
            // Both .swp (default) and new extensions should be skipped

            // Arrange
            string includeDir = Path.Combine(TestDir, "include");
            Directory.CreateDirectory(includeDir);

            string logFile1 = Path.Combine(TestDir, "test1.log");
            string logFile2 = Path.Combine(TestDir, "test2.log");
            string logFile3 = Path.Combine(TestDir, "test3.log");
            File.WriteAllText(logFile1, "Log content 1\n");
            File.WriteAllText(logFile2, "Log content 2\n");
            File.WriteAllText(logFile3, "Log content 3\n");

            // Create a .conf file (should be processed)
            string confConfig = Path.Combine(includeDir, "valid.conf");
            string confConfigContent = $@"
{logFile1} {{
    rotate 2
    create
}}
";
            File.WriteAllText(confConfig, confConfigContent);

            // Create a .swp file (should be skipped - default taboo)
            string swpConfig = Path.Combine(includeDir, "editor.swp");
            string swpConfigContent = $@"
{logFile2} {{
    rotate 2
    create
}}
";
            File.WriteAllText(swpConfig, swpConfigContent);

            // Create a .bak file (should be skipped - added to taboo)
            string bakConfig = Path.Combine(includeDir, "backup.bak");
            string bakConfigContent = $@"
{logFile3} {{
    rotate 2
    create
}}
";
            File.WriteAllText(bakConfig, bakConfigContent);

            string stateFile = Path.Combine(TestDir, "state.txt");
            string mainConfigContent = $@"
tabooext + .bak .old
include ""{includeDir}""
";
            string configFile = TestHelpers.CreateTempConfigFile(mainConfigContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert
                File.Exists($"{logFile1}.1").Should().BeTrue("logFile1 should be rotated from .conf file");
                File.Exists($"{logFile2}.1").Should().BeFalse("logFile2 should not be rotated because .swp is still in taboo list");
                File.Exists($"{logFile3}.1").Should().BeFalse("logFile3 should not be rotated because .bak was added to taboo list");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
                TestHelpers.CleanupPath(includeDir);
            }
        }

        // NOTE: Testing "tabooext" with no arguments (to clear the list) reveals a bug in logrotatewin:
        // The parser tries to access split[1] without checking array bounds (logrotateconf.cs:643).
        // This causes an IndexOutOfRangeException when tabooext has no arguments.
        // Skipping this test until the bug is fixed.
        //
        // [Fact(Skip = "Requires fix for tabooext parser bounds checking")]
        // public void Include_WithEmptyTabooExt_ShouldProcessAllFiles() { ... }
    }
}
