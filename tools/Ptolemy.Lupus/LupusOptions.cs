using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;
using Ptolemy.Cli;
using Ptolemy.Draco.Request;
using Ptolemy.Libra.Request;

namespace Ptolemy.Lupus {
    public class LupusOptions : IPtolemyOption<LupusRequest> {
        [Option('d', "directory", HelpText = "数え上げたいファイルが格納されいているディレクトリを指定できます")]
        public string TargetDirectory { get; set; }

        [Option('e',"expression", HelpText = "数え上げの条件式です")]
        public string Expressions { get; set; }

        private const long Seed = 1;
        private const int BufferSize = (int)1E6;

        [Value(0, Default = null, HelpText = "入力ファイルのリスト")]
        public IEnumerable<string> Files { get; set; }

        public LupusRequest BuildRequest() {
            var tmp = Path.Combine(Path.GetTempPath(), "Ptolemy.Lupus");
            Directory.CreateDirectory(tmp);
            var db = Path.Combine(tmp, $"{Guid.NewGuid()}");

            if (Files is null || !Files.Any())
                Files = Directory.GetFiles(
                    FilePath.FilePath.Expand(TargetDirectory ?? throw new LupusException("-d,--directoryを指定してください")));

            if(!Files.Any()) throw new LupusException("ファイルが少なくとも1つは必要です");

            var targets = Files.Select(FilePath.FilePath.Expand).ToList();

            return new LupusRequest {
                LibraRequest = new LibraRequest(Expressions, (Seed, Seed), (0, targets.Count), db),
                DracoRequests = targets.Select((s, i) => new DracoRequest {
                    Sweep = i, Seed = Seed, BufferSize = BufferSize, InputFile = s, OutputFile = db,
                    GroupId = Guid.NewGuid()
                }).ToArray()
            };
        }

    }

    public class LupusException : Exception {
        public LupusException(string msg) : base(msg) { }
    }
}
