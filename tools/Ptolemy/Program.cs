using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using CommandLine;
using Kurukuru;
using Ptolemy.Interface;
using Ptolemy.SiMetricPrefix;
using Ptolemy.Repository;

namespace Ptolemy {
    public class Program {
        internal static void Main(string[] args) {

            var log = new Logger.Logger();
            using var sub=new Subject<string>();

            IEnumerable<long> Range() {
                for (var l = 1; l <= 100; l++) yield return l;
            }

            var rt = new List<ResultEntity>();
            sub.Where(s => !string.IsNullOrEmpty(s))
                .SkipWhile(s => s[0] != 'x')
                .TakeWhile(s => s[0] != 'y')
                .ToList()
                .Repeat()
                .Zip(Range(), (list, l) => Tuple.Create(list.Skip(3), l))
                .Subscribe(pair => {
                    var (doc, sweep) = pair;
                    foreach (var line in doc) {
                        rt.AddRange(ResultEntity.Parse(1, sweep, line, new[]{"n1","n2","blb","bl"}));
                    }
                });

            using var sr = new StreamReader(@"C:\Users\xztaityozx\Workspace\hspice.lis");
            while (sr.Peek() > 0) {
                sub.OnNext(sr.ReadLine());
            }

            sub.OnCompleted();

            log.Info(rt.Count);
        }
    }

}
