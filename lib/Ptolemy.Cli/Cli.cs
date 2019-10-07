using System;
using System.Collections.Generic;
using System.Threading;
using CommandLine;
using Ptolemy.Logger;
using Ptolemy.OptionException;

namespace Ptolemy.Cli {
    public class PtolemyCliException : Exception {
        public PtolemyCliException(){}
        public PtolemyCliException(string msg):base(msg){}
    }
    
    public abstract class SingleCli<TOptions, TRequest, TResult> :IDisposable where TOptions : IPtolemyOption<TRequest> {
        private readonly CancellationTokenSource cts;
        protected readonly CancellationToken token;
        protected readonly TRequest request;
        private readonly Logger.Logger log;
        
        protected SingleCli(IEnumerable<string> args, Logger.Logger log) {
            this.log = log;
            cts = new CancellationTokenSource();
            token = cts.Token;
            request = Parser.Default.ParseArguments<TOptions>(args).MapResult(
                o => o.BuildRequest(),
                e => throw new ParseFailedException()
            );
        }

        public TResult Run<TException>() where TException : Exception {
            Console.CancelKeyPress += (sender, args) => {
                args.Cancel = true;
                cts.Cancel();
            };
            log.Warn("Press Ctrl+C to cancel");

            try {
                return Process();
            }
            catch (OperationCanceledException) {
                throw new PtolemyCliException("Canceled by user");
            }
            catch (TException e) {
                throw new PtolemyCliException($"{e}");
            }
            catch (Exception e) {
                throw new PtolemyCliException($"Unknown error has occured\n\t-->{e}");
            }
        }

        protected abstract TResult Process();

        public void Dispose() {
            cts?.Dispose();
        }
    }
}