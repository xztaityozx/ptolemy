using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using CommandLine;
using CommandLine.Text;
using Ptolemy.Argo.Request;
using Ptolemy.Parameters;

namespace Ptolemy.Argo {
    internal static class Program {
        private static void Main(string[] args) {
            
            // TODO: ここやろうね
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(o => {
                    ArgoRequest request = null;
                    if (string.IsNullOrEmpty(o.JsonFile)) {
                        if (!File.Exists(o.JsonFile))
                            throw new FileNotFoundException("json file not found", o.JsonFile);
                        using (var sr = new StreamReader(o.JsonFile)) request = ArgoRequest.FromJson(sr.ReadToEnd());
                    }
                    else {
                        request = new ArgoRequest {
                            TargetCircuit = o.TargetCircuit,
                            Sweep = o.Sweeps,
                            Gnd = (decimal)o.Gnd,
                            Seed = o.Seed,
                            Temperature = (decimal)o.Temperature,
                            Vdd = (decimal)o.Vdd,
                            
                        };
                    }
                });
        }
    }

    public class Options {
        private const string NonJson = "nonJson", Json = "Json";
        [Option('N', "vtn", Default = "0.6,0.046,1.0", SetName = NonJson,
            HelpText = "vtnの値をカンマ区切りで指定します。何も書かない場合はデフォルト値が適用されます [threshold],[sigma],[deviation]")]
        public string VtnString { get; set; }

        [Option('P', "vtp", Default = "-0.6,0.046,1.0", SetName = NonJson,
            HelpText = "vtpの値をカンマ区切りで指定します。何も書かない場合はデフォルト値が適用されます [threshold],[sigma],[deviation]")]
        public string VtpString { get; set; }

        [Option('T',"temperature", Default = 25.0, HelpText = "温度です", SetName = NonJson)]
        public double Temperature { get; set; }
        
        [Option('t', "target", SetName = NonJson, Required = true, HelpText = "シミュレーションしたい回路の名前を /username/cell/circuit で指定します")]
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
        
        [Option('m',"model",Required = true, SetName = NonJson, HelpText = "modelファイルへのパスです")]
        public string ModelFile { get; set; }
        
        [Option("sweepStart", Default = 1L, HelpText = "Sweepの開始値です", SetName = NonJson)]
        public long SweepStart { get; set; }
        
        [Option("generateJson", SetName = NonJson, Default = false, HelpText = "指定されたパラメータをもとにJSONを作成し、stdoutに出力します")]
        public bool GenerateJson { get; set; }
        
        [Option("json", SetName = Json)]
        public string JsonFile { get; set; }
    }
}
