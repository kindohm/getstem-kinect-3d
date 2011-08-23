using System;
using System.Linq;
using System.Windows;
using Coding4Fun.Kinect.Wpf;
using GetSTEM.Model3DBrowser.Logging;
using Microsoft.Research.Kinect.Nui;
using System.Collections.Generic;

namespace GetSTEM.Model3DBrowser.Services
{
    public class KinectNuiService : INuiService
    {
        const int Zero = 0;
        const float SkeletonMaxX = 0.60f;
        const float SkeletonMaxY = 0.40f;
        const double RaiseHandMilliseconds = 1500;

        bool initialized;
        int currentTrackingId;
        Runtime runtime;
        Dictionary<JointID, bool> handsRaising;
        Dictionary<JointID, DateTime> handsRaisingStart;
        Dictionary<JointID, bool> handsWaitingToLower;

        public KinectNuiService()
        {
            try
            {
                this.handsRaisingStart = new Dictionary<JointID, DateTime>();
                this.handsRaising = new Dictionary<JointID, bool>();
                this.handsWaitingToLower = new Dictionary<JointID, bool>();
                this.BoundsWidth = .5d;
                this.BoundsDepth = .5d;
                this.MinDistanceFromCamera = 1.0d;
                this.runtime = new Runtime();
                this.runtime.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(runtime_SkeletonFrameReady);
                this.runtime.VideoFrameReady += new EventHandler<ImageFrameReadyEventArgs>(runtime_VideoFrameReady);
                this.runtime.DepthFrameReady += new EventHandler<ImageFrameReadyEventArgs>(runtime_DepthFrameReady);
                this.runtime.Initialize(RuntimeOptions.UseSkeletalTracking | RuntimeOptions.UseColor | RuntimeOptions.UseDepthAndPlayerIndex);
                this.initialized = true;

                this.runtime.SkeletonEngine.TransformSmooth = true;
                var parameters = new TransformSmoothParameters();
                parameters.Smoothing = 0.7f;
                parameters.Correction = 0.9f;
                parameters.Prediction = 0.5f;
                parameters.JitterRadius = 0.5f;
                parameters.MaxDeviationRadius = 0.5f;
                runtime.SkeletonEngine.SmoothParameters = parameters;

                runtime.VideoStream.Open(ImageStreamType.Video, 2,
                   ImageResolution.Resolution640x480, ImageType.Color);

                runtime.DepthStream.Open(ImageStreamType.Depth, 2,
                    ImageResolution.Resolution320x240, ImageType.DepthAndPlayerIndex);


                DebugLogWriter.WriteMessage("Kinect initialized.");
            }
            catch (InvalidOperationException)
            {
                ErrorLogWriter.WriteMessage("Kinect not connected, or there was a device problem.");
            }
        }

        public ImageFrame LastVideoFrame { get; set; }
        public ImageFrame LastDepthFrame { get; set; }
        public double BoundsWidth { get; set; }
        public double BoundsDepth { get; set; }
        public double MinDistanceFromCamera { get; set; }
        public bool IsInConfigMode { get; set; }

        bool userIsInRange;
        public bool UserIsInRange
        {
            get { return this.userIsInRange; }
            set
            {
                var oldValue = this.userIsInRange;
                if (value != oldValue)
                {
                    if (this.userIsInRange)
                    {
                        this.userIsInRange = value;
                        InfoLogWriter.WriteMessage("User exited bounds.");
                        if (this.UserExitedBounds != null)
                        {
                            this.UserExitedBounds(this, EventArgs.Empty);
                        }
                    }
                    else
                    {
                        this.userIsInRange = value;
                        InfoLogWriter.WriteMessage("User entered bounds.");
                        if (this.UserEnteredBounds != null)
                        {
                            this.UserEnteredBounds(this, EventArgs.Empty);
                        }
                    }
                }
            }
        }

        void runtime_DepthFrameReady(object sender, ImageFrameReadyEventArgs e)
        {
            this.LastDepthFrame = e.ImageFrame;
        }

        void runtime_VideoFrameReady(object sender, ImageFrameReadyEventArgs e)
        {
            this.LastVideoFrame = e.ImageFrame;
        }

        void runtime_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            var skeletons = e.SkeletonFrame.Skeletons.Where(s =>
                s.TrackingState == SkeletonTrackingState.Tracked);

            var count = skeletons.Count();

            if (count == Zero)
            {
                this.currentTrackingId = int.MinValue;
                return;
            }

            var trackedSkeleton =
                skeletons.Where(s => s.TrackingID == this.currentTrackingId).FirstOrDefault();

