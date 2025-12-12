using FluentAssertions;
using System;
using System.IO;
using Xunit;

namespace logrotate.Tests.Integration
{
    /// <summary>
    /// Integration tests for the preremove directive functionality.
    /// The preremove directive executes a script before deleting old log files.
    /// This is useful for archiving, auditing, or custom cleanup tasks.
    /// </summary>
    public class PreRemoveDirectiveTests : IntegrationTestBase
    {
        [Fact]
        public void RotateLog_WithPreRemoveScript_ShouldExecuteBeforeDeletion()
        {
            // Tests that preremove script executes before file deletion

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Log content\n");

            string markerFile = Path.Combine(TestDir, "preremove_executed.txt");
            string stateFile = Path.Combine(TestDir, "state.txt");

            // Create a script that writes to a marker file
            string configContent = $@"
{logFile} {{
    rotate 1
    create
    preremove
        echo PreRemove executed > ""{markerFile}""
    endscript
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act - First rotation (creates .1)
                RunLogRotate("-s", stateFile, "-f", configFile);
                File.Exists($"{logFile}.1").Should().BeTrue("first rotation should create .1");

                // Act - Second rotation (should trigger preremove before deleting .1)
                File.WriteAllText(logFile, "New content\n");
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Preremove script should have executed
                File.Exists(markerFile).Should().BeTrue("preremove script should have created marker file");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
                TestHelpers.CleanupPath(markerFile);
            }
        }

        [Fact]
        public void RotateLog_WithPreRemoveAndRotateCount_ShouldExecuteOnExcess()
        {
            // Tests that preremove only runs when files actually get deleted (beyond rotate count)

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Log content\n");

            string preremoveLog = Path.Combine(TestDir, "preremove.log");
            string stateFile = Path.Combine(TestDir, "state.txt");

            string configContent = $@"
{logFile} {{
    rotate 2
    create
    preremove
        echo Deleting file >> ""{preremoveLog}""
    endscript
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act - First rotation (no deletion)
                RunLogRotate("-s", stateFile, "-f", configFile);
                File.Exists(preremoveLog).Should().BeFalse("no file deleted yet");

                // Act - Second rotation (no deletion yet)
                File.WriteAllText(logFile, "Content 2\n");
                RunLogRotate("-s", stateFile, "-f", configFile);
                File.Exists(preremoveLog).Should().BeFalse("still no file deleted (within rotate 2 limit)");

                // Act - Third rotation (should delete .2, triggering preremove)
                File.WriteAllText(logFile, "Content 3\n");
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Preremove should have run for the deleted file
                File.Exists(preremoveLog).Should().BeTrue("preremove should execute when file exceeds rotate count");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
                TestHelpers.CleanupPath(preremoveLog);
            }
        }

        [Fact]
        public void RotateLog_WithPreRemoveAndShred_ShouldExecuteBeforeShredding()
        {
            // Tests that preremove executes before shredding

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Sensitive log content\n");

            string markerFile = Path.Combine(TestDir, "before_shred.txt");
            string stateFile = Path.Combine(TestDir, "state.txt");

            string configContent = $@"
{logFile} {{
    rotate 1
    create
    shred
    shredcycles 1
    preremove
        echo Before shredding >> ""{markerFile}""
    endscript
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act - First rotation
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Act - Second rotation (triggers shred of .1 file)
                File.WriteAllText(logFile, "New content\n");
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert
                File.Exists(markerFile).Should().BeTrue("preremove should execute before shredding");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
                TestHelpers.CleanupPath(markerFile);
            }
        }

        [Fact]
        public void RotateLog_WithPreRemoveMultiline_ShouldExecuteAllCommands()
        {
            // Tests that preremove can contain multiple commands

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Log content\n");

            string marker1 = Path.Combine(TestDir, "marker1.txt");
            string marker2 = Path.Combine(TestDir, "marker2.txt");
            string stateFile = Path.Combine(TestDir, "state.txt");

            string configContent = $@"
{logFile} {{
    rotate 1
    create
    preremove
        echo Command 1 > ""{marker1}""
        echo Command 2 > ""{marker2}""
    endscript
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act - First rotation
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Act - Second rotation (triggers preremove)
                File.WriteAllText(logFile, "New content\n");
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Both commands should have executed
                File.Exists(marker1).Should().BeTrue("first preremove command should execute");
                File.Exists(marker2).Should().BeTrue("second preremove command should execute");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
                TestHelpers.CleanupPath(marker1);
                TestHelpers.CleanupPath(marker2);
            }
        }

        [Fact]
        public void RotateLog_WithPreRemoveAndCompress_ShouldWorkTogether()
        {
            // Tests that preremove works with compressed log files

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Log content for compression\n");

            string preremoveLog = Path.Combine(TestDir, "preremove.log");
            string stateFile = Path.Combine(TestDir, "state.txt");

            string configContent = $@"
{logFile} {{
    rotate 1
    compress
    create
    preremove
        echo Deleting compressed file >> ""{preremoveLog}""
    endscript
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act - First rotation (creates .1.gz)
                RunLogRotate("-s", stateFile, "-f", configFile);
                File.Exists($"{logFile}.1.gz").Should().BeTrue();

                // Act - Second rotation (deletes .1.gz, should run preremove)
                File.WriteAllText(logFile, "New content\n");
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert
                File.Exists(preremoveLog).Should().BeTrue("preremove should execute for compressed files");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
                TestHelpers.CleanupPath(preremoveLog);
            }
        }

        [Fact]
        public void RotateLog_WithPreRemoveAndDateExt_ShouldWorkWithDateExtensions()
        {
            // Tests that preremove works with dateext naming

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Log content\n");

            string markerFile = Path.Combine(TestDir, "preremove_dateext.txt");
            string stateFile = Path.Combine(TestDir, "state.txt");

            string configContent = $@"
{logFile} {{
    rotate 1
    dateext
    create
    preremove
        echo Removing dated file > ""{markerFile}""
    endscript
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act - First rotation
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Act - Second rotation (should delete old dated file)
                System.Threading.Thread.Sleep(1000); // Ensure different timestamp
                File.WriteAllText(logFile, "New content\n");
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert
                File.Exists(markerFile).Should().BeTrue("preremove should execute for dateext files");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
                TestHelpers.CleanupPath(markerFile);
            }
        }

        [Fact]
        public void RotateLog_WithoutPreRemove_ShouldDeleteNormally()
        {
            // Tests that deletion works normally without preremove directive

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    rotate 1
    create
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act - First rotation
                RunLogRotate("-s", stateFile, "-f", configFile);
                File.Exists($"{logFile}.1").Should().BeTrue();

                // Act - Second rotation (should delete .1)
                File.WriteAllText(logFile, "New content\n");
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Old file should be deleted
                File.Exists($"{logFile}.2").Should().BeFalse("old file beyond rotate count should be deleted");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void ParseConfig_WithPreRemoveDirective_ShouldParseSuccessfully()
        {
            // Tests that the preremove directive is properly parsed

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "test");

            string configContent = $@"
{logFile} {{
    rotate 1
    preremove
        echo Test preremove script
    endscript
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                var exitCode = RunLogRotate(configFile);

                // Assert
                exitCode.Should().Be(0, "config with preremove directive should parse successfully");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }
    }
}
