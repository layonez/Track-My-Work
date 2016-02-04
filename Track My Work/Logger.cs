using System;
using System.IO;

namespace Track_My_Work
{
    static class Logger
    {
        private const string LogName = "log.txt";
        private static readonly string Path = AppDomain.CurrentDomain.BaseDirectory + LogName;

        public static void Log(Exception ex)
        {
            File.AppendAllLines(Path, new[] {DateTime.Now.ToString(), ex.Message, ex.StackTrace,Environment.NewLine});
        }
    }
}
