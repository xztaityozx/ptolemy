using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace Ptolemy.Exec {
    public class Exec : IDisposable {
        private Process process;

        //private readonly CancellationTokenSource cts;
        private readonly CancellationToken token;

        public Exec(CancellationToken token) {
            this.token = token;
            //cts = CancellationTokenSource.CreateLinkedTokenSource(token);
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
                EnableRaisingEvents = true
            };

            try {
                process.Start();
            }
            catch (Exception) {
                throw new InvalidOperationException("Failed start command");
            }


            token.ThrowIfCancellationRequested();
            token.Register(() => {
                process.Kill();
            });

            // TODO: Issue #17(https://github.com/xztaityozx/ptolemy/issues/17)
            // キャンセルできない。どうにかしろ
            token.ThrowIfCancellationRequested();

            var stdoutReader = Task.Factory.StartNew(() => {
                string l;
                while (!token.IsCancellationRequested && (l = process.StandardOutput.ReadLine()) != null) {
                    StdOut.OnNext(l);
                }
            }, token);

            var stderrReader = Task.Factory.StartNew(() => {
                string l;
                while (!token.IsCancellationRequested && (l = process.StandardError.ReadLine()) != null) {
                    StdError.OnNext(l);
                }
            }, token);

            try {
                Task.WaitAll(new[] {
                    stderrReader, stdoutReader,
                    Task.Factory.StartNew(() => { process.WaitForExit(); }, token)
                }, token);
            }
            catch (OperationCanceledException) {
                throw;
            }
            catch (AggregateException e) {
                foreach (var exception in e.InnerExceptions) {
                    if (exception.GetType() == typeof(OperationCanceledException)) throw exception;
                }
            }
        }


        /// <summary>
        /// Get exit code this execution
        /// </summary>
        public int ExitCode => token.IsCancellationRequested ? 1 : process.ExitCode;

        public void Dispose() {
            process?.Dispose();
            StdError?.Dispose();
            StdOut?.Dispose();
        }
    }
}