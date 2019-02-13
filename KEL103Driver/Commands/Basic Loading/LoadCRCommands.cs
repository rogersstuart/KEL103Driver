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
        public static async Task SetConstantResistanceTarget(IPAddress device_address, double target_resistance)
        {
            using (UdpClient client = new UdpClient(KEL103Configuration.command_port))
            {
                KEL103Tools.ConfigureClient(device_address, client);

                var tx_bytes = Encoding.ASCII.GetBytes(":RES " + KEL103Tools.FormatString(target_resistance) + "OHM\n");

                await client.SendAsync(tx_bytes, tx_bytes.Length);
            }
        }

        public static async Task<double> GetConstantResistanceTarget(IPAddress device_address)
        {
            using (UdpClient client = new UdpClient(KEL103Configuration.command_port))
            {
                KEL103Tools.ConfigureClient(device_address, client);

                var tx_bytes = Encoding.ASCII.GetBytes(":RES?\n");

                await client.SendAsync(tx_bytes, tx_bytes.Length);

                var rx = (await client.ReceiveAsync()).Buffer;

                return Convert.ToDouble(Encoding.ASCII.GetString(rx).Split('O')[0]);
            }
        }
    }
}
