using System.Collections.Generic;
using OmniResources.InfraredControl;
using OmniResources.InfraredControl.IguanaIr;
using OmniResources.InfraredControl.Syma;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SharpDX.DirectInput;

namespace HelicopterControl.Joystick
{
    class Program
    {
        private static SharpDX.DirectInput.Joystick _joystick;
        private static int _currentThrottle;
        private static int _currentYawTrim;
        private const double Ratio = 2000.0 / ushort.MaxValue;
        private const short Speed = 20;
        private static readonly IguanaIrInterop IrInterop = new IguanaIrInterop();
        private static readonly IHelicopterControl Helicopter = new HelicopterControl<S107HeliCommand>(IrInterop);
        
        static void Main()
        {
            IrInterop.SetChannels(new byte[]{1,3});
            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;
            var task = Task.Run(() => StartJoystickCapture(token));
            Console.ReadLine();
            tokenSource.Cancel();
            task.Wait();
        }

        private static void StartJoystickCapture(CancellationToken token)
        {
            var joystickGuid = Guid.Empty;
            var di = new DirectInput();
            foreach (var device in di.GetDevices(DeviceType.Joystick, DeviceEnumerationFlags.AttachedOnly))
            {
                joystickGuid = device.InstanceGuid;
                break;
            }
            _joystick = new SharpDX.DirectInput.Joystick(di, joystickGuid);
            _joystick.Acquire();
            _currentThrottle = 0;
            _currentYawTrim = 0;
            while (!token.IsCancellationRequested)
            {
                var state = _joystick.GetCurrentState();

                _currentThrottle = (1000 - GetAnalogStickValue(state.Sliders[0])) / 2;
                Helicopter.Command.MainThrottle = _currentThrottle;

                Helicopter.Command.Pitch = GetAnalogStickValue(state.Y);

                //Helicopter.Command.Yaw = -1 * (GetAnalogStickValue(state.X) + _currentYawTrim); //For n64 controller
                Helicopter.Command.Yaw = -1 * (GetAnalogStickValue(state.RotationZ) + _currentYawTrim);

                var yawtrimChange = GetYawTrimChange(state.Buttons);
                _currentYawTrim += yawtrimChange;
                Helicopter.Command.YawTrim = _currentYawTrim;

                SetConsoleDisplay(state);
                Thread.Sleep(Speed);
            }
        }

        private static void SetConsoleDisplay(JoystickState state)
        {
            Console.WriteLine("Helicopter Controls");
            Console.WriteLine("--------------------");
            Console.WriteLine("Throttle:\t{0}   ", _currentThrottle);
            //Console.WriteLine("Yaw:\t\t{0}    ", -1 * (GetAnalogStickValue(state.X) + _currentYawTrim)); //For n64 controller
            Console.WriteLine("Yaw:\t\t{0}    ", GetAnalogStickValue(state.RotationZ) + _currentYawTrim);
            Console.WriteLine("Pitch:\t\t{0}   ", GetAnalogStickValue(state.Y));
            Console.WriteLine();
            Console.WriteLine("YawTrim:\t{0}   ", _currentYawTrim);
            Console.WriteLine();
            Console.WriteLine("POV:{0} deg      ", state.PointOfViewControllers[0]/100);
            Console.WriteLine();
            Console.WriteLine("Buttons");
            Console.WriteLine("--------------------");
            Console.WriteLine("{0}", BoolArrayToString(state.Buttons));
            Console.WriteLine("{0}", GetButtonsPressed(state.Buttons));
            Console.WriteLine("X:\t\t{0}    ", GetAnalogStickValue(state.X));
            Console.WriteLine("Y:\t\t{0}    ", GetAnalogStickValue(state.Y));
            Console.WriteLine("Z:\t\t{0}    ", GetAnalogStickValue(state.RotationZ));
            Console.WriteLine("Slider:\t\t{0}    ", (1000 - GetAnalogStickValue(state.Sliders[0])) / 2);

            Console.SetCursorPosition(0, 0);
        }

        private static int GetAnalogStickValue(int state)
        {
            return (int)(state * Ratio - 1000);
        }

        private static string GetButtonsPressed(bool[] array)
        {
            var s = new StringBuilder();
            for (int i = 0; i < 10; i++)
            {
                if (array[i])
                {
                    if (s.Length > 0) s.Append("+");
                    s.Append(GetButtonName(i));
                }
            }
            return s.ToString().PadRight(80, ' ');
        }

        private static string BoolArrayToString(ICollection<bool> array)
        {
            var s = new StringBuilder(array.Count);
            foreach (var b in array.Take(10))
            {
                s.Append(b ? '1' : '0');
            }
            return s.ToString();
        }

        private static int GetThrottleChange(IList<bool> buttons)
        {
            if (buttons[0]) return _currentThrottle >= 1000 ? 0 : 1*Speed;
            if (buttons[2]) return _currentThrottle <= 0 ? 0 : -1*Speed;
            return 0;
        }

        private static int GetYawTrimChange(bool[] buttons)
        {
            if (buttons[4]) return _currentYawTrim >= 200 ? 0 : (int) (.1*Speed);
            if (buttons[5]) return _currentYawTrim <= -200 ? 0 : (int) (-.1*Speed);
            return 0;
        }

        //n64 Button names
        private static string GetButtonName(int index)
        {
            switch (index)
            {
                case 0:
                    return "Up";
                case 1:
                    return "Right";
                case 2:
                    return "Down";
                case 3:
                    return "Left";
                case 4:
                    return "L";
                case 5:
                    return "R";
                case 6:
                    return "A";
                case 7:
                    return "Z";
                case 8:
                    return "B";
                case 9:
                    return "Start";
            }
            return "Unknown";
        }
    }
}
