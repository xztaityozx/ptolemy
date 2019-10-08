using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.Json;
using System.Threading;
using CommandLine;
using Ptolemy.Argo.Request;
using Ptolemy.Interface;
using Ptolemy.Parameters;
using ShellProgressBar;

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

        private const string sbw = "SplitBySweep", sbe = "SplitBySeed";
        [Option('W',"splitBySweep", Default = null, SetName = sbw, HelpText = "合計Sweep数をこのオプションに与えた値で分割します")]
        public long? SplitBySweep { get; set; }

        [Option('e', "seed",Default = 1,HelpText = "Seed値です")]
        public long Seed { get; set; }

        [Option('E',"splitBySeed", Default = null, SetName = sbe, HelpText = "合計Sweep数をこのオプションに与えたSeed数で分割します")]
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

        [Value(0, HelpText = "NetListへのパスです", MetaName = "netlist")]
        public string NetList { get; set; }

        public void Run(CancellationToken token) {
            var transistors = this.Bind(Ptolemy.Config.Config.Instance.ArgoDefault.Transistors);
            var guid = Guid.NewGuid();
            var baseDir = Path.Combine(FilePath.FilePath.DotConfig, "aries", "task");
            
            if(!Directory.Exists(baseDir)) Directory.CreateDirectory(baseDir);

            if (!File.Exists(NetList)) throw new AriesException($"Netlistファイルが見つかりません: {NetList}");

            var baseRequest = new ArgoRequest {
                GroupId = guid, Gnd = (decimal) Gnd, Includes = Includes.ToList(), Seed = Seed, Signals = Signals.ToList(),
                Sweep = TotalSweeps, Temperature = (decimal) Temperature, Time = new RangeParameter(TimeString),
                Transistors = transistors, Vdd = (decimal) Vdd, HspiceOptions = Options.ToList(), HspicePath = HspicePath,
                IcCommands = IcCommands.ToList(), NetList = NetList, SweepStart = SweepStart 
            };

            if (SplitBySeed != null) {
                var range = (long)SplitBySeed;
                var sweep = TotalSweeps / range;
                var rest = TotalSweeps - sweep * range;
                using var bar = new ProgressBar((int)range + (rest > 0 ? 1 : 0), "Write task file", ConsoleColor.Green);
                for (var seed = Seed; seed <= Seed + range; seed++) {
                    var path = Path.Combine(baseDir, $"{guid}-{seed}.json");
                    // TODO: ここやれ
                }
            }

        }
        
        private void WriteTaskFile(string path, ArgoRequest request) {
            using var sw = new StreamWriter(path);
            sw.WriteLine(request.ToJson());
            sw.Flush();
        }
    }

    public class AriesException : Exception {
        public AriesException(){}
        public AriesException(string msg):base(msg){}
    }
}