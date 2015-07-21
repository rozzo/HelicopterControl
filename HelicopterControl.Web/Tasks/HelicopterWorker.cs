using System;
using HelicopterControl.Web.Models;
using OmniResources.InfraredControl;
using OmniResources.InfraredControl.IguanaIr;
using OmniResources.InfraredControl.Syma;
using System.Diagnostics;
using System.Threading;

namespace HelicopterControl.Web.Tasks
{
    public class HelicopterWorker
    {
        private static bool _processCommands = false;
        private static readonly IguanaIrInterop IrInterop = new IguanaIrInterop();
        private readonly IHelicopterControl _helicopter;
        private const short Speed = 20;

        public static bool ProcessCommands
        {
            get
            {
                try
                {
                    return _processCommands;
                }
                catch (TypeInitializationException e)
                {
                    _processCommands = false;
                    return _processCommands;
                }
            }
        }

        public HelicopterWorker()
        {
            _helicopter = new HelicopterControl<S107HeliCommand>(IrInterop);
        }

        public static void Stop()
        {
            _processCommands = false;
        }

        public void Run()
        {
            Debug.WriteLine("run()");
            if (!_processCommands)
            {
                Debug.WriteLine("starting worker");
                Startup();
                Debug.WriteLine("Processing commands");
                while (_processCommands)
                {
                    SendCommand();
                    Thread.Sleep(Speed);
                }
            }
        }

        private void Startup()
        {
            IrInterop.SetChannels(new byte[] { 1, 3 });
            _helicopter.Command.Channel = HelicopterState.Channel;
            _processCommands = true;
        }

        public void SendCommand()
        {
            _helicopter.Command.MainThrottle = HelicopterState.Throttle;
            _helicopter.Command.Pitch = HelicopterState.Pitch;
            _helicopter.Command.YawTrim = HelicopterState.YawTrim;
            _helicopter.Command.Yaw = -1 * HelicopterState.Yaw + _helicopter.Command.YawTrim;

            //Debug.WriteLine(_helicopter.Command.ToString());
        }
    }
}