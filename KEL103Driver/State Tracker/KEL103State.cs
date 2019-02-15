using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KEL103Driver
{
    public class KEL103State
    {
        public DateTime time_stamp { get; set; }

        public double Voltage { get; set; }
        public double Current { get; set; }
        public double Resistance { get; set; }
        public double Power { get; set; }

        public bool input_state { get; set; }

        public TimeSpan retreval_span { get; set; }
    }
}
