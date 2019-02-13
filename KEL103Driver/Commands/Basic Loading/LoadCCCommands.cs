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
        public static async Task SetConstantCurrentTarget(IPAddress device_address, double target_current)
        {
            using (UdpClient client = new UdpClient(KEL103Configuration.command_port))
            {
                client.Client.ReceiveTimeout = 2000;
                client.Client.SendTimeout = 2000;
                client.DontFragment = false;

                client.Connect(device_address, KEL103Configuration.command_port);

                var tx_bytes = Encoding.ASCII.GetBytes(":CURR " + KEL103Tools.FormatString(target_current) + "A\n");

                await client.SendAsync(tx_bytes, tx_bytes.Length);
            }
        }

        public static async Task<double> GetConstantCurrentTarget(IPAddress device_address)
        {
            using (UdpClient client = new UdpClient(KEL103Configuration.command_port))
            {
                client.Client.ReceiveTimeout = 2000;
                client.Client.SendTimeout = 2000;
                client.DontFragment = false;

                client.Connect(device_address, KEL103Configuration.command_port);

                var tx_bytes = Encoding.ASCII.GetBytes(":CURR?\n");

                await client.SendAsync(tx_bytes, tx_bytes.Length);

                var rx = (await client.ReceiveAsync()).Buffer;

                return Convert.ToDouble(Encoding.ASCII.GetString(rx).Split('A')[0]);
            }
        }
    }
}
