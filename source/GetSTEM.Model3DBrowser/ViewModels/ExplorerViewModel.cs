using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using _3DTools;
using Ab3d;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GetSTEM.Model3DBrowser.Services;
using Petzold.Media3D;
using GetSTEM.Model3DBrowser.Messages;
using GetSTEM.Model3DBrowser.Logging;

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
                this.RotationYAngle += rotationYAngleIncrement;
            }

            if (rightActive && !leftActive)
            {
                var diff = current.Y - previousX.Y;
                var rotationXAngleIncrement = rotationScaleFunction(diff, rotationScaleFactor);
                this.RotationXAngle -= rotationXAngleIncrement;
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
                DebugLogWriter.WriteMessage("stopping x rotation");
                this.rotationXMoving = false;
                return;
            }

            if (this.rotationYMoving & !this.rightHandCanMove)
            {
                // stop y rotation
                DebugLogWriter.WriteMessage("stopping y rotation");
                this.rotationYMoving = false;
                return;
            }

            if (!this.rotationXMoving & this.leftHandCanMove)
            {
                // start x rotation
                DebugLogWriter.WriteMessage("starting x rotation");
                this.previousX = new Point(e.LeftHandJoint.Position.X, e.LeftHandJoint.Position.Y);
                this.rotationXMoving = true;
                return;
            }

            if (!this.rotationYMoving & this.rightHandCanMove)
            {
                // start y rotation
                this.previousY = new Point(e.RightHandJoint.Position.X, e.RightHandJoint.Position.Y);
                DebugLogWriter.WriteMessage("Starting Y Rotation.");
                DebugLogWriter.WriteMessage("previousY: " + this.previousY.X + "," + this.previousY.Y);
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
                this.ConvertAllWireframesThickness(1d);
                this.EngineeringLineThickness = 1d;
            }
            else
            {
                this.stemView = false;
                this.ConvertAllModelsOpacity(1d);
                this.ConvertAllWireframesThickness(0);
                this.EngineeringLineThickness = 0;
            }
        }

        void ConvertAllWireframesThickness(double thickness)
        {
            foreach (var item in this.ScreenLinesCollection)
            {
                item.Thickness = thickness;
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
            if (this.IsInDesignMode)
            {
                var mesh = new CubeMesh();
                var model = new GeometryModel3D(mesh.Geometry,
                     new DiffuseMaterial(new SolidColorBrush(Colors.Red)));
                var group = new Transform3DGroup();
                group.Children.Add(new ScaleTransform3D(new Vector3D(20, 20, 20)));
                group.Children.Add(new RotateTransform3D(
                    new AxisAngleRotation3D(new Vector3D(1, 1, 0), 30)));
                model.Transform = group;
                this.Model3DGroup = new Model3DGroup();
                this.Model3DGroup.Children.Add(model);

                return;
            }

            this.ScreenLinesCollection.Clear();

            var model3DGroup = new Model3DGroup();
            var modelConfig = configService.GetModelConfiguration();

            foreach (var letter in modelConfig.LetterModels)
            {
                var reader = new Reader3ds();
                var thing = reader.ReadFile(letter.ModelPath);
                var internalModel = this.GetModelFromGroup(thing);

                if (internalModel == null)
                {
                    throw new InvalidOperationException("3D Model could not be found in 3DS file '" + letter.ModelPath + "'");
                }

                var mesh = (MeshGeometry3D)internalModel.Geometry;

                var brush = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString(letter.Color));
                var materialGroup = new MaterialGroup();
                var diffuse = new DiffuseMaterial(brush);
                var specular = new SpecularMaterial(new SolidColorBrush(Colors.White), 1000);
                materialGroup.Children.Add(diffuse);
                materialGroup.Children.Add(specular);

                var model = new GeometryModel3D(mesh, materialGroup);

                var transformGroup = new Transform3DGroup();

                var scale = new ScaleTransform3D(
                    new Vector3D(letter.Scale, letter.Scale, letter.Scale));

                var translate = new TranslateTransform3D(
                    new Vector3D(letter.OffsetX, letter.OffsetY, letter.OffsetZ));

                transformGroup.Children.Add(translate);
                transformGroup.Children.Add(scale);

                model.Transform = transformGroup;

                model3DGroup.Children.Add(model);

                var wireframe = new ScreenLines();

                for (var i = 0; i < mesh.Positions.Count; i++)
                {
                    wireframe.Points.Add(mesh.Positions[i]);
                    wireframe.Thickness = 0;
                    wireframe.Color = Colors.White;
                }

                wireframe.Transform = transformGroup;
                this.ScreenLinesCollection.Add(wireframe);
            }

            this.Model3DGroup = model3DGroup;
        }

        GeometryModel3D GetModelFromGroup(Model3DGroup group)
        {
            foreach (var child in group.Children)
            {
                if (child is GeometryModel3D)
                {
                    return (GeometryModel3D)child;
                }

                if (child is Model3DGroup)
                {
                    var result = GetModelFromGroup((Model3DGroup)child);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }

            return null;
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
