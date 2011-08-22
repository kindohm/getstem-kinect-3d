using System.Diagnostics;
using log4net;
using log4net.Core;

namespace GetSTEM.Model3DBrowser.Logging
{
    public abstract class DisposableLogWriter : IDisposableLogWriter
    {
        static ILog Logger = LogManager.GetLogger("AvtexRoutingLog");
        Level level;

        public DisposableLogWriter(Level level)
        {
            this.level = level;
            WriteMessage(string.Format("{0}", "BEGIN"));
        }

        public DisposableLogWriter(Level level, string message)
        {
            this.level = level;
            WriteMessage(string.Format("{0}: {1}", "BEGIN", message));
        }

        public void WriteMessage(string message)
        {
            var st = new StackTrace(true);
            var frame = st.GetFrame(1); // Gets the frame that called this function
            var method = frame.GetMethod();
            var messageText = string.Format("{0} - {1} - {2}", method.DeclaringType.FullName, method.Name, message);

            if (level == Level.Debug && Logger.IsDebugEnabled)
                Logger.Debug(messageText);
            if (level == Level.Info && Logger.IsInfoEnabled)
                Logger.Info(messageText);
            if (level == Level.Warn && Logger.IsWarnEnabled)
                Logger.Warn(messageText);
            if (level == Level.Fatal && Logger.IsFatalEnabled)
                Logger.Fatal(messageText);
            if (level == Level.Error && Logger.IsErrorEnabled)
                Logger.Error(messageText);
        }

        public void Dispose()
        {
            WriteMessage("END");
        }

    }
}
