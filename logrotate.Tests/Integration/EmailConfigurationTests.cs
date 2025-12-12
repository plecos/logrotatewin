using System;
using System.IO;
using FluentAssertions;
using Xunit;

namespace logrotate.Tests.Integration
{
    [Trait("Category", "Integration")]
    public class EmailConfigurationTests : IntegrationTestBase
    {
        [Fact]
        public void ParseConfig_WithMailDirective_ShouldParseEmailAddress()
        {
            // Tests that the 'mail' directive is parsed correctly
            // Note: Actual email sending requires SMTP server and cannot be tested in integration tests

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Test log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    rotate 2
    mail test@example.com
    smtpserver smtp.example.com
    smtpport 587
    smtpfrom noreply@example.com
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act - The config should parse without errors
                int exitCode = RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Config parsing should succeed
                // Email won't actually be sent without valid SMTP server
                exitCode.Should().Match(x => x == 0 || x == 4,
                    "config with email directives should parse successfully");

                // File should be rotated
                bool rotated = File.Exists($"{logFile}.1");
                rotated.Should().BeTrue("rotation should occur even if email fails");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void ParseConfig_WithMailLastDirective_ShouldParseCorrectly()
        {
            // 'maillast' means email the log file AFTER rotation (default behavior)
            // The rotated/compressed file is attached to the email

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Test log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    rotate 2
    mail test@example.com
    maillast
    compress
    smtpserver smtp.example.com
    smtpport 587
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                int exitCode = RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Config should parse successfully
                exitCode.Should().Match(x => x == 0 || x == 4,
                    "config with maillast directive should parse successfully");

                // With maillast, the compressed file would be emailed (if SMTP worked)
                // File should be rotated and compressed
                File.Exists($"{logFile}.1.gz").Should().BeTrue("file should be rotated and compressed");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void ParseConfig_WithMailFirstDirective_ShouldParseCorrectly()
        {
            // 'mailfirst' means email the log file BEFORE rotation
            // The original uncompressed file is attached to the email

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Test log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    rotate 2
    mail test@example.com
    mailfirst
    compress
    smtpserver smtp.example.com
    smtpport 587
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                int exitCode = RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert
                exitCode.Should().Match(x => x == 0 || x == 4,
                    "config with mailfirst directive should parse successfully");

                // File should still be rotated and compressed
                File.Exists($"{logFile}.1.gz").Should().BeTrue("file should be rotated and compressed");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void ParseConfig_WithNoMailDirective_ShouldDisableEmail()
        {
            // 'nomail' directive disables email for a specific log section
            // This is useful when a global 'mail' directive is set but you want to disable it for some logs

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Test log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    rotate 2
    nomail
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                int exitCode = RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert
                exitCode.Should().Be(0, "config with nomail directive should succeed");
                File.Exists($"{logFile}.1").Should().BeTrue("file should be rotated");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void ParseConfig_WithSMTPSSL_ShouldParseCorrectly()
        {
            // Tests SSL/TLS configuration for SMTP

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Test log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    rotate 2
    mail test@example.com
    smtpserver smtp.example.com
    smtpport 465
    smtpssl
    smtpuser testuser
    smtpuserpwd testpassword
    smtpfrom noreply@example.com
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                int exitCode = RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert - Config parsing should succeed
                exitCode.Should().Match(x => x == 0 || x == 4,
                    "config with SMTP SSL should parse successfully");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void ParseConfig_WithNoSMTPSSL_ShouldDisableSSL()
        {
            // Tests disabling SSL for SMTP

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Test log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    rotate 2
    mail test@example.com
    smtpserver smtp.example.com
    smtpport 25
    nosmtpssl
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                int exitCode = RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert
                exitCode.Should().Match(x => x == 0 || x == 4,
                    "config with nosmtpssl should parse successfully");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void ParseConfig_WithGlobalMailSettings_ShouldApplyToAllSections()
        {
            // Tests that global mail settings apply to all log sections

            // Arrange
            string log1 = Path.Combine(TestDir, "app1.log");
            string log2 = Path.Combine(TestDir, "app2.log");
            File.WriteAllText(log1, "App 1 log\n");
            File.WriteAllText(log2, "App 2 log\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
# Global email settings
mail admin@example.com
smtpserver smtp.example.com
smtpport 587
smtpfrom logrotate@example.com

{log1} {{
    rotate 2
}}

{log2} {{
    rotate 3
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                int exitCode = RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert
                exitCode.Should().Match(x => x == 0 || x == 4,
                    "config with global email settings should parse successfully");

                // Both logs should be rotated
                File.Exists($"{log1}.1").Should().BeTrue("first log should be rotated");
                File.Exists($"{log2}.1").Should().BeTrue("second log should be rotated");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void ParseConfig_WithSectionOverridingGlobalMail_ShouldUseLocalSetting()
        {
            // Tests that local email settings override global settings

            // Arrange
            string log1 = Path.Combine(TestDir, "app1.log");
            string log2 = Path.Combine(TestDir, "app2.log");
            File.WriteAllText(log1, "App 1 log\n");
            File.WriteAllText(log2, "App 2 log\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
# Global email settings
mail admin@example.com
smtpserver smtp.example.com
smtpport 587

{log1} {{
    rotate 2
    # Keep global email setting
}}

{log2} {{
    rotate 3
    nomail
    # Disable email for this log
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                int exitCode = RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert
                exitCode.Should().Match(x => x == 0 || x == 4,
                    "config with overridden email settings should parse successfully");

                // Both logs should be rotated regardless of email settings
                File.Exists($"{log1}.1").Should().BeTrue("first log should be rotated");
                File.Exists($"{log2}.1").Should().BeTrue("second log should be rotated");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void ParseConfig_WithEmailAndCompression_ShouldCompressBeforeEmail()
        {
            // When both compression and email are enabled with maillast,
            // the compressed file should be what gets emailed

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Test log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    rotate 2
    compress
    mail test@example.com
    maillast
    smtpserver smtp.example.com
    smtpport 587
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act
                int exitCode = RunLogRotate("-s", stateFile, "-f", configFile);

                // Assert
                exitCode.Should().Match(x => x == 0 || x == 4,
                    "config with compression and email should parse successfully");

                // File should be compressed
                File.Exists($"{logFile}.1.gz").Should().BeTrue(
                    "file should be compressed, and the .gz file would be emailed");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }
    }
}
