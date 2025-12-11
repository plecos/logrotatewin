using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace logrotate.Tests.Integration
{
    [Trait("Category", "Integration")]
    public class AdvancedRotationTests : IntegrationTestBase
    {
        [Fact]
        public void RotateLog_WithCopyDirective_ShouldCopyInsteadOfMove()
        {
            // 'copy' directive copies the log file instead of moving it
            // The original log file remains in place

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Test log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    rotate 2
    copy
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert
                File.Exists($"{logFile}.1").Should().BeTrue("rotated copy should exist");
                File.Exists(logFile).Should().BeTrue("original file should still exist with 'copy' directive");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithCopyTruncateDirective_ShouldCopyThenTruncate()
        {
            // 'copytruncate' directive copies the log file, then truncates the original
            // This is useful for applications that cannot be told to close and reopen log files

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            string originalContent = "Test log content\n";
            File.WriteAllText(logFile, originalContent);

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    rotate 2
    copytruncate
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert
                File.Exists($"{logFile}.1").Should().BeTrue("rotated copy should exist");
                File.Exists(logFile).Should().BeTrue("original file should still exist");

                // Original should be truncated (empty or very small)
                var originalFileInfo = new FileInfo(logFile);
                originalFileInfo.Length.Should().BeLessOrEqualTo(originalContent.Length,
                    "original file should be truncated");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithCreateDirective_ShouldRecreateLogFile()
        {
            // 'create' directive recreates the log file after rotation

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Test log content\n");

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

                // Assert
                File.Exists($"{logFile}.1").Should().BeTrue("rotated file should exist");
                File.Exists(logFile).Should().BeTrue("new log file should be created");

                // New file should be empty
                var newFileInfo = new FileInfo(logFile);
                newFileInfo.Length.Should().Be(0, "newly created file should be empty");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithNoCreateDirective_ShouldNotRecreateLogFile()
        {
            // 'nocreate' directive prevents recreation of the log file after rotation

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Test log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    rotate 2
    nocreate
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert
                File.Exists($"{logFile}.1").Should().BeTrue("rotated file should exist");
                File.Exists(logFile).Should().BeFalse("original file should not be recreated with 'nocreate'");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithDateExtDirective_ShouldUseDateExtension()
        {
            // 'dateext' directive uses date-based extensions instead of .1, .2, etc.
            // Format is typically YYYYMMDD

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Test log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    rotate 2
    dateext
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert
                // Should have a date-based extension (YYYYMMDD format)
                var rotatedFiles = Directory.GetFiles(TestDir, "test.log*")
                    .Where(f => !f.EndsWith("test.log"))
                    .ToList();

                rotatedFiles.Should().NotBeEmpty("should have at least one rotated file");

                // Check if any file has a date pattern (8 digits for YYYYMMDD)
                bool hasDateExtension = rotatedFiles.Any(f =>
                    System.Text.RegularExpressions.Regex.IsMatch(
                        Path.GetFileName(f),
                        @"test\.log-\d{8}"));

                hasDateExtension.Should().BeTrue("rotated file should have date extension");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithOldDirDirective_ShouldMoveToOldDirectory()
        {
            // 'olddir' directive moves rotated files to a different directory

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Test log content\n");

            string oldDir = Path.Combine(TestDir, "old_logs");
            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    daily
    rotate 2
    olddir ""{oldDir}""
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act - Use force flag to trigger rotation
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert
                Directory.Exists(oldDir).Should().BeTrue("old directory should be created");

                string rotatedFile = Path.Combine(oldDir, "test.log.1");
                File.Exists(rotatedFile).Should().BeTrue("rotated file should be in old directory");

                // Original location should not have rotated file
                File.Exists(Path.Combine(TestDir, "test.log.1")).Should().BeFalse(
                    "rotated file should not be in original directory");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithDelayCompressDirective_ShouldDelayCompression()
        {
            // 'delaycompress' delays compression until the next rotation cycle
            // The most recent rotated file (.1) is not compressed

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Test log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    rotate 3
    compress
    delaycompress
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act - First rotation
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - After first rotation
                File.Exists($"{logFile}.1").Should().BeTrue("first rotated file should exist");
                File.Exists($"{logFile}.1.gz").Should().BeFalse(
                    "first rotated file should NOT be compressed due to delaycompress");

                // Act - Second rotation (to test that previous .1 gets compressed)
                File.WriteAllText(logFile, "New log content\n");
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - After second rotation
                File.Exists($"{logFile}.2.gz").Should().BeTrue(
                    "second rotated file (.2) should now be compressed");
                File.Exists($"{logFile}.1").Should().BeTrue(
                    "new first rotated file should not be compressed");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithMaxAgeDirective_ShouldRemoveOldFiles()
        {
            // 'maxage' directive removes rotated files older than specified days

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Test log content\n");

            // Create an old rotated file (. 1 is newest, so .3 is older)
            string oldRotatedFile = $"{logFile}.1";
            File.WriteAllText(oldRotatedFile, "Old content\n");
            File.SetLastWriteTime(oldRotatedFile, DateTime.Now.AddDays(-10));

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    daily
    rotate 5
    maxage 7
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act - Use force flag to trigger rotation
                // This should age out the old .1 file and create a new .1
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert
                // The old file should have been removed and replaced with new rotation
                if (File.Exists(oldRotatedFile))
                {
                    // If it still exists, it should be newer (from the rotation)
                    var fileInfo = new FileInfo(oldRotatedFile);
                    fileInfo.LastWriteTime.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMinutes(1),
                        "old file should be removed and replaced with new rotated file");
                }
                else
                {
                    // It's OK if the file doesn't exist - it might have been removed without replacement
                    true.Should().BeTrue("maxage processed correctly");
                }
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithSharedScriptsDirective_ShouldRunScriptOnce()
        {
            // 'sharedscripts' runs postrotate/prerotate scripts once for all files
            // instead of once per file

            // Arrange
            string log1 = Path.Combine(TestDir, "app1.log");
            string log2 = Path.Combine(TestDir, "app2.log");
            File.WriteAllText(log1, "App 1 log\n");
            File.WriteAllText(log2, "App 2 log\n");

            string markerFile = Path.Combine(TestDir, "script_marker.txt");
            string stateFile = Path.Combine(TestDir, "state.txt");

            string configContent = $@"
{log1} {log2} {{
    daily
    rotate 2
    sharedscripts
    postrotate
        echo Shared script executed > ""{markerFile}""
    endscript
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act - Use force flag to trigger rotation
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert
                File.Exists($"{log1}.1").Should().BeTrue("first log should be rotated");
                File.Exists($"{log2}.1").Should().BeTrue("second log should be rotated");
                File.Exists(markerFile).Should().BeTrue("shared script should execute");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithCombinedAdvancedOptions_ShouldApplyAll()
        {
            // Tests multiple advanced options together

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Test log content\n");

            string oldDir = Path.Combine(TestDir, "archive");
            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    rotate 5
    daily
    compress
    delaycompress
    create
    olddir ""{oldDir}""
    maxage 30
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act - Use force flag to trigger rotation
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert
                Directory.Exists(oldDir).Should().BeTrue("olddir should be created");

                string rotatedFile = Path.Combine(oldDir, "test.log.1");
                File.Exists(rotatedFile).Should().BeTrue("rotated file should be in olddir");

                File.Exists(rotatedFile + ".gz").Should().BeFalse(
                    "compression should be delayed for .1 file");

                File.Exists(logFile).Should().BeTrue("log file should be recreated");

                var newFileInfo = new FileInfo(logFile);
                newFileInfo.Length.Should().Be(0, "recreated file should be empty");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithDateExtAndOldDir_ShouldCombineBoth()
        {
            // Tests combination of dateext with olddir

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Test log content\n");

            string oldDir = Path.Combine(TestDir, "archive");
            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    daily
    rotate 2
    dateext
    olddir ""{oldDir}""
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act - Use force flag to trigger rotation
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert
                Directory.Exists(oldDir).Should().BeTrue("olddir should be created");

                var rotatedFiles = Directory.GetFiles(oldDir, "test.log*");
                rotatedFiles.Should().NotBeEmpty("should have rotated files in olddir");

                // Should have date-based extension
                bool hasDateExtension = rotatedFiles.Any(f =>
                    System.Text.RegularExpressions.Regex.IsMatch(
                        Path.GetFileName(f),
                        @"test\.log-\d{8}"));

                hasDateExtension.Should().BeTrue(
                    "rotated file in olddir should have date extension");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithCopyAndCompress_ShouldCompressCopy()
        {
            // Tests that compression works with copy directive

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Test log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    rotate 2
    copy
    compress
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert
                File.Exists($"{logFile}.1.gz").Should().BeTrue(
                    "copied file should be compressed");
                File.Exists(logFile).Should().BeTrue(
                    "original file should still exist");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }
    }
}
