using System;
using System.Linq;
using System.Threading;
using Ptolemy.Libra.Request;
using Ptolemy.Repository;
using ShellProgressBar;

namespace Ptolemy.Libra {
    public class Libra {
        private readonly CancellationToken token;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        public Libra(CancellationToken token) {
            this.token = token;
        }

        /// <summary>
        /// requestに従って数え上げる
        /// </summary>
        /// <returns>Expression,結果のペアリスト</returns>
        public Tuple<string,long>[] Run(LibraRequest request) {
            try {
                var delegates = request.BuildFilter();
                var signals = request.SignalList;

                if (!signals.Any()) throw new LibraException("Conditions have no signals");

                using var bar = new ProgressBar((int) request.Sweeps.Times, "Ptolemy.Libra", new ProgressBarOptions {
                    ForegroundColor = ConsoleColor.DarkYellow, BackgroundCharacter = '-',
                    ProgressCharacter = '>', CollapseWhenFinished = true, BackgroundColor = ConsoleColor.Gray,
                    ForegroundColorDone = ConsoleColor.Green
                });
                var db = new ReadOnlyRepository(request.SqliteFile);
                db.IntervalEvent += () => bar.Tick();

                var result = request.IsSplitWithSeed switch {
                    true => db.Aggregate(token, signals, delegates,
                        request.Sweeps.Repeat().ToList(),
                        request.Sweeps.Start,
                        request.Sweeps.Size,
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
    }

}
