using System;
using System.Collections.Generic;
using System.Text;

namespace Ptolemy.Simulator
{
    public class SimulatorException : Exception {
        public SimulatorException(string msg) : base(msg) { }
        public SimulatorException() { }
    }
}
