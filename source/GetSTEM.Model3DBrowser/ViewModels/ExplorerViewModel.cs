using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using _3DTools;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using GetSTEM.Model3DBrowser.Logging;
using GetSTEM.Model3DBrowser.Messages;
using GetSTEM.Model3DBrowser.Services;

namespace GetSTEM.Model3DBrowser.ViewModels
{
    public class ExplorerViewModel : ViewModelBase
    {
        const double MinZoom = 3;
        const double MaxZoom = 500;
        const float MinTorsoDistance = .4f;

        bool userIsInRange;
        bool leftHandCanMove;
        bool rightHandCanMove;
        bool canZoom;
        bool autoPlay;
        bool stemView;
        bool rotationXMoving;
        bool rotationYMoving;
        bool zooming;
        Point current;
        Point previousY;
        Point previousX;
        IConfigurationService configService;
        INuiService nuiService;
        FrameworkElement eventSource;

        public ExplorerViewModel(INuiService nuiService, IConfigurationService configService)
        {
            this.ScreenLinesCollection = new List<ScreenLines>();

            this.nuiService = nuiService;
            this.configService = configService;
            this.MouseRotationAdjustment = 5d;
            this.MouseZoomAdjustment = 5d;
            this.MetersRotationAdjustment = 200d;
            this.MetersZoomAdjustment = 400d;

            Messenger.Default.Register<AutoPlayMessage>(this, this.ReceiveAutoPlayMessage);
            Messenger.Default.Register<ToggleMessage>(this, this.ReceiveToggleMessage);
            this.nuiService.SkeletonUpdated +=new EventHandler<SkeletonUpdatedEventArgs>(nuiService_SkeletonUpdated);
            this.nuiService.UserEnteredBounds +=new EventHandler(nuiService_UserEnteredBounds);
            this.nuiService.UserExitedBounds +=new EventHandler(nuiService_UserExitedBounds);
            this.LoadModel3DGroup();

        }

        public double MouseRotationAdjustment { get; set; }
        public double MouseZoomAdjustment { get; set; }
        public double MetersRotationAdjustment { get; set; }
        public double MetersZoomAdjustment { get; set; }
        public List<ScreenLines> ScreenLinesCollection { get; private set; }
        public Model3DGroup Model3DGroup { get; set; }

        public FrameworkElement EventSource
        {
            get
            {
                return this.eventSource;
            }
            set
            {
                this.eventSource = value;
                this.eventSource.MouseMove += new MouseEventHandler(eventSource_MouseMove);
            }
        }
        public const string CurrentCursorXPropertyName = "CursorX";
        double currentCursorX = 0;
        public double CurrentCursorX
        {
            get
            {
                return currentCursorX;
            }
            set
            {
                if (currentCursorX == value)
                {
                    return;
                }
                var oldValue = currentCursorX;
                currentCursorX = value;
                RaisePropertyChanged(CurrentCursorXPropertyName);
            }
        }

        public const string CurrentCursorYPropertyName = "CursorY";
        double currentCursorY = 0;
        public double CurrentCursorY
        {
            get
            {
                return currentCursorY;
            }
            set
            {
                if (currentCursorY == value)
                {
                    return;
                }
                var oldValue = currentCursorY;
                currentCursorY = value;
                RaisePropertyChanged(CurrentCursorYPropertyName);
            }
        }

        public const string RotationXAnglePropertyName = "RotationXAngle";
        double rotationXAngle = 0;
        public double RotationXAngle
        {
            get
            {
                return rotationXAngle;
            }
            set
            {
                if (rotationXAngle == value)
                {
                    return;
                }
                var oldValue = rotationXAngle;
                rotationXAngle = value;
                RaisePropertyChanged(RotationXAnglePropertyName);
            }
        }

        public const string RotationYAnglePropertyName = "RotationYAngle";
        double rotationYAngle = 0;
        public double RotationYAngle
        {
            get
            {
                return rotationYAngle;
            }
            set
            {
                if (rotationYAngle == value)
                {
                    return;
                }
                var oldValue = rotationYAngle;
                rotationYAngle = value;
                RaisePropertyChanged(RotationYAnglePropertyName);
            }
        }

        public const string CameraZOffsetPropertyName = "CameraZOffset";
        double cameraZOffset = 200;
        public double CameraZOffset
        {
            get
            {
                return cameraZOffset;
            }
            set
            {
                if (cameraZOffset == value)
                {
                    return;
                }
                var oldValue = cameraZOffset;
                cameraZOffset = value;
                RaisePropertyChanged(CameraZOffsetPropertyName);
            }
        }

        public const string EngineeringLineThicknessPropertyName = "EngineeringLineThickness";
        double engineeringLineThickness = 0;
        public double EngineeringLineThickness
        {
            get
            {
                return engineeringLineThickness;
            }
            set
            {
                if (engineeringLineThickness == value)
                {
                    return;
                }
                var oldValue = engineeringLineThickness;
                engineeringLineThickness = value;
                RaisePropertyChanged(EngineeringLineThicknessPropertyName);
            }
        }

