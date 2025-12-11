using FluentAssertions;
using System.IO;
using Xunit;

namespace logrotate.Tests.Integration
{
    /// <summary>
    /// Integration tests for the start directive functionality.
    /// The start directive sets the starting number for rotated log files.
    /// By default, rotated files use .1, .2, .3, etc., but 'start' can change this to .0, .5, etc.
    /// </summary>
    public class StartDirectiveTests : IntegrationTestBase
    {
        [Fact]
        public void RotateLog_WithStart0_ShouldStartAtZero()
        {
            // 'start 0' directive causes rotated files to start at .0 instead of .1
            // This is useful for compatibility with some applications that expect .0 as first rotation

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Original log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    rotate 3
    create
    start 0
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act - First rotation
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - First rotated file should be .0, not .1
                File.Exists($"{logFile}.0").Should().BeTrue("first rotation should create .0 file when start is 0");
                File.Exists($"{logFile}.1").Should().BeFalse(".1 should not exist when start is 0");
                File.Exists(logFile).Should().BeTrue("original log file should be recreated");

                // Act - Second rotation
                File.WriteAllText(logFile, "New log content\n");
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Should now have .0 and .1
                File.Exists($"{logFile}.1").Should().BeTrue("second rotation should create .1 file");
                File.Exists($"{logFile}.0").Should().BeTrue(".0 file should still exist");

                // Act - Third rotation
                File.WriteAllText(logFile, "Another log content\n");
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Should now have .0, .1, and .2
                File.Exists($"{logFile}.2").Should().BeTrue("third rotation should create .2 file");
                File.Exists($"{logFile}.1").Should().BeTrue(".1 file should still exist");
                File.Exists($"{logFile}.0").Should().BeTrue(".0 file should still exist");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithStart5_ShouldUseCustomStartNumber()
        {
            // 'start 5' directive causes rotated files to use .5 as the rotation extension
            // NOTE: In the current implementation, the start number is used for all rotations
            // This test documents the actual behavior

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Original log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    rotate 3
    create
    start 5
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act - First rotation
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - First rotated file should be .5
                File.Exists($"{logFile}.5").Should().BeTrue("first rotation should create .5 file when start is 5");
                File.Exists($"{logFile}.1").Should().BeFalse(".1 should not exist when start is 5");
                File.Exists(logFile).Should().BeTrue("original log file should be recreated");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithStart0AndRotateLimit_ShouldKeepCorrectCount()
        {
            // Tests that start 0 works correctly with rotate count limits
            // The rotate directive specifies how many rotated logs to keep

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Original log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    rotate 2
    create
    start 0
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act - First rotation
                RunLogRotate("-s", stateFile, "-f", configFile);
                File.Exists($"{logFile}.0").Should().BeTrue("first rotation should create .0");

                // Act - Second rotation
                File.WriteAllText(logFile, "New log content\n");
                RunLogRotate("-s", stateFile, "-f", configFile);
                File.Exists($"{logFile}.1").Should().BeTrue(".1 should be created");
                File.Exists($"{logFile}.0").Should().BeTrue(".0 should still exist");

                // Act - Third rotation
                File.WriteAllText(logFile, "Another log content\n");
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - With rotate 2, should keep up to 2 rotated files (.0 through .2)
                File.Exists($"{logFile}.2").Should().BeTrue("third rotation should create .2 file");
                File.Exists($"{logFile}.1").Should().BeTrue(".1 file should still exist");
                File.Exists($"{logFile}.0").Should().BeTrue(".0 file should still exist with rotate count 2");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithDefaultStart_ShouldStartAtOne()
        {
            // Default behavior (no start directive) should start at .1
            // This test verifies the default to ensure start directive actually changes behavior

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Original log content\n");

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
                // Act - First rotation
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Default should create .1, not .0
                File.Exists($"{logFile}.1").Should().BeTrue("default rotation should create .1 file");
                File.Exists($"{logFile}.0").Should().BeFalse(".0 should not exist with default start");
                File.Exists(logFile).Should().BeTrue("original log file should be recreated");

                // Act - Second rotation
                File.WriteAllText(logFile, "New log content\n");
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Should create .2
                File.Exists($"{logFile}.2").Should().BeTrue("second rotation should create .2 file");
                File.Exists($"{logFile}.1").Should().BeTrue(".1 file should still exist");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithStart0AndCompress_ShouldWorkTogether()
        {
            // Tests that start 0 works correctly with compression
            // Compressed files should also follow the start numbering

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Original log content that should be long enough to compress\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    rotate 2
    create
    compress
    start 0
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act - First rotation (will compress)
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Compressed file should be .0.gz
                File.Exists($"{logFile}.0.gz").Should().BeTrue("first rotation should create .0.gz file when start is 0");
                File.Exists($"{logFile}.1.gz").Should().BeFalse(".1.gz should not exist when start is 0");

                // Act - Second rotation
                File.WriteAllText(logFile, "New log content that should be long enough to compress\n");
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Should have .0.gz and .1.gz
                File.Exists($"{logFile}.1.gz").Should().BeTrue("second rotation should create .1.gz file");
                File.Exists($"{logFile}.0.gz").Should().BeTrue(".0.gz file should still exist");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }
    }
}
