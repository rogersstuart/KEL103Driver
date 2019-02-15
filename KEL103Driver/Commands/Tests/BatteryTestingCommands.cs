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
        
        public static async Task RecallBatteryTestModeParameters(IPAddress device_address, int list_index)
        {
            using (UdpClient client = new UdpClient(KEL103Persistance.Configuration.CommandPort))
            {
                KEL103Tools.ConfigureClient(device_address, client);

                await RecallBatteryTestModeParameters(client, list_index);
            }
        }

        public static async Task RecallBatteryTestModeParameters(UdpClient client, int list_index)
        {
            var tx_bytes = Encoding.ASCII.GetBytes(":RCL:BATT " + list_index + "\n");

            await client.SendAsync(tx_bytes, tx_bytes.Length);
        }

        //recall before querying
        public static async Task<string> QueryBatteryTestModeParameters(IPAddress device_address, int list_index)
        {
            using (UdpClient client = new UdpClient(KEL103Persistance.Configuration.CommandPort))
            {
                KEL103Tools.ConfigureClient(device_address, client);

                return await QueryBatteryTestModeParameters(client, list_index);
            }
        }

        //recall before querying
        public static async Task<string> QueryBatteryTestModeParameters(UdpClient client, int list_index)
        {
            var tx_bytes = Encoding.ASCII.GetBytes(":RCL:BATT?\n");

            await client.SendAsync(tx_bytes, tx_bytes.Length);

            var rx = (await client.ReceiveAsync()).Buffer;

            return Encoding.ASCII.GetString(rx);
        }

        public static async Task SetBatteryTestModeParameters(IPAddress device_address, int list_index, double current_range,
            double discharge_current, double cutoff_voltage, double cutoff_capacity, double discharge_time)
        {
            using (UdpClient client = new UdpClient(KEL103Persistance.Configuration.CommandPort))
            {
                KEL103Tools.ConfigureClient(device_address, client);

                await SetBatteryTestModeParameters(client, list_index, current_range, discharge_current, cutoff_voltage, cutoff_capacity, discharge_time);
            }
        }

        public static async Task SetBatteryTestModeParameters(UdpClient client, int list_index, double current_range,
            double discharge_current, double cutoff_voltage, double cutoff_capacity, double discharge_time)
        {
            var tx_bytes = Encoding.ASCII.GetBytes(":BATT " + list_index + "," + KEL103Tools.FormatString(current_range) +
                "A," + KEL103Tools.FormatString(discharge_current) + "A," + KEL103Tools.FormatString(cutoff_voltage) +
                "V," + KEL103Tools.FormatString(cutoff_capacity) + "AH," + KEL103Tools.FormatString(discharge_time) + "S\n");

            await client.SendAsync(tx_bytes, tx_bytes.Length);
        }

        public static async Task<double> QueryBatteryTestTimer(IPAddress device_address)
        {
            using (UdpClient client = new UdpClient(KEL103Persistance.Configuration.CommandPort))
            {
                KEL103Tools.ConfigureClient(device_address, client);

                return await QueryBatteryTestTimer(client);
            }
        }

        public static async Task<double> QueryBatteryTestTimer(UdpClient client)
        {
            var tx_bytes = Encoding.ASCII.GetBytes(":BATT:TIM?\n");

            await client.SendAsync(tx_bytes, tx_bytes.Length);

            var rx = (await client.ReceiveAsync()).Buffer;

            return Convert.ToDouble(Encoding.ASCII.GetString(rx).Split('S')[0]);
        }

        public static async Task<double> QueryBatteryTestCapacityCounter(IPAddress device_address)
        {
            using (UdpClient client = new UdpClient(KEL103Persistance.Configuration.CommandPort))
            {
                KEL103Tools.ConfigureClient(device_address, client);

                return await QueryBatteryTestCapacityCounter(client);
            }
        }

        public static async Task<double> QueryBatteryTestCapacityCounter(UdpClient client)
        {
            var tx_bytes = Encoding.ASCII.GetBytes(":BATT:CAP?\n");

            await client.SendAsync(tx_bytes, tx_bytes.Length);

            var rx = (await client.ReceiveAsync()).Buffer;

            return Convert.ToDouble(Encoding.ASCII.GetString(rx).Split('A')[0]);
        }
    }
}
