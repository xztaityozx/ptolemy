using System;
using System.Net;
using Ptolemy.Parameters;

namespace Ptolemy.Draco.Request {
    public class DracoRequest {
        public Guid GroupId { get; set; }
        public TransistorPair Transistors { get; set; }
        public bool UseSqlServer { get; set; }
        public string SqLiteFile { get; set; }
        public IPAddress Host { get; set; }
        public int Port { get; set; }
    }
}
