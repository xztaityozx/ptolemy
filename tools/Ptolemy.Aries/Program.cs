using System;
using System.Text.Json;
using System.Threading;
using CommandLine;
using Ptolemy.Interface;
using Ptolemy.OptionException;

namespace Ptolemy.Aries {
    internal static class Program {
        private static void Main(string[] args) {
            var log = new Logger.Logger();

            try {
                Config.Config.Load();
                log.Info($"use config file: {Config.Config.ConfigFile}");
            }
            catch (Exception e) {
                log.Error($"Failed load config file: {Config.Config.ConfigFile}\n\t-->{e}");
                Environment.Exit(1);
            }


            Console.Clear();
            using var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (sender, eventArgs) => {
                eventArgs.Cancel = true;
                cts.Cancel();
            };

            log.Warn("Press Ctrl+C to cancel");
            var token = cts.Token;

            try {
                var itf = Parser.Default.ParseArguments<AriesMake, AriesRun, AriesLsDb>(args)
                    .MapResult(
                        (AriesMake a) => a,
                        (AriesRun a) => a,
                        (AriesLsDb a) => (IAriesVerb)a,
                        e => throw new ParseFailedException());
                
                itf.Run(token);
            }
            catch (ParseFailedException) {
            }
            catch (AriesException e) {
                log.Error(e);
            }
            catch (Exception e) {
                log.Error($"Unknown error has occured\n\t-->{e}");
            }
            
            Console.ResetColor();
        }
    }
}
