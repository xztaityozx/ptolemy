using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Ptolemy.Parameters;

namespace Ptolemy.Lupus.Record {
    public class LupusGetRequest : LupusRequest<LupusGetRequest> {
        public override string ToJson() => JsonConvert.SerializeObject(this);

        public enum RequestMode {
            Single,
            Range
        }

        public RequestMode Mode { get; set; }
        public long SweepStart { get; set; }
        public long SweepStep { get; set; }
        public long SweepEnd { get; set; }
        public long SeedStart { get; set; }
        public long SeedStep { get; set; }
        public long SeedEnd { get; set; }
        public decimal SigmaStart { get; set; }
        public decimal SigmaStep { get; set; }
        public decimal SigmaEnd { get; set; }
        public Filter Filter { get; set; }

        public LupusGetRequest() {
        }

        public LupusGetRequest(Transistor vtn, Transistor vtp, Filter filter) =>
            (Vtn, Vtp, Filter) = (vtn, vtp, filter);

        public LupusGetRequest(
            Transistor vtn, Transistor vtp,
            Filter filter,
            (decimal start, decimal step, decimal stop) sigma,
            (long start, long step, long stop) sweep,
            (long start, long step, long stop) seed
        ) : this(vtn, vtp, filter) {
            Mode = RequestMode.Range;
            (SweepStart, SweepStep, SweepEnd) = sweep;
            (SeedStart, SeedStep, SeedEnd) = seed;
            (SigmaStart, SigmaStep, SigmaEnd) = sigma;
        }

        public LupusGetRequest(
            Transistor vtn, Transistor vtp,
            Filter filter,
            (long start, long step, long stop) sweep,
            (long start, long step, long stop) seed
        ) : this(vtn, vtp, filter) {
            (SweepStart, SweepStep, SweepEnd) = sweep;
            (SeedStart, SeedStep, SeedEnd) = seed;
            Mode = RequestMode.Single;
        }

        public LupusGetRequest(
            decimal vtnThreshold, decimal vtnDeviation,
            decimal vtpThreshold, decimal vtpDeviation,
            Filter filter,
            (decimal start, decimal step, decimal stop) sigma,
            (long start, long step, long stop) sweep,
            (long start, long step, long stop) seed
        ) : this(
            new Transistor(vtnThreshold, 0, vtnDeviation),
            new Transistor(vtpThreshold, 0, vtpDeviation),
            filter,
            sigma, sweep, seed
        ) {
        }

        public LupusGetRequest(
            decimal vtnThreshold, decimal vtnSigma, decimal vtnDeviation,
            decimal vtpThreshold, decimal vtpSigma, decimal vtpDeviation,
            Filter filter,
            (long start, long step, long stop) sweep,
            (long start, long step, long stop) seed
        ) : this(
            new Transistor(vtnThreshold, vtnSigma, vtnDeviation),
            new Transistor(vtpThreshold, vtpSigma, vtpDeviation),
            filter, sweep, seed
        )
        {
        }

        /// <summary>
        /// Generate sigma list
        /// </summary>
        /// <returns></returns>
        public IEnumerable<decimal> GetSigmaEnumerable() {
            for (var s = SigmaStart; s <= SigmaEnd; s += SigmaStep) yield return s;
        }

        public IEnumerable<(Transistor,Transistor)> GetTransistorEnumerable() {
            return GetSigmaEnumerable().Select(s => (
                new Transistor(Vtn.Threshold, s, Vtn.Deviation),
                new Transistor(Vtp.Threshold, s, Vtp.Deviation)
            ));
        }
    }

    public abstract class LupusRequest<T> {
        public Transistor Vtn { get; set; }
        public Transistor Vtp { get; set; }

        protected LupusRequest() { }
        protected LupusRequest(Transistor vtn, Transistor vtp) => (Vtn, Vtp) = (vtn, vtp);
        protected LupusRequest(
            decimal vtnThreshold, decimal vtnSigma, decimal vtnDeviation,
            decimal vtpThreshold, decimal vtpSigma, decimal vtpDeviation
        ) : this(new Transistor(vtnThreshold,vtnSigma,vtnDeviation), new Transistor(vtpThreshold, vtpSigma, vtpDeviation)) { }

        public abstract string ToJson();
        public static T FromJson(string str) => JsonConvert.DeserializeObject<T>(str);
    }


    public class LupusPushRequest : LupusRequest<LupusPushRequest> {
        public IReadOnlyList<string> FileList { get; set; }

        public LupusPushRequest() {
        }

        public LupusPushRequest(Transistor vtn, Transistor vtp, IEnumerable<string> files) : base(vtn, vtp) =>
            FileList = files.ToList();

        public LupusPushRequest(
            decimal vtnThreshold, decimal vtnSigma, decimal vtnDeviation,
            decimal vtpThreshold, decimal vtpSigma, decimal vtpDeviation,
            IEnumerable<string> files
        ) : this(new Transistor(vtnThreshold, vtnSigma, vtnDeviation),
            new Transistor(vtpThreshold, vtpSigma, vtpDeviation), files) {
        }

        /// <summary>
        /// Convert to Json string
        /// </summary>
        /// <returns></returns>
        public override string ToJson() => JsonConvert.SerializeObject(this);
    }

}
