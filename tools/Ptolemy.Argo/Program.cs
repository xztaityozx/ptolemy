using System;
using Ptolemy.Parameters;

namespace Ptolemy.Argo {
    internal class Program {
        private static void Main(string[] args) {
            Console.WriteLine(new YamlDotNet.Serialization.Serializer().Serialize(new {
                x = new Range<int> {
                    Start = 10,
                    Stop = 20,
                    Step = 1,
                }
            }));
        }
    }
}
