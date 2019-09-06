using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using YamlDotNet.Serialization;

namespace Ptolemy.Config
{
    public class Config {
        public ArgoRequest.ArgoRequest ArgoDefault { get; set; }

        // TODO: ここ実装しような
        private static Config instance = null;
        public static string ConfigFile { get; set; } = Path.Combine(FilePath.FilePath.DotConfig, "config.yaml");
        private void Load() {
            var ext = Path.GetExtension(ConfigFile);
            using (var sr = new StreamReader(ConfigFile, Encoding.UTF8)) {

            }
        }

        public Config Instance {
            get { return instance; }
        }
    }
}
