namespace OmniResources.InfraredControl
{
    /// <summary>
    /// Pulse data
    /// </summary>
    public class PulseData
    {
        /// <summary>
        /// Gets or sets a value indicating whether this packet is a pulse or a space
        /// </summary>
        public bool IsPulse { get; set; }

        /// <summary>
        /// Gets or sets the length of the pulse (in microseconds)
        /// </summary>
        public uint Length { get; set; }
    }
}
