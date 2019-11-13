using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Ptolemy.Libra.Request;
using Ptolemy.Map;
using Ptolemy.Repository;

namespace Ptolemy {
    public static class Program {
        internal static void Main(string[] args) {
            
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