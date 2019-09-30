using System;

namespace Ptolemy.OptionException {
    public class ParseFailedException : Exception {
        public ParseFailedException() { }
        public ParseFailedException(string msg) : base(msg) { }
    }
}
