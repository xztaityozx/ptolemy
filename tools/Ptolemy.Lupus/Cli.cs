using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLine;
using Ptolemy.Lupus.Request;
using Ptolemy.Parameters;

namespace Ptolemy.Lupus {
    public class Options {
        [Option('s',"signals", HelpText = "取り出したい信号のリストです。カンマ区切りです。", Separator = ',')]
        public List<string> Signals { get; set; }
        [Option('p',"plotPoint", Default = "2.5n,7.5n,17.5n", HelpText = "プロットの時間を指定します。[start],[step],[stop]")]
        public string PlotPointString { get; set; }
        [Value(0, HelpText = "取り出したい波形ファイルが置かれているディレクトリへのパスです")]
        public string TargetDirectory { get; set; }
        [Value(1, HelpText = "出力するファイルへのパスです")]
        public string ResultFileName { get; set; }
        [Option('w', "wv", HelpText = "WaveViewへのパスです。指定しない場合環境変数 `LUPUS_WAVEVIEW` が使われます")]
        public string WaveView { get; set; }
        [Option("wvOptions", HelpText = "WaveViewへ渡すオプションです。カンマ区切りです")]
        public List<string> WaveViewOptions { get; set; }

        public LupusRequest BuildLupusResult() {
            var rt = new LupusRequest();
            try {
                rt.TargetDirectory = TargetDirectory;
                rt.ResultFileName = ResultFileName;
                rt.Signals = Signals;
                rt.WaveViewPath = WaveView ?? Environment.GetEnvironmentVariable("LUPUS_WAVEVIEW");
                rt.WaveViewOptions = WaveViewOptions ?? new List<string>();

                var box = PlotPointString.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(SiMetricPrefix.SiMetricPrefix.ParseDecimalWithSiPrefix).ToArray();

                if(box.Length!=3) throw new LupusException("Invalid value PlotPoint value. [start],[step],[stop]");

                rt.PlotPoint = new Range(box[0], box[1], box[2]);

            }
            catch (Exception e) {
                throw new LupusException($"Failed build request\n\t-->{e}");
            }

            return rt;
        }
    }
}
