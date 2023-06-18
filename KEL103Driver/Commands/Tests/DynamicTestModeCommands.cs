using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace KEL103Driver
{
    public static partial class KEL103Command
    {
        public static readonly int DYNAMIC_CONSTANT_VOLTAGE_MODE = 1;
        public static readonly int DYNAMIC_CONSTANT_CURRENT_MODE = 2;
        public static readonly int DYNAMIC_CONSTANT_RESISTANCE_MODE = 3;
        public static readonly int DYNAMIC_CONSTANT_POWER_MODE = 4;
        public static readonly int DYNAMIC_PULSE_MODE = 5;
        public static readonly int DYNAMIC_FLIP_MODE = 6;

        public static readonly string[] dynamic_mode_strings = {
            "Dynamic Test; Constant Voltage Mode",
            "Dynamic Test; Constant Current Mode",
            "Dynamic Test; Constant Resistance Mode",
            "Dynamic Test; Constant Power Mode",
            "Dynamic Test; Pulse Mode",
            "Dynamic Test; Flip Mode"};

        private static readonly string[] dynamic_mode_suffix_strings = {"V","A","OHM","W","",""};

        public static Task<string> QueryDynamicMode(IPAddress device_address)
        {
            using (UdpClient client = new UdpClient(KEL103Persistance.Configuration.CommandPort))
            {
                KEL103Tools.ConfigureClient(device_address, client);

                return QueryDynamicMode(client);
            }
        }

        public static Task<string> QueryDynamicMode(UdpClient client)
        {
            return Task.Run(() => { 
                var endpoint = client.Client.RemoteEndPoint as IPEndPoint;
                var tx_bytes = Encoding.ASCII.GetBytes(":DYN?\n");

                 client.Send(tx_bytes, tx_bytes.Length);

                var rx = client.Receive(ref endpoint);

                return Encoding.ASCII.GetString(rx).Split('\n')[0];
            });
        }

        public static Task SetDynamicMode(IPAddress device_address, int mode, double low_value, double high_value, double frequency, double duty_cycle)
        {
            using (UdpClient client = new UdpClient(KEL103Persistance.Configuration.CommandPort))
            {
                KEL103Tools.ConfigureClient(device_address, client);

                return SetDynamicMode(client, mode, low_value, high_value, frequency, duty_cycle);
            }
        }

        public static Task SetDynamicMode(UdpClient client, int mode, double low_value, double high_value, double frequency, double duty_cycle)
        {
            return Task.Run(() => {
                string mode_string = mode + "," + KEL103Tools.FormatString(low_value) + dynamic_mode_suffix_strings[mode - 1] + "," +
                KEL103Tools.FormatString(high_value) + dynamic_mode_suffix_strings[mode - 1] + "," + KEL103Tools.FormatString(frequency) + "HZ," +
                KEL103Tools.FormatString(duty_cycle) + "%";

            var tx_bytes = Encoding.ASCII.GetBytes(":DYN " + mode_string + "\n");

             client.Send(tx_bytes, tx_bytes.Length);
            });
        }

        //these work but don't seem to do anything
        public static Task SetDynamicMode_Pulse(IPAddress device_address, double low_slope, double high_slope, double low_current,
            double high_current, double time)
        {
            using (UdpClient client = new UdpClient(KEL103Persistance.Configuration.CommandPort))
            {
                KEL103Tools.ConfigureClient(device_address, client);

                return SetDynamicMode_Pulse(client, low_slope, high_slope, low_current, high_current, time);
            }
        }

        //these work but don't seem to do anything
        public static Task SetDynamicMode_Pulse(UdpClient client, double low_slope, double high_slope, double low_current,
            double high_current, double time)
        {
            return Task.Run(() => {
                string mode_string = 5 + "," + KEL103Tools.FormatString(low_slope) + "A/uS," +
                KEL103Tools.FormatString(high_slope) + "A/uS," + KEL103Tools.FormatString(low_current) + "A," +
                KEL103Tools.FormatString(high_current) + "A," + KEL103Tools.FormatString(time) + "S";

                var tx_bytes = Encoding.ASCII.GetBytes(":DYN " + mode_string + "\n");

                 client.Send(tx_bytes, tx_bytes.Length);
            });
        }

        public static Task SetDynamicMode_Flip(IPAddress device_address, double low_slope, double high_slope, double low_current,
            double high_current)
        {
            using (UdpClient client = new UdpClient(KEL103Persistance.Configuration.CommandPort))
            {
                KEL103Tools.ConfigureClient(device_address, client);

                return SetDynamicMode_Flip(client, low_slope, high_slope, low_current, high_current);
            }
        }

        public static Task SetDynamicMode_Flip(UdpClient client, double low_slope, double high_slope, double low_current,
            double high_current)
        {
            return Task.Run(() => {
                string mode_string = 6 + "," + KEL103Tools.FormatString(low_slope) + "A/uS," +
                KEL103Tools.FormatString(high_slope) + "A/uS," + KEL103Tools.FormatString(low_current) + "A," +
                KEL103Tools.FormatString(high_current) + "A";

            var tx_bytes = Encoding.ASCII.GetBytes(":DYN " + mode_string + "\n");

             client.Send(tx_bytes, tx_bytes.Length);
            });
        }













    }
}
