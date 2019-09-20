using System.Collections.Generic;
using CommandLine;

namespace Ptolemy.Interface {
    public interface ISignalOption {
        [Option("signals", HelpText = "信号のリストです", Separator = ',')]
        IEnumerable<string> Signals { get; set; }
    }
}