            if (trackedSkeleton == null) // new user
            {
                trackedSkeleton = skeletons.FirstOrDefault();
                this.currentTrackingId = trackedSkeleton.TrackingID;
                InfoLogWriter.WriteMessage("Started tracking new user with ID '" + this.currentTrackingId.ToString() + "'");
            }

            this.UserIsInRange = this.GetUserIsInRange(trackedSkeleton.Joints[JointID.Spine]);

            if (this.UserIsInRange || this.IsInConfigMode)
            {
                this.ProcessSkeleton(trackedSkeleton);
            }
        }

        bool GetUserIsInRange(Joint torso)
        {
            var torsoPosition = torso.Position;
            return torsoPosition.Z > this.MinDistanceFromCamera &
                torsoPosition.Z < (this.MinDistanceFromCamera + this.BoundsDepth)
                & torsoPosition.X > -this.BoundsWidth / 2 &
                torsoPosition.X < this.BoundsWidth / 2;
        }

        void ProcessSkeleton(SkeletonData skeleton)
        {
            this.CheckRaisedHand(
                skeleton.Joints[JointID.HandRight],
                skeleton.Joints[JointID.Head]);

            this.CheckRaisedHand(
                skeleton.Joints[JointID.HandLeft],
                skeleton.Joints[JointID.Head]);

            this.BroadcastSkeletonPositions(
                skeleton.Joints[JointID.HandRight],
                skeleton.Joints[JointID.HandLeft],
                skeleton.Joints[JointID.Spine],
                skeleton.Joints[JointID.Head]);
        }

        void BroadcastSkeletonPositions(
            Joint rightHandJoint,
            Joint leftHandJoint,
            Joint torsoJoint,
            Joint headJoint)
        {
            if (this.SkeletonUpdated != null)
            {
                //Application.Current.Dispatcher.BeginInvoke(
                //    new Action(() =>
                //    {
                        this.SkeletonUpdated(this, new SkeletonUpdatedEventArgs()
                        {
                            RightHandJoint = rightHandJoint,
                            LeftHandJoint = leftHandJoint,
                            TorsoJoint = torsoJoint,
                            HeadJoint = headJoint
                        });
                    //}));
            }
        }

        void CheckRaisedHand(Joint hand, Joint head)
        {
            var id = hand.ID;
            if (!this.handsRaising.ContainsKey(id))
            {
                this.handsRaising.Add(id, false);
            }

            if (!this.handsWaitingToLower.ContainsKey(id))
            {
                this.handsWaitingToLower.Add(id, false);
            }

            if (!this.handsRaisingStart.ContainsKey(id))
            {
                this.handsRaisingStart.Add(id, DateTime.MinValue);
            }

            // user started raising their hand
            if (!this.handsRaising[id] & hand.Position.Y >= head.Position.Y)
            {
                this.handsRaisingStart[id] = DateTime.Now;
                this.handsRaising[id] = true;
                return;
            }

            var elapsed = DateTime.Now - this.handsRaisingStart[id];

            // user lowered their hand before the time limit
            if (this.handsRaising[id] & hand.Position.Y < head.Position.Y & elapsed.TotalMilliseconds < RaiseHandMilliseconds)
            {
                this.handsRaising[id] = false;
                return;
            }

            // user kept their hand raised until the time limit
            if (this.handsRaising[id] & elapsed.TotalMilliseconds > RaiseHandMilliseconds & !this.handsWaitingToLower[id])
            {
                this.handsWaitingToLower[id] = true;
                if (this.UserRaisedHand != null)
                {
                    DebugLogWriter.WriteMessage("User raised hand with ID '" + id.ToString() + "'");
                    this.UserRaisedHand(this, new HandRaisedEventArgs() { JointId = id });
                }
                return;
            }

            // user lowered their hand after a "raise" was recorded.
            if (this.handsRaising[id] & this.handsWaitingToLower[id] & hand.Position.Y < head.Position.Y)
            {
                this.handsWaitingToLower[id] = false;
                this.handsRaising[id] = false;
                return;
            }
        }

        public void Shutdown()
        {
            if (this.runtime != null)
            {
                this.runtime.SkeletonFrameReady -= runtime_SkeletonFrameReady;
                if (this.initialized)
                {
                    this.runtime.Uninitialize();
                    InfoLogWriter.WriteMessage("Kinect runtime uninitialized.");
                    this.runtime = null;
                }
            }
        }

        public event EventHandler<SkeletonUpdatedEventArgs> SkeletonUpdated;
        public event EventHandler<HandRaisedEventArgs> UserRaisedHand;
        public event EventHandler UserEnteredBounds;
        public event EventHandler UserExitedBounds;
    }
}
