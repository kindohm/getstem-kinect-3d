using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Coding4Fun.Kinect.Wpf;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GetSTEM.Model3DBrowser.Messages;
using GetSTEM.Model3DBrowser.Services;

namespace GetSTEM.Model3DBrowser.ViewModels
{
    public class MainViewModel : ViewModelBase
    {

        bool stemView;
        bool kinectVisionEnabled;
        IConfigurationService configService;
        INuiService nuiService;
        IKeyboardService keyboardService;
        DispatcherTimer videoTimer;

        public MainViewModel(IConfigurationService configService, INuiService nuiService, IKeyboardService keyboardService)
        {
            this.keyboardService = keyboardService;
            this.keyboardService.KeyUp += new EventHandler<KeyEventArgs>(keyboardService_KeyUp);
            this.configService = configService;
            this.nuiService = nuiService;
            this.nuiService.UserRaisedHand += new EventHandler<HandRaisedEventArgs>(nuiService_UserRaisedHand);
            this.nuiService.UserEnteredBounds += new EventHandler(nuiService_UserEnteredBounds);
            this.nuiService.UserExitedBounds += new EventHandler(nuiService_UserExitedBounds);
            this.ToggleCommand = new RelayCommand(this.ExecuteToggleCommand);
            this.AutoPlayCommand = new RelayCommand(this.ExecuteAutoPlayCommand);
            this.ToggleKinectVisionCommand = new RelayCommand(this.ExecuteToggleKinectVisionCommand);
            this.MainBackgroundBrush = (Brush)Application.Current.Resources["DefaultBackground"];
            this.EngineeringBackgroundBrush = new SolidColorBrush(Color.FromArgb(255, 0, 49, 83));

            this.videoTimer = new DispatcherTimer();
            this.videoTimer.Interval = TimeSpan.FromMilliseconds(50);
            this.videoTimer.Tick += new EventHandler(videoTimer_Tick);
            this.videoTimer.Start();

            if (!IsInDesignMode)
            {
                Application.Current.MainWindow.SizeChanged += new SizeChangedEventHandler(MainWindow_SizeChanged);
            }

        }

        #region " properties "

        public RelayCommand ToggleKinectVisionCommand { get; set; }
        public RelayCommand ToggleCommand { get; private set; }
        public RelayCommand AutoPlayCommand { get; set; }

        public const string ShowBoundingBoxPropertyName = "ShowBoundingBox";
        bool showBoundingBox = false;
        public bool ShowBoundingBox
        {
            get
            {
                return showBoundingBox;
            }
            set
            {
                if (showBoundingBox == value)
                {
                    return;
                }
                var oldValue = showBoundingBox;
                showBoundingBox = value;
                RaisePropertyChanged(ShowBoundingBoxPropertyName);
                this.BoundingBoxVisibility = this.showBoundingBox ? Visibility.Visible : Visibility.Collapsed;
                this.nuiService.IsInConfigMode = this.showBoundingBox;

                Messenger.Default.Send<BoundingBoxEnabledMessage>(
                    new BoundingBoxEnabledMessage() { Enabled = showBoundingBox });
            }
        }

        public const string BoundingBoxVisibilityPropertyName = "BoundingBoxVisibility";
        Visibility boundingBoxVisibility = Visibility.Collapsed;
        public Visibility BoundingBoxVisibility
        {
            get
            {
                return boundingBoxVisibility;
            }
            set
            {
                if (boundingBoxVisibility == value)
                {
                    return;
                }
                var oldValue = boundingBoxVisibility;
                boundingBoxVisibility = value;
                RaisePropertyChanged(BoundingBoxVisibilityPropertyName);
            }
        }

        public const string UserIsInRangePropertyName = "UserIsInRange";
        bool userIsInRange = false;
        public bool UserIsInRange
        {
            get
            {
                return userIsInRange;
            }
            set
            {
                if (userIsInRange == value)
                {
                    return;
                }
                var oldValue = userIsInRange;
                userIsInRange = value;
                RaisePropertyChanged(UserIsInRangePropertyName);
            }
        }       

