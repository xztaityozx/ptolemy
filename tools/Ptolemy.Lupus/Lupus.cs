using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Ptolemy.Lupus.Request;

namespace Ptolemy.Lupus {
    public class Lupus {
        private readonly LupusRequest request;

        public Lupus(LupusRequest request) => this.request = request;

        public LupusResult Run(CancellationToken token) {
            var rt = new LupusResult();
            try {
                var ace = CreateAceScript();
                var netlistDir = FilePath.FilePath.Expand(Path.Combine(request.TargetDirectory, "..", "netlist"));
                CreateResultsMapXml(netlistDir);
                CreateResultsXml(netlistDir);

                rt.ExecutedCommand =
                    $"cd {request.TargetDirectory} && {request.WaveViewPath} {string.Join(" ", request.WaveViewOptions)} -k -ace_no_gui {ace}";
                using (var exec = new Exec.Exec(token)) {
                    exec.Run(rt.ExecutedCommand, o => { }, true);
                    if(exec.ExitCode != 0) throw new LupusException("Failed to extract csv from files");
                }
            }
            catch (LupusException e) {
                return new LupusResult {
                    Exception = e, Message = e.Message
                };
            }
            catch (Exception e) {
                return new LupusResult {
                    Exception = new LupusException($"Unknown exception has occured: {e}"), Message = e.Message
                };
            }

            return rt;
        }

        private void CreateResultsXml(string netlistDir) {
            try {
                // list up sweeps
                var targets = Directory.GetFiles(request.TargetDirectory, "*.tr0@*")
                    .Select(s => s.Split('@').Last())
                    .Select(long.Parse)
                    .OrderBy(l => l).ToList();
                var path = Path.Combine(request.TargetDirectory, "results.xml");
                Xml.LupusXml.GenerateResultsXml(netlistDir, targets).WriteTo(path);
            }
            catch (Exception e) {
                throw new LupusException($"Failed to write results.xml\n\t-->{e}");
            }
        }

        private void CreateResultsMapXml(string netlistDir) {
            try {
                var resultsMap = Path.Combine(request.TargetDirectory, "resultsMap.xml");
                Xml.LupusXml.GenerateResultsMapXml(netlistDir).WriteTo(resultsMap);
            }
            catch (Exception exception) {
                throw new LupusException($"Failed to write resultsMap.xml\n\t-->{exception}");
            }
        }
        private string CreateAceScript() {
            request.TargetDirectory = FilePath.FilePath.Expand(request.TargetDirectory);
            if (!Directory.Exists(request.TargetDirectory))
                throw new LupusException($"Lupus cannot find target directory({request.TargetDirectory})");

            try {
                // request.TargetDirectory/ace
                var acePath = Path.Combine(request.TargetDirectory, "ace");
                using (var sw = new StreamWriter(acePath)) {
                    sw.WriteLine("set xml [ sx_open_wdf \"resultsMap.xml\" ]");
                    sw.WriteLine("sx_current_sim_file $xml");
                    sw.WriteLine($"set www [ sx_signal \"{string.Join(" ", request.Signals)}\" ]");
                    sw.WriteLine("sx_export_csv on");
                    sw.WriteLine($"sx_export_range {request.PlotPoint.Start:E} {request.PlotPoint.Stop:E} {request.PlotPoint.Step:E}");
                    sw.WriteLine($"sx_export_data {request.ResultFileName} $www");
                    sw.Flush();
                }

                return acePath;
            }
            catch (Exception e) {
                throw new LupusException($"Failed create ace script\n\t-->innerException:{e}");
            }
        }
    }

    public class LupusResult {
        public string Message { get; set; }
        public string ExecutedCommand { get; set; }
        public LupusException Exception { get; set; }
    }

    public class LupusException : Exception {
        public LupusException(string msg) : base(msg) { }
    }
}
