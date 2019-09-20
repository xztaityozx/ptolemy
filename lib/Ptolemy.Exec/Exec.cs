using System;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace Ptolemy.Exec {
    public class Exec : IDisposable {
        private Process process;

        private static string Shell =>
            Environment.OSVersion.ToString().StartsWith("Unix") ? "/bin/sh" : "powershell.exe";

        private readonly CancellationTokenSource cts;

        public Exec(CancellationToken token) {
            cts = CancellationTokenSource.CreateLinkedTokenSource(token);
            StdOut=new Subject<string>();
            StdError=new Subject<string>();
        }

        public readonly Subject<string> StdOut, StdError;

        /// <summary>
        /// Run command
        /// </summary>
        /// <param name="command"></param>
        /// <param name="arguments"></param>
        /// <exception cref="Exception"></exception>
        public void Run(string command, string[] arguments = null) {
            process = new Process {
                StartInfo = new ProcessStartInfo {
                    FileName = command,
                    Arguments = string.Join(" ", arguments??new[]{""}),
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardInput = false, 
                    RedirectStandardOutput = true,
                },
            };

            try {
                process.Start();
            }
            catch (Exception) {
                throw new InvalidOperationException("Failed start command");
            }

            cts.Token.Register(process.Kill);

            Task.WaitAll(
                Task.Factory.StartNew(() =>
                process.WaitForExit(), cts.Token),
                Task.Factory.StartNew(() => {
                    string l;
                    while ((l = process.StandardOutput.ReadLine()) != null && !cts.IsCancellationRequested) StdOut.OnNext(l);
                }),
                Task.Factory.StartNew(() => {
                    string l;
                    while ((l = process.StandardError.ReadLine()) != null && !cts.IsCancellationRequested)
                        StdError.OnNext(l);
                })
            );
        }


        /// <summary>
        /// Get exit code this execution
        /// </summary>
        public int ExitCode => process.ExitCode;

        public void Dispose() {
            process?.Dispose();
            StdError?.Dispose();
            cts?.Dispose();
            StdOut?.Dispose();
        }
    }
}