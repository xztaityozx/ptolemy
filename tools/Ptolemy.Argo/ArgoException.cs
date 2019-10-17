using System;

namespace Ptolemy.Argo {
    public class ArgoException :Exception {
        public ArgoException(string msg) : base(msg) { }
    }

    public class ArgoClean : Exception {}
}