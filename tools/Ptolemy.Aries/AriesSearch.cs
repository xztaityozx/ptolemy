using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using CommandLine;
using Ptolemy.Argo.Request;
using Ptolemy.Interface;
using Ptolemy.Parameters;

namespace Ptolemy.Aries {
    [Verb("search", HelpText = "パラメータ情報からDBを検索します")]
    public class AriesSearch:ITransistorOption,ISignalOption,IAriesVerb {
        
        public IEnumerable<string> VtnStrings { get; set; }
        public IEnumerable<string> VtpStrings { get; set; }
        public double? Sigma { get; set; }

        [Option("sweepStart", Default = 1, HelpText = "Sweepの開始値です")]
        public long SweepStart { get; set; }

        [Option('w',"sweeps",Default = 2000, HelpText = "合計のSweep数です")]
        public long TotalSweeps { get; set; }

        [Option('e', "seed",Default = 1,HelpText = "Seed値です")]
        public long Seed { get; set; }

        [Option('R',"dbRoot", HelpText = "DBファイルの格納されるディレクトリルートへのパスです", Default = "~/.config/ptolemy/aries/dbRoot")]
        public string DbRoot { get; set; }
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

        [Option("hspice", HelpText = "Hspiceへのパスです(env: " + Argo.Argo.EnvArgoHspice + ")")]
        public string HspicePath { get; set; }

        [Option("options", HelpText = "Hspiceに渡したいオプションです")]
        public IEnumerable<string> Options { get; set; }

        [Option("icCommand", Default = new[] {"V(N1)=0.8V", "V(N2)=0V"}, HelpText = ".ICへの引数です")]
        public IEnumerable<string> IcCommands { get; set; }

        [Value(0, HelpText = "NetListへのパスです", MetaName = "netlist")]
        public string NetList { get; set; }

        public void Run(CancellationToken token) {
            var transistors = this.Bind(null);
            var hash = new ArgoRequest {
                Gnd = (decimal) Gnd, Includes = Includes.ToList(), Seed = Seed, Signals = Signals.ToList(),
                Sweep = TotalSweeps, Temperature = (decimal) Temperature, Time = new RangeParameter(TimeString),
                Transistors = transistors, Vdd = (decimal) Vdd, HspiceOptions = Options.ToList(),
                HspicePath = HspicePath,
                IcCommands = IcCommands.ToList(), NetList = NetList, SweepStart = SweepStart
            }.GetHashString();

            var path = Path.Combine(FilePath.FilePath.Expand(DbRoot), hash + ".sqlite");
            if(File.Exists(path))
                Console.WriteLine(Path.Combine(FilePath.FilePath.Expand(DbRoot), hash + ".sqlite"));
            else 
                Console.Error.WriteLine("DBが見つかりませんでした");
        }
    }
}