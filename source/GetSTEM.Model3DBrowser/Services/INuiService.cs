using System;
using System.Windows.Media.Imaging;

namespace GetSTEM.Model3DBrowser.Services
{
    public interface INuiService
    {
        BitmapSource LastDepthBitmap { get; set; }
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
