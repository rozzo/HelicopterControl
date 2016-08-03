using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace OmniResources.InfraredControl
{
    /// <summary>
    /// Controls an IR helicopter
    /// </summary>
    public class HelicopterControl<TCommand> : IHelicopterControl where TCommand : IHeliCommand, new()
    {
        private readonly IInfraredCommunicator _communicator;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly Task _communicatorTask;
        private int _previousMainThrottle;
        private bool startup;

        /// <summary>
        /// Initializes a new instance of the HelicopterControl class
        /// </summary>
        public HelicopterControl(IInfraredCommunicator communicator)
        {
            _communicator = communicator;

            IsEnabled = false;
            startup = true;

            Command = new TCommand();
            _previousMainThrottle = Command.MainThrottle;

            _communicatorTask = Task.Run(() =>
            {
                while (!_cts.IsCancellationRequested)
                {
                    if (IsEnabled)
                    {
                        if (Command.MainThrottle > 0)
                        {
                            _previousMainThrottle = Command.MainThrottle;
                            var pulses = Command.GetPulses();
                            _communicator.WriteData(pulses);
                            Thread.Sleep(10);
                        }
                        else if (Command.MainThrottle == 0 && _previousMainThrottle > 0)
                        {
                            //send a blank command to clear the copter
                            Debug.WriteLine("Clearing throttle");
                            _previousMainThrottle = Command.MainThrottle;
                            Command.Pitch = Command.PitchNeutral;
                            Command.Yaw = Command.YawNeutral;
                            
                            for(int i=0; i<5; i++){
                                var pulses = Command.GetPulses();
                                _communicator.WriteData(pulses);
                                Thread.Sleep(10);
                            }
                        }
                    }
                    else if (startup)
                    {
                        Debug.WriteLine("Helicopter Startup - Initializing Communication");
                        _previousMainThrottle = Command.MainThrottle;
                        Command.Pitch = Command.PitchNeutral;
                        Command.Yaw = Command.YawNeutral;

                        for (int i = 0; i < 5; i++)
                        {
                            var pulses = Command.GetPulses();
                            _communicator.WriteData(pulses);
                            Thread.Sleep(10);
                        }

                        startup = false;
                        IsEnabled = true;
                    }
                    else
                    {
                        Thread.Sleep(500);
                    }
                }
            }, _cts.Token);
        }

        /// <summary>
        /// Gets or sets a value indicating whether transmission is enabled for the helicopter
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Gets the current helicopter command
        /// </summary>
        public IHeliCommand Command { get; private set; }

        /// <summary>
        /// Increases or decreases the throttle by the given value
        /// </summary>
        public void ChangeThrottle(int value)
        {
            Command.MainThrottle += value;
            if (Command.MainThrottle > 1000)
                Command.MainThrottle = 1000;
            else if (Command.MainThrottle < 0)
                Command.MainThrottle = 0;
        }

        /// <summary>
        /// Turns the copter by the given value
        /// </summary>
        public void Turn(int value)
        {
            Command.Yaw += value;
        }

        /// <summary>
        /// Disposes the object
        /// </summary>
        public void Dispose()
        {
            IsEnabled = false;
            _cts.Cancel();
            _communicatorTask.Wait();
        }
    }
}
