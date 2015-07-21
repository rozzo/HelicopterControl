using System.Collections.Generic;

namespace OmniResources.InfraredControl
{
    /// <summary>
    /// Defines common commands for all helicopters
    /// </summary>
    public interface IHeliCommand
    {
        /// <summary>
        /// Gets or sets a value indicating the channel (false = A, true = B)
        /// </summary>
        bool Channel { get; set; }

        /// <summary>
        /// Gets or sets the value for throttle (up/down, 0 - 1000)
        /// </summary>
        int MainThrottle { get; set; }

        /// <summary>
        /// Gets or sets the value for pitch (forward: 1000, still: 0, back: -1000)
        /// </summary>
        int Pitch { get; set; }

        /// <summary>
        /// Gets the max value that the pitch can be set to based on trim
        /// </summary>
        int PitchMax { get; }

        /// <summary>
        /// Gets the min value that the pitch can be set to based on trim
        /// </summary>
        int PitchMin { get; }

        /// <summary>
        /// Gets the neutral value of the pitch based on trim
        /// </summary>
        int PitchNeutral { get; }

        /// <summary>
        /// Gets or sets the value for the pitch trim (left: 1000, still: 0, right: -1000)
        /// </summary>
        int PitchTrim { get; set; }

        /// <summary>
        /// Gets or sets the value for yaw (left: 1000, still: 0, right: -1000)
        /// </summary>
        int Yaw { get; set; }

        /// <summary>
        /// Gets the max value that the yaw can be set to based on trim
        /// </summary>
        int YawMax { get; }

        /// <summary>
        /// Gets the min value that the yaw can be set to based on trim
        /// </summary>
        int YawMin { get; }

        /// <summary>
        /// Gets the neutral value of the yaw based on trim
        /// </summary>
        int YawNeutral { get; }

        /// <summary>
        /// Gets or sets the value for the yaw trim (left: 1000, still: 0, right: -1000)
        /// </summary>
        int YawTrim { get; set; }

        /// <summary>
        /// Returns the IR pulses needed to transmit this command
        /// </summary>
        IEnumerable<PulseData> GetPulses();
    }
}