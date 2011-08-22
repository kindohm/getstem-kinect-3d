using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace GetSTEM.Model3DBrowser.Services
{
    public interface IKeyboardService
    {
        event EventHandler<KeyEventArgs> KeyUp;
    }
}
