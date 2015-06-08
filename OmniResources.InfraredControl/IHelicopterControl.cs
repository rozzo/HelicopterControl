using System;

namespace OmniResources.InfraredControl
{
    /// <summary>
    /// Common Helicopter control interface
    /// </summary>
    public interface IHelicopterControl : IDisposable
    {
        /// <summary>
        /// Gets or sets a value indicating whether transmission is enabled for the helicopter
        /// </summary>
        bool IsEnabled { get; set; }

        /// <summary>
        /// Gets the current helicopter command
        /// </summary>
        IHeliCommand Command { get; }

        /// <summary>
        /// Increases or decreases the throttle by the given value
        /// </summary>
        void ChangeThrottle(int value);

        /// <summary>
        /// Turns the copter by the given value
        /// </summary>
        void Turn(int value);
    }
}
