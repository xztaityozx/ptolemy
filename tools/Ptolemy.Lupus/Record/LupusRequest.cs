using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Ptolemy.Parameters;

namespace Ptolemy.Lupus.Record {
    public class LupusRequest {
        public Transistor Vtn { get; set; }
        public Transistor Vtp { get; set; }
        public IReadOnlyList<string> FileList { get; set; }

        public LupusRequest() {
        }

        public LupusRequest(Transistor vtn, Transistor vtp, IEnumerable<string> files) =>
            (Vtn, Vtp, FileList) = (vtn, vtp, files.ToList());

        public LupusRequest(
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
        public string ToJson() => JsonConvert.SerializeObject(this);

        /// <summary>
        /// Convert string to FactoryRecord
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static LupusRequest FromJson(string json) => JsonConvert.DeserializeObject<LupusRequest>(json);
    }

}
