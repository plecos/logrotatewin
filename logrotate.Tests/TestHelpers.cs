using System;
using System.IO;
using System.Text;

namespace logrotate.Tests
{
    public static class TestHelpers
    {
        private static readonly Random _random = new Random();

        /// <summary>
        /// Creates a temporary log file with specified size
        /// </summary>
        public static string CreateTempLogFile(long sizeInBytes, string extension = ".log")
        {
            string tempPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}{extension}");

            using (FileStream fs = new FileStream(tempPath, FileMode.Create, FileAccess.Write))
            {
                // Write test data
                byte[] buffer = new byte[Math.Min(sizeInBytes, 8192)];
                long remaining = sizeInBytes;

                while (remaining > 0)
                {
                    int toWrite = (int)Math.Min(remaining, buffer.Length);
                    // Fill with somewhat realistic log data
                    string logLine = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [INFO] Test log entry {_random.Next()}\n";
                    byte[] lineBytes = Encoding.UTF8.GetBytes(logLine);

                    for (int i = 0; i < toWrite && i < lineBytes.Length; i++)
                    {
                        buffer[i] = lineBytes[i % lineBytes.Length];
                    }

                    fs.Write(buffer, 0, toWrite);
                    remaining -= toWrite;
                }
            }

            return tempPath;
        }

        /// <summary>
        /// Creates a temporary configuration file
        /// </summary>
        public static string CreateTempConfigFile(string content)
        {
            string tempPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.conf");
            File.WriteAllText(tempPath, content);
            return tempPath;
        }

        /// <summary>
        /// Creates a temporary directory
        /// </summary>
        public static string CreateTempDirectory()
        {
            string tempPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}");
            Directory.CreateDirectory(tempPath);
            return tempPath;
        }

        /// <summary>
        /// Cleans up a file or directory
        /// </summary>
        public static void CleanupPath(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                else if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
            }
            catch
            {
                // Ignore cleanup errors in tests
            }
        }

        /// <summary>
        /// Asserts that a file was rotated
        /// </summary>
        public static void AssertFileWasRotated(string originalPath, string expectedRotatedPath)
        {
            if (!File.Exists(expectedRotatedPath))
            {
                throw new Exception($"Expected rotated file not found: {expectedRotatedPath}");
            }
        }

        /// <summary>
        /// Asserts that a file is gzip compressed
        /// </summary>
        public static bool IsFileCompressed(string filePath)
        {
            if (!File.Exists(filePath))
                return false;

            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                if (fs.Length < 2)
                    return false;

                byte[] header = new byte[2];
                fs.Read(header, 0, 2);

                // Gzip magic number: 0x1f 0x8b
                return header[0] == 0x1f && header[1] == 0x8b;
            }
        }

        /// <summary>
        /// Gets the size of a file in bytes
        /// </summary>
        public static long GetFileSize(string filePath)
        {
            if (!File.Exists(filePath))
                return 0;

            return new FileInfo(filePath).Length;
        }

        /// <summary>
        /// Creates a test state file
        /// </summary>
        public static string CreateTempStateFile()
        {
            string tempPath = Path.Combine(Path.GetTempPath(), $"test_state_{Guid.NewGuid()}.txt");
            return tempPath;
        }

        /// <summary>
        /// Counts files matching a pattern in a directory
        /// </summary>
        public static int CountFiles(string directory, string pattern)
        {
            if (!Directory.Exists(directory))
                return 0;

            return Directory.GetFiles(directory, pattern).Length;
        }
    }
}
