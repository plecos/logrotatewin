using FluentAssertions;
using System.IO;
using System.IO.Compression;
using Xunit;

namespace logrotate.Tests.Integration
{
    /// <summary>
    /// Integration tests for compression command directives (compresscmd, uncompresscmd, compressoptions).
    /// These directives allow using custom external compression tools instead of the built-in GZipStream.
    /// </summary>
    public class CompressionCommandDirectiveTests : IntegrationTestBase
    {
        [Fact]
        public void RotateLog_WithGzipCompressCmd_ShouldUseExternalGzip()
        {
            // Tests using external gzip command (if available)
            // This tests the compresscmd directive with a standard compression tool

            // Skip if gzip is not available on the system
            if (!IsCommandAvailable("gzip"))
            {
                // Test passes by default if gzip is not installed
                return;
            }

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Original log content for external gzip compression test\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    rotate 2
    compress
    compresscmd gzip
    compressext gz
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Should create .gz file using external gzip
                File.Exists($"{logFile}.1.gz").Should().BeTrue("external gzip should create .1.gz file");

                // Verify it's a valid gzip file by checking magic bytes
                byte[] magicBytes = new byte[2];
                using (FileStream fs = File.OpenRead($"{logFile}.1.gz"))
                {
                    fs.Read(magicBytes, 0, 2);
                }
                magicBytes[0].Should().Be(0x1F, "gzip magic byte 1 should be 0x1F");
                magicBytes[1].Should().Be(0x8B, "gzip magic byte 2 should be 0x8B");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithCompressOptions_ShouldPassOptionsToCommand()
        {
            // Tests the compressoptions directive
            // Uses gzip with custom compression level

            // Skip if gzip is not available
            if (!IsCommandAvailable("gzip"))
            {
                return;
            }

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Log content for compression options test\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    rotate 2
    compress
    compresscmd gzip
    compressoptions -1
    compressext gz
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - File should be compressed
                File.Exists($"{logFile}.1.gz").Should().BeTrue("should create compressed file with custom options");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithoutCompressCmd_ShouldUseBuiltInGZip()
        {
            // Tests that without compresscmd, the built-in GZipStream is used (default behavior)
            // This verifies backward compatibility

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Log content for built-in gzip test\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    rotate 2
    compress
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Should use built-in GZipStream
                File.Exists($"{logFile}.1.gz").Should().BeTrue("built-in compression should create .gz file");

                // Verify it's a valid gzip file
                byte[] magicBytes = new byte[2];
                using (FileStream fs = File.OpenRead($"{logFile}.1.gz"))
                {
                    fs.Read(magicBytes, 0, 2);
                }
                magicBytes[0].Should().Be(0x1F);
                magicBytes[1].Should().Be(0x8B);

                // Verify we can decompress it with .NET
                using (FileStream fs = File.OpenRead($"{logFile}.1.gz"))
                using (GZipStream gzipStream = new GZipStream(fs, CompressionMode.Decompress))
                using (StreamReader reader = new StreamReader(gzipStream))
                {
                    string content = reader.ReadToEnd();
                    content.Should().Contain("Log content for built-in gzip test");
                }
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithCompressCmdAndCustomExt_ShouldUseBothDirectives()
        {
            // Tests that compresscmd works with compressext to create custom extensions
            // Example: using a hypothetical compression tool with .myzip extension

            // Skip if gzip is not available
            if (!IsCommandAvailable("gzip"))
            {
                return;
            }

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    rotate 2
    compress
    compresscmd gzip
    compressext customext
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Should use custom extension
                File.Exists($"{logFile}.1.customext").Should().BeTrue("should use custom compressext");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void ParseConfig_WithCompressCmdDirective_ShouldStoreCommand()
        {
            // Tests that the compresscmd directive is properly parsed and stored
            // This is a configuration parsing test

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "test");

            string configContent = $@"
{logFile} {{
    compresscmd /usr/bin/gzip
    rotate 1
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act - Just parse, don't rotate
                // The fact that it doesn't error means parsing succeeded
                var exitCode = RunLogRotate(configFile);

                // Assert - No error during parsing
                exitCode.Should().Be(0, "config with compresscmd should parse successfully");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void ParseConfig_WithCompressOptionsDirective_ShouldStoreOptions()
        {
            // Tests that the compressoptions directive is properly parsed

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "test");

            string configContent = $@"
{logFile} {{
    compressoptions -9 --best
    rotate 1
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                var exitCode = RunLogRotate(configFile);

                // Assert
                exitCode.Should().Be(0, "config with compressoptions should parse successfully");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        /// <summary>
        /// Helper method to check if a command is available on the system
        /// </summary>
        private bool IsCommandAvailable(string command)
        {
            try
            {
                var psi = new System.Diagnostics.ProcessStartInfo();
                psi.FileName = command;
                psi.Arguments = "--version";
                psi.UseShellExecute = false;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;
                psi.CreateNoWindow = true;

                using (var process = System.Diagnostics.Process.Start(psi))
                {
                    process.WaitForExit(1000);
                    return process.ExitCode == 0 || process.ExitCode == 1; // Some commands return 1 for --version
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
