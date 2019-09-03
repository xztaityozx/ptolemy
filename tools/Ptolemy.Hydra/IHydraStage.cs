using System.Threading;

namespace Ptolemy.Hydra {
    public interface IHydraStage {
        void Run(CancellationToken token, Logger.Logger logger);
    }
}