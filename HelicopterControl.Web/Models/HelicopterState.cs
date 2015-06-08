using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HelicopterControl.Web.Models
{
    public class HelicopterState
    {
        public int Throttle { get; set; }
        public int Pitch { get; set; }
        public int Yaw { get; set; }
    }
}