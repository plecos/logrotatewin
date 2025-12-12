using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace logrotate.Tests.Unit
{
    [Trait("Category", "Unit")]
    public class CmdLineArgsTests
    {
        [Fact]
        public void ParseDebugFlag_ShouldEnableDebugAndVerbose()
        {
            // Arrange
            string[] args = { "-d", "test.conf" };

            // Act
            var cmdLineArgs = new CmdLineArgs(args);

            // Assert
            cmdLineArgs.Debug.Should().BeTrue();
            cmdLineArgs.Verbose.Should().BeTrue();
            cmdLineArgs.ConfigFilePaths.Should().ContainSingle()
                .Which.Should().Be("test.conf");
        }

        [Fact]
        public void ParseForceFlag_ShouldEnableForce()
        {
            // Arrange
            string[] args = { "-f", "test.conf" };

            // Act
            var cmdLineArgs = new CmdLineArgs(args);

            // Assert
            cmdLineArgs.Force.Should().BeTrue();
            cmdLineArgs.ConfigFilePaths.Should().ContainSingle();
        }

        [Fact]
        public void ParseForceLongFlag_ShouldEnableForce()
        {
            // Arrange
            string[] args = { "--force", "test.conf" };

            // Act
            var cmdLineArgs = new CmdLineArgs(args);

            // Assert
            cmdLineArgs.Force.Should().BeTrue();
        }

        [Fact]
        public void ParseVerboseFlag_ShouldEnableVerbose()
        {
            // Arrange
            string[] args = { "-v", "test.conf" };

            // Act
            var cmdLineArgs = new CmdLineArgs(args);

            // Assert
            cmdLineArgs.Verbose.Should().BeTrue();
            cmdLineArgs.Debug.Should().BeFalse();
        }

        [Fact]
        public void ParseVerboseLongFlag_ShouldEnableVerbose()
        {
            // Arrange
            string[] args = { "--verbose", "test.conf" };

            // Act
            var cmdLineArgs = new CmdLineArgs(args);

            // Assert
            cmdLineArgs.Verbose.Should().BeTrue();
        }

        [Fact]
        public void ParseStateFlag_WithValidPath_ShouldSetAlternateStateFile()
        {
            // Arrange
            string[] args = { "-s", "custom_state.txt", "test.conf" };

            // Act
            var cmdLineArgs = new CmdLineArgs(args);

            // Assert
            cmdLineArgs.AlternateStateFile.Should().Be("custom_state.txt");
            cmdLineArgs.ConfigFilePaths.Should().ContainSingle()
                .Which.Should().Be("test.conf");
        }

        [Fact]
        public void ParseStateLongFlag_WithValidPath_ShouldSetAlternateStateFile()
        {
            // Arrange
            string[] args = { "--state", "custom_state.txt", "test.conf" };

            // Act
            var cmdLineArgs = new CmdLineArgs(args);

            // Assert
            cmdLineArgs.AlternateStateFile.Should().Be("custom_state.txt");
        }

        [Fact]
        public void ParseUsageFlag_ShouldSetUsageTrue()
        {
            // Arrange
            string[] args = { "-?" };

            // Act
            var cmdLineArgs = new CmdLineArgs(args);

            // Assert
            cmdLineArgs.Usage.Should().BeTrue();
        }

        [Fact]
        public void ParseUsageLongFlag_ShouldSetUsageTrue()
        {
            // Arrange
            string[] args = { "--usage" };

            // Act
            var cmdLineArgs = new CmdLineArgs(args);

            // Assert
            cmdLineArgs.Usage.Should().BeTrue();
        }

        [Fact]
        public void ParseConfigPath_ShouldAddToConfigFilePaths()
        {
            // Arrange
            string[] args = { "test.conf" };

            // Act
            var cmdLineArgs = new CmdLineArgs(args);

            // Assert
            cmdLineArgs.ConfigFilePaths.Should().ContainSingle()
                .Which.Should().Be("test.conf");
        }

        [Fact]
        public void ParseMultipleConfigPaths_ShouldAddAll()
        {
            // Arrange
            string[] args = { "test1.conf", "test2.conf", "test3.conf" };

            // Act
            var cmdLineArgs = new CmdLineArgs(args);

            // Assert
            cmdLineArgs.ConfigFilePaths.Should().HaveCount(3);
            cmdLineArgs.ConfigFilePaths.Should().Contain("test1.conf");
            cmdLineArgs.ConfigFilePaths.Should().Contain("test2.conf");
            cmdLineArgs.ConfigFilePaths.Should().Contain("test3.conf");
        }

        [Fact]
        public void ParseCombinedFlags_ShouldSetAllCorrectly()
        {
            // Arrange
            string[] args = { "-d", "-f", "-s", "state.txt", "test.conf" };

            // Act
            var cmdLineArgs = new CmdLineArgs(args);

            // Assert
            cmdLineArgs.Debug.Should().BeTrue();
            cmdLineArgs.Verbose.Should().BeTrue();
            cmdLineArgs.Force.Should().BeTrue();
            cmdLineArgs.AlternateStateFile.Should().Be("state.txt");
            cmdLineArgs.ConfigFilePaths.Should().ContainSingle()
                .Which.Should().Be("test.conf");
        }

        [Fact]
        public void ParseMixedShortAndLongFlags_ShouldWork()
        {
            // Arrange
            string[] args = { "-d", "--force", "-v", "test.conf" };

            // Act
            var cmdLineArgs = new CmdLineArgs(args);

            // Assert
            cmdLineArgs.Debug.Should().BeTrue();
            cmdLineArgs.Force.Should().BeTrue();
            cmdLineArgs.Verbose.Should().BeTrue();
        }

        [Fact]
        public void Constructor_WithMultipleFlags_ShouldProcessInOrder()
        {
            // Arrange
            string[] args = { "-v", "-f", "-s", "mystate.txt", "config1.conf", "config2.conf" };

            // Act
            var cmdLineArgs = new CmdLineArgs(args);

            // Assert
            cmdLineArgs.Verbose.Should().BeTrue();
            cmdLineArgs.Force.Should().BeTrue();
            cmdLineArgs.AlternateStateFile.Should().Be("mystate.txt");
            cmdLineArgs.ConfigFilePaths.Should().HaveCount(2);
        }
    }
}
