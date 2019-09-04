using System;
using System.Threading;

namespace Ptolemy.Hydra {
    public interface IHydraStage {
        void Run(HydraRequest request);
    }
}