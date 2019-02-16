using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KEL103Driver
{
    public class KEL103State
    {
        public DateTime TimeStamp { get; set; }

        public double Voltage { get; set; }
        public double Current { get; set; }
        public double Power { get; set; }

        public bool InputState { get; set; }

        public TimeSpan ValueAquisitionTimespan { get; set; }
    }
}
