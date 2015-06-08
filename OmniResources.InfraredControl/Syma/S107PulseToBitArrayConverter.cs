using System.Collections.Generic;

namespace OmniResources.InfraredControl.Syma
{
    /// <summary>
    /// Translates between pulses and the Syma S107 binary array format
    /// </summary>
    public class S107PulseToBitArrayConverter
    {
        private bool _gotHeaderPulse;
        private bool _gotHeaderSpace;
        private readonly List<bool> _curValues = new List<bool>();
        //Old Values
        //private const uint PulseLength = 397;
        //private const uint HeaderPulseDuration = 2005;
        //private const uint HeaderPulseWidth = 1920;
        //private const uint OnPulse = 618;
        //private const uint OffPulse = 234;
        //http://www.kerrywong.com/2012/08/27/reverse-engineering-the-syma-s107g-ir-protocol/
        private const uint PulseWidth = 380;
        private const uint HeaderPulseDuration = 2040;
        private const uint HeaderPulseWidth = 2000;
        private const uint OnPulseDuration = 600;
        private const uint OffPulseDuration = 220;

        /// <summary>
        /// Converts pulses to binary data
        /// </summary>
        public IEnumerable<bool[]> Convert(IEnumerable<PulseData> data)
        {
            foreach (var sample in data)
            {
                if (!_gotHeaderPulse)
                {
                    _gotHeaderPulse = sample.IsPulse && IsCloseTo(sample, 2000);
                }
                else if (!_gotHeaderSpace)
                {
                    _gotHeaderSpace = !sample.IsPulse && IsCloseTo(sample, 1900);

                    if (!_gotHeaderSpace)
                        AbortPacket();
                }
                else
                {
                    if (sample.IsPulse)
                    {
                        if (!IsCloseTo(sample, 300))
                            AbortPacket();
                    }
                    else
                    {
                        if (IsCloseTo(sample, 300))
                            _curValues.Add(false);
                        else if (IsCloseTo(sample, 600))
                            _curValues.Add(true);
                        else
                            AbortPacket();
                    }

                    if (_curValues.Count != 32) continue;

                    yield return _curValues.ToArray();
                    AbortPacket();
                }
            }
        }

        /// <summary>
        /// Converts binary data to pulses
        /// </summary>
        public static IEnumerable<PulseData> Convert(bool[] data)
        {
            var result = new List<PulseData>
            {
                new PulseData {IsPulse = true, Length = HeaderPulseDuration},
                new PulseData {IsPulse = false, Length = HeaderPulseWidth}
            };

            foreach (var bit in data)
            {
                result.Add(new PulseData { IsPulse = true, Length = PulseWidth });
                result.Add(new PulseData { IsPulse = false, Length = (bit ? OnPulseDuration : OffPulseDuration) });
            }

            //result.Add(new PulseData { IsPulse = true, Length = PulseLength });
            //result.Add(new PulseData { IsPulse = false, Length = (uint)(120500 - result.Sum(x => x.Length)) });

            return result;
        }

        private void AbortPacket()
        {
            _gotHeaderPulse = false;
            _gotHeaderSpace = false;
            _curValues.Clear();
        }

        /// <summary>
        /// Returns true if the sample is within +/- 150 MS of the given length
        /// </summary>
        private static bool IsCloseTo(PulseData sample, uint length)
        {
            return sample.Length > length - 150 && sample.Length < length + 150;
        }
    }
}
