using FluentAssertions;
using System;
using System.Diagnostics;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace logrotate.Tests.Integration
{
    /// <summary>
    /// Integration tests for the include directive functionality.
    /// The include directive allows splitting configuration across multiple files.
    /// </summary>
    public class IncludeDirectiveTests : IntegrationTestBase
    {
        private readonly ITestOutputHelper _output;

        public IncludeDirectiveTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void RotateLog_WithBasicInclude_ShouldProcessIncludedConfig()
        {
            // Basic test to verify include directive works at all
            // This is a prerequisite for TabooExt testing

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Original log content\n");

            // Create an included config file
            string includeDir = Path.Combine(TestDir, "conf.d");
            Directory.CreateDirectory(includeDir);

            string includedConfig = Path.Combine(includeDir, "test.conf");
            string includedConfigContent = $@"{logFile} {{
    rotate 2
    create
}}";
            File.WriteAllText(includedConfig, includedConfigContent);

            string stateFile = Path.Combine(TestDir, "state.txt");
            string mainConfigContent = $@"include {includeDir}";
            string configFile = TestHelpers.CreateTempConfigFile(mainConfigContent);

            try
            {
                // Act - Run with verbose to capture output
                var (exitCode, stdout, stderr) = RunLogRotateWithOutput("-s", stateFile, "-v", "-f", configFile);

                _output.WriteLine("=== STDOUT ===");
                _output.WriteLine(stdout);
                _output.WriteLine("=== STDERR ===");
                _output.WriteLine(stderr);
                _output.WriteLine($"=== EXIT CODE: {exitCode} ===");

                // Assert
                File.Exists($"{logFile}.1").Should().BeTrue("included config should cause log rotation");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
                TestHelpers.CleanupPath(includeDir);
            }
        }

        private (int exitCode, string stdout, string stderr) RunLogRotateWithOutput(params string[] args)
        {
            var exePath = GetType().BaseType
                .GetMethod("GetLogRotateExePath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(this, null) as string;

            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = exePath;
            psi.Arguments = string.Join(" ", args);
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.CreateNoWindow = true;

            using (Process process = Process.Start(psi))
            {
                string stdout = process.StandardOutput.ReadToEnd();
                string stderr = process.StandardError.ReadToEnd();
                process.WaitForExit();
                return (process.ExitCode, stdout, stderr);
            }
        }
    }
}
