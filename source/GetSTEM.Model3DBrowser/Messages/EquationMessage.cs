
namespace GetSTEM.Model3DBrowser.Messages
{
    public class EquationMessage
    {
        public Equation Equation { get; set; }
        public double Previous { get; set; }
        public double Current { get; set; }
        public double Scale { get; set; }
        public double Increment { get; set; }
        public double CurrentAngle { get; set; }
        public double PreviousAngle { get; set; }
    }
}
