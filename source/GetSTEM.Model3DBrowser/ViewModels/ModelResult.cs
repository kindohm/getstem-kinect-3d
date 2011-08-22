using System.Windows.Media.Media3D;
using _3DTools;
using System.Collections.Generic;

namespace GetSTEM.Model3DBrowser.ViewModels
{
    public class ModelResult
    {
        public ModelResult()
        {
            this.Model3DGroup = new Model3DGroup();
            this.Wireframes = new List<ScreenLines>();
        }

        public Model3DGroup Model3DGroup { get; set; }
        public List<ScreenLines> Wireframes { get; set; }
    }
}
