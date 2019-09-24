using System.Collections.Generic;
using System.IO;
using CommandLine;
using Ptolemy.Draco.Request;
using Ptolemy.Interface;

namespace Ptolemy.Draco {
    public class DracoOption :ITransistorOption {
        public IEnumerable<string> VtnStrings { get; set; }
        public IEnumerable<string> VtpStrings { get; set; }
        public double? Sigma { get; set; }
        
        [Option("netlist", HelpText = "このファイルのNetListです", Required = true)]
        public string NetList { get; set; }
        
        [Option('w', "sweep", HelpText = "このファイルのSweep値です", Default = 1)]
        public long Sweep { get; set; }
        [Option('e', "seed", HelpText = "このファイルのSeed値です", Default = 1)]
        public long Seed { get; set; }
        [Option('b', "bufferSize", HelpText = "一度にBulkUpsertする個数です", Default = 10000)]
        public int BufferSize { get; set; }

        [Value(0, Required = true, MetaName = "input")] 
        public string InputFile { get; set; }
        [Value(1, Required = true, MetaName = "outDir")]
        public string OutputDirectory { get; set; }

        public DracoRequest Build() {
            return new DracoRequest {
                Seed = Seed,
                Sweep = Sweep,
                InputFile = InputFile,
                BufferSize = BufferSize,
                SqLiteFile = Path.Combine(FilePath.FilePath.Expand(OutputDirectory), $"vtn_{string.Join(",", VtnStrings)}_vtp_{string.Join(",", VtpStrings)}_{NetList}")
            };
        }
    }
}