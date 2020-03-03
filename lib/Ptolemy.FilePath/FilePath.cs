using System;
using System.IO;

namespace Ptolemy.FilePath {
    public static class FilePath {
        /// <summary>
        /// パスを展開する
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string Expand(string path) {
            var split = path.Split('\\', '/');
            if (split[0] != "~") return Path.GetFullPath(path);

            split[0] = Home;
            path = Path.Combine(split);

            return Path.GetFullPath(path);
        }

        /// <summary>
        /// ホームディレクトリへのパスを返す
        /// </summary>
        public static string Home => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        /// <summary>
        /// ptolemyのコンフィグディレクトリへのパスを返す
        /// </summary>
        public static string DotConfig => Expand("~/.config/ptolemy");

        /// <summary>
        /// ディレクトリを作成する
        /// </summary>
        /// <param name="path"></param>
        public static void TryMakeDirectory(string path) {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        }
    }
}
