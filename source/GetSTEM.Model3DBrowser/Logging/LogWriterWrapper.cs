using System.Diagnostics;
using log4net;
using log4net.Core;

namespace GetSTEM.Model3DBrowser.Logging
{
    public class LogWriterWrapper
    {
        private static ILog Logger = LogManager.GetLogger("GetSTEM3D");

        public static void WriteMessage(string message, Level level)
        {
            var stackTrace = new StackTrace(true);
            var frame = stackTrace.GetFrame(1);

            if (level == Level.Debug && Logger.IsDebugEnabled)
            {
                Logger.Debug(LoggerMessage.CreateMessage(message, frame));
            }
            if (level == Level.Error && Logger.IsErrorEnabled)
            {
                Logger.Error(LoggerMessage.CreateMessage(message, frame));
            }
            if (level == Level.Fatal && Logger.IsFatalEnabled)
            {
                Logger.Fatal(LoggerMessage.CreateMessage(message, frame));
            }
            if (level == Level.Warn && Logger.IsWarnEnabled)
            {
                Logger.Warn(LoggerMessage.CreateMessage(message, frame));
            }
            if (level == Level.Info && Logger.IsInfoEnabled)
            {
                Logger.Info(LoggerMessage.CreateMessage(message, frame));
            }
        }
    }
}
