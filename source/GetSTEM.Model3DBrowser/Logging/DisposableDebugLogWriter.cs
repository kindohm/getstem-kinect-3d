using log4net.Core;

namespace GetSTEM.Model3DBrowser.Logging
{
    public class DisposableDebugLogWriter : DisposableLogWriter
    {
        public DisposableDebugLogWriter()
            : base(Level.Debug)
        {
        }

        public DisposableDebugLogWriter(string message):base(Level.Debug, message)
        {
        }
    }
}
