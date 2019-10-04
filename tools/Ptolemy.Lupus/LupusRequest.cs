using Ptolemy.Draco.Request;
using Ptolemy.Libra.Request;

namespace Ptolemy.Lupus {
    public class LupusRequest {
        public LibraRequest LibraRequest { get; set; }
        public DracoRequest[] DracoRequests { get; set; }
    }
}