        public const string ImageWidthPropertyName = "ImageWidth";
        double imageWidth = 200;
        public double ImageWidth
        {
            get
            {
                return imageWidth;
            }
            set
            {
                if (imageWidth == value)
                {
                    return;
                }
                var oldValue = imageWidth;
                imageWidth = value;
                RaisePropertyChanged(ImageWidthPropertyName);
            }
        }

        public const string KinectVisionVisibilityPropertyName = "KinectVisionVisibility";
        Visibility kinectVisionVisibility = Visibility.Collapsed;
        public Visibility KinectVisionVisibility
        {
            get
            {
                return kinectVisionVisibility;
            }
            set
            {
                if (kinectVisionVisibility == value)
                {
                    return;
                }
                var oldValue = kinectVisionVisibility;
                kinectVisionVisibility = value;
                RaisePropertyChanged(KinectVisionVisibilityPropertyName);
            }
        }

        public const string VideoBitmapSourcePropertyName = "VideoBitmapSource";
        BitmapSource videoBitmapSource = null;
        public BitmapSource VideoBitmapSource
        {
            get
            {
                return videoBitmapSource;
            }
            set
            {
                if (videoBitmapSource == value)
                {
                    return;
                }
                var oldValue = videoBitmapSource;
                videoBitmapSource = value;
                RaisePropertyChanged(VideoBitmapSourcePropertyName);
            }
        }

        public const string DepthBitmapSourcePropertyName = "DepthBitmapSource";
        BitmapSource depthBitmapSource = null;
        public BitmapSource DepthBitmapSource
        {
            get
            {
                return depthBitmapSource;
            }
            set
            {
                if (depthBitmapSource == value)
                {
                    return;
                }
                var oldValue = depthBitmapSource;
                depthBitmapSource = value;
                RaisePropertyChanged(DepthBitmapSourcePropertyName);
            }
        }

        public const string MouseFeatureVisibilityPropertyName = "MouseFeatureVisibility";
        Visibility mouseFeatureVisibility = Visibility.Collapsed;
        public Visibility MouseFeatureVisibility
        {
            get
            {
                return mouseFeatureVisibility;
            }
            set
            {
                if (mouseFeatureVisibility == value)
                {
                    return;
                }
                var oldValue = mouseFeatureVisibility;
                mouseFeatureVisibility = value;
                RaisePropertyChanged(MouseFeatureVisibilityPropertyName);
            }
        }

        public const string MainBackgroundBrushPropertyName = "MainBackgroundBrush";
        Brush mainBackgroundBrush = null;
        public Brush MainBackgroundBrush
        {
            get
            {
                return mainBackgroundBrush;
            }
            set
            {
                if (mainBackgroundBrush == value)
                {
                    return;
                }
                var oldValue = mainBackgroundBrush;
                mainBackgroundBrush = value;
                RaisePropertyChanged(MainBackgroundBrushPropertyName);
            }
        }

        public const string EngineeringBackgroundBrushPropertyName = "EngineeringBackgroundBrush";
        SolidColorBrush engineeringBackgroundBrush = null;
        public SolidColorBrush EngineeringBackgroundBrush
        {
            get
            {
                return engineeringBackgroundBrush;
            }
            set
            {
                if (engineeringBackgroundBrush == value)
                {
                    return;
                }
                var oldValue = engineeringBackgroundBrush;
                engineeringBackgroundBrush = value;
                RaisePropertyChanged(EngineeringBackgroundBrushPropertyName);
            }
        }

        public const string ToggleContentPropertyName = "ToggleContent";
        string toggleContent = "STEM View";
        public string ToggleContent
        {
            get
            {
                return toggleContent;
            }
            set
            {
                if (toggleContent == value)
                {
                    return;
                }
                var oldValue = toggleContent;
                toggleContent = value;
                RaisePropertyChanged(ToggleContentPropertyName);
            }
        }

        public const string MathVisibilityPropertyName = "MathVisibility";
        Visibility mathVisibility = Visibility.Collapsed;
        public Visibility MathVisibility
        {
            get
            {
                return mathVisibility;
            }
            set
            {
                if (mathVisibility == value)
                {
                    return;
                }
                var oldValue = mathVisibility;
                mathVisibility = value;
                RaisePropertyChanged(MathVisibilityPropertyName);
            }
        }

