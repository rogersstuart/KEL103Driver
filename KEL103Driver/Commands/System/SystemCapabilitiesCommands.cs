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
        public static Task<double> GetMaximumSupportedSystemInputVoltage(IPAddress device_address)
        {
            using (UdpClient client = new UdpClient(KEL103Persistance.Configuration.CommandPort))
            {
                KEL103Tools.ConfigureClient(device_address, client);

                return GetMaximumSupportedSystemInputVoltage(client);
            }
        }

        public static Task<double> GetMaximumSupportedSystemInputVoltage(UdpClient client)
        {
            return Task.Run(() => {
                var endpoint = client.Client.RemoteEndPoint as IPEndPoint;
                var tx_bytes = Encoding.ASCII.GetBytes(":VOLT:UPP?\n");

                client.Send(tx_bytes, tx_bytes.Length);

                var rx = client.Receive(ref endpoint);

                return Convert.ToDouble(Encoding.ASCII.GetString(rx).Split('V')[0]);
            });
        }

        public static Task<double> GetMinimumSupportedSystemInputVoltage(IPAddress device_address)
        {
            using (UdpClient client = new UdpClient(KEL103Persistance.Configuration.CommandPort))
            {
                KEL103Tools.ConfigureClient(device_address, client);

                return GetMinimumSupportedSystemInputVoltage(client);
            }
        }

        public static Task<double> GetMinimumSupportedSystemInputVoltage(UdpClient client)
        {
            return Task.Run(() => {
                var endpoint = client.Client.RemoteEndPoint as IPEndPoint;
                var tx_bytes = Encoding.ASCII.GetBytes(":VOLT:LOW?\n");

                client.Send(tx_bytes, tx_bytes.Length);

                var rx = client.Receive(ref endpoint);

                return Convert.ToDouble(Encoding.ASCII.GetString(rx).Split('V')[0]);
            });
        }

        public static Task<double> GetMaximumSupportedSystemInputCurrent(IPAddress device_address)
        {
            using (UdpClient client = new UdpClient(KEL103Persistance.Configuration.CommandPort))
            {
                KEL103Tools.ConfigureClient(device_address, client);

                return GetMaximumSupportedSystemInputCurrent(client);
            }
        }

        public static Task<double> GetMaximumSupportedSystemInputCurrent(UdpClient client)
        {
            return Task.Run(() => {
                var endpoint = client.Client.RemoteEndPoint as IPEndPoint;
                var tx_bytes = Encoding.ASCII.GetBytes(":CURR:UPP?\n");

                client.Send(tx_bytes, tx_bytes.Length);

                var rx = client.Receive(ref endpoint);

                return Convert.ToDouble(Encoding.ASCII.GetString(rx).Split('A')[0]);
            });
        }

        public static Task<double> GetMinimumSupportedSystemInputCurrent(IPAddress device_address)
        {
            using (UdpClient client = new UdpClient(KEL103Persistance.Configuration.CommandPort))
            {
                KEL103Tools.ConfigureClient(device_address, client);

                return GetMinimumSupportedSystemInputCurrent(client);
            }
        }

        public static Task<double> GetMinimumSupportedSystemInputCurrent(UdpClient client)
        {
            return Task.Run(() => {
                var endpoint = client.Client.RemoteEndPoint as IPEndPoint;
                var tx_bytes = Encoding.ASCII.GetBytes(":CURR:LOW?\n");

                client.Send(tx_bytes, tx_bytes.Length);

                var rx = client.Receive(ref endpoint);

                return Convert.ToDouble(Encoding.ASCII.GetString(rx).Split('A')[0]);
            });
        }

        public static Task<double> GetMaximumSupportedSystemInputResistance(IPAddress device_address)
        {
            using (UdpClient client = new UdpClient(KEL103Persistance.Configuration.CommandPort))
            {
                KEL103Tools.ConfigureClient(device_address, client);

                return GetMaximumSupportedSystemInputResistance(client);
            }
        }

        public static Task<double> GetMaximumSupportedSystemInputResistance(UdpClient client)
        {
            return Task.Run(() => {
                var endpoint = client.Client.RemoteEndPoint as IPEndPoint;
                var tx_bytes = Encoding.ASCII.GetBytes(":RES:UPP?\n");

                client.Send(tx_bytes, tx_bytes.Length);

                var rx = client.Receive(ref endpoint);

                return Convert.ToDouble(Encoding.ASCII.GetString(rx).Split('O')[0]);
            });
        }

        public static Task<double> GetMinimumSupportedSystemInputResistance(IPAddress device_address)
        {
            using (UdpClient client = new UdpClient(KEL103Persistance.Configuration.CommandPort))
            {
                KEL103Tools.ConfigureClient(device_address, client);

                return GetMinimumSupportedSystemInputResistance(client);
            }
        }

        public static Task<double> GetMinimumSupportedSystemInputResistance(UdpClient client)
        {
            return Task.Run(() => {
                var endpoint = client.Client.RemoteEndPoint as IPEndPoint;
                var tx_bytes = Encoding.ASCII.GetBytes(":RES:LOW?\n");

                client.Send(tx_bytes, tx_bytes.Length);

                var rx = client.Receive(ref endpoint);

                return Convert.ToDouble(Encoding.ASCII.GetString(rx).Split('O')[0]);
            });
        }

        public static Task<double> GetMaximumSupportedSystemInputPower(IPAddress device_address)
        {
            using (UdpClient client = new UdpClient(KEL103Persistance.Configuration.CommandPort))
            {
                KEL103Tools.ConfigureClient(device_address, client);

                return GetMaximumSupportedSystemInputPower(client);
            }
        }

        public static Task<double> GetMaximumSupportedSystemInputPower(UdpClient client)
        {
            return Task.Run(() => {
                var endpoint = client.Client.RemoteEndPoint as IPEndPoint;
                var tx_bytes = Encoding.ASCII.GetBytes(":POW:UPP?\n");

                client.Send(tx_bytes, tx_bytes.Length);

                var rx = client.Receive(ref endpoint);

                return Convert.ToDouble(Encoding.ASCII.GetString(rx).Split('W')[0]);
            });
        }

        public static Task<double> GetMinimumSupportedSystemInputPower(IPAddress device_address)
        {
            using (UdpClient client = new UdpClient(KEL103Persistance.Configuration.CommandPort))
            {
                KEL103Tools.ConfigureClient(device_address, client);

                return GetMinimumSupportedSystemInputPower(client);
            }
        }

        public static Task<double> GetMinimumSupportedSystemInputPower(UdpClient client)
        {
            return Task.Run(() => {
                var endpoint = client.Client.RemoteEndPoint as IPEndPoint;
                var tx_bytes = Encoding.ASCII.GetBytes(":POW:LOW?\n");

                client.Send(tx_bytes, tx_bytes.Length);

                var rx = client.Receive(ref endpoint);

                return Convert.ToDouble(Encoding.ASCII.GetString(rx).Split('W')[0]);
            });
        }
    }
}
