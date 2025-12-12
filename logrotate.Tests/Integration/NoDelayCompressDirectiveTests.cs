using FluentAssertions;
using System.IO;
using Xunit;

namespace logrotate.Tests.Integration
{
    /// <summary>
    /// Integration tests for the nodelaycompress directive functionality.
    /// The nodelaycompress directive ensures that rotated files are compressed immediately,
    /// overriding any delaycompress setting. This is the opposite of delaycompress.
    /// </summary>
    public class NoDelayCompressDirectiveTests : IntegrationTestBase
    {
        [Fact]
        public void RotateLog_WithNoDelayCompress_ShouldCompressImmediately()
        {
            // Tests that nodelaycompress causes immediate compression

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Original log content for compression\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    rotate 3
    compress
    nodelaycompress
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act - First rotation
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - File should be compressed immediately
                File.Exists($"{logFile}.1.gz").Should().BeTrue("nodelaycompress should compress immediately");
                File.Exists($"{logFile}.1").Should().BeFalse("uncompressed file should not exist");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithNoDelayCompressOverridingDelayCompress_ShouldCompressImmediately()
        {
            // Tests that nodelaycompress overrides delaycompress

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    rotate 3
    compress
    delaycompress
    nodelaycompress
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - nodelaycompress should win (last directive wins)
                File.Exists($"{logFile}.1.gz").Should().BeTrue("nodelaycompress should override delaycompress");
                File.Exists($"{logFile}.1").Should().BeFalse("file should be compressed immediately");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithDefaultBehavior_ShouldCompressImmediately()
        {
            // Tests default behavior without delaycompress - should compress immediately

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Default behavior test\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
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

                // Assert - Default is immediate compression
                File.Exists($"{logFile}.1.gz").Should().BeTrue("default should compress immediately");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithDelayCompress_ShouldDelayCompression()
        {
            // Tests that delaycompress delays compression to next rotation

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "First content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    rotate 3
    compress
    delaycompress
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act - First rotation (should NOT compress)
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - First rotation should not compress with delaycompress
                File.Exists($"{logFile}.1").Should().BeTrue("delaycompress should keep .1 uncompressed");
                File.Exists($"{logFile}.1.gz").Should().BeFalse("should not compress on first rotation");

                // Act - Second rotation (should compress .1 now)
                File.WriteAllText(logFile, "Second content\n");
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - .1 should now be compressed (becomes .2.gz), new .1 is uncompressed
                File.Exists($"{logFile}.1").Should().BeTrue(".1 should be uncompressed (newly rotated)");
                File.Exists($"{logFile}.2.gz").Should().BeTrue(".2 should be compressed (was .1)");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithNoDelayCompressMultipleRotations_ShouldCompressAllImmediately()
        {
            // Tests that nodelaycompress compresses all rotations immediately

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "First\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    rotate 3
    compress
    nodelaycompress
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act - First rotation
                RunLogRotate("-s", stateFile, "-f", configFile);
                File.Exists($"{logFile}.1.gz").Should().BeTrue();

                // Act - Second rotation
                File.WriteAllText(logFile, "Second\n");
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Both should be compressed
                File.Exists($"{logFile}.1.gz").Should().BeTrue(".1 should be compressed");
                File.Exists($"{logFile}.2.gz").Should().BeTrue(".2 should be compressed");
                File.Exists($"{logFile}.1").Should().BeFalse("no uncompressed .1");
                File.Exists($"{logFile}.2").Should().BeFalse("no uncompressed .2");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithNoDelayCompressAndDateExt_ShouldWorkTogether()
        {
            // Tests that nodelaycompress works with dateext

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Date extension test\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    rotate 3
    compress
    nodelaycompress
    dateext
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Should have compressed date-stamped file
                string[] gzFiles = Directory.GetFiles(TestDir, "test.log-*.gz");
                gzFiles.Should().HaveCountGreaterOrEqualTo(1, "should have at least one compressed dated file");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void ParseConfig_WithNoDelayCompressDirective_ShouldParseSuccessfully()
        {
            // Tests that the nodelaycompress directive is properly parsed

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "test");

            string configContent = $@"
{logFile} {{
    nodelaycompress
    rotate 1
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                var exitCode = RunLogRotate(configFile);

                // Assert
                exitCode.Should().Be(0, "config with nodelaycompress directive should parse successfully");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithNoDelayCompressGlobal_ShouldApplyToAll()
        {
            // Tests that nodelaycompress in global config applies to all sections

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Global config test\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
compress
nodelaycompress

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

                // Assert
                File.Exists($"{logFile}.1.gz").Should().BeTrue("global nodelaycompress should apply");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }
    }
}
