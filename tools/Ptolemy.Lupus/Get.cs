using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CommandLine;
using Ptolemy.Verb;

namespace Ptolemy.Lupus {
    [Verb("get")]
    public class Get : Verb.Verb {

        [Option('i', "sigmaRange", Default = "0.046,0.004,0.2", HelpText = "[開始値],[刻み幅],[終了値]でシグマを範囲指定します")]
        public string SigmaRange { get; set; }

        [Option('w', "sweepRange", Default = "1,5000", HelpText = "[開始値],[終了値]でSweepの範囲を指定します")]
        public string SweepRange { get; set; }

        [Option('e', "seedRange",Default = "1,2000", HelpText = "[開始値],[終了値]でSeedの範囲を指定します")]
        public string SeedRange { get; set; }

        private (decimal start, decimal step, decimal stop) sigma, sweep, seed;

        protected override Exception Do(CancellationToken token) {
            {
              var split = SigmaRange.Split(',').Select(x => string.IsNullOrEmpty(x)?)
            }
            return null;
        }
    }
}
