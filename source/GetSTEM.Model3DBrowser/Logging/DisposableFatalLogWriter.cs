using log4net.Core;

namespace GetSTEM.Model3DBrowser.Logging
{
    public class DisposableFatalLogWriter : DisposableLogWriter
    {
        public DisposableFatalLogWriter()
            : base(Level.Fatal)
        {
        }

        public DisposableFatalLogWriter(string message):base(Level.Fatal, message)
        {
        }
    }
}
