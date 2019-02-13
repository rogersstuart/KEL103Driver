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
            IPEndPoint search_endpoint = new IPEndPoint(IPAddress.Parse("192.168.1.255"), 18191);

            //client.Open
            var tx_bytes = Encoding.ASCII.GetBytes("find_ka000");

            UdpClient udp_client = new UdpClient();
            udp_client.Client.Bind(new IPEndPoint(IPAddress.Any, KEL103Configuration.broadcast_port));
            var from = new IPEndPoint(0, 0);

            udp_client.Client.ReceiveTimeout = 2000;
            udp_client.Client.SendTimeout = 2000;

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
    }
}
