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
        public static async Task SetConstantPowerTarget(IPAddress device_address, double target_power)
        {
            using (UdpClient client = new UdpClient(KEL103Persistance.Configuration.CommandPort))
            {
                KEL103Tools.ConfigureClient(device_address, client);

                await SetConstantPowerTarget(client, target_power);
            }
        }

        public static async Task SetConstantPowerTarget(UdpClient client, double target_power)
        {
            var tx_bytes = Encoding.ASCII.GetBytes(":POW " + KEL103Tools.FormatString(target_power) + "W\n");

            await client.SendAsync(tx_bytes, tx_bytes.Length);
        }

        public static async Task<double> GetConstantPowerTarget(IPAddress device_address)
        {
            using (UdpClient client = new UdpClient(KEL103Persistance.Configuration.CommandPort))
            {
                KEL103Tools.ConfigureClient(device_address, client);

                return await GetConstantPowerTarget(client);
            }
        }

        public static async Task<double> GetConstantPowerTarget(UdpClient client)
        {
            var tx_bytes = Encoding.ASCII.GetBytes(":POW?\n");

            await client.SendAsync(tx_bytes, tx_bytes.Length);

            var rx = (await client.ReceiveAsync()).Buffer;

            return Convert.ToDouble(Encoding.ASCII.GetString(rx).Split('W')[0]);
        }
    }
}
