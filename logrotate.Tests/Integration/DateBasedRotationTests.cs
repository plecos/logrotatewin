using System;
using System.IO;
using FluentAssertions;
using Xunit;

namespace logrotate.Tests.Integration
{
    [Trait("Category", "Integration")]
    public class DateBasedRotationTests : IntegrationTestBase
    {
        [Fact]
        public void RotateLog_WithDaily_ShouldRotateWhenLastRotationOverOneDayAgo()
        {
            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Test log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    daily
    rotate 2
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Create state file with a rotation date more than 1 day ago
                string twoDaysAgo = DateTime.Now.AddDays(-2).ToString("yyyy-M-d");
                File.WriteAllLines(stateFile, new[]
                {
                    "# logrotate state file",
                    "logrotate state -- version 2",
                    $"\"{logFile}\" {twoDaysAgo}"
                });

                // Act
                RunLogRotate("-s", stateFile, configFile);

                // Assert
                File.Exists($"{logFile}.1").Should().BeTrue("file should be rotated when last rotation was over 1 day ago");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithDaily_ShouldNotRotateWhenLastRotationLessThanOneDayAgo()
        {
            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Test log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    daily
    rotate 2
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Create state file with today's date
                string today = DateTime.Now.ToString("yyyy-M-d");
                File.WriteAllLines(stateFile, new[]
                {
                    "# logrotate state file",
                    "logrotate state -- version 2",
                    $"\"{logFile}\" {today}"
                });

                // Act
                RunLogRotate("-s", stateFile, configFile);

                // Assert
                File.Exists($"{logFile}.1").Should().BeFalse("file should not be rotated when last rotation was today");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithWeekly_ShouldRotateWhenLastRotationOverOneWeekAgo()
        {
            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Test log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    weekly
    rotate 2
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Create state file with a rotation date more than 1 week ago
                string eightDaysAgo = DateTime.Now.AddDays(-8).ToString("yyyy-M-d");
                File.WriteAllLines(stateFile, new[]
                {
                    "# logrotate state file",
                    "logrotate state -- version 2",
                    $"\"{logFile}\" {eightDaysAgo}"
                });

                // Act
                RunLogRotate("-s", stateFile, configFile);

                // Assert
                File.Exists($"{logFile}.1").Should().BeTrue("file should be rotated when last rotation was over 1 week ago");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithWeekly_ShouldRotateWhenWeekRollsOver()
        {
            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Test log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    weekly
    rotate 2
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Find a date in the past where the weekday was later in the week than today
                // For example, if today is Monday (1), set last rotation to Thursday (4) of last week
                DateTime testDate = DateTime.Now;
                int daysToSubtract = 3; // At least a few days ago

                // If we can find a date within 7 days that has a higher DayOfWeek, use it
                for (int i = 1; i <= 6; i++)
                {
                    DateTime candidate = DateTime.Now.AddDays(-i);
                    if (candidate.DayOfWeek > DateTime.Now.DayOfWeek)
                    {
                        testDate = candidate;
                        break;
                    }
                }

                // Only run this test if we found a suitable date
                if (testDate != DateTime.Now)
                {
                    string lastRotation = testDate.ToString("yyyy-M-d");
                    File.WriteAllLines(stateFile, new[]
                    {
                        "# logrotate state file",
                        "logrotate state -- version 2",
                        $"\"{logFile}\" {lastRotation}"
                    });

                    // Act
                    RunLogRotate("-s", stateFile, configFile);

                    // Assert
                    File.Exists($"{logFile}.1").Should().BeTrue("file should be rotated when week rolls over (DayOfWeek decreased)");
                }
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithMonthly_ShouldRotateWhenLastRotationInPreviousMonth()
        {
            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Test log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    monthly
    rotate 2
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Create state file with a rotation date from last month
                DateTime lastMonth = DateTime.Now.AddMonths(-1);
                string lastMonthDate = lastMonth.ToString("yyyy-M-d");
                File.WriteAllLines(stateFile, new[]
                {
                    "# logrotate state file",
                    "logrotate state -- version 2",
                    $"\"{logFile}\" {lastMonthDate}"
                });

                // Act
                RunLogRotate("-s", stateFile, configFile);

                // Assert
                File.Exists($"{logFile}.1").Should().BeTrue("file should be rotated when last rotation was in previous month");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithMonthly_ShouldNotRotateWhenLastRotationInCurrentMonth()
        {
            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Test log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    monthly
    rotate 2
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Create state file with a rotation date from earlier this month
                DateTime earlierThisMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                string thisMonthDate = earlierThisMonth.ToString("yyyy-M-d");
                File.WriteAllLines(stateFile, new[]
                {
                    "# logrotate state file",
                    "logrotate state -- version 2",
                    $"\"{logFile}\" {thisMonthDate}"
                });

                // Act
                RunLogRotate("-s", stateFile, configFile);

                // Assert
                File.Exists($"{logFile}.1").Should().BeFalse("file should not be rotated when last rotation was in current month");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithYearly_ShouldRotateWhenLastRotationInPreviousYear()
        {
            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Test log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    yearly
    rotate 2
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Create state file with a rotation date from last year
                DateTime lastYear = DateTime.Now.AddYears(-1);
                string lastYearDate = lastYear.ToString("yyyy-M-d");
                File.WriteAllLines(stateFile, new[]
                {
                    "# logrotate state file",
                    "logrotate state -- version 2",
                    $"\"{logFile}\" {lastYearDate}"
                });

                // Act
                RunLogRotate("-s", stateFile, configFile);

                // Assert
                File.Exists($"{logFile}.1").Should().BeTrue("file should be rotated when last rotation was in previous year");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithYearly_ShouldNotRotateWhenLastRotationInCurrentYear()
        {
            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Test log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    yearly
    rotate 2
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Create state file with a rotation date from earlier this year
                DateTime earlierThisYear = new DateTime(DateTime.Now.Year, 1, 1);
                string thisYearDate = earlierThisYear.ToString("yyyy-M-d");
                File.WriteAllLines(stateFile, new[]
                {
                    "# logrotate state file",
                    "logrotate state -- version 2",
                    $"\"{logFile}\" {thisYearDate}"
                });

                // Act
                RunLogRotate("-s", stateFile, configFile);

                // Assert
                File.Exists($"{logFile}.1").Should().BeFalse("file should not be rotated when last rotation was in current year");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_FirstRun_ShouldRotateWhenNoStateFileExists()
        {
            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Test log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    daily
    rotate 2
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Act - Run without existing state file
                RunLogRotate("-s", stateFile, configFile);

                // Assert
                File.Exists($"{logFile}.1").Should().BeTrue("file should be rotated on first run when no state exists");
                File.Exists(stateFile).Should().BeTrue("state file should be created");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_StateFile_ShouldUpdateRotationDate()
        {
            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            File.WriteAllText(logFile, "Test log content\n");

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    daily
    rotate 2
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Create state file with old rotation date
                string twoDaysAgo = DateTime.Now.AddDays(-2).ToString("yyyy-M-d");
                File.WriteAllLines(stateFile, new[]
                {
                    "# logrotate state file",
                    "logrotate state -- version 2",
                    $"\"{logFile}\" {twoDaysAgo}"
                });

                // Act
                RunLogRotate("-s", stateFile, configFile);

                // Assert - State file should be updated with today's date
                string stateContent = File.ReadAllText(stateFile);
                string todayDate = DateTime.Now.ToString("yyyy-M-d");
                stateContent.Should().Contain(todayDate, "state file should be updated with today's rotation date");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact]
        public void RotateLog_WithMinSizeAndDaily_ShouldRequireBothConditions()
        {
            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            TestHelpers.CreateTempLogFile(logFile, 512); // Small file below minsize

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    daily
    minsize 1k
    rotate 2
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Create state file with rotation date from 2 days ago
                string twoDaysAgo = DateTime.Now.AddDays(-2).ToString("yyyy-M-d");
                File.WriteAllLines(stateFile, new[]
                {
                    "# logrotate state file",
                    "logrotate state -- version 2",
                    $"\"{logFile}\" {twoDaysAgo}"
                });

                // Act
                RunLogRotate("-s", stateFile, configFile);

                // Assert - Should NOT rotate because file is below minsize
                File.Exists($"{logFile}.1").Should().BeFalse("file should not rotate when below minsize even if time criteria met");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }

        [Fact(Skip = "maxsize directive not implemented - see logrotateconf.cs")]
        public void RotateLog_WithMaxSizeAndDaily_ShouldRotateWhenMaxSizeExceeded()
        {
            // This test reveals that the 'maxsize' directive is not implemented in the config parser
            // According to Linux logrotate spec, maxsize should rotate when size exceeded OR time criteria met
            // Implementation needed: Add "case maxsize:" in logrotateconf.cs similar to minsize and size

            // Arrange
            string logFile = Path.Combine(TestDir, "test.log");
            TestHelpers.CreateTempLogFile(logFile, 2048); // Large file exceeding maxsize

            string stateFile = Path.Combine(TestDir, "state.txt");
            string configContent = $@"
{logFile} {{
    daily
    maxsize 1k
    rotate 2
}}
";
            string configFile = TestHelpers.CreateTempConfigFile(configContent);

            try
            {
                // Create state file with today's date (normally would NOT rotate)
                string today = DateTime.Now.ToString("yyyy-M-d");
                File.WriteAllLines(stateFile, new[]
                {
                    "# logrotate state file",
                    "logrotate state -- version 2",
                    $"\"{logFile}\" {today}"
                });

                // Act
                RunLogRotate("-s", stateFile, configFile);

                // Assert - Should rotate because maxsize is exceeded, overriding daily check
                File.Exists($"{logFile}.1").Should().BeTrue("file should rotate when maxsize exceeded even if daily not met");
            }
            finally
            {
                TestHelpers.CleanupPath(configFile);
            }
        }
    }
}
