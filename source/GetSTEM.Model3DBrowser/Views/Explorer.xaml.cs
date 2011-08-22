using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using GalaSoft.MvvmLight.Messaging;
using GetSTEM.Model3DBrowser.Messages;
using GetSTEM.Model3DBrowser.ViewModels;

namespace GetSTEM.Model3DBrowser.Views
{
    public partial class Explorer : UserControl
    {
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
            this.LoadWireframes();

            Messenger.Default.Register<StartAutoPlayMessage>(this, this.ReceiveStartAutoPlay);
            Messenger.Default.Register<StopAutoPlayMessage>(this, this.ReceiveStopAutoPlay);
        }

        void LoadWireframes()
        {
            var wireframes = this.ViewModel.ScreenLinesCollection;
            foreach (var wireframe in wireframes)
            {
                this.mainVisual.Children.Insert(0, wireframe);
            }
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
    }
}
