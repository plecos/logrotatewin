using FluentAssertions;
using System.IO;
using Xunit;

namespace logrotate.Tests.Integration
{
    /// <summary>
    /// Integration tests for the createolddir and nocreateolddir directive functionality.
    ///
    /// The createolddir directive (default behavior) automatically creates the olddir directory
    /// if it doesn't exist when rotating logs.
    ///
    /// The nocreateolddir directive prevents automatic creation of olddir, causing an error
    /// if the directory doesn't exist.
    /// </summary>
    public class CreateOldDirDirectiveTests : IntegrationTestBase
    {
        [Fact]
        public void RotateLog_WithOldDirAndDefaultCreateOldDir_ShouldCreateDirectory()
        {
            // Tests default behavior - olddir is created automatically if it doesn't exist

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Log content\n");

            string oldDir = Path.Combine(TestDir, "rotated");
            string stateFile = Path.Combine(TestDir, "state.txt");

            string configContent = $@"
{logFile} {{
    rotate 3
    olddir {oldDir}
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Ensure olddir doesn't exist before rotation
                Directory.Exists(oldDir).Should().BeFalse("olddir should not exist before rotation");

                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Directory should be created automatically
                Directory.Exists(oldDir).Should().BeTrue("olddir should be created automatically (default createolddir behavior)");
                File.Exists(Path.Combine(oldDir, "test.log.1")).Should().BeTrue("rotated file should be in olddir");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
                TestHelpers.CleanupPath(oldDir);
            }
        }

        [Fact]
        public void RotateLog_WithCreateOldDirExplicit_ShouldCreateDirectory()
        {
            // Tests explicit createolddir directive

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Log content\n");

            string oldDir = Path.Combine(TestDir, "archives");
            string stateFile = Path.Combine(TestDir, "state.txt");

            string configContent = $@"
{logFile} {{
    rotate 3
    olddir {oldDir}
    createolddir
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Ensure olddir doesn't exist
                Directory.Exists(oldDir).Should().BeFalse("olddir should not exist initially");

                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert
                Directory.Exists(oldDir).Should().BeTrue("createolddir should create the directory");
                File.Exists(Path.Combine(oldDir, "test.log.1")).Should().BeTrue("rotated file should be in created olddir");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
                TestHelpers.CleanupPath(oldDir);
            }
        }

        [Fact]
        public void RotateLog_WithNoCreateOldDirAndMissingDirectory_ShouldFallbackToLogDir()
        {
            // Tests that nocreateolddir prevents directory creation

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Log content\n");

            string oldDir = Path.Combine(TestDir, "nonexistent");
            string stateFile = Path.Combine(TestDir, "state.txt");

            string configContent = $@"
{logFile} {{
    rotate 3
    olddir {oldDir}
    nocreateolddir
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Ensure olddir doesn't exist
                Directory.Exists(oldDir).Should().BeFalse("olddir should not exist initially");

                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Directory should NOT be created
                Directory.Exists(oldDir).Should().BeFalse("nocreateolddir should prevent directory creation");

                // Rotated file should be in the same directory as the log file (fallback)
                File.Exists(Path.Combine(TestDir, "test.log.1")).Should().BeTrue(
                    "rotated file should be in log directory when olddir doesn't exist and nocreateolddir is set");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithNoCreateOldDirAndExistingDirectory_ShouldUseDirectory()
        {
            // Tests that nocreateolddir works fine when directory already exists

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Log content\n");

            string oldDir = Path.Combine(TestDir, "existing");
            Directory.CreateDirectory(oldDir); // Pre-create the directory

            string stateFile = Path.Combine(TestDir, "state.txt");

            string configContent = $@"
{logFile} {{
    rotate 3
    olddir {oldDir}
    nocreateolddir
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Should use existing directory
                File.Exists(Path.Combine(oldDir, "test.log.1")).Should().BeTrue(
                    "nocreateolddir should use olddir when it already exists");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
                TestHelpers.CleanupPath(oldDir);
            }
        }

        [Fact]
        public void RotateLog_WithCreateOldDirOverridingNoCreateOldDir_ShouldCreate()
        {
            // Tests that createolddir can override nocreateolddir (last directive wins)

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Log content\n");

            string oldDir = Path.Combine(TestDir, "override");
            string stateFile = Path.Combine(TestDir, "state.txt");

            string configContent = $@"
{logFile} {{
    rotate 3
    olddir {oldDir}
    nocreateolddir
    createolddir
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Ensure olddir doesn't exist
                Directory.Exists(oldDir).Should().BeFalse();

                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - createolddir should win (last directive)
                Directory.Exists(oldDir).Should().BeTrue("createolddir should override nocreateolddir");
                File.Exists(Path.Combine(oldDir, "test.log.1")).Should().BeTrue("rotated file should be in created olddir");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
                TestHelpers.CleanupPath(oldDir);
            }
        }

        [Fact]
        public void RotateLog_WithCreateOldDirAndDateExt_ShouldWorkTogether()
        {
            // Tests that createolddir works with date-based rotation

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Log content\n");

            string oldDir = Path.Combine(TestDir, "dated");
            string stateFile = Path.Combine(TestDir, "state.txt");

            string configContent = $@"
{logFile} {{
    rotate 3
    olddir {oldDir}
    createolddir
    dateext
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert
                Directory.Exists(oldDir).Should().BeTrue("olddir should be created");

                string[] dateFiles = Directory.GetFiles(oldDir, "test.log-*");
                dateFiles.Should().HaveCount(1, "should have one dated rotated file in olddir");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
                TestHelpers.CleanupPath(oldDir);
            }
        }

        [Fact]
        public void RotateLog_WithCreateOldDirAndCompress_ShouldWorkTogether()
        {
            // Tests that createolddir works with compression

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Log content for compression\n");

            string oldDir = Path.Combine(TestDir, "compressed");
            string stateFile = Path.Combine(TestDir, "state.txt");

            string configContent = $@"
{logFile} {{
    rotate 3
    olddir {oldDir}
    createolddir
    compress
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert
                Directory.Exists(oldDir).Should().BeTrue("olddir should be created");
                File.Exists(Path.Combine(oldDir, "test.log.1.gz")).Should().BeTrue(
                    "compressed rotated file should be in created olddir");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
                TestHelpers.CleanupPath(oldDir);
            }
        }

        [Fact]
        public void RotateLog_WithCreateOldDirMultipleRotations_ShouldUseDirectory()
        {
            // Tests that createolddir works across multiple rotations

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "First content\n");

            string oldDir = Path.Combine(TestDir, "multi");
            string stateFile = Path.Combine(TestDir, "state.txt");

            string configContent = $@"
{logFile} {{
    rotate 3
    olddir {oldDir}
    createolddir
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // First rotation
                RunLogRotate("-s", stateFile, "-f", configFile);
                File.Exists(Path.Combine(oldDir, "test.log.1")).Should().BeTrue();

                // Second rotation
                File.WriteAllText(logFile, "Second content\n");
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Both rotated files should be in olddir
                File.Exists(Path.Combine(oldDir, "test.log.1")).Should().BeTrue("newest rotation in olddir");
                File.Exists(Path.Combine(oldDir, "test.log.2")).Should().BeTrue("older rotation in olddir");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
                TestHelpers.CleanupPath(oldDir);
            }
        }

        [Fact]
        public void ParseConfig_WithCreateOldDirDirective_ShouldParseSuccessfully()
        {
            // Tests that the createolddir directive is properly parsed

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "test");

            string configContent = $@"
{logFile} {{
    createolddir
    rotate 1
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                var exitCode = RunLogRotate(configFile);

                // Assert
                exitCode.Should().Be(0, "config with createolddir directive should parse successfully");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void ParseConfig_WithNoCreateOldDirDirective_ShouldParseSuccessfully()
        {
            // Tests that the nocreateolddir directive is properly parsed

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "test");

            string configContent = $@"
{logFile} {{
    nocreateolddir
    rotate 1
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                var exitCode = RunLogRotate(configFile);

                // Assert
                exitCode.Should().Be(0, "config with nocreateolddir directive should parse successfully");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithCreateOldDirGlobal_ShouldApplyToAll()
        {
            // Tests that createolddir in global config applies to all sections

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Log content\n");

            string oldDir = Path.Combine(TestDir, "global");
            string stateFile = Path.Combine(TestDir, "state.txt");

            string configContent = $@"
createolddir

{logFile} {{
    rotate 3
    olddir {oldDir}
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Global createolddir should apply
                Directory.Exists(oldDir).Should().BeTrue("global createolddir should apply");
                File.Exists(Path.Combine(oldDir, "test.log.1")).Should().BeTrue("rotated file should be in olddir");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
                TestHelpers.CleanupPath(oldDir);
            }
        }
    }
}
