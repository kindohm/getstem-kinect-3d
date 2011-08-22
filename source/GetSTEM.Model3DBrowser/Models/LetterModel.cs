
namespace GetSTEM.Model3DBrowser.Models
{
    public class LetterModel
    {
        public LetterModel()
        {
            this.Scale = 1.0d;
        }

        public string Color { get; set; }
        public double OffsetX { get; set; }
        public double OffsetY { get; set; }
        public double OffsetZ { get; set; }
        public string ModelPath { get; set; }
        public double Scale { get; set; }
    }
}
