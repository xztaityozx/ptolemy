namespace Ptolemy.Hydra.Exception {
    public class InvalidRequestException : System.Exception {
        public InvalidRequestException(string name, object value, string message) : base(
            $"{name}={value} <-- {message}"){}
    }
}