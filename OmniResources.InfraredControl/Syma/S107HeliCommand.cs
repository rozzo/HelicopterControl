using System.Collections.Generic;
using System.Diagnostics;

namespace OmniResources.InfraredControl.Syma
{
    /// <summary>
    /// Translates between normalized HeliCommands and RawCommands for the S107
    /// </summary>
    public class S107HeliCommand : IHeliCommand, IHeliCameraCommand
    {
        private int _yaw;
        private int _pitch;
        private const int MaxValue = 1000;

        /// <summary>
        /// Gets or sets a value indicating the channel (false = A, true = B)
        /// </summary>
        public bool Channel { get; set; }

        /// <summary>
        /// Gets or sets the value for throttle (up/down, 0 - 1000)
        /// </summary>
        public int MainThrottle { get; set; }

        /// <summary>
        /// Gets or sets the value for pitch (forward: 1000, still: 0, back: -1000)
        /// </summary>
        public int Pitch
        {
            get { return _pitch; }
            set
            {
                if (value > PitchMax)
                    _pitch = PitchMax;
                else if (value < PitchMin)
                    _pitch = PitchMin;
                else
                    _pitch = value;
            }
        }

        /// <summary>
        /// Gets the max value that the pitch can be set to based on trim
        /// </summary>
        public int PitchMax
        {
            get { return PitchTrim < 0 ? MaxValue + PitchTrim : MaxValue; }
        }

        /// <summary>
        /// Gets the min value that the pitch can be set to based on trim
        /// </summary>
        public int PitchMin
        {
            get { return PitchTrim > 0 ? -1 * MaxValue + PitchTrim : -1 * MaxValue; }
        }

        /// <summary>
        /// Gets the neutral value of the pitch based on trim
        /// </summary>
        public int PitchNeutral
        {
            get { return Pitch - PitchTrim; }
        }

        /// <summary>
        /// Gets or sets the value for the pitch trim (left: 1000, still: 0, right: -1000)
        /// </summary>
        public int PitchTrim { get; set; }
        
        /// <summary>
        /// Gets or sets the value for yaw (left: 1000, still: 0, right: -10000)
        /// </summary>
        public int Yaw
        {
            get { return _yaw; }
            set
            {
                if (value > YawMax)
                    _yaw = YawMax;
                else if (value < YawMin)
                    _yaw = YawMin;
                else
                    _yaw = value;
            }
        }

        /// <summary>
        /// Gets the max value that the yaw can be set to based on trim
        /// </summary>
        public int YawMax
        {
            get { return YawTrim < 0 ? MaxValue + YawTrim : MaxValue; }
        }

        /// <summary>
        /// Gets the min value that the yaw can be set to based on trim
        /// </summary>
        public int YawMin
        {
            get { return YawTrim > 0 ? -1 * MaxValue + YawTrim : -1 * MaxValue; }
        }

        /// <summary>
        /// Gets the neutral value of the yaw based on trim
        /// </summary>
        public int YawNeutral
        {
            get { return Yaw - YawTrim; }
        }

        /// <summary>
        /// Gets or sets the value for the yaw trim (left: 1000, still: 0, right: -1000)
        /// </summary>
        public int YawTrim { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to take a picture (S107-C)
        /// </summary>
        public bool TakePicture { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to take a video (S107-C)
        /// </summary>
        public bool TakeVideo { get; set; }

        /// <summary>
        /// Returns the IR pulses needed to transmit this command
        /// </summary>
        public IEnumerable<PulseData> GetPulses()
        {
            var rawCommand = new S107RawCommand
            {
                Channel = Channel,
                MainThrottle = IntToS107Byte(MainThrottle, true),
                Pitch = IntToS107Byte(Pitch),
                Yaw = IntToS107Byte(Yaw),
                YawTrim = IntToS107Byte(YawTrim),
                TakePicture = TakePicture,
                TakeVideo = TakeVideo
            };
            Debug.WriteLine(ToString());
            return S107PulseToBitArrayConverter.Convert(rawCommand.RawData);
        }

        /// <summary>
        /// ToString override
        /// </summary>
        public override string ToString()
        {
            return string.Format("Ch: {0}, MT: {1}, P: {2}, Y: {3}, YT: {4}, TP: {5}, TV: {6}",
                Channel ? "B" : "A",
                MainThrottle,
                Pitch,
                Yaw,
                YawTrim,
                TakePicture,
                TakeVideo);
        }

        private static byte IntToS107Byte(int intValue, bool zeroMin = false)
        {
            if (intValue > MaxValue)
                intValue = (int)MaxValue;
            else if (zeroMin && intValue < 0)
                intValue = 0;
            else if (!zeroMin && intValue < MaxValue * -1)
                intValue = (int)MaxValue * -1;

            var adj = (double)intValue;

            if (!zeroMin)
                adj = (adj + MaxValue) / 2;

            adj /= (MaxValue / 127.0d);

            return (byte)adj;
        }
    }
}
