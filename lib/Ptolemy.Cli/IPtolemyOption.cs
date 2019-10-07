namespace Ptolemy.Cli {
    public interface IPtolemyOption<TRequest> {
        TRequest BuildRequest();
    }
}