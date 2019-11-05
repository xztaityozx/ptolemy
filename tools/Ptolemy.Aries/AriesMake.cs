using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using CommandLine;
using Ptolemy.Argo;
using Ptolemy.Argo.Request;
using Ptolemy.Interface;
using Ptolemy.Parameters;
using Ptolemy.SiMetricPrefix;
using ShellProgressBar;

namespace Ptolemy.Aries {
    [Verb("make", HelpText = "タスクを作ります")]
    public class AriesMake : ITransistorOption,ISignalOption,IAriesVerb {
        public IEnumerable<string> VtnStrings { get; set; }
        public IEnumerable<string> VtpStrings { get; set; }
        public double? Sigma { get; set; }

        [Option("sweepStart", Default = "1", HelpText = "Sweepの開始値です")]
        public string SweepStart { get; set; }

        [Option('w',"sweeps",Default = "2000", HelpText = "合計のSweep数です")]
        public string TotalSweeps { get; set; }

        [Option('e', "seed",Default = "1",HelpText = "Seed値です")]
        public string Seed { get; set; }

        [Option('W',"splitOption", HelpText = "合計Sweepをseedかsweepで分割します.[seed or sweep]:[num]", Default = "none")]
        public string SplitOption { get; set; }

        public IEnumerable<string> Signals { get; set; }

        [Option("temp", Default = "25.0", HelpText = "温度です")]
        public string Temperature { get; set; }

        [Option("time", Default = "0,100p,20n", HelpText = "シミュレーション時間を[start],[step],[stop]で指定します")]
        public string TimeString { get; set; }

        [Option("gnd", Default = "0.0", HelpText = "Gndの電圧です")]
        public string Gnd { get; set; }

        [Option("vdd", Default = "0.8", HelpText = "Vddの電圧です")]
        public string Vdd { get; set; }

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

        [Option('t', "plotTime", HelpText = "Plotする時間をリストもしくは範囲で指定します", Default = "all")]
        public string PlotTimeRequest { get; set; }

        public static IEnumerable<decimal> GeneratePlotTimeEnumerable(string plotTimeString, string timeRange) {
            if(plotTimeString=="all")
                foreach (var x in new RangeParameter(timeRange).ToEnumerable())
                    yield return x;
            else {
                var split = plotTimeString.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var segment in split) {
                    if (segment.Contains(':')) {
                        // 範囲
                        foreach (var x in new RangeParameter(segment.Replace(':', ',')).ToEnumerable()) {
                            yield return x;
                        }
                    }
                    else {
                        // 単品
                        yield return segment.ParseDecimalWithSiPrefix();
                    }
                }
            }
        }

