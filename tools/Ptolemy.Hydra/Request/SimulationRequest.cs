using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Ptolemy.Hydra.Exception;
using Ptolemy.Parameters;

namespace Ptolemy.Hydra.Request {
    public class SimulationRequest {
        public Guid Id { get; set; }
        public Guid RequestGroupId { get; set; }
        [JsonIgnore] public long Seed { get; set; }
        [JsonIgnore] public long Sweep { get; set; }
        public Transistor Vtn { get; set; }
        public Transistor Vtp { get; set; }
        public bool UseDatabase { get; set; }
        public bool KeepCsv { get; set; }
        public List<string> Signals { get; set; }
        public Range<decimal> Time { get; set;}
        public Range<decimal> PlotPoint { get; set; }
        public string TargetCel { get; set; }
    }

    public static class Factory {
        private static IEnumerable<SimulationRequest> GenerateRequestSplitBySeed(
            Transistor vtn, Transistor vtp,
            Range<long> seed, long totalSweeps
        ) {
            var rt = new List<SimulationRequest>();

            var seeds = seed.GenerateRange((t, stop) => t <= stop, (t, step) => t + step).ToList();
            if (seeds.Count <= 0)
                throw new InvalidRequestException("number of seeds", seeds.Count, "Seedの個数を0個以下にすることはできません");
            
            var sweepSize = totalSweeps / seeds.Count;
            if (sweepSize <= 0)
                throw new InvalidRequestException("sweep size", sweepSize, "seedの個数が多いため、Sweepの値が0になりました");

            var current = 0L;
            foreach (var s in seeds) {
                // TODO: 
//                rt.Add(
////                    totalSweeps - current >= sweepSize ? SimulationRequest.New()
//                );
            }
            
            return rt;
        }
        
    }
}