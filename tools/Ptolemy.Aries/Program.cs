using System;
using System.Text.Json;
using System.Threading;
using CommandLine;
using Ptolemy.Interface;
using Ptolemy.OptionException;

namespace Ptolemy.Aries {
    internal class Program {
        private static void Main(string[] args) {
            using var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (sender, eventArgs) => {
                eventArgs.Cancel = true;
                cts.Cancel();
            };

            var token = cts.Token;

            Parser.Default.ParseArguments<AriesMake, AriesRun>(args)
                .MapResult(
                    (AriesMake a) => {
                        a.Run(token);
                        return 1;
                    },
                    (AriesRun a) => {
                        a.Run(token);
                        return 1;
                    },
                    e => throw new ParseFailedException());

        }
    }
}
