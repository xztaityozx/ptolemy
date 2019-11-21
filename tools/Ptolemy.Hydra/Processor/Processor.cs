using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ptolemy.Argo.Request;
using Ptolemy.Repository;

namespace Ptolemy.Hydra.Processor {
    public class Processor : IDisposable {
        private readonly BlockingCollection<ArgoRequest> jobQueue;
        private readonly CancellationToken token;

        public Processor(CancellationToken token) {
            jobQueue = new BlockingCollection<ArgoRequest>();
            this.token = token;
        }

        public void Dispose() {
            jobQueue?.Dispose();
        }

        public void AddJob(HydraJob job) {
            if (job.TryParseRequest(out var ar)) {
                jobQueue.Add(ar, token);
            }
        }
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
