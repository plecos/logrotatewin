using System;
using System.IO;
using FluentAssertions;
using Xunit;

namespace logrotate.Tests.Integration
{
    [Trait("Category", "Integration")]
    public class CompressionIntegrationTests : IntegrationTestBase
    {
        [Fact]
        public void RotateLog_WithCompress_ShouldCompressRotatedFile()
        {
            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            string testContent = "This is test log content that should be compressed\n";
            File.WriteAllText(logFile, testContent);

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    compress
    rotate 2
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert
                File.Exists($"{logFile}.1.gz").Should().BeTrue("rotated file should be compressed");
                TestHelpers.IsFileCompressed($"{logFile}.1.gz").Should().BeTrue("file should have gzip magic number");
                File.Exists($"{logFile}.1").Should().BeFalse("uncompressed rotated file should not exist");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithNoCompress_ShouldNotCompressRotatedFile()
        {
            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Test log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    nocompress
    rotate 2
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert
                File.Exists($"{logFile}.1").Should().BeTrue("rotated file should exist uncompressed");
                File.Exists($"{logFile}.1.gz").Should().BeFalse("compressed file should not exist");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_CompressedFile_ShouldBeSmallerThanOriginal()
        {
            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            string testContent = new string('A', 10000); // Large repetitive content compresses well
            File.WriteAllText(logFile, testContent);

            long originalSize = new FileInfo(logFile).Length;

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    compress
    rotate 2
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert
                string compressedFile = $"{logFile}.1.gz";
                File.Exists(compressedFile).Should().BeTrue();

                long compressedSize = new FileInfo(compressedFile).Length;
                compressedSize.Should().BeLessThan(originalSize, "compressed file should be smaller than original");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }
    }
}
