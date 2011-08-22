using GalaSoft.MvvmLight;
using GetSTEM.Model3DBrowser.Services;
using GetSTEM.Model3DBrowser.ViewModels;
using Ninject;

namespace GetSTEM.Model3DBrowser.Framework
{
    public class ViewModelLocator
    {
        static MainViewModel main;
        static BoundingBoxViewModel boundingBox;
        static ExplorerViewModel explorer;
        static MathViewModel math;
        static INuiService nuiService;

        public ViewModelLocator()
        {
            var kernel = new StandardKernel();
            kernel.Bind<IKeyboardService>().To<DefaultKeyboardService>();

            if (ViewModelBase.IsInDesignModeStatic)
            {
                kernel.Bind<IConfigurationService>().To<DesignConfigurationService>();
                kernel.Bind<INuiService>().To<MockNuiService>();
            }
            else
            {
                kernel.Bind<IConfigurationService>().To<AppConfigConfigurationService>();
                kernel.Bind<INuiService>().To<KinectNuiService>();
            }

            nuiService = kernel.Get<INuiService>();

            main = new MainViewModel(
                kernel.Get<IConfigurationService>(),
                nuiService,
                kernel.Get<IKeyboardService>());

            boundingBox = new BoundingBoxViewModel(
                nuiService);

            explorer = new ExplorerViewModel(
                nuiService, kernel.Get<IConfigurationService>());

            math = new MathViewModel();

        }

        public MainViewModel Main
        {
            get { return main; }
        }

        public BoundingBoxViewModel BoundingBox
        {
            get { return boundingBox; }
        }

        public ExplorerViewModel Explorer
        {
            get { return explorer; }
        }

        public MathViewModel Math
        {
            get { return math; }
        }

        public static void Cleanup()
        {
            math.Cleanup();
            nuiService.Shutdown();
            boundingBox.Cleanup();
            main.Cleanup();
        }
    }
}