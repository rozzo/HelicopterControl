﻿using System.Diagnostics;
using System.Threading;
using OmniResources.InfraredControl;
using OmniResources.InfraredControl.IguanaIr;
using OmniResources.InfraredControl.Syma;

namespace ConferenceDemo
{
    public class Helicopter
    {
        private static readonly IguanaIrInterop IrInterop = new IguanaIrInterop();
        private readonly IHelicopterControl _helicopter;

        public Helicopter(int yawTrim = 0)
        {
            IrInterop.SetChannels(new byte[] { 1, 3 });
            _helicopter = new HelicopterControl<S107HeliCommand>(IrInterop);
            _helicopter.Command.YawTrim = yawTrim;
        }

        public IHeliCommand Command
        {
            get { return _helicopter.Command; }
        }

        public void ChangeThrottle(int value)
        {
            Debug.WriteLine("Throttle: " + value);
            _helicopter.Command.MainThrottle = value;
            sleep();
        }

        public void TakeOff()
        {
            Debug.WriteLine("Take Off");
            _helicopter.ChangeThrottle(200);
            Thread.Sleep(2000);
            _helicopter.ChangeThrottle(400);
            Thread.Sleep(2000);
        }

        public void Land()
        {
            Debug.WriteLine("Landing");
            while (_helicopter.Command.MainThrottle > 0)
            {
                _helicopter.Command.MainThrottle -= 100;
                if (_helicopter.Command.MainThrottle < 0)
                {
                    _helicopter.Command.MainThrottle = 0;
                }
                sleep();
            }
        }

        public void TurnLeft()
        {
            Debug.WriteLine("Left");
            _helicopter.Turn(200);
            sleep();
            _helicopter.Turn(0);
            sleep();
        }

        public void TurnRight()
        {
            Debug.WriteLine("Right");
            _helicopter.Turn(-200);
            sleep();
            _helicopter.Turn(0);
            sleep();
        }

        public void MoveForward()
        {
            Debug.WriteLine("Forward");
            _helicopter.Command.Pitch = 200;
            sleep();
            _helicopter.Command.Pitch = 0;
            sleep();
        }

        public void MoveBackward()
        {
            Debug.WriteLine("Back");
            _helicopter.Command.Pitch = -300;
            sleep();
            _helicopter.Command.Pitch = 0;
            sleep();
        }

        private void sleep()
        {
            Thread.Sleep(500);
        }
    }
}
