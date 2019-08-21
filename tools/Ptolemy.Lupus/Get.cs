using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using CommandLine;

namespace Ptolemy.Lupus {
    [Verb("get", HelpText = "get data from database")]
    internal class Get :Verb.Verb {
        protected override void Do(CancellationToken token) {
            
        }
    }
}
