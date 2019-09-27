using System;
using System.Collections.Generic;
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
    }
}
