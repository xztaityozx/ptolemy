using System;
using System.IO;

namespace Ptolemy.FilePath {
    public static class FilePath {
        public static string Expand(string path) {
            var split = path.Split('\\', '/');
            if (split[0] != "~") return Path.GetFullPath(path);

            split[0] = Home;
            path = Path.Combine(split);

            return Path.GetFullPath(path);
        }

        public static string Home => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        public static string DotConfig => Expand("~/.config/ptolemy");

        public static void TryMakeDirectory(string path) {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        }
    }
}
