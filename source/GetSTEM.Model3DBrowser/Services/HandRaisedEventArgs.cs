using System;
using Microsoft.Kinect;

namespace GetSTEM.Model3DBrowser.Services
{
    public class HandRaisedEventArgs : EventArgs
    {
        public JointType JointId { get; set; }
    }
}
