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
        public static Task<string> Identify(IPAddress device_address)
        {
            using (UdpClient client = new UdpClient(KEL103Persistance.Configuration.CommandPort))
            {
                KEL103Tools.ConfigureClient(device_address, client);

                return Identify(client);
            }
        }

        public static Task<string> Identify(UdpClient client)
        {
            return Task.Run(() => {
                var endpoint = client.Client.RemoteEndPoint as IPEndPoint;
                var tx_bytes = Encoding.ASCII.GetBytes("*IDN?\n");

                client.Send(tx_bytes, tx_bytes.Length);

                var rx = client.Receive(ref endpoint);

                return Encoding.ASCII.GetString(rx).Split('\n')[0];
            });
        }

        public static Task StoreToUnit(IPAddress device_address, int location_index)
        {
            using (UdpClient client = new UdpClient(KEL103Persistance.Configuration.CommandPort))
            {
                KEL103Tools.ConfigureClient(device_address, client);
                return StoreToUnit(client, location_index);
            }
        }

        public static Task StoreToUnit(UdpClient client, int location_index)
        {
            return Task.Run(() => {
                var tx_bytes = Encoding.ASCII.GetBytes("*SAV " + location_index + "\n");

                client.Send(tx_bytes, tx_bytes.Length);
            });
        }

        public static Task RecallToUnit(IPAddress device_address, int location_index)
        {
            using (UdpClient client = new UdpClient(KEL103Persistance.Configuration.CommandPort))
            {
                KEL103Tools.ConfigureClient(device_address, client);

                return RecallToUnit(client, location_index);
            }
        }

        public static Task RecallToUnit(UdpClient client, int location_index)
        {
            return Task.Run(() => {
                var tx_bytes = Encoding.ASCII.GetBytes("*RCL " + location_index + "\n");

                client.Send(tx_bytes, tx_bytes.Length);
            });
        }

        public static Task SetLoadInputSwitchState(IPAddress device_address, bool switch_state) //true is on, false is off
        {
            using (UdpClient client = new UdpClient(KEL103Persistance.Configuration.CommandPort))
            {
                KEL103Tools.ConfigureClient(device_address, client);

                return SetLoadInputSwitchState(client, switch_state);
            }
        }

        public static Task SetLoadInputSwitchState(UdpClient client, bool switch_state) //true is on, false is off
        {
            return Task.Run(() => {
                var tx_bytes = Encoding.ASCII.GetBytes(":INP " + (switch_state ? "ON" : "OFF") + "\n");

                client.Send(tx_bytes, tx_bytes.Length);
            });
        }

        public static Task<bool> GetLoadInputSwitchState(IPAddress device_address)  //true is on, false is off
        {
            using (UdpClient client = new UdpClient(KEL103Persistance.Configuration.CommandPort))
            {
                KEL103Tools.ConfigureClient(device_address, client);

                return GetLoadInputSwitchState(client);
            }
        }

        public static Task<bool> GetLoadInputSwitchState(UdpClient client)  //true is on, false is off
        {
            return Task.Run(() => {
                var endpoint = client.Client.RemoteEndPoint as IPEndPoint;
                var tx_bytes = Encoding.ASCII.GetBytes(":INP?\n");

                client.Send(tx_bytes, tx_bytes.Length);

                var rx = client.Receive(ref endpoint);

                return Encoding.ASCII.GetString(rx).Split('\n')[0] == "ON" ? true : false;
            });
        }

        public static Task SimulateTrigger(IPAddress device_address)
        {
            using (UdpClient client = new UdpClient(KEL103Persistance.Configuration.CommandPort))
            {
                KEL103Tools.ConfigureClient(device_address, client);

                return SimulateTrigger(client);
            }
        }

        public static Task SimulateTrigger(UdpClient client)
        {
            return Task.Run(() => {
                var tx_bytes = Encoding.ASCII.GetBytes("*TRG\n");

                client.Send(tx_bytes, tx_bytes.Length);
            });
        }

        /*
        public static async Task SetSystemParameter(IPAddress device_address, int parameter, bool state)
        {
            using (UdpClient client = new UdpClient(KEL103Configuration.command_port))
            {
                KEL103Tools.ConfigureClient(device_address, client);

                var tx_bytes = Encoding.ASCII.GetBytes(":SYST:BEEP OFF\n");

                await client.SendAsync(tx_bytes, tx_bytes.Length);
            }
        }
        */
    }
}
