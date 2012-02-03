using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Imaging;
using Coding4Fun.Kinect.Wpf;
using GetSTEM.Model3DBrowser.Logging;
using Microsoft.Kinect;

namespace GetSTEM.Model3DBrowser.Services
{
    public class KinectNuiService : INuiService
    {
        const int Zero = 0;
        const int Two = 2;
        const float SkeletonMaxX = 0.60f;
        const float SkeletonMaxY = 0.40f;
        const double RaiseHandMilliseconds = 1500;

        bool initialized;
        int currentTrackingId;
        KinectSensor sensor;
        Dictionary<JointType, bool> handsRaising;
        Dictionary<JointType, DateTime> handsRaisingStart;
        Dictionary<JointType, bool> handsWaitingToLower;
        SkeletonFrame skeletonFrame;
        Skeleton[] sensorSkeletons = new Skeleton[6];

        public KinectNuiService()
        {
            if (KinectSensor.KinectSensors.Count == 0)
            {
                return;
            }

            this.handsRaisingStart = new Dictionary<JointType, DateTime>();
            this.handsRaising = new Dictionary<JointType, bool>();
            this.handsWaitingToLower = new Dictionary<JointType, bool>();
            this.BoundsWidth = .5d;
            this.BoundsDepth = .5d;
            this.MinDistanceFromCamera = 1.0d;
            this.sensor = KinectSensor.KinectSensors[0];
            this.sensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(sensor_AllFramesReady);
            this.initialized = true;

            var parameters = new TransformSmoothParameters();
            parameters.Smoothing = 0.7f;
            parameters.Correction = 0.9f;
            parameters.Prediction = 0.5f;
            parameters.JitterRadius = 0.5f;
            parameters.MaxDeviationRadius = 0.5f;

            this.sensor.SkeletonStream.Enable(parameters);
            this.sensor.SkeletonStream.Enable();
            this.sensor.DepthStream.Enable(DepthImageFormat.Resolution320x240Fps30);

            this.sensor.Start();
            DebugLogWriter.WriteMessage("Kinect initialized.");

        }

        public BitmapSource LastDepthBitmap { get; set; }
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

        void sensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {

            using (this.skeletonFrame = e.OpenSkeletonFrame())
            {
                if (this.skeletonFrame == null)
                {
                    return;
                }

                var depthFrame = e.OpenDepthImageFrame();
                if (depthFrame != null)
                {
                    this.LastDepthBitmap = depthFrame.ToBitmapSource();
                    depthFrame.Dispose();
                }

                this.skeletonFrame.CopySkeletonDataTo(this.sensorSkeletons);

                var trackedSkeletons = this.sensorSkeletons.Where(s =>
                    s.TrackingState == SkeletonTrackingState.Tracked);

                var count = trackedSkeletons.Count();

                if (count == Zero)
                {
                    this.currentTrackingId = int.MinValue;
                    return;
                }

                var trackedSkeleton =
                    trackedSkeletons.Where(s => s.TrackingId == this.currentTrackingId).FirstOrDefault();

                if (trackedSkeleton == null) // new user
                {
                    trackedSkeleton = trackedSkeletons.FirstOrDefault();
                    this.currentTrackingId = trackedSkeleton.TrackingId;
                    InfoLogWriter.WriteMessage("Started tracking new user with ID '" + this.currentTrackingId.ToString() + "'");
                }

                this.UserIsInRange = this.GetUserIsInRange(trackedSkeleton.Joints[JointType.Spine]);

                if (this.UserIsInRange || this.IsInConfigMode)
                {
                    this.ProcessSkeleton(trackedSkeleton);
                }
            }


        }

        //void runtime_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        //{
        //}

        //void runtime_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        //{
        //}

        bool GetUserIsInRange(Joint torso)
        {
            var torsoPosition = torso.Position;
            return torsoPosition.Z > this.MinDistanceFromCamera &&
                torsoPosition.Z < (this.MinDistanceFromCamera + this.BoundsDepth) &&
                torsoPosition.X > -this.BoundsWidth / Two &&
                torsoPosition.X < this.BoundsWidth / Two;
        }

        void ProcessSkeleton(Skeleton skeleton)
        {
            this.CheckRaisedHand(
                skeleton.Joints[JointType.HandRight],
                skeleton.Joints[JointType.Head]);

            this.CheckRaisedHand(
                skeleton.Joints[JointType.HandLeft],
                skeleton.Joints[JointType.Head]);

            this.BroadcastSkeletonPositions(
                skeleton.Joints[JointType.HandRight],
                skeleton.Joints[JointType.HandLeft],
                skeleton.Joints[JointType.Spine],
                skeleton.Joints[JointType.Head]);
        }

        void BroadcastSkeletonPositions(
            Joint rightHandJoint,
            Joint leftHandJoint,
            Joint torsoJoint,
            Joint headJoint)
        {
            if (this.SkeletonUpdated != null)
            {
                this.SkeletonUpdated(this, new SkeletonUpdatedEventArgs()
                {
                    RightHandJoint = rightHandJoint,
                    LeftHandJoint = leftHandJoint,
                    TorsoJoint = torsoJoint,
                    HeadJoint = headJoint
                });
            }
        }

        void CheckRaisedHand(Joint hand, Joint head)
        {
            var id = hand.JointType;
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
            if (this.sensor != null)
            {
                //this.sensor.SkeletonFrameReady -= runtime_SkeletonFrameReady;
                this.sensor.AllFramesReady -= this.sensor_AllFramesReady;
                if (this.initialized)
                {
                    this.sensor.Stop();
                    InfoLogWriter.WriteMessage("Kinect runtime uninitialized.");
                    this.sensor = null;
                }
            }
        }

        public event EventHandler<SkeletonUpdatedEventArgs> SkeletonUpdated;
        public event EventHandler<HandRaisedEventArgs> UserRaisedHand;
        public event EventHandler UserEnteredBounds;
        public event EventHandler UserExitedBounds;
    }
}