        #endregion

        void ExecuteAutoPlayCommand()
        {
            Messenger.Default.Send<AutoPlayMessage>(new AutoPlayMessage());
        }

        void ExecuteToggleCommand()
        {
            if (!stemView)
            {
                this.stemView = true;
                this.ToggleContent = "Normal View";
                this.MainBackgroundBrush = this.EngineeringBackgroundBrush;
                this.MathVisibility = Visibility.Visible;
            }
            else
            {
                this.stemView = false;
                this.ToggleContent = "STEM View";
                this.MainBackgroundBrush = (Brush)Application.Current.Resources["DefaultBackground"];
                this.MathVisibility = Visibility.Collapsed;
            }

            Messenger.Default.Send<ToggleMessage>(new ToggleMessage());
        }

        void ExecuteToggleKinectVisionCommand()
        {
            if (this.kinectVisionEnabled)
            {
                this.kinectVisionEnabled = false;
                this.KinectVisionVisibility = Visibility.Collapsed;
            }
            else
            {
                this.kinectVisionEnabled = true;
                this.KinectVisionVisibility = Visibility.Visible;
            }
        }

        //void nuiService_SkeletonUpdated(object sender, SkeletonUpdatedEventArgs e)
        //{
        //    if (this.UserIsInRange)
        //    {
        //        this.CheckRaisedHand(e);
        //    }
        //}

        void nuiService_UserExitedBounds(object sender, EventArgs e)
        {
            this.UserIsInRange = false;
        }

        void nuiService_UserEnteredBounds(object sender, EventArgs e)
        {
            this.UserIsInRange = true;
        }


        void keyboardService_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.M)
            {
                this.MouseFeatureVisibility = this.MouseFeatureVisibility == Visibility.Collapsed ?
                    Visibility.Visible : Visibility.Collapsed;
                return;
            }

            if (e.Key == Key.F11)
            {
                if (Application.Current.MainWindow.WindowStyle != WindowStyle.None)
                {
                    Application.Current.MainWindow.WindowState = WindowState.Maximized;
                    Application.Current.MainWindow.WindowStyle = WindowStyle.None;
                }
                else
                {
                    Application.Current.MainWindow.WindowState = WindowState.Normal;
                    Application.Current.MainWindow.WindowStyle = WindowStyle.SingleBorderWindow;
                }
                return;
            }

            if (e.Key == Key.Escape)
            {
                if (Application.Current.MainWindow.WindowStyle == WindowStyle.None)
                {
                    Application.Current.MainWindow.WindowState = WindowState.Normal;
                    Application.Current.MainWindow.WindowStyle = WindowStyle.SingleBorderWindow;
                }
                return;
            }

            if (e.Key == Key.K)
            {
                this.ExecuteToggleKinectVisionCommand();
                return;
            }

            if (e.Key == Key.S)
            {
                this.ExecuteToggleCommand();
                return;
            }

            if (e.Key == Key.B)
            {
                this.ShowBoundingBox = !this.showBoundingBox;
                return;
            }

        }

        void nuiService_UserRaisedHand(object sender, HandRaisedEventArgs e)
        {
            this.ExecuteToggleCommand();
        }

        void videoTimer_Tick(object sender, EventArgs e)
        {
            if (this.kinectVisionEnabled)
            {
                if (this.nuiService.LastVideoFrame != null)
                {
                    this.VideoBitmapSource = this.nuiService.LastVideoFrame.ToBitmapSource();
                }

                if (this.nuiService.LastDepthFrame != null)
                {
                    this.DepthBitmapSource = this.nuiService.LastDepthFrame.ToBitmapSource();
                }
            }
        }

        void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.ImageWidth = e.NewSize.Width / 5d;
        }

        public override void Cleanup()
        {

            this.keyboardService.KeyUp -= keyboardService_KeyUp;
            this.nuiService.UserRaisedHand -= nuiService_UserRaisedHand;
            this.nuiService.UserEnteredBounds -= nuiService_UserEnteredBounds;
            this.nuiService.UserExitedBounds -= nuiService_UserExitedBounds;
            base.Cleanup();
        }
    }
}