using log4net.Core;

namespace GetSTEM.Model3DBrowser.Logging
{
    public class DisposableInfoLogWriter : DisposableLogWriter
    {
        public DisposableInfoLogWriter()
            : base(Level.Info)
        {
        }

        public DisposableInfoLogWriter(string message):base(Level.Info, message)
        {
        }
    }
}
