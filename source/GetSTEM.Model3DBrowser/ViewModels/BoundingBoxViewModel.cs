using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using GetSTEM.Model3DBrowser.Services;
using GetSTEM.Model3DBrowser.Logging;
using System.Windows.Media;

namespace GetSTEM.Model3DBrowser.ViewModels
{
    public class BoundingBoxViewModel : ViewModelBase
    {
        INuiService nuiService;

        public BoundingBoxViewModel(INuiService nuiService)
        {
            this.nuiService = nuiService;
            this.nuiService.SkeletonUpdated += new EventHandler<SkeletonUpdatedEventArgs>(nuiService_SkeletonUpdated);
            this.nuiService.UserEnteredBounds += new EventHandler(nuiService_UserEnteredBounds);
            this.nuiService.UserExitedBounds += new EventHandler(nuiService_UserExitedBounds);
        }

        public const string TorsoOffsetZPropertyName = "TorsoOffsetZ";
        double torsoOffsetZ = 100d;
        public double TorsoOffsetZ
        {
            get
            {
                return torsoOffsetZ;
            }
            set
            {
                if (torsoOffsetZ == value)
                {
                    return;
                }
                var oldValue = torsoOffsetZ;
                torsoOffsetZ = value;
                RaisePropertyChanged(TorsoOffsetZPropertyName);
            }
        }

        public const string TorsoOffsetXPropertyName = "TorsoOffsetX";
        double torsoOffsetX = 100d;
        public double TorsoOffsetX
        {
            get
            {
                return torsoOffsetX;
            }
            set
            {
                if (torsoOffsetX == value)
                {
                    return;
                }
                var oldValue = torsoOffsetX;
                torsoOffsetX = value;
                RaisePropertyChanged(TorsoOffsetXPropertyName);
            }
        }

        public const string BoundsDisplaySizePropertyName = "BoundsDisplaySize";
        double boundsDisplaySize = 300d;
        public double BoundsDisplaySize
        {
            get
            {
                return boundsDisplaySize;
            }
            set
            {
                if (boundsDisplaySize == value)
                {
                    return;
                }
                var oldValue = boundsDisplaySize;
                boundsDisplaySize = value;
                RaisePropertyChanged(BoundsDisplaySizePropertyName);
            }
        }

        public const string BoundsWidthPropertyName = "BoundsWidth";
        double boundsWidth = .5d;
        public double BoundsWidth
        {
            get
            {
                return boundsWidth;
            }
            set
            {
                if (boundsWidth == value)
                {
                    return;
                }
                var oldValue = boundsWidth;
                boundsWidth = value;
                this.nuiService.BoundsWidth = boundsWidth;
                RaisePropertyChanged(BoundsWidthPropertyName);
            }
        }

        public const string BoundsDepthPropertyName = "BoundsDepth";
        double boundsDepth = .5d;
        public double BoundsDepth
        {
            get
            {
                return boundsDepth;
            }
            set
            {
                if (boundsDepth == value)
                {
                    return;
                }
                var oldValue = boundsDepth;
                boundsDepth = value;
                this.nuiService.BoundsDepth = boundsDepth;
                RaisePropertyChanged(BoundsDepthPropertyName);
            }
        }

        public const string MinDistanceFromCameraPropertyName = "MinDistanceFromCamera";
        double minDistanceFromCamera = 1.35d;
        public double MinDistanceFromCamera
        {
            get
            {
                return minDistanceFromCamera;
            }
            set
            {
                if (minDistanceFromCamera == value)
                {
                    return;
                }
                var oldValue = minDistanceFromCamera;
                minDistanceFromCamera = value;
                this.nuiService.MinDistanceFromCamera = minDistanceFromCamera;
                RaisePropertyChanged(MinDistanceFromCameraPropertyName);
            }
        }

        public const string UserPointColorPropertyName = "UserPointColor";
        Color userPointColor = Colors.Red;
        public Color UserPointColor
        {
            get
            {
                return userPointColor;
            }
            set
            {
                if (userPointColor == value)
                {
                    return;
                }
                var oldValue = userPointColor;
                userPointColor = value;
                RaisePropertyChanged(UserPointColorPropertyName);
            }
        }

        void nuiService_SkeletonUpdated(object sender, SkeletonUpdatedEventArgs e)
        {
            this.TorsoOffsetX =
                           (this.BoundsDisplaySize / 2) * e.TorsoJoint.Position.X / (this.BoundsWidth / 2);
            this.TorsoOffsetZ = (this.BoundsDisplaySize / 2) * (e.TorsoJoint.Position.Z
                - (this.MinDistanceFromCamera + this.BoundsDepth / 2)) / (this.BoundsDepth / 2);
        }

        void nuiService_UserExitedBounds(object sender, EventArgs e)
        {
            this.UserPointColor = Color.FromArgb(255, 255, 0, 0);
        }

        void nuiService_UserEnteredBounds(object sender, EventArgs e)
        {
            this.UserPointColor = Color.FromArgb(255, 0, 255, 0);
        }

        public override void Cleanup()
        {
            this.nuiService.SkeletonUpdated -= nuiService_SkeletonUpdated;
            base.Cleanup();
        }
    }
}
