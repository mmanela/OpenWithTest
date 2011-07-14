using System;

namespace MattManela.OpenWithTest
{
    public interface ILogger
    {
        void Log(string message, string source, LogType logType);
        void Log(string message, string source, Exception e);
        void MessageBox(string title, string message, LogType logType);
    }
}
