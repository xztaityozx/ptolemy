using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Ptolemy.Exec {
    public class Exec : IDisposable {
        private Process process;

        private static string Shell =>
            Environment.OSVersion.ToString().StartsWith("Unix") ? "/bin/sh" : "powershell.exe";

        private readonly CancellationTokenSource cts;

        public Exec(CancellationToken token) =>
            cts = CancellationTokenSource.CreateLinkedTokenSource(token);

        /// <summary>
        /// Run command
        /// </summary>
        /// <param name="command"></param>
        /// <exception cref="Exception"></exception>
        public void Run(string command) {
            process = new Process {
                StartInfo = new ProcessStartInfo {
                    FileName = Shell,
                    Arguments = $"-c \"{command}\"",
                    UseShellExecute = false,
                },
            };
            
            if (!process.Start()) throw new Exception($"failed start command: {Shell} -c {command}");
            cts.Token.Register(process.Kill);

            process.WaitForExit();
        }

        /// <summary>
        /// Run command
        /// </summary>
        /// <param name="command">command string</param>
        /// <param name="onStdOut">invoke on data received from stdout</param>
        /// <param name="combineOutput">combine stdout and stderr</param>
        public void Run(string command, Action<string> onStdOut, bool combineOutput = false) => Run(command,
            onStdOut,
            s => { }, combineOutput);
       
        /// <summary>
        /// Run command
        /// </summary>
        /// <param name="command">command string</param>
        /// <param name="onStdOut">invoke on data received from stdout</param>
        /// <param name="onStdErr">invoke on data received from stderr</param>
        /// <param name="combineOutput">combine stderr and stdout</param>
        /// <exception cref="Exception"></exception>
        public void Run(string command, Action<string> onStdOut, Action<string> onStdErr, bool combineOutput) {
            process = new Process {
                StartInfo = new ProcessStartInfo {
                    FileName = Shell,
                    Arguments = $"-c \"{command}\"",
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                },
                EnableRaisingEvents = true,
            };

            process.Exited += (sender, args) => cts.Cancel();
            if (!process.Start()) throw new Exception($"failed start command: {Shell} -c {command}");
            
            var stderr = combineOutput ? onStdOut : onStdErr;
            
            Task.WaitAll(
                Task.Factory.StartNew(() => {
                    string line;
                    while ((line = process.StandardOutput.ReadLine()) != null) onStdOut(line);
                    
                }, cts.Token),
                Task.Factory.StartNew(() => {
                    string line;
                    while ((line = process.StandardError.ReadLine()) != null) stderr(line);
                }, cts.Token),
                Task.Factory.StartNew(() => {
                    cts.Token.WaitHandle.WaitOne();
                    process.WaitForExit();
                })
            );
        }

        /// <summary>
        /// Throw exception if exit code is non zero
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void ThrowIfNonZeroExitCode() {
            if (process.ExitCode != 0) throw new Exception($"exit status {process.ExitCode}\n\tcommand-->{Shell} {process.StartInfo.Arguments}");
        }

        /// <summary>
        /// Get exit code this execution
        /// </summary>
        public int ExitCode => process.ExitCode;

        public void Dispose() {
            process?.Dispose();
            cts?.Dispose();
        }
    }
}
