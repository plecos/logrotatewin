using FluentAssertions;
using System.IO;
using Xunit;

namespace logrotate.Tests.Integration
{
    /// <summary>
    /// Integration tests for the taboopat directive functionality.
    /// The taboopat directive specifies glob patterns of files to ignore when
    /// processing include directives. This is similar to tabooext but uses patterns
    /// instead of just file extensions.
    /// </summary>
    public class TabooPatDirectiveTests : IntegrationTestBase
    {
        [Fact]
        public void ProcessInclude_WithTabooPatAsteriskPattern_ShouldSkipMatchingFiles()
        {
            // Tests that taboopat with wildcard pattern skips matching config files

            // Arrange
            string includeDir = Path.Combine(TestDir, "include");
            Directory.CreateDirectory(includeDir);

            string goodConfig = Path.Combine(includeDir, "good.conf");
            string backupConfig = Path.Combine(includeDir, "backup.conf.bak");

            string testLog = Path.Combine(TestDir, "test.log");
            File.WriteAllText(testLog, "Log content\n");

            // Create config files
            File.WriteAllText(goodConfig, $@"
{testLog} {{
    rotate 1
    create
}}
");
            File.WriteAllText(backupConfig, $@"
{testLog} {{
    rotate 99
    create
}}
");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string mainConfig = $@"
taboopat *.bak

include {includeDir}
";
            string configFile = TestHelpers.CreateTempConfigFile(mainConfig);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - File should be rotated only once (backup.conf.bak ignored)
                File.Exists($"{testLog}.1").Should().BeTrue("log should be rotated");
                File.Exists($"{testLog}.2").Should().BeFalse("backup config should be ignored due to taboopat");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
                TestHelpers.CleanupPath(includeDir);
            }
        }

        [Fact]
        public void ProcessInclude_WithTabooPatMultiplePatterns_ShouldSkipAll()
        {
            // Tests that taboopat can specify multiple patterns

            // Arrange
            string includeDir = Path.Combine(TestDir, "include");
            Directory.CreateDirectory(includeDir);

            string goodConfig = Path.Combine(includeDir, "app.conf");
            string tmpConfig = Path.Combine(includeDir, "temp.tmp");
            string bakConfig = Path.Combine(includeDir, "old.bak");

            string testLog = Path.Combine(TestDir, "test.log");
            File.WriteAllText(testLog, "Log content\n");

            // Create config files
            File.WriteAllText(goodConfig, $@"
{testLog} {{
    rotate 1
    create
}}
");
            File.WriteAllText(tmpConfig, $@"
{testLog} {{
    rotate 2
    create
}}
");
            File.WriteAllText(bakConfig, $@"
{testLog} {{
    rotate 3
    create
}}
");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string mainConfig = $@"
taboopat *.tmp *.bak

include {includeDir}
";
            string configFile = TestHelpers.CreateTempConfigFile(mainConfig);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Only app.conf should be processed
                File.Exists($"{testLog}.1").Should().BeTrue("log should be rotated once from app.conf");
                File.Exists($"{testLog}.2").Should().BeFalse("temp.tmp and old.bak should be ignored");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
                TestHelpers.CleanupPath(includeDir);
            }
        }

        [Fact]
        public void ProcessInclude_WithTabooPatPlusAppend_ShouldAppendPatterns()
        {
            // Tests that taboopat with + appends to existing patterns

            // Arrange
            string includeDir = Path.Combine(TestDir, "include");
            Directory.CreateDirectory(includeDir);

            string goodConfig = Path.Combine(includeDir, "app.conf");
            string tmpConfig = Path.Combine(includeDir, "temp.tmp");
            string bakConfig = Path.Combine(includeDir, "old.bak");

            string testLog = Path.Combine(TestDir, "test.log");
            File.WriteAllText(testLog, "Log content\n");

            File.WriteAllText(goodConfig, $@"{testLog} {{ rotate 1 create }}");
            File.WriteAllText(tmpConfig, $@"{testLog} {{ rotate 2 create }}");
            File.WriteAllText(bakConfig, $@"{testLog} {{ rotate 3 create }}");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string mainConfig = $@"
taboopat *.tmp
taboopat + *.bak

include {includeDir}
";
            string configFile = TestHelpers.CreateTempConfigFile(mainConfig);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Both patterns should be applied
                File.Exists($"{testLog}.1").Should().BeTrue("log should be rotated from app.conf");
                File.Exists($"{testLog}.2").Should().BeFalse("both .tmp and .bak should be ignored");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
                TestHelpers.CleanupPath(includeDir);
            }
        }

        [Fact]
        public void ProcessInclude_WithTabooPatWithoutPlus_ShouldReplacePatterns()
        {
            // Tests that taboopat without + replaces existing patterns

            // Arrange
            string includeDir = Path.Combine(TestDir, "include");
            Directory.CreateDirectory(includeDir);

            string tmpConfig = Path.Combine(includeDir, "temp.tmp");
            string bakConfig = Path.Combine(includeDir, "old.bak");

            string testLog = Path.Combine(TestDir, "test.log");
            File.WriteAllText(testLog, "Log content\n");

            File.WriteAllText(tmpConfig, $@"{testLog} {{ rotate 1 create }}");
            File.WriteAllText(bakConfig, $@"{testLog} {{ rotate 2 create }}");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string mainConfig = $@"
taboopat *.tmp
taboopat *.bak

include {includeDir}
";
            string configFile = TestHelpers.CreateTempConfigFile(mainConfig);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Only *.bak should be ignored (replaces *.tmp)
                File.Exists($"{testLog}.1").Should().BeTrue("temp.tmp should be processed (pattern was replaced)");
                File.Exists($"{testLog}.2").Should().BeFalse("old.bak should be ignored");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
                TestHelpers.CleanupPath(includeDir);
            }
        }

        [Fact]
        public void ProcessInclude_WithTabooPatQuestionMark_ShouldMatchSingleChar()
        {
            // Tests that taboopat supports ? wildcard for single character

            // Arrange
            string includeDir = Path.Combine(TestDir, "include");
            Directory.CreateDirectory(includeDir);

            string config1 = Path.Combine(includeDir, "app1.conf");
            string config2 = Path.Combine(includeDir, "app2.conf");
            string configAbc = Path.Combine(includeDir, "appabc.conf");

            string testLog = Path.Combine(TestDir, "test.log");
            File.WriteAllText(testLog, "Log content\n");

            File.WriteAllText(config1, $@"{testLog} {{ rotate 1 create }}");
            File.WriteAllText(config2, $@"{testLog} {{ rotate 2 create }}");
            File.WriteAllText(configAbc, $@"{testLog} {{ rotate 3 create }}");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string mainConfig = $@"
taboopat app?.conf

include {includeDir}
";
            string configFile = TestHelpers.CreateTempConfigFile(mainConfig);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - app1.conf and app2.conf should be ignored, appabc.conf processed
                File.Exists($"{testLog}.1").Should().BeTrue("appabc.conf should be processed");
                File.Exists($"{testLog}.2").Should().BeFalse("app1.conf and app2.conf should be ignored");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
                TestHelpers.CleanupPath(includeDir);
            }
        }

