using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.Text;

namespace ManagementPortal.Services
{
    public interface IPlatform
    {
        (int ExitCode, string output, string error) Execute(bool waitForExit, string executable, string arguments = null, string workingDirectory = null, Dictionary<string, string> environment = null);
        int ShellExecute(bool waitForExit, string executable, string arguments = null, string workingDirectory = null);
        int KillProcess(string processName, string commandLine);
    }

    public class WindowsPlatform : IPlatform
    {
        public (int ExitCode, string output, string error) Execute(bool waitForExit, string executable, string arguments = null, string workingDirectory = null, Dictionary<string, string> environment = null)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = executable,
                    Arguments = arguments,
                    WorkingDirectory = workingDirectory,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                },
                EnableRaisingEvents = true
            };

            if (environment != null)
            {
                foreach(var env in environment)
                {
                    process.StartInfo.Environment.Add(env);
                }
            }

            if (waitForExit)
            {
                var output = new StringBuilder();
                var error = new StringBuilder();
                process.OutputDataReceived += (_, outLine) =>
                {
                    if (string.IsNullOrEmpty(outLine.Data))
                    {
                        return;
                    }
                    output.AppendLine(outLine.Data);
                };
                process.ErrorDataReceived += (_, outLine) =>
                {
                    if (string.IsNullOrEmpty(outLine.Data))
                    {
                        return;
                    }
                    error.AppendLine(outLine.Data);
                };

                var exitCode = 0;
                process.Exited += (_, __) => exitCode = process.ExitCode;

                process.Start();

                process.BeginErrorReadLine();
                process.BeginOutputReadLine();

                process.WaitForExit(15000);
                return (exitCode, output.ToString(), error.ToString());
            }

            process.Start();
            return (0, string.Empty, string.Empty);
        }

        public int ShellExecute(bool waitForExit, string executable, string arguments = null, string workingDirectory = null)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = executable,
                    Arguments = arguments,
                    WorkingDirectory = workingDirectory,
                    UseShellExecute = true
                },
                EnableRaisingEvents = true
            };

            if (waitForExit)
            {
                var exitCode = 0;
                process.Exited += (_, __) => exitCode = process.ExitCode;

                process.Start();

                process.WaitForExit(15000);
                return exitCode;
            }

            process.Start();
            return 0;
        }

        public int KillProcess(string processName, string commandLine)
        {
            int killedCount = 0;
            foreach (var process in Process.GetProcessesByName(processName))
            {
                using (var searcher = new ManagementObjectSearcher("SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + process.Id))
                {
                    foreach (var o in searcher.Get())
                    {
                        if (commandLine != null)
                        {
                            if (commandLine != o["CommandLine"].ToString())
                            {
                                continue;
                            }
                        }
                        process.Kill();
                        killedCount++;
                    }
                }
            }
            return killedCount;
        }
    }
}
