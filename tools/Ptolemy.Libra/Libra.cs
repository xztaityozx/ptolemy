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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <param name="request"></param>
        public Libra(CancellationToken token, LibraRequest request) {
            this.token = token;
            this.request = request;
        }

        /// <summary>
        /// requestに従って数え上げる
        /// </summary>
        /// <returns>Expression,結果のペアリスト</returns>
        public Tuple<string,long>[] Run() {
            try {
                var delegates = request.BuildFilter();
                var signals = request.SignalList;

                if (!signals.Any()) throw new LibraException("Conditions have no signals");


                return null;
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
