using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Ptolemy.Libra.Request;

namespace Ptolemy.Libra {
    public class Libra {
        private readonly CancellationToken token;
        private readonly LibraRequest request;

        public Libra(CancellationToken token, LibraRequest request) {
            this.token = token;
            this.request = request;
        }

        public Tuple<string, long>[] Run() {
            try {
                var delegates = request.BuildFilter();
                var signals = request.SignalList;
                var times = request.TimeList;

                if (!signals.Any()) throw new LibraException("Conditions have no signals");
                if (!times.Any()) throw new LibraException("Conditions have no time");

                using var repo = new Repository.SqliteRepository(request.SqliteFile);
                return repo.Aggregate(signals, (request.SeedStart, request.SeedEnd),
                        (request.SweepStart, request.SweepEnd), delegates, LibraRequest.GetKey)
                    .Zip(signals, (l, s) => Tuple.Create(s, l)).ToArray();
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
