using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Ptolemy.Argo.Request;
using Ptolemy.Parameters;

namespace Ptolemy.Aries {
    public class AriesRequest {
        public IEnumerable<ArgoRequest> ArgoRequests { get; set; }

        public string ToJson() => JsonSerializer.Serialize(this);
        public static AriesRequest FromJson(string doc) => JsonSerializer.Deserialize<AriesRequest>(doc);
    }
}
