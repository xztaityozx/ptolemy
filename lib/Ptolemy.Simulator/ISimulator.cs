using System;
using System.Collections.Generic;
using System.Threading;
using Ptolemy.Argo.Request;
using Ptolemy.Repository;

namespace Ptolemy.Simulator {
    public interface ISimulator {
        IReadOnlyList<ResultEntity> Run(CancellationToken token, ArgoRequest request, Action intervalAction);
    }
}
