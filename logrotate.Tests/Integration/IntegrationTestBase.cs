using System;
using System.Diagnostics;
using System.IO;

namespace logrotate.Tests.Integration
{
    public abstract class IntegrationTestBase : IDisposable
    {
        protected readonly string TestDir;
        private readonly string _exePath;

        protected IntegrationTestBase()
        {
            TestDir = TestHelpers.CreateTempDirectory();
            _exePath = GetLogRotateExePath();
        }

        public virtual void Dispose()
        {
            TestHelpers.CleanupPath(TestDir);
        }

        protected int RunLogRotate(params string[] args)
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = _exePath;
            psi.Arguments = string.Join(" ", args) + " --verbose";
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.CreateNoWindow = true;


            using (Process process = new Process())
            {
                process.StartInfo = psi;

                process.EnableRaisingEvents = true;

                process.OutputDataReceived += (sender, e) =>
                {
                    if (e.Data != null)
                    {
                        // Write the line to your debug output immediately
                        System.Diagnostics.Debug.WriteLine($"[OUTPUT]: {e.Data}");
                    }
                };

                process.ErrorDataReceived += (sender, e) =>
                {
                    if (e.Data != null)
                    {
                        // Write the line to your debug output immediately
                        System.Diagnostics.Debug.WriteLine($"[ERROR]: {e.Data}");
                    }
                };

                process.Start();

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                process.WaitForExit();

                process.CancelOutputRead();
                process.CancelErrorRead();

                return process.ExitCode;
            }
        }

        private string GetLogRotateExePath()
        {
            // Use CodeBase instead of Location to get the actual file path (not shadow copy)
            string testAssemblyCodeBase = this.GetType().Assembly.CodeBase;
            Uri uri = new Uri(testAssemblyCodeBase);
            string testAssemblyPath = Uri.UnescapeDataString(uri.AbsolutePath);
            string testBinDir = Path.GetDirectoryName(testAssemblyPath);

            // Navigate to solution root and find the exe
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

            return exePath;
        }
    }
}
