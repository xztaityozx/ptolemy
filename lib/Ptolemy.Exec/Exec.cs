using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Ptolemy.Exec {
    public class Exec : IDisposable {
        private Process process;

        private static string Shell =>
            Environment.OSVersion.ToString().StartsWith("Unix") ? "/bin/sh" : "powershell.exe";

        private readonly CancellationToken token;
        private readonly bool combineOutput;

        public Exec(CancellationToken token, bool combineOutput = false) =>
            (this.token, this.combineOutput) = (token, combineOutput);

        private void BuildCommand(string command) {
            process = new Process {
                StartInfo = new ProcessStartInfo {
                    FileName = Shell,
                    Arguments = $"-c \"{command}\"",
                    RedirectStandardInput = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                }
            };
            if (!process.Start()) throw new Exception($"failed start command: {Shell} -c {command}");
            token.Register(process.Kill);
            process.OutputDataReceived += (sender, args) => Append(args.Data, true);
            process.ErrorDataReceived += (sender, args) => Append(args.Data, false);

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            process.Exited += (sender, args) => {
                stdoutBuffer.CompleteAdding();
                stderrBuffer.CompleteAdding();
            };
        }

        public async Task RunAsync(string command) {
            BuildCommand(command);

            await Task.Factory.StartNew(() => process.WaitForExit(), token);
        }

        public void Start(string command) {
            BuildCommand(command);
        }

        private readonly BlockingCollection<string> stdoutBuffer = new BlockingCollection<string>();
        private readonly BlockingCollection<string> stderrBuffer = new BlockingCollection<string>();


        private void Append(string output, bool isStdout) {
            try {
                if (isStdout || combineOutput) stdoutBuffer.Add(output, token);
                else stderrBuffer.Add(output, token);
            }
            catch (Exception) {
                stdoutBuffer.CompleteAdding();
                stderrBuffer.CompleteAdding();
            }
        }

        public IEnumerable<string> StdOutPipe {
            get {
                while (!stdoutBuffer.IsAddingCompleted && !token.IsCancellationRequested && !process.HasExited) {
                    if (stdoutBuffer.TryTake(out var s)) yield return s;
                }
            }
        }

        public IEnumerable<string> StdErrPipe {
            get {
                while (!stderrBuffer.IsAddingCompleted && !token.IsCancellationRequested && !process.HasExited) {
                    if (stderrBuffer.TryTake(out var s)) yield return s;
                }
            }
        }
        public void Wait() => process.WaitForExit();

        public bool Wait(int millisecond) {
            for (var i = 0; i < millisecond && !token.IsCancellationRequested; i++) {
                Thread.Sleep(1);
            }

            if (process.HasExited) return true;

            process.Kill();
            return process.ExitCode == 0;
        }

        public void Run(string command) {
            Start(command);
            Wait();
        }

        public int ExitCode => process.ExitCode;

        public void Dispose() {
            process?.Dispose();
            stdoutBuffer?.Dispose();
            stderrBuffer?.Dispose();
        }
    }

}
