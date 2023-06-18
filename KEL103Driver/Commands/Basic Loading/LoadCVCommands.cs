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
        public static Task SetConstantVoltageTarget(IPAddress device_address, double target_voltage)
        {
            using (UdpClient client = new UdpClient(KEL103Persistance.Configuration.CommandPort))
            {
                KEL103Tools.ConfigureClient(device_address, client);
                return SetConstantVoltageTarget(client, target_voltage);
            }
        }

        public static Task SetConstantVoltageTarget(UdpClient client, double target_voltage)
        {
            return Task.Run(() => { 
                var tx_bytes = Encoding.ASCII.GetBytes(":VOLT " + KEL103Tools.FormatString(target_voltage) + "V\n");
                client.Send(tx_bytes, tx_bytes.Length);
            });
        }

        public static Task<double> GetConstantVoltageTarget(IPAddress device_address)
        {
            using (UdpClient client = new UdpClient(KEL103Persistance.Configuration.CommandPort))
            {
                KEL103Tools.ConfigureClient(device_address, client);

                return GetConstantVoltageTarget(client);
            }
        }

        public static Task<double> GetConstantVoltageTarget(UdpClient client)
        {
            return Task.Run(() => {
                var endpoint = client.Client.RemoteEndPoint as IPEndPoint;
                var tx_bytes = Encoding.ASCII.GetBytes(":VOLT?\n");
                client.Send(tx_bytes, tx_bytes.Length);
                var rx = client.Receive(ref endpoint);
                return Convert.ToDouble(Encoding.ASCII.GetString(rx).Split('V')[0]);
            });
        }
    }
}
