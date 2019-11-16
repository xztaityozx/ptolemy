using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Ptolemy.Libra.Request;
using Ptolemy.Repository;

namespace Ptolemy {
    public static class Program {
        internal static void Main(string[] args) {
            var db = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "workspace", "db",
                "db");
            var req = new LibraRequest("n1[4n]<n2[4n]", (1,1), "20x500", 1L, db);
            var libra = new Libra.Libra(CancellationToken.None);

            var res = libra.Run(req);

            foreach (var tuple in res) {
                Console.WriteLine(tuple);
            }
        }
    }
}