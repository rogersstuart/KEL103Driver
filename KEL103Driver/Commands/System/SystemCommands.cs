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
        public static async Task<string> Identify(IPAddress device_address)
        {
            using (UdpClient client = new UdpClient(KEL103Configuration.command_port))
            {
                KEL103Tools.ConfigureClient(device_address, client);

                var tx_bytes = Encoding.ASCII.GetBytes("*IDN?\n");

                await client.SendAsync(tx_bytes, tx_bytes.Length);

                var rx = (await client.ReceiveAsync()).Buffer;

                return Encoding.ASCII.GetString(rx).Split('\n')[0];
            }
        }

        public static async Task StoreToUnit(IPAddress device_address, int location_index)
        {
            using (UdpClient client = new UdpClient(KEL103Configuration.command_port))
            {
                KEL103Tools.ConfigureClient(device_address, client);

                var tx_bytes = Encoding.ASCII.GetBytes("*SAV "+ location_index + "\n");

                await client.SendAsync(tx_bytes, tx_bytes.Length);

            }
        }

        public static async Task RecallToUnit(IPAddress device_address, int location_index)
        {
            using (UdpClient client = new UdpClient(KEL103Configuration.command_port))
            {
                KEL103Tools.ConfigureClient(device_address, client);

                var tx_bytes = Encoding.ASCII.GetBytes("*RCL " + location_index + "\n");

                await client.SendAsync(tx_bytes, tx_bytes.Length);
            }
        }

        public static async Task SetLoadInputSwitchState(IPAddress device_address, bool switch_state) //true is on, false is off
        {
            using (UdpClient client = new UdpClient(KEL103Configuration.command_port))
            {
                KEL103Tools.ConfigureClient(device_address, client);

                var tx_bytes = Encoding.ASCII.GetBytes(":INPUT " + (switch_state ? "ON" : "OFF") + "\n");

                await client.SendAsync(tx_bytes, tx_bytes.Length);
            }
        }

        public static async Task<bool> GetLoadInputSwitchState(IPAddress device_address)  //true is on, false is off
        {
            using (UdpClient client = new UdpClient(KEL103Configuration.command_port))
            {
                KEL103Tools.ConfigureClient(device_address, client);

                var tx_bytes = Encoding.ASCII.GetBytes(":INPUT?\n");

                await client.SendAsync(tx_bytes, tx_bytes.Length);

                var rx = (await client.ReceiveAsync()).Buffer;

                return Encoding.ASCII.GetString(rx).Split('\n')[0] == "1" ? true : false;
            }
        }

        public static async Task SimulateTrigger(IPAddress device_address)
        {
            using (UdpClient client = new UdpClient(KEL103Configuration.command_port))
            {
                KEL103Tools.ConfigureClient(device_address, client);

                var tx_bytes = Encoding.ASCII.GetBytes("*TRG\n");

                await client.SendAsync(tx_bytes, tx_bytes.Length);
            }
        }

        public static async Task SetSystemParameter(IPAddress device_address, int parameter, bool state)
        {
            using (UdpClient client = new UdpClient(KEL103Configuration.command_port))
            {
                KEL103Tools.ConfigureClient(device_address, client);

                var tx_bytes = Encoding.ASCII.GetBytes(":SYST:BEEP OFF\n");

                await client.SendAsync(tx_bytes, tx_bytes.Length);
            }
        }
    }
}
