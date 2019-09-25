using System;
using Ptolemy.Draco.Request;

namespace Ptolemy.Draco {
    public class DracoException : Exception {
        public DracoException(string msg) :base(msg) {}
    }

    public class DracoParseFailedException : Exception {
    }
}