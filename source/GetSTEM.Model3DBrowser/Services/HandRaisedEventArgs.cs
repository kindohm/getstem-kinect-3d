using System;
using Microsoft.Research.Kinect.Nui;

namespace GetSTEM.Model3DBrowser.Services
{
    public class HandRaisedEventArgs : EventArgs
    {
        public JointID JointId { get; set; }
    }
}
