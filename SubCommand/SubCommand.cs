using System;
using System.Linq;
using CommandLine;

namespace SubCommand {
    public delegate void OnBegin();

    public delegate void OnFinish();
    public abstract class SubCommand : ISubCommand {
        public void Run() {
            OnBegin?.Invoke();
            Do();
            OnFinish?.Invoke();
        }

        protected abstract void Do();

        public event OnBegin OnBegin;
        public event OnFinish OnFinish;
        public decimal VtnThreshold {get;private set;} = 0.6M;
        public decimal VtnSigma     {get;private set;} = 0.046M;
        public decimal VtnDeviation {get;private set;} = 1.0M;
        public decimal VtpThreshold {get;private set;} = -0.6M;
        public decimal VtpSigma     {get;private set;} = 0.046M;
        public decimal VtpDeviation { get; private set; } = 1.0M;
        public string Vtn { get; set; }
        public string Vtp { get; set; }
    }

    public interface ISubCommand {
        [Option('N', "vtn", Default = "0.6,0.046,1.0", HelpText = "Vtnを[閾値],[シグマ],[偏差]で指定します")]
        string Vtn { get; set; }

        [Option('P', "vtp", Default = "-0.6,0.046,1.0", HelpText = "Vtnを[閾値],[シグマ],[偏差]で指定します")]
        string Vtp { get; set; }
    }
}
