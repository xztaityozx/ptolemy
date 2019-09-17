using System;
using System.Diagnostics;
using System.Threading;
using CommandLine;

namespace Ptolemy.Argo {
    internal static class Program {
        private static void Main(string[] args) {
            args = "-N 1,2,3".Split(' ', StringSplitOptions.RemoveEmptyEntries);
            Parser.Default.ParseArguments<ArgoOption>(args).WithParsed(Console.WriteLine);
        }
    }
}
