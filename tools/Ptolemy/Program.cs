using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using CommandLine;
using Kurukuru;
using Ptolemy.Interface;

namespace Ptolemy {
    public class Program {
        internal static void Main(string[] args) {
            Spinner.Start("Wait", () => {
                for (var i = 0; i < 10; i++) {
                    Thread.Sleep(100);
                }
            });
        }
    }

}