        [Fact]
        public void ProcessInclude_WithTabooPatAndTabooExt_ShouldUseBoth()
        {
            // Tests that taboopat and tabooext work together

            // Arrange
            string includeDir = Path.Combine(TestDir, "include");
            Directory.CreateDirectory(includeDir);

            string confFile = Path.Combine(includeDir, "app.conf");
            string rpmsaveFile = Path.Combine(includeDir, "old.rpmsave");
            string bakFile = Path.Combine(includeDir, "backup.bak");

            string testLog = Path.Combine(TestDir, "test.log");
            File.WriteAllText(testLog, "Log content\n");

            File.WriteAllText(confFile, $@"{testLog} {{ rotate 1 create }}");
            File.WriteAllText(rpmsaveFile, $@"{testLog} {{ rotate 2 create }}");
            File.WriteAllText(bakFile, $@"{testLog} {{ rotate 3 create }}");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string mainConfig = $@"
tabooext .rpmsave
taboopat *.bak

include {includeDir}
";
            string configFile = TestHelpers.CreateTempConfigFile(mainConfig);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Only app.conf should be processed
                File.Exists($"{testLog}.1").Should().BeTrue("app.conf should be processed");
                File.Exists($"{testLog}.2").Should().BeFalse("both rpmsave and bak should be ignored");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
                TestHelpers.CleanupPath(includeDir);
            }
        }

        [Fact]
        public void ProcessInclude_WithTabooPatComplexPattern_ShouldMatchCorrectly()
        {
            // Tests complex glob patterns with taboopat

            // Arrange
            string includeDir = Path.Combine(TestDir, "include");
            Directory.CreateDirectory(includeDir);

            string prodConfig = Path.Combine(includeDir, "prod.conf");
            string testConfig = Path.Combine(includeDir, "test.conf");
            string devConfig = Path.Combine(includeDir, "dev.conf");

            string testLog = Path.Combine(TestDir, "test.log");
            File.WriteAllText(testLog, "Log content\n");

            File.WriteAllText(prodConfig, $@"{testLog} {{ rotate 1 create }}");
            File.WriteAllText(testConfig, $@"{testLog} {{ rotate 2 create }}");
            File.WriteAllText(devConfig, $@"{testLog} {{ rotate 3 create }}");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string mainConfig = $@"
taboopat test.* dev.*

include {includeDir}
";
            string configFile = TestHelpers.CreateTempConfigFile(mainConfig);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Only prod.conf should be processed
                File.Exists($"{testLog}.1").Should().BeTrue("prod.conf should be processed");
                File.Exists($"{testLog}.2").Should().BeFalse("test.conf and dev.conf should be ignored");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
                TestHelpers.CleanupPath(includeDir);
            }
        }

        [Fact]
        public void ParseConfig_WithTabooPatDirective_ShouldParseSuccessfully()
        {
            // Tests that the taboopat directive is properly parsed

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "test");

            string configContent = $@"
taboopat *.bak *.tmp

{logFile} {{
    rotate 1
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                var exitCode = RunLogRotate(configFile);

                // Assert
                exitCode.Should().Be(0, "config with taboopat directive should parse successfully");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }
    }
}
