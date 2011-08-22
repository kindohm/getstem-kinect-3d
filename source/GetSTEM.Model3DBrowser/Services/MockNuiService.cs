using System;
using Microsoft.Research.Kinect.Nui;

namespace GetSTEM.Model3DBrowser.Services
{
    public class MockNuiService : INuiService
    {
        public void Shutdown() { }
        public ImageFrame LastVideoFrame { get; set; }
        public ImageFrame LastDepthFrame { get; set; }
        public double BoundsWidth { get; set; }
        public double BoundsDepth { get; set; }
        public double MinDistanceFromCamera { get; set; }
        public bool IsInConfigMode { get; set; }

        public event EventHandler<SkeletonUpdatedEventArgs> SkeletonUpdated;
        public event EventHandler<HandRaisedEventArgs> UserRaisedHand;
        public event EventHandler UserEnteredBounds;
        public event EventHandler UserExitedBounds;
    }
}
