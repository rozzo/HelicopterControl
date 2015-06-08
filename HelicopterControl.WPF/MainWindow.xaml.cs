using OmniResources.InfraredControl;
using OmniResources.InfraredControl.IguanaIr;
using OmniResources.InfraredControl.Syma;
using System.Threading;
using System.Windows.Input;
using Key = System.Windows.Input.Key;

namespace HelicopterControl.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly IHelicopterControl _helicopter;
        public MainWindow()
        {
            InitializeComponent();
            _helicopter = new HelicopterControl<S107HeliCommand>(new IguanaIrInterop());
            _helicopter.Command.YawTrim = 250;

            KeyDown += MainWindow_KeyDown;
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
                _helicopter.ChangeThrottle(50);
            else if (e.Key == Key.Down)
                _helicopter.ChangeThrottle(-50);
            else if (e.Key == Key.Left)
                _helicopter.Turn(500);
            else if (e.Key == Key.Right)
                _helicopter.Turn(-500);
            else if (e.Key == Key.NumPad0)
            {
                _helicopter.Command.Yaw = 0;
                _helicopter.Command.MainThrottle = 0;
            }
            else if (e.Key == Key.NumPad1)
                AutoMagicalLandFeature();


            Throttle.Text = "Throttle: " + _helicopter.Command.MainThrottle;
            Yaw.Text = "Yaw: " + _helicopter.Command.Yaw;
        }

        private void AutoMagicalLandFeature()
        {
            _helicopter.Command.Yaw = 0;
            _helicopter.Command.MainThrottle = 600;
            for (var i = 10; i > 0; i--)
            {
                _helicopter.Command.MainThrottle -= 60;
                Thread.Sleep(120);
            }

            _helicopter.Command.MainThrottle = 0;
        }
    }
}
