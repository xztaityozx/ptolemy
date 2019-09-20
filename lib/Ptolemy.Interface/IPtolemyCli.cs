using System;
using System.Collections.Generic;
using System.Threading;
using CommandLine;

namespace Ptolemy.Interface {
    public interface IPtolemyCli {
        Exception Run(CancellationToken token, string[] args);
        [Value(0)]
        IEnumerable<string> Args { get; set; }
    }
}
