using FluentAssertions;
using System;
using System.IO;
using Xunit;

namespace logrotate.Tests.Integration
{
    /// <summary>
    /// Integration tests for the shred directive functionality.
    /// The shred directive securely deletes old log files by overwriting them with random data
    /// before deletion to prevent data recovery.
    /// </summary>
    public class ShredDirectiveTests : IntegrationTestBase
    {
        [Fact]
        public void RotateLog_WithShred_ShouldDeleteFileAfterShredding()
        {
            // 'shred' directive causes files to be securely deleted by overwriting with random data
            // This test verifies that the old log file is deleted after rotation when shred is enabled

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Original log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    rotate 2
    create
    shred
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act - First rotation
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - File should be rotated
                File.Exists($"{logFile}.1").Should().BeTrue("first rotation should create .1 file");
                File.Exists(logFile).Should().BeTrue("original log file should be recreated");

                // Act - Write new content and rotate again to exceed rotate count
                File.WriteAllText(logFile, "New log content\n");
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Old .1 file should be rotated to .2
                File.Exists($"{logFile}.2").Should().BeTrue("second rotation should create .2 file");
                File.Exists($"{logFile}.1").Should().BeTrue("new .1 file should exist");

                // Act - Rotate again to trigger deletion via shred (rotate count is 2)
                File.WriteAllText(logFile, "Another log content\n");
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Oldest file should be shredded and deleted
                File.Exists($"{logFile}.3").Should().BeFalse("files beyond rotate count should be shredded and deleted");
                File.Exists($"{logFile}.2").Should().BeTrue(".2 file should still exist");
                File.Exists($"{logFile}.1").Should().BeTrue(".1 file should still exist");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithNoShred_ShouldDeleteFileNormally()
        {
            // 'noshred' directive (or absence of shred) causes normal file deletion
            // This is the default behavior

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Original log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    rotate 1
    create
    noshred
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act - First rotation
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert
                File.Exists($"{logFile}.1").Should().BeTrue("first rotation should create .1 file");

                // Act - Rotate again to trigger deletion (rotate count is 1)
                File.WriteAllText(logFile, "New log content\n");
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Old file should be deleted normally (not shredded)
                File.Exists($"{logFile}.2").Should().BeFalse("files beyond rotate count should be deleted");
                File.Exists($"{logFile}.1").Should().BeTrue(".1 file should exist");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithShredCycles_ShouldUseSpecifiedCycles()
        {
            // 'shredcycles' directive specifies how many times to overwrite the file
            // Default is 3, but can be customized

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Original log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    rotate 1
    create
    shred
    shredcycles 5
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act - First rotation
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert
                File.Exists($"{logFile}.1").Should().BeTrue("first rotation should create .1 file");

                // Act - Rotate again to trigger shredding with 5 cycles
                File.WriteAllText(logFile, "New log content\n");
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Old file should be shredded with 5 cycles and deleted
                File.Exists($"{logFile}.2").Should().BeFalse("files beyond rotate count should be shredded and deleted");
                File.Exists($"{logFile}.1").Should().BeTrue(".1 file should exist");

                // Note: We can't directly verify that 5 cycles were used, but we can verify
                // the file was successfully deleted after the shredding process
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithShredAndCompress_ShouldWorkTogether()
        {
            // Test that shred works correctly with compression
            // Compressed files should also be shredded when deleted

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Original log content that should be long enough to compress\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    rotate 1
    create
    compress
    shred
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act - First rotation (will compress)
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Compressed file should exist
                File.Exists($"{logFile}.1.gz").Should().BeTrue("first rotation should create compressed .1.gz file");

                // Act - Rotate again to trigger deletion of compressed file via shred
                File.WriteAllText(logFile, "New log content that should be long enough to compress\n");
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Old compressed file should be shredded and deleted
                File.Exists($"{logFile}.2.gz").Should().BeFalse("compressed files beyond rotate count should be shredded");
                File.Exists($"{logFile}.1.gz").Should().BeTrue("new compressed .1.gz file should exist");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }
    }
}
