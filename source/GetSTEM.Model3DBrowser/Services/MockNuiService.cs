using System;
using Microsoft.Kinect;
using System.Windows.Media.Imaging;

namespace GetSTEM.Model3DBrowser.Services
{
    public class MockNuiService : INuiService
    {
        public void Shutdown() { }
        //public DepthImageFrame LastDepthFrame { get; set; }
        public BitmapSource LastDepthBitmap { get; set; }
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
