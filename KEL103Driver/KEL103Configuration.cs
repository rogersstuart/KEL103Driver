using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace KEL103Driver
{
    public static class KEL103Configuration
    {
        public static readonly IPAddress broadcast_address = IPAddress.Parse("192.168.1.255");
        public static readonly int broadcast_port = 18191;
        public static readonly int command_port = 18190;
        public static readonly byte[] search_message = Encoding.ASCII.GetBytes("find_ka000");
        public static readonly int read_timeout = 2000;
        public static readonly int write_timeout = 2000;
    }
}
