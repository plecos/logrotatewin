using System;
using System.Diagnostics;
using System.IO;
using FluentAssertions;
using Xunit;

namespace logrotate.Tests.Unit
{
    [Trait("Category", "Unit")]
    public class ExitCodeTests
    {
        private const int EXIT_SUCCESS = 0;
        private const int EXIT_GENERAL_ERROR = 1;
        private const int EXIT_INVALID_ARGUMENTS = 2;
        private const int EXIT_CONFIG_ERROR = 3;
        private const int EXIT_NO_FILES_TO_ROTATE = 4;

        private int RunLogRotate(string args)
        {
            // Use CodeBase instead of Location to get the actual file path (not shadow copy)
            string testAssemblyCodeBase = typeof(ExitCodeTests).Assembly.CodeBase;
            Uri uri = new Uri(testAssemblyCodeBase);
            string testAssemblyPath = Uri.UnescapeDataString(uri.AbsolutePath);
            string testBinDir = Path.GetDirectoryName(testAssemblyPath);

            // Navigate to solution root and find the exe
            // Test assembly is at: F:\Repos\logrotatewin\logrotate.Tests\bin\Debug\net48\
            // Main exe is at:     F:\Repos\logrotatewin\logrotate\bin\Debug\net48\logrotate.exe
            // So we go up 4 levels to solution root, then down to logrotate project
            string exePath = Path.GetFullPath(Path.Combine(testBinDir, "..", "..", "..", "..", "logrotate", "bin", "Debug", "net48", "logrotate.exe"));

            // If debug build doesn't exist, try release
            if (!File.Exists(exePath))
            {
                exePath = Path.GetFullPath(Path.Combine(testBinDir, "..", "..", "..", "..", "logrotate", "bin", "Release", "net48", "logrotate.exe"));
            }

            // If still not found, throw a helpful error
            if (!File.Exists(exePath))
            {
                throw new FileNotFoundException(
                    $"Could not find logrotate.exe. Looked in:\n" +
                    $"- {Path.GetFullPath(Path.Combine(testBinDir, "..", "..", "..", "..", "logrotate", "bin", "Debug", "net48", "logrotate.exe"))}\n" +
                    $"- {Path.GetFullPath(Path.Combine(testBinDir, "..", "..", "..", "..", "logrotate", "bin", "Release", "net48", "logrotate.exe"))}\n" +
                    $"Test bin directory: {testBinDir}\n" +
                    $"CodeBase: {testAssemblyCodeBase}"
                );
            }

            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = exePath;
            psi.Arguments = args;
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.CreateNoWindow = true;

            using (Process process = Process.Start(psi))
            {
                process.WaitForExit();
                return process.ExitCode;
            }
        }

        [Fact]
        public void NoArgs_ShouldExitWithSuccess()
        {
            // Act
            int exitCode = RunLogRotate("");

            // Assert
            exitCode.Should().Be(EXIT_SUCCESS);
        }

        [Fact]
        public void HelpFlag_ShouldExitWithSuccess()
        {
            // Act
            int exitCode = RunLogRotate("--usage");

            // Assert
            exitCode.Should().Be(EXIT_SUCCESS);
        }

        [Fact]
        public void QuestionMarkFlag_ShouldExitWithSuccess()
        {
            // Act
            int exitCode = RunLogRotate("-?");

            // Assert
            exitCode.Should().Be(EXIT_SUCCESS);
        }

        [Fact]
        public void MissingConfigFile_ShouldExitWithError()
        {
            // Arrange
            string nonExistentConfig = Path.Combine(Path.GetTempPath(), $"nonexistent_{Guid.NewGuid()}.conf");

            // Act
            int exitCode = RunLogRotate(nonExistentConfig);

            // Assert
            // Currently returns GENERAL_ERROR (1) instead of CONFIG_ERROR (3)
            // This could be improved to use CONFIG_ERROR for missing config files
            exitCode.Should().Be(EXIT_GENERAL_ERROR);
        }

        [Fact]
        public void EmptyConfigFile_ShouldExitWithError()
        {
            // Arrange
            string emptyConfig = TestHelpers.CreateTempConfigFile("# Empty config\n");

            try
            {
                // Act
                int exitCode = RunLogRotate(emptyConfig);

                // Assert
                // Currently returns GENERAL_ERROR (1) when config has no file sections
                // This could be improved to return NO_FILES_TO_ROTATE (4)
                exitCode.Should().Be(EXIT_GENERAL_ERROR);
            }
            finally
            {
                TestHelpers.CleanupPath(emptyConfig);
            }
        }

        [Fact]
        public void ValidConfig_WithExistingFile_ShouldExitWithSuccess()
        {
            // Arrange
            string testLog = TestHelpers.CreateTempLogFile(1024);
            string config = TestHelpers.CreateTempConfigFile($@"
{testLog} {{
    daily
    rotate 5
}}
");

            try
            {
                // Act
                int exitCode = RunLogRotate(config);

                // Assert
                exitCode.Should().BeOneOf(EXIT_SUCCESS, EXIT_NO_FILES_TO_ROTATE);
            }
            finally
            {
                TestHelpers.CleanupPath(testLog);
                TestHelpers.CleanupPath(config);
            }
        }

        [Fact]
        public void ConfigWithNonExistentFile_AndMissingOk_ShouldExitWithSuccess()
        {
            // Arrange
            string nonExistentLog = Path.Combine(Path.GetTempPath(), $"missing_{Guid.NewGuid()}.log");
            string config = TestHelpers.CreateTempConfigFile($@"
{nonExistentLog} {{
    daily
    rotate 5
    missingok
}}
");

            try
            {
                // Act
                int exitCode = RunLogRotate(config);

                // Assert
                // With missingok, the program exits successfully even though no files exist
                // This is reasonable behavior - not finding files with missingok is not an error
                exitCode.Should().Be(EXIT_SUCCESS);
            }
            finally
            {
                TestHelpers.CleanupPath(config);
            }
        }
    }
}
