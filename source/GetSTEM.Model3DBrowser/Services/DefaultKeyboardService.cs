using System;
using System.Windows;
using System.Windows.Input;

namespace GetSTEM.Model3DBrowser.Services
{
    public class DefaultKeyboardService : IKeyboardService
    {
        public DefaultKeyboardService()
        {
            if (Application.Current != null &&
                Application.Current.MainWindow != null)
            {
                Application.Current.MainWindow.KeyUp += new KeyEventHandler(MainWindow_KeyUp);
            }
        }

        void MainWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (this.KeyUp != null)
            {
                this.KeyUp(sender, e);
            }
        }



        public event EventHandler<KeyEventArgs> KeyUp;
    }
}
