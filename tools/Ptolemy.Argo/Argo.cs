using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Ptolemy.Argo.Request;
using Ptolemy.Parameters;

namespace Ptolemy.Argo {
    public class Argo {
        private readonly ArgoRequest request;
        private readonly string circuitRoot;
        private readonly Logger.Logger log;

        public ArgoResult Run(CancellationToken token) {
            var res = new Runner(token, request, circuitRoot).RunWithSpinner();
            return res;
        }

        private static string Expand(string name,string p) {
            try {
                return FilePath.FilePath.Expand(p);
            }
            catch (Exception ) {
                throw new ArgoException($"Failed expand path:{name}={p}");
            }
        }

        public Argo(ArgoRequest request, string circuitRoot) =>
            (this.request, this.circuitRoot) = (request, circuitRoot);

        public Argo(Options o) {
            o.CircuitRoot = o.CircuitRoot ?? Environment.GetEnvironmentVariable("ARGO_CIRCUIT_ROOT");
            o.Hspice = o.Hspice ?? Environment.GetEnvironmentVariable("ARGO_HSPICE");
            o.TargetCircuit = o.TargetCircuit ?? Environment.GetEnvironmentVariable("ARGO_TARGET_CIRCUIT");
            o.ModelFile = o.ModelFile ?? Environment.GetEnvironmentVariable("ARGO_MODEL_FILE");

            o.CircuitRoot = Expand(nameof(o.CircuitRoot), o.CircuitRoot);
            o.ModelFile = Expand(nameof(o.ModelFile), o.ModelFile);
            o.Hspice = Expand(nameof(o.Hspice), o.Hspice);

            if (string.IsNullOrEmpty(o.CircuitRoot)) {
                throw new ArgoException(
                    "circuit root is not set. please check env:ARGO_CIRCUIT_ROOT or -r,--root option");
            }
            circuitRoot = o.CircuitRoot;
            if (string.IsNullOrEmpty(o.Hspice)) {
                throw new ArgoException(
                    $"hspice is not set. please check env:ARGO_HSPICE or -h,--hspice option");
            }
            if (string.IsNullOrEmpty(o.TargetCircuit)) {
                throw new ArgoException(
                    $"target circuit is not set. please check env:ARGO_TARGET_CIRCUIT or -t,--target option");
            }
            if (string.IsNullOrEmpty(o.ModelFile)) {
                throw new ArgoException(
                    $"model file is not set. please check env:ARGO_MODEL_FILE or -m,--model option");
            }
            if (!string.IsNullOrEmpty(o.JsonFile)) {
                if (!File.Exists(o.JsonFile)) {
                    throw new ArgoException($"Ptolemy.Argo cannot find {o.JsonFile}");
                }

                try {
                    using (var sr = new StreamReader(o.JsonFile))
                        request = ArgoRequest.FromJson(sr.ReadToEnd());
                }
                catch (Exception e) {
                    throw new ArgoException($"Failed to parse json to ArgoRequest\n\tinnerException-->{e}");
                }

            }
            else {
                (decimal, decimal, decimal) Bind(string def, string input) {
                    var box = input.Split(',')
                        .Zip(def.Split(','), (i, d) => string.IsNullOrEmpty(i) ? d : i)
                        .Select(SiMetricPrefix.SiMetricPrefix.ParseDecimalWithSiPrefix)
                        .ToList();
                    return (box[0], box[1], box[2]);
                }
                try {
                    request = new ArgoRequest {
                        TargetCircuit = o.TargetCircuit,
                        Sweep = o.Sweeps,
                        Gnd = (decimal) o.Gnd,
                        Seed = o.Seed,
                        Temperature = (decimal) o.Temperature,
                        Vdd = (decimal) o.Vdd,
                        IcCommands = o.IcCommands.ToList(),
                        SweepStart = o.SweepStart,
                        ModelFilePath = o.ModelFile,
                        BaseDirectory = o.BaseDir,
                        HspiceOptions = o.HspiceOptions.ToList(),
                        Time = new Range(Bind(Options.TimeDefault, o.TimeString)),
                        Vtn = new Transistor(Bind(Options.VtnDefault, o.VtnString)),
                        Vtp = new Transistor(Bind(Options.VtpDefault, o.VtpString)),
                        HspicePath = o.Hspice ?? Environment.GetEnvironmentVariable("ARGO_HSPICE"),
                        GroupId = Guid.NewGuid()
                    };
                }
                catch (Exception e) {
                    throw new ArgoException(
                        $"Failed to build request from cli options\n\tinnerException-->{e}");
                }
            }
        }

        public Argo(Options o, Logger.Logger log) : this(o) {
            this.log = log;
            log.Info($"Circuit Root: {o.CircuitRoot}");
            log.Info($"Target Circuit: {o.TargetCircuit}");
            log.Info($"Model File: {o.ModelFile}");
            log.Info($"hspice: {request.HspicePath}");
            log.Info("Parameters");
            log.Info($"\tVtn: {request.Vtn}");
            log.Info($"\tVtp: {request.Vtp}");
            log.Info($"\tSweeps: {request.Sweep}");
            log.Info($"\tSeed: {request.Seed}");
            log.Info("Ptolemy.Argo generated request");
        }
    }
}