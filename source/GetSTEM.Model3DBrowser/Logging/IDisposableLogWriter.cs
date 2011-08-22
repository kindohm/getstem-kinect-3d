using System;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace GetSTEM.Model3DBrowser.Logging
{
    public interface IDisposableLogWriter : IDisposable
    {
        void WriteMessage(string message);
    }
}
