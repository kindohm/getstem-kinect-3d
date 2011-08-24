using System;
using Microsoft.Research.Kinect.Nui;

namespace GetSTEM.Model3DBrowser.Services
{
    public interface INuiService
    {
        ImageFrame LastDepthFrame { get; set; }
        double BoundsWidth { get; set; }
        double BoundsDepth { get; set; }
        double MinDistanceFromCamera { get; set; }
        bool IsInConfigMode { get; set; }
        void Shutdown();

        event EventHandler<SkeletonUpdatedEventArgs> SkeletonUpdated;
        event EventHandler UserEnteredBounds;
        event EventHandler UserExitedBounds;
        event EventHandler<HandRaisedEventArgs> UserRaisedHand;
    }
}
