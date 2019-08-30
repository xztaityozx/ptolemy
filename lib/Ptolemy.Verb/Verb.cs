using System;
using System.Linq;
using System.Threading;
using CommandLine;
using Ptolemy.Logger;
using Ptolemy.Parameters;
using Ptolemy.SiMetricPrefix;

namespace Ptolemy.Verb {
    public delegate void OnBeginEnventHandler();

    public delegate void OnFinishEnventHandler();

    public abstract class Verb : IVerb {
        protected Logger.Logger Logger;

        public Exception Run(CancellationToken token, string logFile) {
            OnBegin?.Invoke();

            // init logger
            Logger = new Logger.Logger();
            if(!string.IsNullOrEmpty(logFile)) Logger.AddHook(new FileHook(logFile));

            // Bind
            Vtn = new Transistor(Bind(VtnString, (VtnThreshold, VtnSigma, VtnDeviation), Sigma));
            Vtp = new Transistor(Bind(VtpString, (VtpThreshold, VtpSigma, VtpDeviation), Sigma));

            var rt = Do(token);
            OnFinish?.Invoke();
            return rt;
        }

        public event OnBeginEnventHandler OnBegin;
        public event OnFinishEnventHandler OnFinish;
        protected abstract Exception Do(CancellationToken token);

        protected Transistor Vtn, Vtp;

        private static (decimal, decimal, decimal) Bind(string str, (double, double, double) ind, double sigma) {
            var (t, s, d) = ind;

            if ($"{s:E10}" == $"{-1.0:E10}") s = sigma;

            var box = str.Split(',');
            if(box.Length!=3) throw new VerbException("vt{n,p} option must be have 3 values");

            var rt = box.Zip(
                new[] {t, s, d}, (input, value) =>
                    string.IsNullOrEmpty(input) ? (decimal) value : input.ParseDecimalWithSiPrefix()).ToArray();

            return (rt[0], rt[1], rt[2]);
        }

        public string VtnString { get; set; }
        public double VtnThreshold { get; set; }
        public double VtnSigma { get; set; }
        public double VtnDeviation { get; set; }
        public string VtpString { get; set; }
        public double VtpThreshold { get; set; }
        public double VtpSigma { get; set; }
        public double VtpDeviation { get; set; }
        public double Sigma { get; set; }
    }

    public class VerbException : Exception {
        public VerbException(string msg) : base(msg) { }
    }

    public interface IVerb {
        Exception Run(CancellationToken token, string logFile);

        [Option('N', "vtn", Default = ",,", HelpText = "Vtnを[閾値],[シグマ],[偏差]で指定します")]
        string VtnString { get; set; }

        [Option("vtnThreshold", Default = 0.6, HelpText = "Vtnの閾値です。優先されます")]
        double VtnThreshold { get; set; }

        [Option("vtnSigma", Default = -1.0, HelpText = "Vtnのシグマです。優先されます")]
        double VtnSigma { get; set; }

        [Option("vtnDeviation", Default = 1.0, HelpText = "Vtnの偏差です。優先されます")]
        double VtnDeviation { get; set; }

        [Option('P', "vtp", Default = ",,", HelpText = "Vtpを[閾値],[シグマ],[偏差]で指定します")]
        string VtpString { get; set; }

        [Option("vtpThreshold", Default = -0.6, HelpText = "Vtpの閾値です。優先されます")]
        double VtpThreshold { get; set; }

        [Option("vtpSigma", Default = -1, HelpText = "Vtpのシグマです。優先されます")]
        double VtpSigma { get; set; }

        [Option("vtpDeviation", Default = 1.0, HelpText = "Vtpの偏差です。優先されます")]
        double VtpDeviation { get; set; }

        [Option('s', "sigma", HelpText = "vtn,vtpのシグマです。個別設定が優先されます", Default = 0.046)]
        double Sigma { get; set; }
    }
}
