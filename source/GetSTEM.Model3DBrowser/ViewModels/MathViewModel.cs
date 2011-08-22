using GalaSoft.MvvmLight;
using GetSTEM.Model3DBrowser.Messages;
using GalaSoft.MvvmLight.Messaging;

namespace GetSTEM.Model3DBrowser.ViewModels
{
    public class MathViewModel : ViewModelBase
    {
        const string NumberFormat = "0.0";
        const string YEquationFormat = "{0} = {4} + ({1}-{2})*{3}";
        const string XEquationFormat = "{0} = {4} - ({1}-{2})*{3}";

        public MathViewModel()
        {
            this.RotationYEquation = string.Format(YEquationFormat,
                "angle", "currentX", "lastX", "scale", "lastAngle");
            this.RotationXEquation = string.Format(XEquationFormat,
                "angle", "currentY", "lastY", "scale", "lastAngle");

            this.ReceiveEquationMessage(new EquationMessage() { Equation = Equation.YRotation });
            this.ReceiveEquationMessage(new EquationMessage() { Equation = Equation.XRotation });

            Messenger.Default.Register<EquationMessage>(this, this.ReceiveEquationMessage);
        }

        public string RotationYEquation { get; set; }
        public string RotationXEquation { get; set; }

        public const string RotationYEquationActualPropertyName = "RotationYEquationActual";
        string rotationYEquationActual = string.Empty;
        public string RotationYEquationActual
        {
            get
            {
                return rotationYEquationActual;
            }
            set
            {
                if (rotationYEquationActual == value)
                {
                    return;
                }
                var oldValue = rotationYEquationActual;
                rotationYEquationActual = value;
                RaisePropertyChanged(RotationYEquationActualPropertyName);
            }
        }

        public const string RotationXEquationActualPropertyName = "RotationXEquationActual";
        string rotationXEquationActual = string.Empty;
        public string RotationXEquationActual
        {
            get
            {
                return rotationXEquationActual;
            }
            set
            {
                if (rotationXEquationActual == value)
                {
                    return;
                }
                var oldValue = rotationXEquationActual;
                rotationXEquationActual = value;
                RaisePropertyChanged(RotationXEquationActualPropertyName);
            }
        }

        void ReceiveEquationMessage(EquationMessage message)
        {
            if (message.Equation == Equation.YRotation)
            {
                this.RotationYEquationActual = string.Format(YEquationFormat,
                    message.CurrentAngle.ToString(NumberFormat), 
                    message.Current.ToString(NumberFormat), 
                    message.Previous.ToString(NumberFormat), 
                    message.Scale.ToString(NumberFormat), 
                    message.PreviousAngle.ToString(NumberFormat));;
            }
            else
            {
                this.RotationXEquationActual = string.Format(XEquationFormat,
                    message.CurrentAngle.ToString(NumberFormat), 
                    message.Current.ToString(NumberFormat), 
                    message.Previous.ToString(NumberFormat), 
                    message.Scale.ToString(NumberFormat), 
                    message.PreviousAngle.ToString(NumberFormat));;
            }
        }
    }
}
