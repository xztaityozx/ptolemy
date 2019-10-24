using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using CommandLine;
using Kurukuru;
using Ptolemy.Interface;
using Ptolemy.SiMetricPrefix;
using Ptolemy.Repository;
using Ptolemy.Slack;

namespace Ptolemy {
    public class Program {
        internal static void Main(string[] args) {
            var log = new Logger.Logger();
            var conf = new SlackConfig {
                Channel = "#test", BotName = "bot", MachineName = "machine", UserName = "xztaityozx",
                WebHookUrl = "https://hooks.slack.com/services/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"
            };
            Slack.Slack.PostToAriesResult(10, 20, TimeSpan.Zero, conf);
        }
    }

}
