using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CommandLine;
using Ptolemy.Draco.Request;
using Ptolemy.Repository;

namespace Ptolemy.Draco {
    internal static class Program {
        private static void Main(string[] args) {
            var request = Parser.Default.ParseArguments<DracoOption>(args)
                .MapResult(
                    o => o.Build(), e => throw new DracoParseFailedException());
            using var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (sender, eventArgs) => {
                eventArgs.Cancel = true;
                cts.Cancel();
            };
            using var draco = new Draco(cts.Token, request);

            draco.Run();

        }
    }
}