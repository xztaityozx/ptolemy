using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Ptolemy.Libra.Request;
using Ptolemy.Map;
using Ptolemy.Parameters;
using Ptolemy.Repository;

namespace Ptolemy {
    public static class Program {
        internal static void Main(string[] args) {
            var db = Path.Combine(Environment.GetEnvironmentVariable("HOME"), "workspace", "db","db");
            
            db.WL();
            
            var repo = new SqliteRepository(db);
            var req = new LibraRequest("!(n1[4n]>0.7999 && n2[4n]<0.002)", (1, 1), (1, (long) 1e7), db);

            var f = req.BuildFilter();

            var res = repo.Aggregate(req.SignalList, (1, 1), (1, (long) 1e7), f, LibraRequest.GetKey,
                CancellationToken.None);
            
            res.WriteLines();
        }
    }
    public static class Ext {
        public static void WL<T>(this T @this) => Console.WriteLine(@this);
        public static void WriteLines<T>(this IEnumerable<T> @this) {
            foreach (var item in @this) {
                Console.WriteLine(item);
            }
        }
    }
}