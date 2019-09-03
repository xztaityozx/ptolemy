using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace Ptolemy.Hydra.Simulation {
    public class SimulationRequest {
        public Hspice Hspice { get; }
        public WaveView WaveView { get; }
        public string SimulationDir { get; }
        public bool KeepCsv { get; }
        public bool AutoRemove { get; }
        public string SpiScript { get; }
        public string AceScript { get; }
        public string ResultDir { get; }

        public SimulationRequest() {}

        public SimulationRequest(Hspice h, WaveView w, string sd, string rd, string spi, string ace, bool keep,
            bool remove) =>
            (Hspice, WaveView, SimulationDir, ResultDir, SpiScript, AceScript, KeepCsv, AutoRemove) =
            (h, w, sd, rd, spi, ace, keep, remove);
    }
    public class Simulation : IHydraStage {
        private readonly SimulationRequest request;

        public Simulation(SimulationRequest req) => request = req;

        public void Run(CancellationToken token, Logger.Logger logger) {
            foreach (var process in new[] {request.Hspice.GetCommand(request.SpiScript), request.WaveView.GetCommand(request.AceScript)}.Select(
                cmd => new Process {
                    StartInfo = new ProcessStartInfo {
                        FileName = "bash",
                        Arguments = "-c " + cmd
                    }
                }
            )) {
                token.ThrowIfCancellationRequested();

                token.Register(() => process.Kill());
                
                var result = process.Start();
                if (!result) {
                    throw new HydraException("failed command");
                }
            }

            if (request.AutoRemove || request.KeepCsv) {
                Directory.Delete(request.SimulationDir, true);
            }

            if (request.AutoRemove) {
                Directory.Delete(request.ResultDir, true);
            }
        }
    }

    public class Hspice {
        [YamlMember(Alias = "path")] public string Path { get; set; }
        [YamlMember(Alias = "options")] public List<string> Options { get; set; } = new List<string>();

        public string GetCommand(string spiPath) => $"{Path} {string.Join(" ", Options)} -i {spiPath} -o ./hspice &> ./hspice.log";
    }

    public class WaveView {
        [YamlMember(Alias = "path")] public string Path { get; set; }

        public string GetCommand(string aceScript) => $"{Path} -k -ace_no_gui {aceScript} &> ./wv.log";
    }
}