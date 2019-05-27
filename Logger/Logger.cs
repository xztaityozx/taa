using System;
using System.Collections.Generic;
using System.IO;

namespace Logger {
    public class Logger  {
        private List<ILogWriter> writers;
        public LogLevel LogLevel { get; }

        private void Write(LogLevel level, object message) {
           if(level<LogLevel) return;
           foreach (var w in writers) {
               w.Write($"{DateTime.Now} [{level}] " + message, colorTable[(int)level]);
           }
        }

        private readonly List<ConsoleColor> colorTable = new List<ConsoleColor> {
            ConsoleColor.Cyan,
            ConsoleColor.DarkYellow,
            ConsoleColor.Red,
            ConsoleColor.DarkRed,
            ConsoleColor.DarkMagenta
        };

        public Logger(params ILogWriter[] writers) : this(LogLevel.Info, writers){}
        
        public Logger(LogLevel level, params ILogWriter[] writers) {
            LogLevel = level;
            this.writers=new List<ILogWriter>();
            foreach (var w in writers) {
                this.writers.Add(w);
            }
        }
        
        public void Info(object message) {
            Write(LogLevel.Info, message);
        }

        public void Error(object message) {
            Write(LogLevel.Error, message);
        }

        public void Fatal(object message) {
            Write(LogLevel.Fatal, message);
        }

        public void Throw(object message) {
            Write(LogLevel.Throw, message);
            throw new LoggerException(message);
        }

        public void Warn(object message) {
            Write(LogLevel.Warn, message);
        }
    }
    
    public enum LogLevel{
        Info,
        Warn,
        Error,
        Throw,
        Fatal
    }

    public interface ILogWriter {
        void Write(object message, ConsoleColor color=ConsoleColor.White);
    }

    public class ConsoleLogger : ILogWriter {

        public void Write(object message, ConsoleColor color = ConsoleColor.White) {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }

    public class FileLogger : ILogWriter {
        public void Write(object message, ConsoleColor color=ConsoleColor.White) {
            File.AppendAllText(filePath, $"{message}");
        }

        private readonly string filePath;

        public FileLogger(string file) {
            filePath = file;
        }
    }

    public class LoggerException : Exception {
        public LoggerException(object message) :base(message.ToString()) {}
    }
}