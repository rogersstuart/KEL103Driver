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
        public static async Task<double> GetMaximumSupportedSystemInputVoltage(IPAddress device_address)
        {
            using (UdpClient client = new UdpClient(KEL103Persistance.Configuration.CommandPort))
            {
                KEL103Tools.ConfigureClient(device_address, client);

                return await GetMaximumSupportedSystemInputVoltage(client);
            }
        }

        public static async Task<double> GetMaximumSupportedSystemInputVoltage(UdpClient client)
        {
            var tx_bytes = Encoding.ASCII.GetBytes(":VOLT:UPP?\n");

            await client.SendAsync(tx_bytes, tx_bytes.Length);

            var rx = (await client.ReceiveAsync()).Buffer;

            return Convert.ToDouble(Encoding.ASCII.GetString(rx).Split('V')[0]);
        }

        public static async Task<double> GetMinimumSupportedSystemInputVoltage(IPAddress device_address)
        {
            using (UdpClient client = new UdpClient(KEL103Persistance.Configuration.CommandPort))
            {
                KEL103Tools.ConfigureClient(device_address, client);

                return await GetMinimumSupportedSystemInputVoltage(client);
            }
        }

        public static async Task<double> GetMinimumSupportedSystemInputVoltage(UdpClient client)
        {
            var tx_bytes = Encoding.ASCII.GetBytes(":VOLT:LOW?\n");

            await client.SendAsync(tx_bytes, tx_bytes.Length);

            var rx = (await client.ReceiveAsync()).Buffer;

            return Convert.ToDouble(Encoding.ASCII.GetString(rx).Split('V')[0]);
        }

        public static async Task<double> GetMaximumSupportedSystemInputCurrent(IPAddress device_address)
        {
            using (UdpClient client = new UdpClient(KEL103Persistance.Configuration.CommandPort))
            {
                KEL103Tools.ConfigureClient(device_address, client);

                return await GetMaximumSupportedSystemInputCurrent(client);
            }
        }

        public static async Task<double> GetMaximumSupportedSystemInputCurrent(UdpClient client)
        {
            var tx_bytes = Encoding.ASCII.GetBytes(":CURR:UPP?\n");

            await client.SendAsync(tx_bytes, tx_bytes.Length);

            var rx = (await client.ReceiveAsync()).Buffer;

            return Convert.ToDouble(Encoding.ASCII.GetString(rx).Split('A')[0]);
        }

        public static async Task<double> GetMinimumSupportedSystemInputCurrent(IPAddress device_address)
        {
            using (UdpClient client = new UdpClient(KEL103Persistance.Configuration.CommandPort))
            {
                KEL103Tools.ConfigureClient(device_address, client);

                return await GetMinimumSupportedSystemInputCurrent(client);
            }
        }

        public static async Task<double> GetMinimumSupportedSystemInputCurrent(UdpClient client)
        {
            var tx_bytes = Encoding.ASCII.GetBytes(":CURR:LOW?\n");

            await client.SendAsync(tx_bytes, tx_bytes.Length);

            var rx = (await client.ReceiveAsync()).Buffer;

            return Convert.ToDouble(Encoding.ASCII.GetString(rx).Split('A')[0]);
        }

        public static async Task<double> GetMaximumSupportedSystemInputResistance(IPAddress device_address)
        {
            using (UdpClient client = new UdpClient(KEL103Persistance.Configuration.CommandPort))
            {
                KEL103Tools.ConfigureClient(device_address, client);

                return await GetMaximumSupportedSystemInputResistance(client);
            }
        }

        public static async Task<double> GetMaximumSupportedSystemInputResistance(UdpClient client)
        {
            var tx_bytes = Encoding.ASCII.GetBytes(":RES:UPP?\n");

            await client.SendAsync(tx_bytes, tx_bytes.Length);

            var rx = (await client.ReceiveAsync()).Buffer;

            return Convert.ToDouble(Encoding.ASCII.GetString(rx).Split('O')[0]);
        }

        public static async Task<double> GetMinimumSupportedSystemInputResistance(IPAddress device_address)
        {
            using (UdpClient client = new UdpClient(KEL103Persistance.Configuration.CommandPort))
            {
                KEL103Tools.ConfigureClient(device_address, client);

                return await GetMinimumSupportedSystemInputResistance(client);
            }
        }

        public static async Task<double> GetMinimumSupportedSystemInputResistance(UdpClient client)
        {
            var tx_bytes = Encoding.ASCII.GetBytes(":RES:LOW?\n");

            await client.SendAsync(tx_bytes, tx_bytes.Length);

            var rx = (await client.ReceiveAsync()).Buffer;

            return Convert.ToDouble(Encoding.ASCII.GetString(rx).Split('O')[0]);
        }

        public static async Task<double> GetMaximumSupportedSystemInputPower(IPAddress device_address)
        {
            using (UdpClient client = new UdpClient(KEL103Persistance.Configuration.CommandPort))
            {
                KEL103Tools.ConfigureClient(device_address, client);

                return await GetMaximumSupportedSystemInputPower(client);
            }
        }

        public static async Task<double> GetMaximumSupportedSystemInputPower(UdpClient client)
        {
            var tx_bytes = Encoding.ASCII.GetBytes(":POW:UPP?\n");

            await client.SendAsync(tx_bytes, tx_bytes.Length);

            var rx = (await client.ReceiveAsync()).Buffer;

            return Convert.ToDouble(Encoding.ASCII.GetString(rx).Split('W')[0]);
        }

        public static async Task<double> GetMinimumSupportedSystemInputPower(IPAddress device_address)
        {
            using (UdpClient client = new UdpClient(KEL103Persistance.Configuration.CommandPort))
            {
                KEL103Tools.ConfigureClient(device_address, client);

                return await GetMinimumSupportedSystemInputPower(client);
            }
        }

        public static async Task<double> GetMinimumSupportedSystemInputPower(UdpClient client)
        {
            var tx_bytes = Encoding.ASCII.GetBytes(":POW:LOW?\n");

            await client.SendAsync(tx_bytes, tx_bytes.Length);

            var rx = (await client.ReceiveAsync()).Buffer;

            return Convert.ToDouble(Encoding.ASCII.GetString(rx).Split('W')[0]);
        }
    }
}
