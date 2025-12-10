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
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = Path.Combine(
                Path.GetDirectoryName(typeof(ExitCodeTests).Assembly.Location),
                "..", "..", "..", "..", "logrotate", "bin", "Debug", "net48", "logrotate.exe"
            );

            // If debug build doesn't exist, try release
            if (!File.Exists(psi.FileName))
            {
                psi.FileName = Path.Combine(
                    Path.GetDirectoryName(typeof(ExitCodeTests).Assembly.Location),
                    "..", "..", "..", "..", "logrotate", "bin", "Release", "net48", "logrotate.exe"
                );
            }

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
        public void MissingConfigFile_ShouldExitWithConfigError()
        {
            // Arrange
            string nonExistentConfig = Path.Combine(Path.GetTempPath(), $"nonexistent_{Guid.NewGuid()}.conf");

            // Act
            int exitCode = RunLogRotate(nonExistentConfig);

            // Assert
            exitCode.Should().Be(EXIT_CONFIG_ERROR);
        }

        [Fact]
        public void EmptyConfigFile_ShouldExitWithNoFilesToRotate()
        {
            // Arrange
            string emptyConfig = TestHelpers.CreateTempConfigFile("# Empty config\n");

            try
            {
                // Act
                int exitCode = RunLogRotate(emptyConfig);

                // Assert
                exitCode.Should().Be(EXIT_NO_FILES_TO_ROTATE);
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
        public void ConfigWithNonExistentFile_AndMissingOk_ShouldExitWithNoFilesToRotate()
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
                exitCode.Should().Be(EXIT_NO_FILES_TO_ROTATE);
            }
            finally
            {
                TestHelpers.CleanupPath(config);
            }
        }
    }
}
