using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Ptolemy.Libra.Request;
using Ptolemy.Repository;
using ShellProgressBar;

namespace Ptolemy.Libra {
    public class Libra {
        private readonly CancellationToken token;
        private readonly Logger.Logger log;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <param name="log"></param>
        public Libra(CancellationToken token, Logger.Logger log) {
            this.token = token;
            this.log = log;
        }

        /// <summary>
        /// requestに従って数え上げる
        /// </summary>
        /// <returns>Expression,結果のペアリスト</returns>
        public Tuple<string, long>[] Run(LibraRequest request) {
            try {
                var delegates = request.BuildFilter();
                var signals = request.SignalList;

                if (!signals.Any()) throw new LibraException("Conditions have no signals");

                using var bar = new ProgressBar(
                    (int) (request.IsSplitWithSeed ? (request.SeedEnd - request.SeedStart + 1) : request.Sweeps.Times),
                    "Ptolemy.Libra", new ProgressBarOptions {
                        ForegroundColor = ConsoleColor.DarkYellow, BackgroundCharacter = '-',
                        ProgressCharacter = '>', CollapseWhenFinished = true, BackgroundColor = ConsoleColor.Gray,
                        ForegroundColorDone = ConsoleColor.Green
                    });
                var db = new ReadOnlyRepository(request.SqliteFile);
                db.IntervalEvent += () => bar.Tick();
                log.Info("----Parameter Info----");
                Console.WriteLine(db.GetParameter());
                log.Info("----------------------");


                var result = request.IsSplitWithSeed switch {
                    true => db.Aggregate(token, signals, delegates,
                        Range(request.SeedStart, request.SeedEnd).ToList(),
                        request.Sweeps.Size,
                        request.Sweeps.Start,
                        LibraRequest.GetKey),
                    false => db.Aggregate(token, signals,
                        delegates,
                        request.SeedStart,
                        request.Sweeps.Section().ToList(),
                        LibraRequest.GetKey)
                    };

                return request.ExpressionNameList.Zip(result, Tuple.Create).ToArray();
            }
            catch (LibraException) {
                throw;
            }
            catch (Exception e) {
                throw new LibraException($"Unknown error has occured\n\t-->{e}");
            }
        }

        private static IEnumerable<long> Range(long start, long end) {
            for (var e = start; e <= end; e++) yield return e;
        }
    }

}
