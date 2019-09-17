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
                
            }
        }
    }

    public class Errors : IPtolemyCli {
        private readonly IEnumerable<Error> errors;
        public Errors(IEnumerable<Error> error) => errors = error;
        public Exception Run(CancellationToken token, string[] args) {
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
