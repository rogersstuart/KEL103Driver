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
        public static async Task SetConstantVoltageTarget(IPAddress device_address, double target_voltage)
        {
            using (UdpClient client = new UdpClient(KEL103Configuration.command_port))
            {
                client.Client.ReceiveTimeout = 2000;
                client.Client.SendTimeout = 2000;

                client.Connect(device_address, KEL103Configuration.command_port);

                var tx_bytes = Encoding.ASCII.GetBytes(":VOLT " + String.Format("{0:F5}", Math.Round(target_voltage, 5)) + "V\n");

                await client.SendAsync(tx_bytes, tx_bytes.Length);
            }
        }

        public static async Task<double> GetConstantVoltageTarget(IPAddress device_address)
        {
            using (UdpClient client = new UdpClient(KEL103Configuration.command_port))
            {
                client.Client.ReceiveTimeout = 2000;
                client.Client.SendTimeout = 2000;

                client.Connect(device_address, KEL103Configuration.command_port);

                var tx_bytes = Encoding.ASCII.GetBytes(":VOLT?\n");

                await client.SendAsync(tx_bytes, tx_bytes.Length);

                var rx = (await client.ReceiveAsync()).Buffer;

                return Convert.ToDouble(Encoding.ASCII.GetString(rx).Split('V')[0]);
            }
        }
    }
}
