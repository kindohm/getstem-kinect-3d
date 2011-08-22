using System.Windows;
using GetSTEM.Model3DBrowser.Framework;

namespace GetSTEM.Model3DBrowser
{
    public partial class App : Application
    {
        protected override void OnExit(ExitEventArgs e)
        {
            ViewModelLocator.Cleanup();
            base.OnExit(e);
        }
    }
}
