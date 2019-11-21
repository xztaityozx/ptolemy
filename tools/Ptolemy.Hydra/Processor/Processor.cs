using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ptolemy.Argo.Request;
using Ptolemy.Repository;
using Ptolemy.Simulator;

namespace Ptolemy.Hydra.Processor {
    public class Processor : IDisposable {
        private readonly BlockingCollection<ArgoRequest> jobQueue;
        private readonly CancellationToken token;
        private readonly ISimulator simulator;
        private readonly ProcessorOption option;
        private readonly DbHub dbHub;

        public Processor(CancellationToken token, ISimulator simulator, string dbRoot, ProcessorOption option) {
            jobQueue = new BlockingCollection<ArgoRequest>();
            this.token = token;
            this.simulator = simulator;
            this.option = option;
            dbHub = new DbHub(token, dbRoot, option.BufferSize, option.Logger);
        }

        private Task workersTask;

        public void Start() {
            var workers = jobQueue.GetConsumingEnumerable().AsParallel().WithCancellation(token);
            Info($"Parallel mode: {option.POption}");
            if (option.POption == ProcessorOption.ParallelOption.Manual) {
                workers = workers.WithDegreeOfParallelism(option.ManualParallelSize);
                Info($"Parallel size: {option.ManualParallelSize}");
            }

            workersTask = Task.Factory.StartNew(() => workers.ForAll(req => {
                try {
                    Worker(req);
                }
                catch (Exception) {
                    if(option.RequestRemoveOption == ProcessorOption.FailedRequestRemoveOption.ReCreate) ReCreate(req);
                }
            }), token);
        }

        public void Wait() {
            try {
                Task.WaitAll(new[] {workersTask}, token);
            }
            catch (TaskCanceledException) {
                Warn("Cancel was requested");
                Fatal("Job queue will be close and requests will be　discard");
            }
            catch (OperationCanceledException) {
                Warn("Cancel was requested");
                Fatal("Job queue will be close and requests will be　discard");
            }
        }

        public void Run() {
            Start();
            Wait();
        }

        private void Worker(ArgoRequest request) {
            try {
                token.ThrowIfCancellationRequested();
                var pe = SimulatorExtension.ConvertToParameterEntity(request);
                var key = dbHub.AddDb(pe);

                foreach (var re in simulator.Run(token, request, null)) {
                    dbHub.AddEntity(key, re);
                }
            }
            catch (OperationCanceledException) {
                Warn("Operation was canceled");
                throw;
            }
            catch (SimulatorException e) {
                Error($"error has occured in simulator: {e}");
                throw;
            }
            catch (Exception e) {
                Error($"unknown error has occured: {e}");
                throw;
            }
        }

        private static void ReCreate(ArgoRequest ar) {
            var path = Path.Combine(Config.Config.Instance.WorkingRoot, "aries", "task", $"{Guid.NewGuid()}.json");
            using var sw=new StreamWriter(path);
            sw.WriteLine(ar.ToJson());
        }

        private void Info(string message) => option.Logger?.LogInformation(message);
        private void Warn(string message) => option.Logger?.LogWarning(message);
        private void Error(string message) => option.Logger?.LogError(message);
        private void Fatal(string message) => option.Logger?.LogCritical(message);


        public void Dispose() {
            jobQueue?.Dispose();
            dbHub?.Dispose();
            Info("Processor has disposed");
        }

        public void EnqueueJob(HydraJob job) {
            if (job.TryParseRequest(out var ar)) {
                jobQueue.Add(ar, token);
                Info("1 job added");
            }
            else {
                Error("added request is not valid request");
                throw new FormatException("追加しようとしたJobは有効ではありません");
            }
        }

        public void CloseJobQueue() {
            jobQueue.CompleteAdding();
            Warn("Closed job queue.");
        }
    }

    public class ProcessorOption {
        public enum ParallelOption {
            Auto,
            Manual
        }

        public enum FailedRequestRemoveOption {
            Delete, ReCreate
        }

        public ParallelOption POption { get; set; } = ParallelOption.Auto;
        public int ManualParallelSize { get; set; } = 1;
        public int BufferSize { get; set; } = 100000;
        public ILogger Logger { get; set; } = null;
        public FailedRequestRemoveOption RequestRemoveOption { get; set; } = FailedRequestRemoveOption.ReCreate;

        public ProcessorOption() { }
        public ProcessorOption(int parallel) => (POption, ManualParallelSize) = (ParallelOption.Manual, parallel);
    }

    public class HydraJob {
        public string ArgoRequestFilePath { get; }
        public HydraJob(string file) => ArgoRequestFilePath = file;

        public bool TryParseRequest(out ArgoRequest ar) {
            ar = null;
            try {
                ar = ArgoRequest.FromFile(ArgoRequestFilePath);
                return ar.IsSimulatable();
            }
            catch (Exception) {
                return false;
            }
        }

    }
}
