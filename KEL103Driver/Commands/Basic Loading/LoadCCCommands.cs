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
            using (UdpClient client = new UdpClient(KEL103Persistance.Configuration.CommandPort))
            {
                KEL103Tools.ConfigureClient(device_address, client);

                await SetConstantCurrentTarget(client, target_current);
            }
        }

        public static async Task SetConstantCurrentTarget(UdpClient client, double target_current)
        {
                var tx_bytes = Encoding.ASCII.GetBytes(":CURR " + KEL103Tools.FormatString(target_current) + "A\n");

                await client.SendAsync(tx_bytes, tx_bytes.Length);       
        }

        public static async Task<double> GetConstantCurrentTarget(IPAddress device_address)
        {
            using (UdpClient client = new UdpClient(KEL103Persistance.Configuration.CommandPort))
            {
                KEL103Tools.ConfigureClient(device_address, client);

                return await GetConstantCurrentTarget(client);
            }
        }

        public static async Task<double> GetConstantCurrentTarget(UdpClient client)
        {
            var tx_bytes = Encoding.ASCII.GetBytes(":CURR?\n");

            await client.SendAsync(tx_bytes, tx_bytes.Length);

            var rx = (await client.ReceiveAsync()).Buffer;

            return Convert.ToDouble(Encoding.ASCII.GetString(rx).Split('A')[0]);
        }
    }
}
