using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Ptolemy.Parameters;

namespace Ptolemy.Lupus.Record {
    public static class Factory {
        public static Record[] Build(string path) {
            var seed = int.Parse(path.Split(Path.DirectorySeparatorChar).Last());

            string doc;
            using (var sr = new StreamReader(path)) doc = sr.ReadToEnd();

            var container = doc.Split("#", StringSplitOptions.RemoveEmptyEntries).Skip(1).ToList();

            // 信号名のリスト
            var signals = container[0].Split(",", StringSplitOptions.RemoveEmptyEntries)
                .Skip(1)
                .Select(item => item.Trim('\n', ' ')).ToArray();


            return container.Skip(1).AsParallel()
                .WithDegreeOfParallelism(10)
                .SelectMany(block => {
                    // ブロックを改行で分割
                    var box = block.Split("\n", StringSplitOptions.RemoveEmptyEntries);

                    // 1行目は#sweep[num] [num]を取り出してsweep値とする
                    var sweep = int.Parse(box[0].Replace("sweep", ""));

                    // データのパース
                    // Time, Val1, Val2, val3...
                    return box
                        .Skip(1) // sweep [num] の行を飛ばす
                        .Select(b => b.Split(',', StringSplitOptions.RemoveEmptyEntries))
                        .Select(b => b.Select(s => decimal.Parse(s, NumberStyles.Float)).ToArray())
                        .SelectMany(b => b
                            .Skip(1) // Timeを飛ばす
                            .Select((v, i) => new Record(sweep, seed, signals[i], b[0], v)));
                }).ToArray();
        }

        public static IEnumerable<Record[]> Build(IReadOnlyList<string> list) {
            foreach (var path in list) {
                yield return Build(path);
            }
        }
    }
}


