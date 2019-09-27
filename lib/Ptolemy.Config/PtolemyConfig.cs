using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using Ptolemy.Argo.Request;
using Ptolemy.Draco.Request;
using Ptolemy.Libra.Request;
using Ptolemy.Interface;
using YamlDotNet.Serialization;

namespace Ptolemy.Config {
    public class Config {
        [YamlMember] public ArgoRequest ArgoDefault { get; set; }
        [YamlMember] public DracoRequest DracoDefault { get; set; }
        [YamlMember] public string DataBaseUsername { get; set; }
        [YamlMember] public string DataBasePassword { get; set; }
        [YamlMember] public LibraRequest LibraDefault { get; set; }


        private static Config instance = null;

        [YamlIgnore]
        public static string ConfigFile { get; set; } = Path.Combine(FilePath.FilePath.DotConfig, "config.yaml");


        /// <summary>
        /// Load config file
        /// </summary>
        public static void Load() {
            string doc;

            if (!File.Exists(ConfigFile)) throw new FileNotFoundException("can not found config file", ConfigFile);

            using (var sr = new StreamReader(ConfigFile, Encoding.UTF8)) doc = sr.ReadToEnd();
            try {
                instance = JsonConvert.DeserializeObject<Config>(doc);
                return;
            }
            catch (Exception) {
                instance = null;
            }

            try {
                instance = new Deserializer().Deserialize<Config>(doc);
                return;
            }
            catch (Exception) {
                instance = null;
            }

            if (instance == null)
                throw new InvalidDataContractException("failed to load config. it should be json or yaml format");
        }

        public static Config Instance {
            get {
                if (instance == null) Load();
                return instance;
            }
        }
    }

}
