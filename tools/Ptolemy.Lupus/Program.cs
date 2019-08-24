using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using CommandLine;
using Ptolemy.Interface;

namespace Ptolemy.Lupus {
    internal class Program {
        private static void Main(string[] args) {
            using (var cts = new CancellationTokenSource()) {
                var token = cts.Token;
                Console.CancelKeyPress += (sender, eventArgs) => {
                    eventArgs.Cancel = true;
                    cts.Cancel();
                };
                var lupus = new Lupus();
                var res = lupus.Invoke(token, args);
                Console.ResetColor();
                if (res == null) return;

                var log = new Logger.Logger();
                switch (res) {
                    case OperationCanceledException _:
                        log.Error("Canceled by user");
                        break;
                    case AggregateException ae:
                        log.Fatal($"Failed lupus command\n\t-->${ae}");
                        break;
                    default:
                        log.Fatal(res);
                        break;
                }
                Console.ResetColor();
                Environment.Exit(1);
            }
        }
    }

    public class Lupus : IPtolemyTool {
        public Exception Invoke(CancellationToken token, string[] args) {

            var logFile = Path.Combine(LupusConfig.Instance.LogDir, $"{DateTime.Now:yyyy-MM-DD-HH-mm-ss-ff}.log");

            return Parser.Default.ParseArguments<Get, Push>(args).MapResult(
                (Get g) => g.Run(token, logFile),
                (Push p) => p.Run(token, logFile),
                e => null
            );
        }

        public IEnumerable<string> Args { get; set; }
    }
}
