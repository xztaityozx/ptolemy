using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Ptolemy.Hydra.Processor;
using Ptolemy.Simulator;

namespace Ptolemy.Hydra {
    public class Program {
        public static void Main(string[] args) {
            using var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (sender, eventArgs) => {
                eventArgs.Cancel = true;
                cts.Cancel();
            };
            var token = cts.Token;

            var host = CreateHostBuilder(args).Build();
            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            var proc = Task.Factory.StartNew(() => {
                new Processor.Processor(token, new Hspice(),
                    Path.Combine(Config.Config.Instance.WorkingRoot, "aries", "db"), new ProcessorOption {
                        Logger = logger
                    }).Run();
            }, token);
            var api = Task.Factory.StartNew(() => host.Run(), token);

            Task.WaitAll(proc, api);
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); })
                .ConfigureLogging((context, builder) => {
                    builder.AddConsole().AddDebug().AddFile("logs/log-{Date}.log");
                });
    }
}
