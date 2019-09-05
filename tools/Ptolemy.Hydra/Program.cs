using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CancellationTokenSource = System.Threading.CancellationTokenSource;

namespace Ptolemy.Hydra {
    public class Program {
        public static void Main(string[] args) {
            using (var cts = new CancellationTokenSource()) {
                var token = cts.Token;
                var webApi = Task.Factory.StartNew(() =>
                    CreateWebHostBuilder(args).Build().Run(), token);
                var doTask = Task.Factory.StartNew(() => {
                    while (!token.IsCancellationRequested) {
                        Trace.WriteLine("Do");
                        Thread.Sleep(1000);
                    }
                }, token);

                Task.WaitAny(webApi, doTask);
                cts.Cancel();
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}