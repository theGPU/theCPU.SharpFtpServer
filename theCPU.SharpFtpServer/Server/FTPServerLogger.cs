using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using theCPU.SharpFtpServer.Commands.Base;

namespace theCPU.SharpFtpServer.Server
{
    public enum LogLevel : byte
    {
        None = 0,
        Trace = 1,
        Debug = 2
    }

    public delegate void FTPServerLoggerCallbackDelegate(LogLevel level, string message);

    public class FTPServerLogger
    {
        public LogLevel LogLevel { get; set; } = LogLevel.None;
        public FTPServerLoggerCallbackDelegate? Callback { private get; set; } = null;

        public void LogTrace(string message) => Log(LogLevel.Trace, message);
        public void LogTrace(IFtpCommand command, string message) => Log(LogLevel.Trace, command, message);

        public void LogDebug(string message) => Log(LogLevel.Debug, message);
        public void LogDebug(IFtpCommand command, string message) => Log(LogLevel.Debug, command, message);

        public void Log(LogLevel level, IFtpCommand command, string message) => Log(level, command.Name, message);
        public void Log(LogLevel level, string command, string message) => Log(level, $"[{command}] {message}");
        public void Log(LogLevel level, string message)
        {
            if (level > LogLevel)
                return;

            Callback?.Invoke(level, message);
        }
    }
}
