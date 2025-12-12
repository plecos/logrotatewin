using FluentAssertions;
using System.IO;
using Xunit;

namespace logrotate.Tests.Integration
{
    /// <summary>
    /// Integration tests for the extension and addextension directive functionality.
    ///
    /// The 'extension' directive preserves the specified extension through rotation:
    ///   - For file "app.log" with "extension .log", rotated file becomes "app-YYYYMMDD.log" instead of "app.log-YYYYMMDD"
    ///   - The extension is removed, rotation suffix applied, then extension re-added
    ///
    /// The 'addextension' directive adds an extension AFTER the rotation suffix:
    ///   - For file "app.log" with "addextension .backup", rotated file becomes "app.log.1.backup"
    ///   - This is useful for marking rotated files or for compatibility with certain tools
    /// </summary>
    public class ExtensionDirectiveTests : IntegrationTestBase
    {
        [Fact]
        public void RotateLog_WithExtension_ShouldPreserveExtensionWithDateExt()
        {
            // Tests that extension directive preserves file extension when using dateext
            // Example: app.log with "extension .log" becomes app-20250101.log (not app.log-20250101)

            // Arrange
            string logFile = Path.Combine(TestDir, "app.log");
            File.WriteAllText(logFile, "Log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    rotate 3
    dateext
    extension .log
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Should have rotated file with format app-YYYYMMDD.log
                string[] rotatedFiles = Directory.GetFiles(TestDir, "app-*.log");
                rotatedFiles.Should().HaveCount(1, "should create one rotated file with preserved extension");
                rotatedFiles[0].Should().MatchRegex(@"app-\d{8}\.log$", "extension should be preserved after date");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithExtension_ShouldPreserveExtensionWithNumberedRotation()
        {
            // Tests that extension directive preserves file extension with numbered rotation
            // Example: app.log with "extension .log" becomes app.1.log (not app.log.1)

            // Arrange
            string logFile = Path.Combine(TestDir, "app.log");
            File.WriteAllText(logFile, "Log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    rotate 3
    extension .log
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Should have app.1.log (not app.log.1)
                File.Exists(Path.Combine(TestDir, "app.1.log")).Should().BeTrue("rotated file should have preserved extension");
                File.Exists(Path.Combine(TestDir, "app.log.1")).Should().BeFalse("extension should not appear before number");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithExtensionAndCompress_ShouldWorkTogether()
        {
            // Tests that extension works with compression
            // Example: app.log becomes app.1.log.gz (not app.log.1.gz)

            // Arrange
            string logFile = Path.Combine(TestDir, "app.log");
            File.WriteAllText(logFile, "Log content for compression\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    rotate 3
    extension .log
    compress
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Should have app.1.log.gz
                File.Exists(Path.Combine(TestDir, "app.1.log.gz")).Should().BeTrue("compressed rotated file should preserve extension");
                File.Exists(Path.Combine(TestDir, "app.log.1.gz")).Should().BeFalse("extension should not appear before number");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithExtensionMultipleRotations_ShouldPreserveConsistently()
        {
            // Tests that extension is preserved across multiple rotations

            // Arrange
            string logFile = Path.Combine(TestDir, "app.log");
            File.WriteAllText(logFile, "First content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    rotate 3
    extension .log
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act - First rotation
                RunLogRotate("-s", stateFile, "-f", configFile);
                File.Exists(Path.Combine(TestDir, "app.1.log")).Should().BeTrue();

                // Act - Second rotation
                File.WriteAllText(logFile, "Second content\n");
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Both rotations should preserve extension
                File.Exists(Path.Combine(TestDir, "app.1.log")).Should().BeTrue("newest rotation should have preserved extension");
                File.Exists(Path.Combine(TestDir, "app.2.log")).Should().BeTrue("older rotation should have preserved extension");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithAddExtension_ShouldAddExtensionAfterRotation()
        {
            // Tests that addextension adds extension after rotation suffix
            // Example: app.log with "addextension .backup" becomes app.log.1.backup

            // Arrange
            string logFile = Path.Combine(TestDir, "app.log");
            File.WriteAllText(logFile, "Log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    rotate 3
    addextension .backup
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Should have app.log.1.backup
                File.Exists(Path.Combine(TestDir, "app.log.1.backup")).Should().BeTrue("addextension should add extension after rotation number");
                File.Exists(Path.Combine(TestDir, "app.log.1")).Should().BeFalse("file should have added extension");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithAddExtensionAndDateExt_ShouldWorkTogether()
        {
            // Tests that addextension works with dateext
            // Example: app.log becomes app.log-20250101.backup

            // Arrange
            string logFile = Path.Combine(TestDir, "app.log");
            File.WriteAllText(logFile, "Log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    rotate 3
    dateext
    addextension .backup
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Should have app.log-YYYYMMDD.backup
                string[] rotatedFiles = Directory.GetFiles(TestDir, "app.log-*.backup");
                rotatedFiles.Should().HaveCount(1, "should create one rotated file with added extension");
                rotatedFiles[0].Should().MatchRegex(@"app\.log-\d{8}\.backup$", "addextension should be added after date");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithAddExtensionAndCompress_ShouldWorkTogether()
        {
            // Tests that addextension works with compression
            // Example: app.log becomes app.log.1.backup.gz

            // Arrange
            string logFile = Path.Combine(TestDir, "app.log");
            File.WriteAllText(logFile, "Log content for compression\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    rotate 3
    addextension .backup
    compress
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Should have app.log.1.backup.gz
                File.Exists(Path.Combine(TestDir, "app.log.1.backup.gz")).Should().BeTrue("compressed file should have added extension");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithBothExtensionAndAddExtension_ShouldApplyBoth()
        {
            // Tests that extension and addextension work together
            // Example: app.log with "extension .log" and "addextension .backup" becomes app.1.log.backup

            // Arrange
            string logFile = Path.Combine(TestDir, "app.log");
            File.WriteAllText(logFile, "Log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    rotate 3
    extension .log
    addextension .backup
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Should have app.1.log.backup (preserved extension, then added extension)
                File.Exists(Path.Combine(TestDir, "app.1.log.backup")).Should().BeTrue("should have both preserved and added extensions");
                File.Exists(Path.Combine(TestDir, "app.log.1.backup")).Should().BeFalse("extension should be preserved in correct position");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithBothExtensionAndAddExtensionAndCompress_ShouldApplyAll()
        {
            // Tests all three: extension, addextension, and compress
            // Example: app.log becomes app.1.log.backup.gz

            // Arrange
            string logFile = Path.Combine(TestDir, "app.log");
            File.WriteAllText(logFile, "Log content for compression\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    rotate 3
    extension .log
    addextension .backup
    compress
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Should have app.1.log.backup.gz
                File.Exists(Path.Combine(TestDir, "app.1.log.backup.gz")).Should().BeTrue("should apply extension, addextension, and compression");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithExtensionOnFileWithoutExtension_ShouldNotAffect()
        {
            // Tests that extension directive has no effect if file doesn't have that extension

            // Arrange
            string logFile = Path.Combine(TestDir, "app");
            File.WriteAllText(logFile, "Log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    rotate 3
    extension .log
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Should rotate normally as app.1 (extension directive has no effect)
                File.Exists(Path.Combine(TestDir, "app.1")).Should().BeTrue("file without matching extension should rotate normally");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void ParseConfig_WithExtensionDirective_ShouldParseSuccessfully()
        {
            // Tests that the extension directive is properly parsed

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "test");

            string configContent = $@"
{logFile} {{
    extension .log
    rotate 1
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                var exitCode = RunLogRotate(configFile);

                // Assert
                exitCode.Should().Be(0, "config with extension directive should parse successfully");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void ParseConfig_WithAddExtensionDirective_ShouldParseSuccessfully()
        {
            // Tests that the addextension directive is properly parsed

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "test");

            string configContent = $@"
{logFile} {{
    addextension .backup
    rotate 1
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                var exitCode = RunLogRotate(configFile);

                // Assert
                exitCode.Should().Be(0, "config with addextension directive should parse successfully");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithExtensionGlobal_ShouldApplyToAll()
        {
            // Tests that extension in global config applies to all sections

            // Arrange
            string logFile = Path.Combine(TestDir, "app.log");
            File.WriteAllText(logFile, "Log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
extension .log

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

                // Assert - Global extension should apply
                File.Exists(Path.Combine(TestDir, "app.1.log")).Should().BeTrue("global extension should apply");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithAddExtensionGlobal_ShouldApplyToAll()
        {
            // Tests that addextension in global config applies to all sections

            // Arrange
            string logFile = Path.Combine(TestDir, "app.log");
            File.WriteAllText(logFile, "Log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
addextension .backup

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

                // Assert - Global addextension should apply
                File.Exists(Path.Combine(TestDir, "app.log.1.backup")).Should().BeTrue("global addextension should apply");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }
    }
}
