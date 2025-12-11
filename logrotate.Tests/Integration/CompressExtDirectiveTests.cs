using FluentAssertions;
using System.IO;
using System.IO.Compression;
using Xunit;

namespace logrotate.Tests.Integration
{
    /// <summary>
    /// Integration tests for the compressext directive functionality.
    /// The compressext directive specifies the file extension to use for compressed files.
    /// Default is 'gz' (gzip compression).
    /// </summary>
    public class CompressExtDirectiveTests : IntegrationTestBase
    {
        [Fact]
        public void RotateLog_WithDefaultCompressExt_ShouldUseGzExtension()
        {
            // Default compress extension is .gz
            // This test verifies the default behavior

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Original log content that should be compressed\n");

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

                // Assert - Default extension should be .gz
                File.Exists($"{logFile}.1.gz").Should().BeTrue("default compress extension should be .gz");
                File.Exists($"{logFile}.1").Should().BeFalse("uncompressed file should not exist");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithCustomCompressExt_ShouldUseCustomExtension()
        {
            // compressext directive allows changing the compressed file extension
            // Note: This only changes the extension, not the compression algorithm

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Original log content that should be compressed\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    rotate 2
    compress
    compressext zip
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Should use custom .zip extension
                File.Exists($"{logFile}.1.zip").Should().BeTrue("custom compress extension should be .zip");
                File.Exists($"{logFile}.1.gz").Should().BeFalse(".gz file should not exist when custom extension is specified");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithCompressExtBz2_ShouldUseBz2Extension()
        {
            // Tests another common compression extension (.bz2)

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Original log content that should be compressed\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    rotate 2
    compress
    compressext bz2
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Should use .bz2 extension
                File.Exists($"{logFile}.1.bz2").Should().BeTrue("custom compress extension should be .bz2");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithCompressExtAndDateExt_ShouldAppendExtensionAfterDate()
        {
            // Tests that compressext works correctly with dateext
            // The extension should be appended after the date suffix

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Original log content that should be compressed\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    rotate 2
    compress
    compressext zip
    dateext
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Should have date suffix followed by custom extension
                // Look for any file matching the pattern test.log-*.zip
                string[] compressedFiles = Directory.GetFiles(TestDir, "test.log-*.zip");
                compressedFiles.Should().HaveCount(1, "should have one compressed file with date suffix and .zip extension");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        // NOTE: Test for compressext without compress directive omitted
        // The behavior of rotation without compression is already well-tested in BasicRotationTests
        // and other test files. The compressext directive is only relevant when compress is enabled.

        [Fact]
        public void RotateLog_WithCompressExtMultipleRotations_ShouldUseExtensionForAll()
        {
            // Tests that compressext is applied consistently across multiple rotations

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Original log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    rotate 3
    compress
    compressext zip
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act - First rotation
                RunLogRotate("-s", stateFile, "-f", configFile);
                File.Exists($"{logFile}.1.zip").Should().BeTrue("first rotation should create .1.zip");

                // Act - Second rotation
                File.WriteAllText(logFile, "Second log content\n");
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Both rotated files should have .zip extension
                File.Exists($"{logFile}.1.zip").Should().BeTrue(".1.zip should exist after second rotation");
                File.Exists($"{logFile}.2.zip").Should().BeTrue(".2.zip should exist after second rotation");

                // Act - Third rotation
                File.WriteAllText(logFile, "Third log content\n");
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - All three rotated files should have .zip extension
                File.Exists($"{logFile}.1.zip").Should().BeTrue(".1.zip should exist after third rotation");
                File.Exists($"{logFile}.2.zip").Should().BeTrue(".2.zip should exist after third rotation");
                File.Exists($"{logFile}.3.zip").Should().BeTrue(".3.zip should exist after third rotation");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }
    }
}
