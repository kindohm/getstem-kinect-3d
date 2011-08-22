using System;
using Microsoft.Research.Kinect.Nui;

namespace GetSTEM.Model3DBrowser.Services
{
    public class SkeletonUpdatedEventArgs : EventArgs
    {
        public Joint RightHandJoint { get; set; }
        public Joint LeftHandJoint { get; set; }
        public Joint ScaledRightHandJoint { get; set; }
        public Joint ScaledLeftHandJoint { get; set; }
        public Joint TorsoJoint { get; set; }
        public Joint HeadJoint { get; set; }
    }
}
