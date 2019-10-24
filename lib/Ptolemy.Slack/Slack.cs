using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Slack.Webhooks;

namespace Ptolemy.Slack {
    public class Slack {
        public const string 進捗 = "#進捗";

        private static SlackMessage BuildBaseMassage(string chn, string name) {
            return new SlackMessage {
                Channel = chn,
                IconEmoji = Emoji.Stars,
                Username = name
            };
        }

        private static SlackAttachment BuildAttachment(string main, string color, params (string title, string value)[] fields) {
            return new SlackAttachment {
                Text = main, Color = color,
                Fields = fields.Select(s=>new SlackField{Title = s.title, Value = s.value}).ToList(),
                Footer = "Ptolemy [xztaityozx/ptolemy]"
            };
        }

        public static bool ReplyTo(string text, SlackConfig config) {
            var msg = BuildBaseMassage(config.Channel, config.BotName);
            msg.Text = $"<@{config.UserName}> {text}";
            return new SlackClient(config.WebHookUrl).Post(msg);
        }

        public static bool PostToAriesResult(int success, int failed, TimeSpan elapsed,
            SlackConfig config) {
            var client = new SlackClient(config.WebHookUrl);
            var msg = BuildBaseMassage(config.Channel, config.BotName);

            msg.Text = $"<@{config.UserName}> やっほー。シミュレーション終わったよ :star:";
            msg.Attachments = new List<SlackAttachment> {
                BuildAttachment("Ptolemy.Aries run result summary", "#673AB7", ("Elapsed", $"{elapsed}"),
                    ("Success", $"{success}"), ("Failed", $"{failed}"))
            };

            return client.Post(msg);
        }
    }

    public class SlackConfig {
        private const string EnvSlackWebHookUrl = "PTOLEMY_SLACK_WEBHOOK_URL";
        public string WebHookUrl { get; set; }
        public string Channel { get; set; }
        public string MachineName { get; set; }
        public string BotName { get; set; }
        public string UserName { get; set; }

        public void Bind() {
            if (string.IsNullOrEmpty(WebHookUrl)) WebHookUrl = Environment.GetEnvironmentVariable(EnvSlackWebHookUrl);
        }
    }
}
