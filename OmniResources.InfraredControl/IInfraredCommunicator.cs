using System;
using System.Collections.Generic;

namespace OmniResources.InfraredControl
{
    /// <summary>
    /// Describes a device that has the ability to receive and send IR pulses
    /// </summary>
    public interface IInfraredCommunicator : IDisposable
    {
        /// <summary>
        /// Reads a group of pulses from the device with the given timeout
        /// </summary>
        IEnumerable<PulseData> ReadData(uint timeout);

        /// <summary>
        /// Writes the pulses to the device
        /// </summary>
        void WriteData(IEnumerable<PulseData> data);

        /// <summary>
        /// Enables the receiving functionality of the device
        /// </summary>
        void EnableReceiver();
    }
}