﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using Ptolemy.Argo.Request;
using Ptolemy.Repository;

namespace Ptolemy.Argo {
    public class Argo : IDisposable {
        public const string EnvArgoHspice = "ARGO_HSPICE";
        private readonly ArgoRequest request;
        private readonly string workDir;
        private readonly Guid id;
        private readonly Exec.Exec exec;
        private readonly CancellationToken token;


        public IObservable<string> Receiver => exec.StdOut
            .Where(s => !string.IsNullOrEmpty(s))
            .SkipWhile(s => s[0] != 'x')
            .TakeWhile(s => s[0] != 'y')
            .Repeat();


        public Argo(ArgoRequest request, CancellationToken token) {
            (this.request, workDir) = (request,
                Path.Combine(Path.GetTempPath(), "Ptolemy.Argo", request.GroupId.ToString()));
            if (!Directory.Exists(workDir)) Directory.CreateDirectory(workDir);

            id = Guid.NewGuid();
            exec = new Exec.Exec(token);
            this.token = token;
        }

        public void RunWithParse(Subject<ResultEntity> receiver) {
            var rt = new List<ResultEntity>();

            IEnumerable<long> Range() {
                for (var l = request.SweepStart; l <= request.SweepStart + request.Sweep; l++) yield return l;
            };

            var rec = exec.StdOut
                .Where(s => !string.IsNullOrEmpty(s))
                .SkipWhile(s => s[0] != 'x')
                .TakeWhile(s => s[0] != 'y')
                .ToList()
                .Repeat()
                .Zip(Range(), (list, l) => Tuple.Create(list.Skip(3), l))
                .Subscribe(pair => {
                    var (doc, sweep) = pair;
                    foreach (var entity in doc.SelectMany(line => ResultEntity.Parse(request.Seed, sweep, line, request.Signals))) {
                        receiver.OnNext(entity);
                    }
                });

            token.Register(rec.Dispose);

            Run();

            receiver.OnCompleted();
        }

        public (bool status, ArgoRequest result) Run() {
            try {
                var spi = MakeScript();
                token.ThrowIfCancellationRequested();

                var stderr = new StringBuilder();
                var errors = exec.StdError.Subscribe(s => stderr.AppendLine(s));
                exec.Run(request.HspicePath, request.HspiceOptions.Concat(new[] {"-i", spi}).ToArray());

                errors.Dispose();
                
                if (!string.IsNullOrEmpty(stderr.ToString().TrimEnd())) throw new ArgoException(stderr.ToString());
                return (exec.ExitCode == 0, request);
            }
            catch (ArgoException) {
                throw;
            }
            catch (OperationCanceledException) {
                throw;
            }
            catch (Exception e) {
                throw new ArgoException($"Unknown error has occured\n\tinnerException-->{e}");
            }
        }

        private string MakeScript() {
            var path = Path.Combine(workDir, id + ".spi");
            var sb = new StringBuilder();
            sb.AppendLine("* Generated for: HSPICE");
            sb.AppendLine("* Generated by: Ptolemy.Argo");
            sb.AppendLine($"* Target: {request.NetList}");

            sb.AppendLine(
                $".param vtn=AGAUSS({request.Transistors.Vtn.Threshold},{request.Transistors.Vtn.Sigma},{request.Transistors.Vtn.Deviation}) vtp=AGAUSS({request.Transistors.Vtp.Threshold},{request.Transistors.Vtp.Sigma},{request.Transistors.Vtp.Deviation})");
            sb.AppendLine(".option PARHIER=LOCAL");
            sb.AppendLine($".option SEED={request.Seed}");
            sb.AppendLine($".temp {request.Temperature}");
            sb.AppendLine($".IC {string.Join(" ", request.IcCommands)}");
            sb.AppendLine($"VDD VDD! 0 {request.Vdd}V");
            sb.AppendLine($"VGND GND! 0 {request.Gnd}V");
            foreach (var include in request.Includes) {
                sb.AppendLine($".include '{include}'");
            }

            // Read NetList
            try {
                using var sr = new StreamReader(request.NetList);
                sb.AppendLine(sr.ReadToEnd());
            }
            catch (FileNotFoundException) {
                throw new ArgoException($"NetList file not found: path={request.NetList}");
            }
            catch (Exception e) {
                throw new ArgoException($"Unknown error has occured\n\tinnerException: {e}");
            }

            sb.AppendLine(
                $".tran {request.Time.Step} {request.Time.Stop} start={request.Time.Start} uic sweep monte={request.Sweep} firstrun={request.SweepStart}");
            sb.AppendLine(".option opfile=0");

            sb.AppendLine($".print {string.Join(" ", request.Signals.Select(x => $"V({x})"))}");
            sb.AppendLine(".end");

            using (var sw = new StreamWriter(path, false, new UTF8Encoding(false))) {
                sw.WriteLine(sb.ToString());
            }

            return path;
        }

        public void Dispose() {
            exec?.Dispose();
        }
    }
}