using System;
using System.Linq;
using System.Threading;
using CommandLine;
using Ptolemy.Parameters;
using Ptolemy.SiMetricPrefix;

namespace Ptolemy.Verb {
    public delegate void OnBeginEnventHandler();

    public delegate void OnFinishEnventHandler();

    public abstract class Verb : IVerb {
        public void Run(CancellationToken token) {
            OnBegin?.Invoke();

            // Bind
            Vtn = new Transistor(Bind(VtnString, (VtnThreshold, VtnSigma, VtnDeviation), Sigma));
            Vtp = new Transistor(Bind(VtpString, (VtpThreshold, VtpSigma, VtpDeviation), Sigma));

            Do(token);
            OnFinish?.Invoke();
        }

        public event OnBeginEnventHandler OnBegin;
        public event OnFinishEnventHandler OnFinish;
        protected abstract void Do(CancellationToken token);

        protected Transistor Vtn, Vtp;

        private static (decimal, decimal, decimal) Bind(string str, (double, double, double) ind, double sigma) {
            var (t, s, d) = ind;

            if ($"{s:E10}" == $"{-1.0:E10}") s = sigma;

            var box = str.Split(',');
            if(box.Length!=3) throw new VerbException("vt{n,p} option must be have 3 values");

            var rt = box.Zip(
                new[] {t, s, d}, (input, value) =>
                    string.IsNullOrEmpty(input) ? (decimal) value : input.ParseDecimal()).ToArray();

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
        void Run(CancellationToken token);

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
