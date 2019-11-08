using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Ptolemy.Parameters;
using Ptolemy.Repository;

namespace Ptolemy {
    public static class Program {
        internal static void Main(string[] args) {
            var pair = new[] {new {n = 0.6, p = -0.2}, new {n = 0.59, p = -0.2}};

            foreach (var np in pair) {
                var vtn = new Transistor(np.n, 0.046, 1.0);
                var vtp = new Transistor(np.p, 0.046, 1.0);
                var pe = new ParameterEntity {
                    Vtn = vtn.ToString(), Vtp = vtp.ToString()
                };

                Console.WriteLine(vtn);
                Console.WriteLine(vtp);
                Console.WriteLine(pe.Hash());
            }
        }
    }
}