        public void Run(CancellationToken token) {

            var log = new Logger.Logger();

            // TimeList
            var plotTimeList = GeneratePlotTimeEnumerable(PlotTimeRequest, TimeString).ToList();

            // Configのデフォルトも見る
            var transistors = this.Bind(Config.Config.Instance.ArgoDefault.Transistors);

            if (string.IsNullOrEmpty(NetList) && string.IsNullOrEmpty(Config.Config.Instance.ArgoDefault.NetList))
                throw new AriesException("NetListを空にできません.一番目の引数もしくはコンフィグファイルのArgoDefault.NetListで指定してください");

            // 引数で与えたNetListが優先
            NetList = FilePath.FilePath.Expand(
                string.IsNullOrEmpty(NetList)
                    ? Config.Config.Instance.ArgoDefault.NetList
                    : NetList);

            var guid = Guid.NewGuid();

            // タスクファイルを保存してるディレクトリへのパスを展開
            var baseDir = Path.Combine(Config.Config.Instance.WorkingRoot, "aries", "task");
            
            if(!Directory.Exists(baseDir)) Directory.CreateDirectory(baseDir);

            if (!File.Exists(NetList)) throw new AriesException($"Netlistファイルが見つかりません: {NetList}");

            HspicePath = string.IsNullOrEmpty(HspicePath)
                ? Environment.GetEnvironmentVariable(Argo.Argo.EnvArgoHspice)
                : HspicePath;


            if (string.IsNullOrEmpty(HspicePath) || !File.Exists(HspicePath)) {
                throw new ArgoException($"Hspice can not found: {HspicePath}");
            }

            try {
                var totalSweep = TotalSweeps.ParseLongWithSiPrefix();
                var seed = Seed.ParseLongWithSiPrefix();
                var start = SweepStart.ParseLongWithSiPrefix();

                var include = new[] {
                    Includes,
                    Config.Config.Instance.ArgoDefault.Includes,
                    Environment.GetEnvironmentVariable(Argo.Argo.EnvArgoIncludes)
                        ?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                };


                var baseRequest = new ArgoRequest {
                    GroupId = guid, Gnd = Gnd.ParseDecimalWithSiPrefix(),

                    // Includesは引数、コンフィグ、環境変数のうち最初にNullじゃないやつを選ぶ。全部Nullなら空のリスト
                    Includes = include.Any(s=>s.Any()) ? include.First(s=>s.Any()).ToList() : new List<string>(),
                    Seed = seed,
                    // Signalsは引数がNullならコンフィグを選ぶ
                    Signals = (Signals ?? Config.Config.Instance.ArgoDefault.Signals).ToList(),
                    Sweep = totalSweep,
                    Temperature = Temperature.ParseDecimalWithSiPrefix(), Time = new RangeParameter(TimeString),
                    Transistors = transistors,
                    Vdd = Vdd.ParseDecimalWithSiPrefix(),
                    HspiceOptions = Options.ToList(), HspicePath = HspicePath,
                    IcCommands = IcCommands.ToList(), NetList = NetList, SweepStart = start,
                    PlotTimeList = plotTimeList
                };

                var dbName = baseRequest.GetHashString();
                baseRequest.ResultFile = dbName;
                if (SplitOption == "none") {
                    WriteTaskFile(Path.Combine(baseDir, $"{guid}.json"), baseRequest);
                }
                else {
                    var (by, size) = SplitOption.Split(':', StringSplitOptions.RemoveEmptyEntries) switch {
                        var s when s.Length != 2 => throw new AriesException(
                            "SplitOptionに与えた引数がフォーマットに従っていません.[seed, sweep]:[size]"),
                        var s when s[1] == "0" => throw new AriesException("[size]に0を指定できません"),
                        var s when s[0] == "seed" => (SplitBy.Seed, long.Parse(s[1])),
                        var s when s[0] == "sweep" => (SplitBy.Sweep, long.Parse(s[1])),
                        _ => throw new AriesException("SplitOptionの解釈に失敗しました. [seed, sweep]:[size]")
                        };

                    var total = totalSweep / size + (totalSweep % size == 0 ? 0 : 1);
                    using var bar = new ProgressBar((int) total, "Write task files", new ProgressBarOptions {
                        BackgroundCharacter = '-', BackgroundColor = ConsoleColor.DarkGray,
                        ForegroundColor = ConsoleColor.DarkGreen, ProgressCharacter = '>',
                        CollapseWhenFinished = false, ForegroundColorDone = ConsoleColor.Green
                    });

                    for (var i = 0; i < total; i++) {

                        token.ThrowIfCancellationRequested();

                        var rest = totalSweep - i * size;
                        baseRequest.Sweep = rest < size ? rest : size;
                        var path = Path.Combine(baseDir, $"{guid}-{i}.json");
                        if (by == SplitBy.Seed) {
                            baseRequest.Seed = seed++;
                        }
                        else {
                            baseRequest.Seed = seed;
                            baseRequest.SweepStart = start;
                            start += size;
                        }

                        WriteTaskFile(path, baseRequest);
                        bar.Tick($"write to {path}");
                    }

                    log.Info("Ptolemy.Aries make done");

                }
            }
            catch (FormatException e) {
                throw  new AriesException($"数値をパースできませんでした: {e}");
            }
        }

        private enum SplitBy {
            Seed, Sweep
        }
        
        private static void WriteTaskFile(string path, ArgoRequest request) {
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