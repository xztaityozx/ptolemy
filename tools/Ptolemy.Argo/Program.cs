using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CommandLine.Text;
using Ptolemy.Argo.Request;
using Ptolemy.Parameters;
using YamlDotNet.Core.Events;

namespace Ptolemy.Argo {
    internal static class Program {
        private static void Main(string[] args) {
            try {
                var log = new Logger.Logger();
                Options.Parse(args);
            }
            catch (ArgoParseFailedException) {
                Console.WriteLine();
            }
        }
    }

    public class Argo {
        private readonly ArgoRequest request;
        private readonly Logger.Logger log;

        public Argo(Options o, Logger.Logger log) {
            this.log = log;

            o.CircuitRoot = o.CircuitRoot ?? Environment.GetEnvironmentVariable("ARGO_CIRCUIT_ROOT");
            o.Hspice = o.Hspice ?? Environment.GetEnvironmentVariable("ARGO_HSPICE");
            o.TargetCircuit = o.TargetCircuit ?? Environment.GetEnvironmentVariable("ARGO_TARGET_CIRCUIT");
            o.ModelFile = o.ModelFile ?? Environment.GetEnvironmentVariable("ARGO_MODEL_FILE");

            if (string.IsNullOrEmpty(o.CircuitRoot)) {
                throw new ArgoException(
                    "circuit root is not set. please check env:ARGO_CIRCUIT_ROOT or -r,--root option");
            }

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
            

            log.Info($"circuit root: {o.CircuitRoot}");
            if (!string.IsNullOrEmpty(o.JsonFile)) {
                log.Info($"use json request: file--> {o.JsonFile}");
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

                log.Info($"Success parse json to request\n{request}");
            }
            else {
                (decimal, decimal, decimal) Bind(string def, string input) {
                    var box = input.Split(',')
                        .Zip(def.Split(','), (i, d) => string.IsNullOrEmpty(i) ? d : i)
                        .Select(SiMetricPrefix.SiMetricPrefix.ParseDecimalWithSiPrefix)
                        .ToList();
                    return (box[0], box[1], box[2]);
                }

                log.Info("Building request from cli options");

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

                log.Info($"Success parse request:{request}");

            }
        }
    }

    public class ArgoParseFailedException:Exception {}
}
