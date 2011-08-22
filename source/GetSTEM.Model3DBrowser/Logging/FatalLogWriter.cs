using log4net.Core;

namespace GetSTEM.Model3DBrowser.Logging
{
    public class FatalLogWriter
    {
        public static void WriteMessage(string message)
        {
            LogWriterWrapper.WriteMessage(message, Level.Fatal);
        }
    }
}
