﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using CommandLine;
using Kurukuru;
using Ptolemy.Interface;

namespace Ptolemy.Lupus {
    internal class Program {
        private static void Main(string[] args) {
            using (var cts = new CancellationTokenSource()) {
                var token = cts.Token;
                var log = new Logger.Logger();
                Console.CancelKeyPress += (sender, eventArgs) => {
                    eventArgs.Cancel = true;
                    cts.Cancel();
                    Console.WriteLine();
                    log.Warn("Waiting cancel...");
                };
                log.Warn("Press Ctrl+C to cancel");

                var lupus = new Lupus();
                var res = lupus.Invoke(token, args);
                Console.ResetColor();
                if (res == null) return;

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

            var logFile = Path.Combine(LupusConfig.Instance.LogDir, $"{DateTime.Now:yyyy-MM-dd-HH-mm-ss-ff}.log");

            return Parser.Default.ParseArguments<Get, Push>(args).MapResult(
                (Get g) => g.Run(token, logFile),
                (Push p) => p.Run(token, logFile),
                e => null
            );
        }

        public IEnumerable<string> Args { get; set; }
    }
}
