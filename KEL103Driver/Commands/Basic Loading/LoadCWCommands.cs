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
        public static Task SetConstantPowerTarget(IPAddress device_address, double target_power)
        {
            using (UdpClient client = new UdpClient(KEL103Persistance.Configuration.CommandPort))
            {
                KEL103Tools.ConfigureClient(device_address, client);

                return SetConstantPowerTarget(client, target_power);
            }
        }

        public static Task SetConstantPowerTarget(UdpClient client, double target_power)
        {
            return Task.Run(() => { 
                var tx_bytes = Encoding.ASCII.GetBytes(":POW " + KEL103Tools.FormatString(target_power) + "W\n");

                client.Send(tx_bytes, tx_bytes.Length);
            });
        }

        public static Task<double> GetConstantPowerTarget(IPAddress device_address)
        {
            using (UdpClient client = new UdpClient(KEL103Persistance.Configuration.CommandPort))
            {
                KEL103Tools.ConfigureClient(device_address, client);

                return GetConstantPowerTarget(client);
            }
        }

        public static Task<double> GetConstantPowerTarget(UdpClient client)
        {
            return Task.Run(() => {
                var endpoint = client.Client.RemoteEndPoint as IPEndPoint;
                var tx_bytes = Encoding.ASCII.GetBytes(":POW?\n");

                client.Send(tx_bytes, tx_bytes.Length);

                var rx = client.Receive(ref endpoint);

                return Convert.ToDouble(Encoding.ASCII.GetString(rx).Split('W')[0]);
            });
        }
    }
}
