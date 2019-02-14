using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace KEL103Driver
{
    public static class KEL103Tools
    {
        public static async Task<IPAddress> FindLoadAddress()
        {
            if (!KEL103Persistance.Configuration.EnableLoadSearch)
                return KEL103Persistance.Configuration.LoadAddress;
            
            //get list of all network interfaces and start a search

            var broadcast_addresses = new List<IPAddress>();
            if (KEL103Persistance.Configuration.EnableInterfaceSearch)
            {
                foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
                {
                    try
                    {
                        if (nic.OperationalStatus == OperationalStatus.Up)
                        {
                            var parts = nic.GetIPProperties().UnicastAddresses[1].Address.ToString().Split('.');
                            parts[3] = "255";
                            broadcast_addresses.Add(IPAddress.Parse(parts[0] + "." + parts[1] + "." + parts[2] + "." + parts[3]));
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
            else
                broadcast_addresses.Add(KEL103Persistance.Configuration.BroadcastAddress);


            var load_addresses = new List<IPAddress>();
            foreach(var address in broadcast_addresses)
            {
                IPEndPoint search_endpoint = new IPEndPoint(address, KEL103Persistance.Configuration.BroadcastPort);

                var tx_bytes = KEL103Persistance.Configuration.SearchMessage;

                using (UdpClient udp_client = new UdpClient())
                {
                    udp_client.Client.Bind(new IPEndPoint(IPAddress.Any, KEL103Persistance.Configuration.BroadcastPort));
                    var from = new IPEndPoint(0, 0);

                    udp_client.Client.ReceiveTimeout = KEL103Persistance.Configuration.ReadTimeout;
                    udp_client.Client.SendTimeout = KEL103Persistance.Configuration.WriteTimeout;

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
                            load_addresses.Add(IPAddress.Parse(str));
                }       
            }

            if (load_addresses.Count() == 0)
                throw new Exception("couldn't find a valid load address");

            var cfg = KEL103Persistance.Configuration;
            cfg.LoadAddressString = load_addresses[0].ToString();
            KEL103Persistance.Configuration = cfg;

            return load_addresses[0];
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
            client.Client.ReceiveTimeout = KEL103Persistance.Configuration.ReadTimeout;
            client.Client.SendTimeout = KEL103Persistance.Configuration.WriteTimeout;
            client.DontFragment = false;

            client.Connect(device_address, KEL103Persistance.Configuration.CommandPort);
        }
    }
}
