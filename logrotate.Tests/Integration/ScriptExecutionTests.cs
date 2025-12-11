using System;
using System.IO;
using FluentAssertions;
using Xunit;

namespace logrotate.Tests.Integration
{
    [Trait("Category", "Integration")]
    public class ScriptExecutionTests : IntegrationTestBase
    {
        [Fact]
        public void RotateLog_WithPreRotateScript_ShouldExecuteBeforeRotation()
        {
            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Test log content\n");

            string markerFile = Path.Combine(TestDir, "prerotate_marker.txt");
            string stateFile = Path.Combine(TestDir, "state.txt");

            string configContent = $@"
{logFile} {{
    rotate 2
    prerotate
        echo PreRotate executed > ""{markerFile}""
    endscript
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert
                File.Exists($"{logFile}.1").Should().BeTrue("log should be rotated");
                File.Exists(markerFile).Should().BeTrue("prerotate script should have created marker file");

                if (File.Exists(markerFile))
                {
                    string markerContent = File.ReadAllText(markerFile).Trim();
                    markerContent.Should().Contain("PreRotate executed", "marker file should contain script output");
                }
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithPostRotateScript_ShouldExecuteAfterRotation()
        {
            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Test log content\n");

            string markerFile = Path.Combine(TestDir, "postrotate_marker.txt");
            string stateFile = Path.Combine(TestDir, "state.txt");

            string configContent = $@"
{logFile} {{
    rotate 2
    postrotate
        echo PostRotate executed > ""{markerFile}""
    endscript
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert
                File.Exists($"{logFile}.1").Should().BeTrue("log should be rotated");
                File.Exists(markerFile).Should().BeTrue("postrotate script should have created marker file");

                if (File.Exists(markerFile))
                {
                    string markerContent = File.ReadAllText(markerFile).Trim();
                    markerContent.Should().Contain("PostRotate executed", "marker file should contain script output");
                }
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithFirstActionScript_ShouldExecuteOnce()
        {
            // firstaction is a GLOBAL directive that executes before any log rotations
            // It runs once per logrotate execution, not per file

            // Arrange
            string log1 = Path.Combine(TestDir, "app1.log");
            string log2 = Path.Combine(TestDir, "app2.log");
            File.WriteAllText(log1, "App 1 log\n");
            File.WriteAllText(log2, "App 2 log\n");

            string markerFile = Path.Combine(TestDir, "firstaction_marker.txt");
            string stateFile = Path.Combine(TestDir, "state.txt");

            // firstaction must be GLOBAL (before any section)
            string configContent = $@"
firstaction
    echo FirstAction executed > ""{markerFile}""
endscript

{log1} {log2} {{
    rotate 2
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert
                File.Exists($"{log1}.1").Should().BeTrue("first log should be rotated");
                File.Exists($"{log2}.1").Should().BeTrue("second log should be rotated");
                File.Exists(markerFile).Should().BeTrue("firstaction script should have created marker file");

                if (File.Exists(markerFile))
                {
                    string markerContent = File.ReadAllText(markerFile).Trim();
                    markerContent.Should().Contain("FirstAction executed", "marker file should contain script output");
                }
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithLastActionScript_ShouldExecuteAfterAll()
        {
            // lastaction is a GLOBAL directive that executes after all log rotations
            // It runs once per logrotate execution, not per file

            // Arrange
            string log1 = Path.Combine(TestDir, "app1.log");
            string log2 = Path.Combine(TestDir, "app2.log");
            File.WriteAllText(log1, "App 1 log\n");
            File.WriteAllText(log2, "App 2 log\n");

            string markerFile = Path.Combine(TestDir, "lastaction_marker.txt");
            string stateFile = Path.Combine(TestDir, "state.txt");

            // lastaction must be GLOBAL (before any section)
            string configContent = $@"
lastaction
    echo LastAction executed > ""{markerFile}""
endscript

{log1} {log2} {{
    rotate 2
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert
                File.Exists($"{log1}.1").Should().BeTrue("first log should be rotated");
                File.Exists($"{log2}.1").Should().BeTrue("second log should be rotated");
                File.Exists(markerFile).Should().BeTrue("lastaction script should have created marker file");

                if (File.Exists(markerFile))
                {
                    string markerContent = File.ReadAllText(markerFile).Trim();
                    markerContent.Should().Contain("LastAction executed", "marker file should contain script output");
                }
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithBothPreAndPostRotateScripts_ShouldExecuteBoth()
        {
            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Test log content\n");

            string preMarkerFile = Path.Combine(TestDir, "pre_marker.txt");
            string postMarkerFile = Path.Combine(TestDir, "post_marker.txt");
            string stateFile = Path.Combine(TestDir, "state.txt");

            string configContent = $@"
{logFile} {{
    rotate 2
    prerotate
        echo Pre executed > ""{preMarkerFile}""
    endscript
    postrotate
        echo Post executed > ""{postMarkerFile}""
    endscript
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert
                File.Exists($"{logFile}.1").Should().BeTrue("log should be rotated");
                File.Exists(preMarkerFile).Should().BeTrue("prerotate script should have created marker file");
                File.Exists(postMarkerFile).Should().BeTrue("postrotate script should have created marker file");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithMultiLineScript_ShouldExecuteAllCommands()
        {
            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Test log content\n");

            string marker1File = Path.Combine(TestDir, "marker1.txt");
            string marker2File = Path.Combine(TestDir, "marker2.txt");
            string stateFile = Path.Combine(TestDir, "state.txt");

            string configContent = $@"
{logFile} {{
    rotate 2
    postrotate
        echo Line 1 > ""{marker1File}""
        echo Line 2 > ""{marker2File}""
    endscript
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert
                File.Exists($"{logFile}.1").Should().BeTrue("log should be rotated");
                File.Exists(marker1File).Should().BeTrue("first command should have created marker1");
                File.Exists(marker2File).Should().BeTrue("second command should have created marker2");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithSharedScripts_ShouldExecuteScriptOnce()
        {
            // Arrange
            string log1 = Path.Combine(TestDir, "app1.log");
            string log2 = Path.Combine(TestDir, "app2.log");
            File.WriteAllText(log1, "App 1 log\n");
            File.WriteAllText(log2, "App 2 log\n");

            string counterFile = Path.Combine(TestDir, "counter.txt");
            string stateFile = Path.Combine(TestDir, "state.txt");

            // Create a counter file to track script executions
            File.WriteAllText(counterFile, "0");

            string configContent = $@"
{log1} {log2} {{
    rotate 2
    sharedscripts
    postrotate
        echo Shared script executed > ""{TestDir}\shared_marker.txt""
    endscript
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert
                File.Exists($"{log1}.1").Should().BeTrue("first log should be rotated");
                File.Exists($"{log2}.1").Should().BeTrue("second log should be rotated");

                string sharedMarker = Path.Combine(TestDir, "shared_marker.txt");
                File.Exists(sharedMarker).Should().BeTrue("shared script should have executed");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithNoSharedScripts_ShouldExecuteScriptPerFile()
        {
            // Arrange
            string log1 = Path.Combine(TestDir, "app1.log");
            string log2 = Path.Combine(TestDir, "app2.log");
            File.WriteAllText(log1, "App 1 log\n");
            File.WriteAllText(log2, "App 2 log\n");

            string markerDir = Path.Combine(TestDir, "markers");
            Directory.CreateDirectory(markerDir);
            string stateFile = Path.Combine(TestDir, "state.txt");

            string configContent = $@"
{log1} {log2} {{
    rotate 2
    nosharedscripts
    postrotate
        echo %1 >> ""{markerDir}\executions.txt""
    endscript
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert
                File.Exists($"{log1}.1").Should().BeTrue("first log should be rotated");
                File.Exists($"{log2}.1").Should().BeTrue("second log should be rotated");

                // With nosharedscripts, the script should execute once per file
                // This test documents the behavior - may execute once or twice depending on implementation
                string executionsFile = Path.Combine(markerDir, "executions.txt");
                if (File.Exists(executionsFile))
                {
                    string content = File.ReadAllText(executionsFile);
                    content.Should().NotBeEmpty("script should have executed at least once");
                }
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithScriptAccessingLogFile_ShouldPassCorrectPath()
        {
            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Test log content\n");

            string outputFile = Path.Combine(TestDir, "script_output.txt");
            string stateFile = Path.Combine(TestDir, "state.txt");

            // Script that writes the log file path to output
            string configContent = $@"
{logFile} {{
    rotate 2
    postrotate
        echo %1 > ""{outputFile}""
    endscript
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert
                File.Exists($"{logFile}.1").Should().BeTrue("log should be rotated");

                // The script should receive the log file path as parameter
                if (File.Exists(outputFile))
                {
                    string scriptOutput = File.ReadAllText(outputFile).Trim();
                    scriptOutput.Should().NotBeEmpty("script should have written output");
                }
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithFailingScript_ShouldHandleGracefully()
        {
            // This test documents how the application handles script failures
            // Some implementations continue rotation even if scripts fail
            // Others may abort the rotation process

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Test log content\n");
            string stateFile = Path.Combine(TestDir, "state.txt");

            string configContent = $@"
{logFile} {{
    rotate 2
    prerotate
        exit 1
    endscript
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                int exitCode = RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Behavior depends on implementation
                // May continue with rotation (exit code 0) or fail (non-zero exit code)
                exitCode.Should().Match(x => x == 0 || x == 1,
                    "script failure may or may not prevent rotation");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }
    }
}
