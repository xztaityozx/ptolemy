using System;
using System.Collections.Generic;
using CommandLine;

namespace Ptolemy.Argo {
    public class Options {

        public static Options Parse(IEnumerable<string> args) {
            Options rt = null;
            Parser.Default.ParseArguments<Options>(args).WithParsed(o => rt = o)
                .WithNotParsed(e => throw new ArgoParseFailedException());
            return rt ?? throw new NullReferenceException();
        }
        
        public const string TimeDefault = "0,100p,20n", VtnDefault = "0.6,0.046,1.0", VtpDefault = "-0.6,0.046,1.0";
        private const string NonJson = "nonJson", Json = "Json";
        [Option("time",Default = TimeDefault, SetName = NonJson, HelpText = "シミュレーション時間を指定します。何も書かない場合はデフォルト値が適応されます [start],[step],[stop]")]
        public string TimeString { get; set; }
        
        [Option('b', "base", SetName = NonJson, Default = "./", HelpText = "結果を出力するディレクトリのルートです")]
        public string BaseDir { get; set; }
        
        [Option('r', "root", HelpText = "回路が格納されているディレクトリ、例えば~/simulationなどへのパスです。指定しない場合、環境変数 `ARGO_CIRCUIT_ROOT` が適用されます")]
        public string CircuitRoot { get; set; }
        
        [Option('h', "hspice", SetName = NonJson, HelpText = "hspiceへのパスです。指定しない場合、環境変数 `ARGO_HSPICE` が適用されます")]
        public string Hspice { get; set; }
        
        [Option("options", SetName = NonJson, HelpText = "hspiceへ渡すオプションです")]
        public IEnumerable<string> HspiceOptions { get; set; }
        
        [Option('N', "vtn", Default = VtnDefault, SetName = NonJson,
            HelpText = "vtnの値をカンマ区切りで指定します。何も書かない場合はデフォルト値が適用されます [threshold],[sigma],[deviation]")]
        public string VtnString { get; set; }

        [Option('P', "vtp", Default = VtpDefault, SetName = NonJson,
            HelpText = "vtpの値をカンマ区切りで指定します。何も書かない場合はデフォルト値が適用されます [threshold],[sigma],[deviation]")]
        public string VtpString { get; set; }

        [Option('T',"temperature", Default = 25.0, HelpText = "温度です", SetName = NonJson)]
        public double Temperature { get; set; }
        
        [Option('t', "target", SetName = NonJson,  HelpText = "シミュレーションしたい回路の名前を /username/cell/circuit で指定します")]
        public string TargetCircuit { get; set; }
        
        [Option('w', "sweeps",Default = 5000L, HelpText = "Sweep数です", SetName = NonJson)]
        public long Sweeps { get; set; }
        
        [Option('e', "seed", SetName = NonJson, Default = 1L, HelpText = "Seed値です")]
        public long Seed { get; set; }
        
        [Option('d', "vdd", Default = 0.8, HelpText = "電源電圧(V)です", SetName = NonJson)]
        public double Vdd { get; set; }
        
        [Option('g', "gnd", Default = 0.0, HelpText = "Gndの電圧(V)です", SetName = NonJson)]
        public double Gnd { get; set; }
        
        [Option('c', "icCommand", Default = new[]{"V(N1)=0.8V","V(N2)=0V"}, Separator = ',',SetName = NonJson,HelpText = "SPIスクリプトでの .IC コマンド渡す値をカンマ区切りで与えます")]
        public IEnumerable<string> IcCommands { get; set; }
        
        [Option('m',"model", SetName = NonJson, HelpText = "modelファイルへのパスです")]
        public string ModelFile { get; set; }
        
        [Option("sweepStart", Default = 1L, HelpText = "Sweepの開始値です", SetName = NonJson)]
        public long SweepStart { get; set; }

        [Option("json", SetName = Json, HelpText = "シミュレーションを記述したJSONをもとに実行します")]
        public string JsonFile { get; set; }
    }
}