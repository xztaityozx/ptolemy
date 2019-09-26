using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using CommandLine;
using Ptolemy.Interface;

namespace Ptolemy {
    public class Program {
        internal static void Main(string[] args) {
            using var cts = new CancellationTokenSource(1000);
            using var exec = new Exec.Exec(cts.Token);
            exec.StdOut.Subscribe(Console.WriteLine);
            try {
                exec.Run("seq", new[] {"100000"});
            }
            catch (Exception) {
                Console.WriteLine("error");
            }
        }
    }

}
