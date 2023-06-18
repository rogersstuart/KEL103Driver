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
        public static Task SetConstantResistanceTarget(IPAddress device_address, double target_resistance)
        {
            using (UdpClient client = new UdpClient(KEL103Persistance.Configuration.CommandPort))
            {
                KEL103Tools.ConfigureClient(device_address, client);

                return SetConstantResistanceTarget(client, target_resistance);
            }
        }

        public static Task SetConstantResistanceTarget(UdpClient client, double target_resistance)
        {
            var tx_bytes = Encoding.ASCII.GetBytes(":RES " + KEL103Tools.FormatString(target_resistance) + "OHM\n");

            return Task.Run(() => {  client.Send(tx_bytes, tx_bytes.Length); });
        }

        public static Task<double> GetConstantResistanceTarget(IPAddress device_address)
        {
            using (UdpClient client = new UdpClient(KEL103Persistance.Configuration.CommandPort))
            {
                KEL103Tools.ConfigureClient(device_address, client);

                return GetConstantResistanceTarget(client);
            }
        }

        public static Task<double> GetConstantResistanceTarget(UdpClient client)
        {
            return Task.Run(() => {
                var endpoint = client.Client.RemoteEndPoint as IPEndPoint;
                var tx_bytes = Encoding.ASCII.GetBytes(":RES?\n");

             client.Send(tx_bytes, tx_bytes.Length);

            var rx = client.Receive(ref endpoint);

            return Convert.ToDouble(Encoding.ASCII.GetString(rx).Split('O')[0]);
            });
        }
    }
}
