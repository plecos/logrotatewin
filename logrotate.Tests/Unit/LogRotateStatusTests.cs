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
        public void GetRotationDate_ForNewFile_ShouldReturnMinValue()
        {
            // Arrange
            var status = new logrotatestatus(_testStateFile);
            string testLogPath = "C:\\test\\newfile.log";

            // Act
            DateTime rotationDate = status.GetRotationDate(testLogPath);

            // Assert
            rotationDate.Should().Be(DateTime.MinValue);
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
            DateTime beforeSet = DateTime.Now.AddSeconds(-1);

            // Act
            status.SetRotationDate(testLogPath);
            DateTime afterSet = DateTime.Now.AddSeconds(1);
            DateTime retrievedDate = status.GetRotationDate(testLogPath);

            // Assert
            retrievedDate.Should().BeAfter(beforeSet);
            retrievedDate.Should().BeBefore(afterSet);
        }

        [Fact]
        public void MultipleFiles_ShouldTrackSeparately()
        {
            // Arrange
            var status = new logrotatestatus(_testStateFile);
            string logPath1 = "C:\\test\\log1.log";
            string logPath2 = "C:\\test\\log2.log";

            // Act
            status.SetRotationDate(logPath1);
            System.Threading.Thread.Sleep(100); // Ensure different timestamps
            status.SetRotationDate(logPath2);

            DateTime date1 = status.GetRotationDate(logPath1);
            DateTime date2 = status.GetRotationDate(logPath2);

            // Assert
            date1.Should().NotBe(DateTime.MinValue);
            date2.Should().NotBe(DateTime.MinValue);
            date2.Should().BeAfter(date1);
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
            retrievedDate.Should().BeCloseTo(originalDate, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void GetRotationDate_WithNullPath_ShouldHandleGracefully()
        {
            // Arrange
            var status = new logrotatestatus(_testStateFile);

            // Act & Assert - Should not throw
            DateTime result = status.GetRotationDate(null);
            result.Should().Be(DateTime.MinValue);
        }

        [Fact]
        public void GetRotationDate_WithEmptyPath_ShouldHandleGracefully()
        {
            // Arrange
            var status = new logrotatestatus(_testStateFile);

            // Act & Assert - Should not throw
            DateTime result = status.GetRotationDate("");
            result.Should().Be(DateTime.MinValue);
        }

        [Fact]
        public void UpdateExistingEntry_ShouldUpdateTimestamp()
        {
            // Arrange
            var status = new logrotatestatus(_testStateFile);
            string testLogPath = "C:\\test\\updateme.log";

            // Act
            status.SetRotationDate(testLogPath);
            DateTime firstDate = status.GetRotationDate(testLogPath);

            System.Threading.Thread.Sleep(100);

            status.SetRotationDate(testLogPath);
            DateTime secondDate = status.GetRotationDate(testLogPath);

            // Assert
            secondDate.Should().BeAfter(firstDate);
        }
    }
}
