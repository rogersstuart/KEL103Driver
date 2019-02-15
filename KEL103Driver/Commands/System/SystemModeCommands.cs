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
        public static readonly int CONSTANT_VOLTAGE_MODE = 0;
        public static readonly int CONSTANT_CURRENT_MODE = 1;
        public static readonly int CONSTANT_RESISTANCE_MODE = 2;
        public static readonly int CONSTANT_POWER_MODE = 3;
        public static readonly int SHORT_MODE = 4;

        public static readonly string[] mode_strings = {
            "Constant Voltage Mode",
            "Constant Current Mode",
            "Constant Resistance Mode",
            "Constant Power Mode",
            "Input Short Circuit Mode" };

        private static readonly string[] mode_conversion_strings = { "CV","CC","CR","CW","SHORt"};

        public static async Task<int> GetSystemMode(IPAddress device_address)
        {
            using (UdpClient client = new UdpClient(KEL103Persistance.Configuration.CommandPort))
            {
                KEL103Tools.ConfigureClient(device_address, client);

                return await GetSystemMode(client);
            }
        }

        public static async Task<int> GetSystemMode(UdpClient client)
        {
            var tx_bytes = Encoding.ASCII.GetBytes(":FUNC?\n");

            await client.SendAsync(tx_bytes, tx_bytes.Length);

            var rx = (await client.ReceiveAsync()).Buffer;

            return mode_conversion_strings.Select((x, i) => new { x, i })
                .Where(y => y.x.Equals(Encoding.ASCII.GetString(rx).Split('\n')[0])).ToArray()[0].i;
        }

        public static async Task SetSystemMode(IPAddress device_address, int mode)
        {
            using (UdpClient client = new UdpClient(KEL103Persistance.Configuration.CommandPort))
            {
                KEL103Tools.ConfigureClient(device_address, client);

                await SetSystemMode(client, mode);
            }
        }

        public static async Task SetSystemMode(UdpClient client, int mode)
        {
            var tx_bytes = Encoding.ASCII.GetBytes(":FUNC " + mode_conversion_strings[mode] + "\n");

            await client.SendAsync(tx_bytes, tx_bytes.Length);
        }
    }
}
