using System;

namespace HelicopterControl.Web.Models
{
    public class HelicopterState
    {
        public static bool Channel { get; set; }
        public static int Throttle { get; set; }
        public static int Pitch { get; set; }
        public static int Yaw { get; set; }
        public static int YawTrim { get; set; }

        public static void SetValues(bool channel, int throttle, int pitch, int yaw, int yawTrim)
        {
            Channel = channel;
            Throttle = throttle;
            Pitch = RoundOff(pitch);
            YawTrim = yawTrim;
            Yaw = RoundOff(yaw);
        }

        private static int RoundOff(int value)
        {
            return ((int)Math.Round(value / 100.0)) * 100;
        }

    }
}