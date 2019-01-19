using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace TranslationsDocGen
{
    public static class Logger
    {
        public static void Log(string s)
        {
            Console.WriteLine(s);
            foreach (var listener in Listeners)
            {
                listener.Log(s + "\n");
            }
        }
        
        public static HashSet<ILogListener> Listeners = new HashSet<ILogListener>();
    }

    public interface ILogListener
    {
        void Log(string s);
    }

    public class FileLogListener : ILogListener, IDisposable
    {
        private StringBuilder _logBuffer = new StringBuilder();
        private readonly string _fileName;
        
        public FileLogListener(string fileName)
        {
            _fileName = fileName + "_" + $"{DateTime.Now:s}".Replace(":", "-") + ".txt";
            Logger.Listeners.Add(this);
        }
        
        public void Dispose()
        {
            Logger.Listeners.Remove(this);
            File.WriteAllText(_fileName, _logBuffer.ToString());
        }

        public void Log(string s)
        {
            _logBuffer.Append(s);
        }

    }
}