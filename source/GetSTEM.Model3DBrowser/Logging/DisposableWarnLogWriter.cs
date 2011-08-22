using log4net.Core;

namespace GetSTEM.Model3DBrowser.Logging
{
    public class DisposableWarnLogWriter : DisposableLogWriter
    {
        public DisposableWarnLogWriter()
            : base(Level.Warn)
        {
        }

        public DisposableWarnLogWriter(string message):base(Level.Warn, message)
        {
        }
    }
}
