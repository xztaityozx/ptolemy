using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using YamlDotNet.Serialization;

namespace Ptolemy.Hydra {
    public class HydraConfig {
        [YamlMember(Alias = "hspice")] public ExternalToolConfig Hspice { get; set; }
        [YamlMember(Alias = "wv")] public ExternalToolConfig WaveView { get; set; }
        [YamlMember(Alias = "default")] public HydraParameters Parameters { get; set; }

        private static HydraConfig instance;

        [YamlIgnore]
        public string ConfigPath { get; set; }
        public HydraConfig Instance {
            get {
                if (instance != null) return instance;
                using(var sr = new StreamReader(ConfigPath)) instance = new Deserializer().Deserialize<HydraConfig>(sr);

                return instance;
            }
        }

        public void EditConfig() {

        }
    }

    public class ExternalToolConfig {
        [YamlMember(Alias ="path")] public string Path { get; set; }
        [YamlMember(Alias = "options")] public List<string> Options { get; set; }
    }
}
