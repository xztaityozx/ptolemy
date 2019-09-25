using System;

namespace Ptolemy.Draco.Request {
    public class DracoRequest {
        public Guid GroupId { get; set; }
        public string OutputFile { get; set; }
        public string InputFile { get; set; }
        public long Seed { get; set; }
        public long Sweep { get; set; }
        public int BufferSize { get; set; }
    }
}
