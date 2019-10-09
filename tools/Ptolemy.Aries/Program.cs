using System;
using System.Text.Json;
using System.Threading;
using CommandLine;
using Ptolemy.Interface;

namespace Ptolemy.Aries {
    internal class Program {
        private static void Main(string[] args) {
            var res = Parser.Default.ParseArguments<AriesMake>(args)
                .MapResult(a => {
                    a.Run(CancellationToken.None);
                    return null;
                }, e => "error");

            Console.WriteLine(res);
        }
    }
}
