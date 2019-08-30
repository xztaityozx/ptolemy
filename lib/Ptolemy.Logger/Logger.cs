using System;
using System.Collections.Generic;
using System.IO;

namespace Ptolemy.Logger {
    public class Logger {
        private readonly List<ILogHook> hooks;

        private readonly ConsoleColor[] consoleColors = {
            ConsoleColor.White, ConsoleColor.DarkCyan,
            ConsoleColor.DarkYellow, ConsoleColor.Red,
            ConsoleColor.DarkRed
        };

        public LogLevel LogLevel { get; set; } = LogLevel.Log;

        public Logger() {
            hooks = new List<ILogHook>();
        }

        public void AddHook(ILogHook hook) => hooks.Add(hook);

        private void Write(object message, LogLevel level, bool hook = true) {
            
            var msg = $"[{level}][{DateTime.Now:yyyy/MM/dd HH:mm:ss.fff}] {message}";

            if(hook) WriteToHooks(msg, level);

            // 設定したLogLevelより低かったらSTDOUTに出さない
            if (level < LogLevel) return;

            Console.ForegroundColor = consoleColors[(int) level];
            Console.WriteLine(msg);
            Console.ResetColor();
        }

        private void WriteToHooks(object message, LogLevel level) {
            foreach (var logHook in hooks) {
                logHook.Write(message, level);
            }
        }

        public void Log(object message, bool hook = true) {
            Write(message, LogLevel.Log, hook);
        }

        public void Info(object message, bool hook = true) {
            Write(message, LogLevel.Info, hook);
        }

        public void Warn(object message, bool hook = true) {
            Write(message, LogLevel.Warn, hook);
        }

        public void Error(object message, bool hook = true) {
            Write(message, LogLevel.Error, hook);
        }

        public void Fatal(object message, bool hook = true) {
            Write(message, LogLevel.Fatal, hook);
        }
    }

    public enum LogLevel {
        Log,
        Info,
        Warn,
        Error,
        Fatal
    }

    public interface ILogHook {
        void Write(object message, LogLevel level);
    }

    public class FileHook : ILogHook {
        private readonly string path;

        public FileHook(string path) {
            this.path = path;
            if (File.Exists(path)) return;
            using (var f = File.Create(path)) {
                Console.Error.WriteLine($"[Logger] : Created logfile {f.Name}");
            }
        }

        public void Write(object message, LogLevel level) {
            File.AppendAllText(path, $"{message}\n");
        }
    }
}