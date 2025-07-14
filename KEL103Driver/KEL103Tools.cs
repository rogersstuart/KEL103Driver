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
        /**
         * Search for a load on the network. Could be expanded to find all loads on the network.
         * 
         * @params none
         * @return A task which will result in an IPAddress
         */
        public static IPAddress FindLoadAddress()
        {
            if (!KEL103Persistance.Configuration.EnableLoadSearch)
                return KEL103Persistance.Configuration.LoadAddress;

            var broadcast_addresses = new List<IPAddress>();
            if (KEL103Persistance.Configuration.EnableInterfaceSearch)
            {
                foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
                {
                    try
                    {
                        if (nic.OperationalStatus == OperationalStatus.Up && 
                            nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                        {
                            var ipProps = nic.GetIPProperties();
                            foreach (var unicastAddress in ipProps.UnicastAddresses)
                            {
                                if (unicastAddress.Address.AddressFamily == AddressFamily.InterNetwork)
                                {
                                    // Calculate broadcast address using subnet mask
                                    var ipBytes = unicastAddress.Address.GetAddressBytes();
                                    var maskBytes = unicastAddress.IPv4Mask.GetAddressBytes();
                                    var broadcastBytes = new byte[4];
                                    for (int i = 0; i < 4; i++)
                                    {
                                        broadcastBytes[i] = (byte)(ipBytes[i] | ~maskBytes[i]);
                                    }
                                    broadcast_addresses.Add(new IPAddress(broadcastBytes));
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Interface error: {ex.Message}");
                    }
                }
            }
            else
            {
                broadcast_addresses.Add(KEL103Persistance.Configuration.BroadcastAddress);
            }

            if (broadcast_addresses.Count == 0)
            {
                throw new Exception("No valid network interfaces found");
            }

            var load_addresses = new List<IPAddress>();
            foreach (var address in broadcast_addresses)
            {
                IPEndPoint search_endpoint = new IPEndPoint(address, KEL103Persistance.Configuration.BroadcastPort);
                var tx_bytes = KEL103Persistance.Configuration.SearchMessage;

                using (UdpClient udp_client = new UdpClient(0)) // Bind to random port
                {
                    try
                    {
                        udp_client.EnableBroadcast = true;
                        udp_client.Client.ReceiveTimeout = KEL103Persistance.Configuration.ReadTimeout;
                        udp_client.Client.SendTimeout = KEL103Persistance.Configuration.WriteTimeout;

                        var feed = new List<string>();
                        udp_client.Send(tx_bytes, tx_bytes.Length, search_endpoint);

                        try
                        {
                            IPEndPoint from = null;
                            while (true)
                            {
                                var rx = udp_client.Receive(ref from);
                                var line = Encoding.ASCII.GetString(rx);
                                feed.AddRange(line.Split('\n'));
                            }
                        }
                        catch (SocketException ex) when (ex.SocketErrorCode == SocketError.TimedOut) 
                        { 
                            // Expected timeout
                        }

                        foreach (var str in feed.Where(s => !string.IsNullOrWhiteSpace(s)))
                        {
                            if (IPAddress.TryParse(str, out var load_address))
                            {
                                load_addresses.Add(load_address);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Search error on {address}: {ex.Message}");
                    }
                }
            }

            if (load_addresses.Count == 0)
                throw new Exception("Couldn't find a valid load address");

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
