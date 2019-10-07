using System.Collections.Generic;
using System.Text.Json;
using CommandLine;
using Ptolemy.Interface;
using Ptolemy.Parameters;

namespace Ptolemy.Aries {
    [Verb("make", HelpText = "タスクを作ります")]
    public class AriesMake : ITransistorOption,ISignalOption {
        public IEnumerable<string> VtnStrings { get; set; }
        public IEnumerable<string> VtpStrings { get; set; }
        public double? Sigma { get; set; }

        [Option("sweepStart", Default = 1, HelpText = "Sweepの開始値です")]
        public long SweepStart { get; set; }

        [Option('w',"sweeps",Default = 2000, HelpText = "合計のSweep数です")]
        public long TotalSweeps { get; set; }

        [Option('W',"splitBySweep", Default = null, HelpText = "合計Sweep数をこのオプションに与えた値で分割します")]
        public long? SplitBySweep { get; set; }

        [Option('e', "seed",Default = 1,HelpText = "Seed値です")]
        public long Seed { get; set; }

        [Option('E',"splitBySeed", Default = null, HelpText = "合計Sweep数をこのオプションに与えたSeed数で分割します")]
        public long? SplitBySeed { get; set; }

        public IEnumerable<string> Signals { get; set; }

        [Option("temp", Default = 25.0, HelpText = "温度です")]
        public double Temperature { get; set; }

        [Option("time", Default = "0,100p,20n", HelpText = "シミュレーション時間を[start],[step],[stop]で指定します")]
        public string TimeString { get; set; }

        [Option("gnd", Default = 0.0, HelpText = "Gndの電圧です")]
        public double Gnd { get; set; }

        [Option("vdd", Default = 0.8, HelpText = "Vddの電圧です")]
        public double Vdd { get; set; }

        [Option("include", HelpText = "モデルファイルなど、NetListにIncludeするファイルのリストです")]
        public IEnumerable<string> Includes { get; set; }

        [Option("hspice", HelpText = "Hspiceへのパスです(env: " + Argo.Argo.ENV_ARGO_HSPICE + ")")]
        public string HspicePath { get; set; }

        [Option("options", HelpText = "Hspiceに渡したいオプションです")]
        public IEnumerable<string> Options { get; set; }

        [Option("icCommand", Default = new[] {"V(N1)=0.8V", "V(N2)=0V"}, HelpText = ".ICへの引数です")]
        public IEnumerable<string> IcCommands { get; set; }
    }
}