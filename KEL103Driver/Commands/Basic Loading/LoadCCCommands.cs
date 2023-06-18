using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace KEL103Driver
{
    public static partial class KEL103Command
    {
        /**
         * Sets the constant current target value.
         * 
         * @param device_address The address of the device.
         * @param target_current The the curent you'd like to set the device to.
         * @return none
         */
        public static Task SetConstantCurrentTarget(IPAddress device_address, double target_current)
        {
            using (UdpClient client = new UdpClient(KEL103Persistance.Configuration.CommandPort))
            {
                KEL103Tools.ConfigureClient(device_address, client);
                return SetConstantCurrentTarget(client, target_current);
            }
        }

        public static Task SetConstantCurrentTarget(UdpClient client, double target_current)
        {
            return Task.Run(() => {    
                var tx_bytes = Encoding.ASCII.GetBytes(":CURR " + KEL103Tools.FormatString(target_current) + "A\n");
                client.Send(tx_bytes, tx_bytes.Length);
            });
        }

        public static Task<double> GetConstantCurrentTarget(IPAddress device_address)
        {
            using (UdpClient client = new UdpClient(KEL103Persistance.Configuration.CommandPort))
            {
                KEL103Tools.ConfigureClient(device_address, client);

                return GetConstantCurrentTarget(client);
            }
        }

        public static Task<double> GetConstantCurrentTarget(UdpClient client)
        {
            return Task.Run(() => {
                var endpoint = client.Client.RemoteEndPoint as IPEndPoint;
                var tx_bytes = Encoding.ASCII.GetBytes(":CURR?\n");
                client.Send(tx_bytes, tx_bytes.Length);
                var rx = client.Receive(ref endpoint);

                return Convert.ToDouble(Encoding.ASCII.GetString(rx).Split('A')[0]);
            });
        }
    }
}
