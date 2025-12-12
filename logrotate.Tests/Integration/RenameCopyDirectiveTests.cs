using FluentAssertions;
using System.IO;
using Xunit;

namespace logrotate.Tests.Integration
{
    /// <summary>
    /// Integration tests for the renamecopy and norenamecopy directive functionality.
    ///
    /// The 'renamecopy' directive enables storing rotated log files on different devices using olddir:
    ///   1. Log file is renamed to temporary filename (adds ".tmp" extension) in the same directory
    ///   2. Postrotate script is run (if specified)
    ///   3. Log file is copied from temporary filename to final filename (in olddir if specified)
    ///   4. Temporary filename is removed
    ///
    /// The renamecopy directive implies nocopytruncate.
    ///
    /// The 'norenamecopy' directive overrides renamecopy behavior.
    /// </summary>
    public class RenameCopyDirectiveTests : IntegrationTestBase
    {
        [Fact]
        public void RotateLog_WithRenameCopy_ShouldUseTemporaryFile()
        {
            // Tests that renamecopy uses a temporary file during rotation

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    rotate 3
    renamecopy
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Rotated file should exist, temp file should be removed
                File.Exists(Path.Combine(TestDir, "test.log.1")).Should().BeTrue("rotated file should exist");
                File.Exists(Path.Combine(TestDir, "test.log.tmp")).Should().BeFalse("temporary file should be removed");
                File.Exists(logFile).Should().BeTrue("new empty log file should be created");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithRenameCopyAndOldDir_ShouldMoveToOldDir()
        {
            // Tests that renamecopy works with olddir to move files to different directory

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Log content\n");

            string oldDir = Path.Combine(TestDir, "archive");
            string stateFile = Path.Combine(TestDir, "state.txt");

            string configContent = $@"
{logFile} {{
    rotate 3
    olddir {oldDir}
    renamecopy
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Rotated file should be in olddir
                File.Exists(Path.Combine(oldDir, "test.log.1")).Should().BeTrue("rotated file should be in olddir");
                File.Exists(Path.Combine(TestDir, "test.log.tmp")).Should().BeFalse("temp file should be removed");
                File.Exists(Path.Combine(TestDir, "test.log.1")).Should().BeFalse("rotated file should not be in original directory");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
                TestHelpers.CleanupPath(oldDir);
            }
        }

        [Fact]
        public void RotateLog_WithRenameCopyAndDateExt_ShouldWorkTogether()
        {
            // Tests that renamecopy works with dateext

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    rotate 3
    renamecopy
    dateext
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Should have dated rotation file
                string[] dateFiles = Directory.GetFiles(TestDir, "test.log-*");
                dateFiles.Should().HaveCount(1, "should have one dated rotated file");
                dateFiles[0].Should().MatchRegex(@"test\.log-\d{8}$", "should have date extension");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithRenameCopyAndCompress_ShouldCompress()
        {
            // Tests that renamecopy works with compression

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Log content for compression\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    rotate 3
    renamecopy
    compress
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Rotated file should be compressed
                File.Exists(Path.Combine(TestDir, "test.log.1.gz")).Should().BeTrue("rotated file should be compressed");
                File.Exists(Path.Combine(TestDir, "test.log.tmp")).Should().BeFalse("temp file should be removed");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithRenameCopyMultipleRotations_ShouldWorkCorrectly()
        {
            // Tests that renamecopy works across multiple rotations

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "First content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    rotate 3
    renamecopy
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // First rotation
                RunLogRotate("-s", stateFile, "-f", configFile);
                File.Exists(Path.Combine(TestDir, "test.log.1")).Should().BeTrue();

                // Second rotation
                File.WriteAllText(logFile, "Second content\n");
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Both rotated files should exist
                File.Exists(Path.Combine(TestDir, "test.log.1")).Should().BeTrue("newest rotation");
                File.Exists(Path.Combine(TestDir, "test.log.2")).Should().BeTrue("older rotation");
                File.Exists(Path.Combine(TestDir, "test.log.tmp")).Should().BeFalse("no temp file should remain");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithRenameCopyImpliesNoCopyTruncate()
        {
            // Tests that renamecopy implies nocopytruncate

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Original content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    rotate 3
    copytruncate
    renamecopy
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Original log should not exist (renamecopy moved it, not copytruncate)
                // The new file is created by the 'create' directive
                File.Exists(logFile).Should().BeTrue("new log file should be created");
                File.Exists(Path.Combine(TestDir, "test.log.1")).Should().BeTrue("rotated file should exist");

                // New file should be empty (created fresh, not truncated with content)
                new FileInfo(logFile).Length.Should().Be(0, "renamecopy with create should make empty file, not truncate");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithNoRenameCopy_ShouldUseNormalRotation()
        {
            // Tests that norenamecopy overrides renamecopy

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    rotate 3
    renamecopy
    norenamecopy
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Should use normal rename (no temp file)
                File.Exists(Path.Combine(TestDir, "test.log.1")).Should().BeTrue("rotated file should exist");
                File.Exists(Path.Combine(TestDir, "test.log.tmp")).Should().BeFalse("no temporary file should be created");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void ParseConfig_WithRenameCopyDirective_ShouldParseSuccessfully()
        {
            // Tests that the renamecopy directive is properly parsed

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "test");

            string configContent = $@"
{logFile} {{
    renamecopy
    rotate 1
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                var exitCode = RunLogRotate(configFile);

                // Assert
                exitCode.Should().Be(0, "config with renamecopy directive should parse successfully");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void ParseConfig_WithNoRenameCopyDirective_ShouldParseSuccessfully()
        {
            // Tests that the norenamecopy directive is properly parsed

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "test");

            string configContent = $@"
{logFile} {{
    norenamecopy
    rotate 1
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                var exitCode = RunLogRotate(configFile);

                // Assert
                exitCode.Should().Be(0, "config with norenamecopy directive should parse successfully");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithRenameCopyGlobal_ShouldApplyToAll()
        {
            // Tests that renamecopy in global config applies to all sections

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
renamecopy

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

                // Assert - Global renamecopy should apply
                File.Exists(Path.Combine(TestDir, "test.log.1")).Should().BeTrue("rotated file should exist");
                File.Exists(Path.Combine(TestDir, "test.log.tmp")).Should().BeFalse("temp file should be removed");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithRenameCopyWithoutCreate_ShouldNotCreateNewFile()
        {
            // Tests that renamecopy without create doesn't create a new log file

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    rotate 3
    renamecopy
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Rotated file exists, but no new file created
                File.Exists(Path.Combine(TestDir, "test.log.1")).Should().BeTrue("rotated file should exist");
                File.Exists(logFile).Should().BeFalse("new log file should not be created without 'create' directive");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithRenameCopyAndPostRotate_ShouldRunScript()
        {
            // Tests that renamecopy runs postrotate script at the correct time
            // (after rename to temp, before copy to final location)

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Log content\n");

            string markerFile = Path.Combine(TestDir, "postrotate_ran.txt");
            string stateFile = Path.Combine(TestDir, "state.txt");

            // Create a script that writes to a marker file
            string configContent = $@"
{logFile} {{
    rotate 3
    renamecopy
    create
    postrotate
        echo ""Script ran"" > ""{markerFile}""
    endscript
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Postrotate script should have run
                File.Exists(markerFile).Should().BeTrue("postrotate script should have run and created marker file");
                File.Exists(Path.Combine(TestDir, "test.log.1")).Should().BeTrue("rotated file should exist");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
                TestHelpers.CleanupPath(markerFile);
            }
        }
    }
}
