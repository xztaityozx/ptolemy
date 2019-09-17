using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Ptolemy.Argo.Request;
using Ptolemy.Parameters;

namespace Ptolemy.Argo {
    public class Argo {
        private readonly ArgoRequest request;
        public Argo(ArgoRequest request) => this.request = request;
    }
}