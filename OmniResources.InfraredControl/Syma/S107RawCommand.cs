using System;
using System.Collections;

namespace OmniResources.InfraredControl.Syma
{
    /// <summary>
    /// Raw Commands for the S107 family of helicopters
    /// </summary>
    public class S107RawCommand
    {
        /// <summary>
        /// Initializes a new instance of the S107Command class for an empty command
        /// </summary>
        public S107RawCommand()
            : this (new bool[32])
        {

        }

        /// <summary>
        /// Initializes a new instance of the S107Command class with the given raw data
        /// </summary>
        public S107RawCommand(bool[] rawData)
        {
            if (rawData == null || rawData.Length != 32)
                throw new ArgumentException("Raw Data must be 32 bits!");

            RawData = rawData;
        }

        /// <summary>
        /// Gets or sets the raw binary data backing the command
        /// </summary>
        public bool[] RawData { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the channel (false = A, true = B)
        /// </summary>
        public bool Channel
        {
            get { return RawData[16]; }
            set { RawData[16] = value; }
        }


        /// <summary>
        /// Gets or sets the value for throttle (up/down, 0 - 127)
        /// </summary>
        public byte MainThrottle
        {
            get { return Get7BitValue(17); }
            set { Set7BitValue(17, value); }
        }

        /// <summary>
        /// Gets or sets the value for pitch (forward: 0, still: 63, back: 127)
        /// </summary>
        public byte Pitch
        {
            get { return Get7BitValue(9); }
            set { Set7BitValue(9, value); }
        }

        /// <summary>
        /// Gets or sets the value for yaw (left: 127, still: 63, right: 0)
        /// </summary>
        public byte Yaw
        {
            get { return Get7BitValue(1); }
            set { Set7BitValue(1, value); }
        }

        /// <summary>
        /// Gets or sets the value for the yaw trim (left/right adjustment, 0 - 127, 63 = neutral)
        /// YawTrim also affects what's sent for yaw, is it really necessary?
        /// </summary>
        public byte YawTrim
        {
            get { return Get7BitValue(25); }
            set { Set7BitValue(25, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to take a picture (S107-C)
        /// </summary>
        public bool TakePicture
        {
            get { return RawData[8]; }
            set { RawData[8] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to take a video (S107-C)
        /// </summary>
        public bool TakeVideo
        {
            get { return RawData[0]; }
            set { RawData[0] = value; }
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

        private byte Get7BitValue(int startIndex)
        {
            byte finalValue = 0;

            for (var i = 0; i < 7; i++)
            {
                if (RawData[i + startIndex])
                    finalValue |= (byte)(1 << (6 - i));
            }

            return finalValue;
        }

        private void Set7BitValue(int startIndex, byte value)
        {
            if (value > 127)
                throw new ArgumentException("Value must be 127 or less!");

            var bitArray = new BitArray(new[] { value });

            for (var i = 0; i < 7; i++)
            {
                RawData[i + startIndex] = bitArray[6 - i];
            }
        } 
    }
}
