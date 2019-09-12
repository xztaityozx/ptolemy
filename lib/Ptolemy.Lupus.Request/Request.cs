using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Ptolemy.Parameters;

namespace Ptolemy.Lupus.Request {
    public class LupusRequest {
        public string TargetDirectory { get; set; }
        public string WaveViewPath { get; set; }
        public List<string> WaveViewOptions { get; set; }
        public List<string> Signals { get; set; }
        public Range PlotPoint { get; set; }
        public string ResultFileName { get; set; }
        public Guid GroupId { get; set; }

        public string ToJson() => JsonConvert.SerializeObject(this);
        public static LupusRequest FromJson(string doc) => JsonConvert.DeserializeObject<LupusRequest>(doc);
    }
}