        void nuiService_SkeletonUpdated(object sender, SkeletonUpdatedEventArgs e)
        {
            if (this.userIsInRange)
            {
                this.ProcessHandControl(e);
            }
        }

        void nuiService_UserExitedBounds(object sender, EventArgs e)
        {
            InfoLogWriter.WriteMessage("ExplorerViewModel: User exited bounds.");
            this.userIsInRange = false;
        }

        void nuiService_UserEnteredBounds(object sender, EventArgs e)
        {
            InfoLogWriter.WriteMessage("ExplorerViewModel: User entered bounds.");
            this.userIsInRange = true;
        }
        
        void eventSource_MouseMove(object sender, MouseEventArgs e)
        {
            this.HandleMouseMovement(
                e.GetPosition(this.EventSource),
                e.LeftButton == MouseButtonState.Pressed,
                e.RightButton == MouseButtonState.Pressed);
        }

        void HandleMouseMovement(Point point, bool leftActive, bool rightActive)
        {
            this.HandleMovement(point, leftActive, rightActive, this.MouseRotationScaleFunction, this.MouseRotationAdjustment,
                this.MouseZoomScaleFunction, this.MouseZoomAdjustment, this.MouseZoomDifferenceFunction);
        }

        void HandleMeterMovement(Point point, bool leftActive, bool rightActive)
        {
            this.HandleMovement(point, leftActive, rightActive, this.MetersRotationScaleFunction, this.MetersRotationAdjustment,
                this.MetersZoomScaleFunction, this.MetersZoomAdjustment, this.MetersZoomDifferenceFunction);
        }

        double MouseRotationScaleFunction(double diff, double scale)
        {
            return diff / scale;
        }

        double MetersRotationScaleFunction(double diff, double scale)
        {
            return diff * scale;
        }

        double MouseZoomScaleFunction(double diff, double scale)
        {
            return diff / scale;
        }

        double MetersZoomScaleFunction(double diff, double scale)
        {
            return -diff * scale;
        }

        double MouseZoomDifferenceFunction(Point current, Point previous)
        {
            return current.Y - previous.Y;
        }

        double MetersZoomDifferenceFunction(Point current, Point previous)
        {
            return current.X - previous.X;
        }

        void HandleMovement(
            Point point,
            bool leftActive,
            bool rightActive,
            Func<double, double, double> rotationScaleFunction,
            double rotationScaleFactor,
            Func<double, double, double> zoomScaleFunction,
            double zoomScaleFactor,
            Func<Point, Point, double> zoomDifferenceFunction)
        {
            this.current = point;
            this.CurrentCursorX = current.X;
            this.CurrentCursorY = current.Y;

            if (leftActive && !rightActive)
            {
                var diff = current.X - previousY.X;
                var rotationYAngleIncrement = rotationScaleFunction(diff, rotationScaleFactor);
                var previousYAngle = this.RotationYAngle;
                this.RotationYAngle += rotationYAngleIncrement;
                var message = new EquationMessage()
                {
                    Increment = rotationYAngleIncrement,
                    Current = current.X,
                    Previous = previousY.X,
                    CurrentAngle = this.RotationYAngle,
                    Equation = Equation.XRotation,
                    Scale = rotationScaleFactor,
                    PreviousAngle = previousYAngle
                };
                Messenger.Default.Send<EquationMessage>(message);
            }

            if (rightActive && !leftActive)
            {
                var diff = current.Y - previousX.Y;
                var rotationXAngleIncrement = rotationScaleFunction(diff, rotationScaleFactor);
                var previousXAngle = this.RotationXAngle;
                this.RotationXAngle -= rotationXAngleIncrement;
                var message = new EquationMessage()
                {
                    Increment = rotationXAngleIncrement,
                    Current = current.Y,
                    Previous = previousX.Y,
                    CurrentAngle = this.RotationXAngle,
                    Equation = Equation.YRotation,
                    Scale = rotationScaleFactor,
                    PreviousAngle = previousXAngle
                };
                Messenger.Default.Send<EquationMessage>(message);
            }

            if (rightActive & leftActive)
            {
                var diff = zoomDifferenceFunction(this.current, this.previousX);
                var actual = zoomScaleFunction(diff, zoomScaleFactor);
                var result = this.CameraZOffset + actual;
                if (result > MinZoom & result < MaxZoom)
                {
                    this.CameraZOffset = result;
                }
            }

            previousX = current;
            previousY = current;
        }

