using System.Threading;

namespace Ptolemy.Aries {
    public interface IAriesVerb {
        void Run(CancellationToken token);
    }
}