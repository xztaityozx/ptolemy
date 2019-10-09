using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CommandLine;
using Ptolemy.Argo.Request;
using Ptolemy.Interface;
using Ptolemy.Parameters;

namespace Ptolemy.Argo {
    public class ArgoOption : ITransistorOption, ISignalOption {
        [Option("clean", Default = false, HelpText = "Ptolemy.Argoの作業用ディレクトリ削除して終了します")]
        public bool Clean { get; set; }
        
        [Option('t',"target", 
            HelpText = "シミュレーションしたい回路のNetListファイルへのパスを指定します")]
        public string Target { get; set; }

        [Option('o',"out", Default  = "./argoResult", HelpText = "出力先のファイル名です" )]
        public string ResultFile { get; set; }
        public IEnumerable<string> VtnStrings { get; set; }
        
        public IEnumerable<string> VtpStrings { get; set; }
        public double? Sigma { get; set; }
        
        public override string ToString() {
            return $"Vtn:{string.Join(",", VtnStrings)}, Vtp:{string.Join(",",VtpStrings)}, Sigma?:{Sigma}";
        }

        [Option('w', "sweep", Default = 5000L, HelpText = "Sweep数です")]
        public long Sweep { get; set; }
        
        [Option("firstrun", Default = 1L, HelpText = "Sweepの開始値です")]
        public long SweepStart { get; set; }
        
        [Option('e', "seed", Default = 1L, HelpText = "Seed値です")]
        public long Seed { get; set; }

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
        
        [Option("hspice", HelpText = "Hspiceへのパスです(env: "+Argo.EnvArgoHspice+")")]
        public string HspicePath { get; set; }
        
        [Option("options", HelpText = "Hspiceに渡したいオプションです")]
        public IEnumerable<string> Options { get; set; }
        
        [Option("icCommand", Default = new[]{"V(N1)=0.8V", "V(N2)=0V"}, HelpText = ".ICへの引数です")]
        public IEnumerable<string> IcCommands { get; set; }

        

        public ArgoRequest BuildRequest() {
            var hspice = string.IsNullOrEmpty(HspicePath)
                ? Environment.GetEnvironmentVariable(Argo.EnvArgoHspice)
                : HspicePath;
            
            if(string.IsNullOrEmpty(hspice)) throw new ArgoException("HspicePath must be set");
            if(!File.Exists(hspice)) throw new ArgoException($"cannot find {hspice}");
            if (string.IsNullOrEmpty(Target)) throw new ArgoException($"Target netlist not set");

            return new ArgoRequest {
                GroupId = Guid.Empty,
                HspicePath = FilePath.FilePath.Expand(hspice),
                HspiceOptions = Options.ToList(),
                Seed = Seed,
                Sweep = Sweep,
                SweepStart = SweepStart,
                Temperature = (decimal)Temperature,
                Transistors = this.Bind(null),
                Time = new RangeParameter(TimeString, (0,100E-12M,20E-9M)),
                NetList =  FilePath.FilePath.Expand(Target),
                Includes = Includes.ToList(),
                Vdd = (decimal)Vdd,
                Gnd = (decimal)Gnd,
                Signals = Signals.ToList(),
                ResultFile = FilePath.FilePath.Expand(ResultFile),
                IcCommands = IcCommands.ToList()
            };
        }
    }
}
