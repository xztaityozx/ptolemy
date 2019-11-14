using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Ptolemy.Libra.Request;
using Ptolemy.Repository;

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

                var db = new ReadOnlyRepository(request.SqliteFile);

                var result = request.IsSplitWithSeed switch {
                    true => db.Aggregate(token, signals, delegates,
                        Range(request.SeedStart, request.SeedEnd).ToList(),
                        request.SweepStart, request.SweepEnd, LibraRequest.GetKey),
                    //false => db.Aggregate(token, signals, delegates,request.SeedStart, )
                    };

                if (request.IsSplitWithSeed) {
                    
                }

                return null;
            }
            catch (LibraException) {
                throw;
            }
            catch (Exception e) {
                throw new LibraException($"Unknown error has occured\n\t-->{e}");
            }
        }

        private static IEnumerable<long> Range(long start, long end) {
            for (var l = start; l <= end; l++) yield return l;
        }
    }

}