        void ProcessHandControl(SkeletonUpdatedEventArgs e)
        {
            this.leftHandCanMove = e.LeftHandJoint.Position.Y < e.HeadJoint.Position.Y &&
                e.TorsoJoint.Position.Z - e.LeftHandJoint.Position.Z >= MinTorsoDistance &&
                e.TorsoJoint.Position.Z - e.RightHandJoint.Position.Z < MinTorsoDistance;

            this.rightHandCanMove = e.RightHandJoint.Position.Y < e.HeadJoint.Position.Y &&
                e.TorsoJoint.Position.Z - e.RightHandJoint.Position.Z >= MinTorsoDistance &&
                e.TorsoJoint.Position.Z - e.LeftHandJoint.Position.Z < MinTorsoDistance;

            this.canZoom = e.LeftHandJoint.Position.Y < e.HeadJoint.Position.Y &&
                e.RightHandJoint.Position.Y < e.HeadJoint.Position.Y &&
                e.TorsoJoint.Position.Z - e.LeftHandJoint.Position.Z >= MinTorsoDistance &&
                e.TorsoJoint.Position.Z - e.RightHandJoint.Position.Z >= MinTorsoDistance;

            if (this.rotationXMoving & !this.leftHandCanMove)
            {
                // stop X rotation
                this.rotationXMoving = false;
                return;
            }

            if (this.rotationYMoving & !this.rightHandCanMove)
            {
                // stop y rotation
                this.rotationYMoving = false;
                return;
            }

            if (!this.rotationXMoving & this.leftHandCanMove)
            {
                // start x rotation
                this.previousX = new Point(e.LeftHandJoint.Position.X, e.LeftHandJoint.Position.Y);
                this.rotationXMoving = true;
                return;
            }

            if (!this.rotationYMoving & this.rightHandCanMove)
            {
                // start y rotation
                this.previousY = new Point(e.RightHandJoint.Position.X, e.RightHandJoint.Position.Y);
                this.rotationYMoving = true;
                return;
            }

            if (this.rotationXMoving & this.leftHandCanMove)
            {
                // continue x rotation
                this.HandleMeterMovement(
                    new Point(e.LeftHandJoint.Position.X, e.LeftHandJoint.Position.Y),
                    false,
                    true); // right and left are inversed
                return;
            }

            if (this.rotationYMoving & this.rightHandCanMove)
            {
                // continue y rotation
                this.HandleMeterMovement(
                    new Point(e.RightHandJoint.Position.X, e.RightHandJoint.Position.Y),
                    true,
                    false); // right and left are inversed
                return;
            }

            if (this.zooming & !this.canZoom)
            {
                // stop zooming
                this.zooming = false;
                return;
            }

            if (!this.zooming & this.canZoom)
            {
                // start zooming
                this.zooming = true;
                this.previousY = this.previousY = new Point(e.RightHandJoint.Position.X, e.RightHandJoint.Position.Y);
                return;
            }

            if (this.zooming & this.canZoom)
            {
                // continue zooming
                this.HandleMeterMovement(
                    new Point(e.RightHandJoint.Position.X, e.RightHandJoint.Position.Y),
                    true,
                    true);
            }
        }

        void ResetModel()
        {
            this.CameraZOffset = 200;
            this.RotationXAngle = 0;
            this.RotationYAngle = 0;
        }
        
        void ReceiveAutoPlayMessage(AutoPlayMessage message)
        {
            if (!this.autoPlay)
            {
                this.ResetModel();
                this.autoPlay = true;
                Messenger.Default.Send<StartAutoPlayMessage>(
                    new StartAutoPlayMessage());
            }
            else
            {
                this.autoPlay = false;
                Messenger.Default.Send<StopAutoPlayMessage>(
                    new StopAutoPlayMessage());
                this.ResetModel();
            }
        }

        void ReceiveToggleMessage(ToggleMessage message)
        {
            if (!stemView)
            {
                this.stemView = true;
                this.ConvertAllModelsOpacity(0);
                this.EngineeringLineThickness = 1d;
            }
            else
            {
                this.stemView = false;
                this.ConvertAllModelsOpacity(1d);
                this.EngineeringLineThickness = 0;
            }
        }   

        void ConvertAllModelsOpacity(double opacity)
        {
            foreach (var item in this.Model3DGroup.Children)
            {
                var model = item as GeometryModel3D;
                var materialGroup = model.Material as MaterialGroup;
                var diffuse = materialGroup.Children[0] as DiffuseMaterial;
                var specular = materialGroup.Children[1] as SpecularMaterial;
                diffuse.Brush.Opacity = opacity;
                specular.Brush.Opacity = opacity;
            }
        }

        void LoadModel3DGroup()
        {
            var result = ModelLoader.LoadModel3DGroup(this.configService, this.IsInDesignMode);
            this.Model3DGroup = result.Model3DGroup;
            this.ScreenLinesCollection = result.Wireframes;
        }

        public override void Cleanup()
        {
            if (this.EventSource != null)
            {
                this.EventSource.MouseMove -= eventSource_MouseMove;
            }

            this.nuiService.SkeletonUpdated -= nuiService_SkeletonUpdated;
            this.nuiService.UserEnteredBounds -= nuiService_UserEnteredBounds;
            this.nuiService.UserExitedBounds -= nuiService_UserExitedBounds;
            base.Cleanup();
        }
    }
}
