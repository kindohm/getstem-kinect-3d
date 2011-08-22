using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using GalaSoft.MvvmLight.Messaging;
using GetSTEM.Model3DBrowser.Messages;
using GetSTEM.Model3DBrowser.ViewModels;
using GetSTEM.Model3DBrowser.Logging;

namespace GetSTEM.Model3DBrowser.Views
{
    public partial class Explorer : UserControl
    {
        bool stemMode;

        public Explorer()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(Explorer_Loaded);
        }

        ExplorerViewModel ViewModel
        {
            get { return (ExplorerViewModel)this.DataContext; }
        }
        
        void Explorer_Loaded(object sender, RoutedEventArgs e)
        {
            this.ViewModel.EventSource = this.trackballEventSource;
            Messenger.Default.Register<ToggleMessage>(this, this.HandleToggleMessage);
            Messenger.Default.Register<StartAutoPlayMessage>(this, this.ReceiveStartAutoPlay);
            Messenger.Default.Register<StopAutoPlayMessage>(this, this.ReceiveStopAutoPlay);
        }

        void LoadWireframes()
        {
            var wireframes = this.ViewModel.ScreenLinesCollection;
            foreach (var wireframe in wireframes)
            {
                this.wireVisual.Children.Insert(0, wireframe);
            }
            DebugLogWriter.WriteMessage("Wireframes loaded.");
        }

        void ClearWireframes()
        {
            this.wireVisual.Children.Clear();
            DebugLogWriter.WriteMessage("Wireframes cleared.");
        }

        void ReceiveStartAutoPlay(StartAutoPlayMessage message)
        {
            var storyboard = (Storyboard)this.Resources["autoRotateStoryboard"];
            storyboard.Begin();
        }

        void ReceiveStopAutoPlay(StopAutoPlayMessage message)
        {
            var storyboard = (Storyboard)this.Resources["autoRotateStoryboard"];
            storyboard.Stop();
        }

        void HandleToggleMessage(ToggleMessage message)
        {
            if (this.stemMode)
            {
                this.stemMode = false;
                this.ClearWireframes();
            }
            else
            {
                this.stemMode = true;
                this.LoadWireframes();
            }
        }

    }
}
