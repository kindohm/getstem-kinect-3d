using log4net.Core;

namespace GetSTEM.Model3DBrowser.Logging
{
    public class DisposableErrorLogWriter : DisposableLogWriter
    {
        public DisposableErrorLogWriter()
            : base(Level.Error)
        {
        }

        public DisposableErrorLogWriter(string message):base(Level.Error, message)
        {
        }
    }
}
