using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace KEL103Driver
{
    public static class KEL103Tools
    {
        public static async Task<IPAddress> FindLoadAddress()
        {
            IPEndPoint search_endpoint = new IPEndPoint(KEL103Configuration.broadcast_address, KEL103Configuration.broadcast_port);

            //client.Open
            var tx_bytes = Encoding.ASCII.GetBytes("find_ka000");

            UdpClient udp_client = new UdpClient();
            udp_client.Client.Bind(new IPEndPoint(IPAddress.Any, KEL103Configuration.broadcast_port));
            var from = new IPEndPoint(0, 0);

            udp_client.Client.ReceiveTimeout = KEL103Configuration.read_timeout;
            udp_client.Client.SendTimeout = KEL103Configuration.write_timeout;

            var feed = new List<string>();

            var comm_wait = true;

            Task.Run(() =>
            {
                try
                {
                    while (true)
                    {

                        var rx = udp_client.Receive(ref from);
                        var line = Encoding.ASCII.GetString(rx);
                        feed.AddRange(line.Split('\n'));
                    }
                }
                catch (Exception ex) {/* looks like we timed out */ comm_wait = false; }
            });

            udp_client.Send(tx_bytes, tx_bytes.Length, search_endpoint);

            while (comm_wait)
                await Task.Delay(1);

            udp_client.Close();

            //find the ip address
            IPAddress load_address = null;
            foreach (var str in feed)
                if (IPAddress.TryParse(str, out load_address))
                    break;

            if (load_address == null)
                throw new Exception("couldn't find a valid load address");

            return load_address;
        }

        public static string FormatString(double d)
        {
            int s = Math.Abs(d).ToString("####0").Length;
            int precision = 5 - s;
            string format = String.Format("###0.{0};###0.{0}", new String('0', precision));

            return d.ToString(format);
        }

        public static void ConfigureClient(IPAddress device_address, UdpClient client)
        {
            client.Client.ReceiveTimeout = KEL103Configuration.read_timeout;
            client.Client.SendTimeout = KEL103Configuration.write_timeout;
            client.DontFragment = false;

            client.Connect(device_address, KEL103Configuration.command_port);
        }
    }
}
