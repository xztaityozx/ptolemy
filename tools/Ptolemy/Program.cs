using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using CommandLine;
using Ptolemy.Interface;

namespace Ptolemy {
    public class Program {
        internal static void Main(string[] args) {
            using (var cts = new CancellationTokenSource()) {
                Console.CancelKeyPress += (sender, eventArgs) => {
                    eventArgs.Cancel = true;
                    cts.Cancel();
                };
                var token = cts.Token;
                var tool = Parser.Default.ParseArguments<Lupus.Lupus>(args)
                    .MapResult<Lupus.Lupus, IPtolemyTool>(
                        (Lupus.Lupus l) => l,
                        e => new Errors(e)
                    );
                var res = tool.Invoke(token, tool.Args.ToArray());
                if (res == null) {
                    Console.ResetColor();
                    return;
                }
                var log=new Logger.Logger();
                switch (res) {
                    case OperationCanceledException e:
                        log.Fatal("Canceled by user");
                        break;
                    default:
                        log.Error(res);
                        break;
                }
            }
        }
    }

    public class Errors : IPtolemyTool {
        private readonly IEnumerable<Error> errors;
        public Errors(IEnumerable<Error> error) => errors = error;
        public Exception Invoke(CancellationToken token, string[] args) {
            var list=new List<Exception>();

            foreach (var error in errors) {
                switch (error) {
                    case HelpRequestedError helpRequestedError:
                        return null;
                    case HelpVerbRequestedError helpVerbRequestedError:
                        return null;
                    case VersionRequestedError versionRequestedError:
                        return null;
                    default:
                        list.Add(new Exception($"{error}"));
                        break;
                }
            }

            return new AggregateException(list);
        }

        public IEnumerable<string> Args { get; set; }
    }
}
