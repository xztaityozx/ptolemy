using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ptolemy.Exec {
    public class Exec : IDisposable {
        private Process process;

        private static string Shell =>
            Environment.OSVersion.ToString().StartsWith("Unix") ? "/bin/sh" : "powershell.exe";

        private readonly CancellationTokenSource cts;

        public Exec(CancellationToken token) =>
            (this.cts) = CancellationTokenSource.CreateLinkedTokenSource(token);

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

        public void Run(string command, Action<string> onStdOut, bool combineOutput = false) => Run(command,
            onStdOut,
            s => { }, combineOutput);
        
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
            // cts.Token.Register(process.Kill);
            
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

        public void ThrowIfNonZeroExitCode() {
            if (process.ExitCode != 0) throw new Exception($"exit status {process.ExitCode}");
        }

        public int ExitCode => process.ExitCode;

        public void Dispose() {
            process?.Dispose();
            cts?.Dispose();
        }
    }

}
