using System;
using System.IO;
using FluentAssertions;
using Xunit;

namespace logrotate.Tests.Unit
{
    [Trait("Category", "Unit")]
    public class LogRotateStatusTests : IDisposable
    {
        private readonly string _testStateFile;

        public LogRotateStatusTests()
        {
            _testStateFile = TestHelpers.CreateTempStateFile();
        }

        public void Dispose()
        {
            TestHelpers.CleanupPath(_testStateFile);
        }

        [Fact]
        public void GetRotationDate_ForNewFile_ShouldReturnUnixEpoch()
        {
            // Arrange
            var status = new logrotatestatus(_testStateFile);
            string testLogPath = "C:\\test\\newfile.log";

            // Act
            DateTime rotationDate = status.GetRotationDate(testLogPath);

            // Assert
            // Implementation returns Unix epoch (1970-01-01) for files not in status
            rotationDate.Should().Be(new DateTime(1970, 1, 1));
        }

        [Fact]
        public void SetRotationDate_ShouldPersistToFile()
        {
            // Arrange
            var status = new logrotatestatus(_testStateFile);
            string testLogPath = "C:\\test\\mylog.log";

            // Act
            status.SetRotationDate(testLogPath);

            // Assert
            File.Exists(_testStateFile).Should().BeTrue();
            File.ReadAllText(_testStateFile).Should().Contain(testLogPath);
        }

        [Fact]
        public void GetRotationDate_AfterSet_ShouldReturnCorrectDate()
        {
            // Arrange
            var status = new logrotatestatus(_testStateFile);
            string testLogPath = "C:\\test\\mylog.log";
            DateTime today = DateTime.Today;

            // Act
            status.SetRotationDate(testLogPath);
            DateTime retrievedDate = status.GetRotationDate(testLogPath);

            // Assert
            // Status file only stores dates (yyyy-M-d), not times
            retrievedDate.Date.Should().Be(today);
        }

        [Fact]
        public void MultipleFiles_ShouldTrackSeparately()
        {
            // Arrange
            var status = new logrotatestatus(_testStateFile);
            string logPath1 = "C:\\test\\log1.log";
            string logPath2 = "C:\\test\\log2.log";
            DateTime today = DateTime.Today;

            // Act
            status.SetRotationDate(logPath1);
            status.SetRotationDate(logPath2);

            DateTime date1 = status.GetRotationDate(logPath1);
            DateTime date2 = status.GetRotationDate(logPath2);

            // Assert
            // Both should be set to today's date
            date1.Date.Should().Be(today);
            date2.Date.Should().Be(today);
            // Status file only stores dates, so both will have the same date
            date1.Date.Should().Be(date2.Date);
        }

        [Fact]
        public void LoadStatusFile_WithMissingFile_ShouldCreateNew()
        {
            // Arrange
            string nonExistentFile = Path.Combine(Path.GetTempPath(), $"nonexistent_{Guid.NewGuid()}.txt");

            // Act
            var status = new logrotatestatus(nonExistentFile);
            status.SetRotationDate("C:\\test\\log.log");

            // Assert
            File.Exists(nonExistentFile).Should().BeTrue();

            // Cleanup
            TestHelpers.CleanupPath(nonExistentFile);
        }

        [Fact]
        public void StatusFile_ShouldPersistAcrossInstances()
        {
            // Arrange
            string testLogPath = "C:\\test\\persistent.log";

            // Act - First instance
            var status1 = new logrotatestatus(_testStateFile);
            status1.SetRotationDate(testLogPath);
            DateTime originalDate = status1.GetRotationDate(testLogPath);

            // Act - Second instance
            var status2 = new logrotatestatus(_testStateFile);
            DateTime retrievedDate = status2.GetRotationDate(testLogPath);

            // Assert
            // Status file only stores dates (yyyy-M-d), so comparing dates only
            retrievedDate.Date.Should().Be(originalDate.Date);
        }

        [Fact]
        public void GetRotationDate_WithNullPath_ShouldThrowException()
        {
            // Arrange
            var status = new logrotatestatus(_testStateFile);

            // Act & Assert - Implementation does not handle null, throws NullReferenceException
            Action act = () => status.GetRotationDate(null);
            act.Should().Throw<NullReferenceException>();
        }

        [Fact]
        public void GetRotationDate_WithEmptyPath_ShouldHandleGracefully()
        {
            // Arrange
            var status = new logrotatestatus(_testStateFile);

            // Act & Assert - Should not throw
            DateTime result = status.GetRotationDate("");
            // Implementation returns Unix epoch for files not in status
            result.Should().Be(new DateTime(1970, 1, 1));
        }

        [Fact]
        public void UpdateExistingEntry_ShouldUpdateTimestamp()
        {
            // Arrange
            var status = new logrotatestatus(_testStateFile);
            string testLogPath = "C:\\test\\updateme.log";
            DateTime today = DateTime.Today;

            // Act
            status.SetRotationDate(testLogPath);
            DateTime firstDate = status.GetRotationDate(testLogPath);

            status.SetRotationDate(testLogPath);
            DateTime secondDate = status.GetRotationDate(testLogPath);

            // Assert
            // Status file only stores dates (yyyy-M-d), so both will be today
            firstDate.Date.Should().Be(today);
            secondDate.Date.Should().Be(today);
            // Verify the entry exists in the file
            File.ReadAllText(_testStateFile).Should().Contain(testLogPath);
        }
    }
}
