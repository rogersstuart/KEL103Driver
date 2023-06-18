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
        
        public static Task RecallBatteryTestModeParameters(IPAddress device_address, int list_index)
        {
            using (UdpClient client = new UdpClient(KEL103Persistance.Configuration.CommandPort))
            {
                KEL103Tools.ConfigureClient(device_address, client);

                return RecallBatteryTestModeParameters(client, list_index);
            }
        }

        public static Task RecallBatteryTestModeParameters(UdpClient client, int list_index)
        {
            return Task.Run(() => { 
            var tx_bytes = Encoding.ASCII.GetBytes(":RCL:BATT " + list_index + "\n");

             client.Send(tx_bytes, tx_bytes.Length);
            });
        }

        //recall before querying
        public static Task<string> QueryBatteryTestModeParameters(IPAddress device_address, int list_index)
        {
            using (UdpClient client = new UdpClient(KEL103Persistance.Configuration.CommandPort))
            {
                KEL103Tools.ConfigureClient(device_address, client);

                return QueryBatteryTestModeParameters(client, list_index);
            }
        }

        //recall before querying
        public static Task<string> QueryBatteryTestModeParameters(UdpClient client, int list_index)
        {
            return Task.Run(() => {
                var endpoint = client.Client.RemoteEndPoint as IPEndPoint;
                var tx_bytes = Encoding.ASCII.GetBytes(":RCL:BATT?\n");

                 client.Send(tx_bytes, tx_bytes.Length);

                var rx = client.Receive(ref endpoint);

                return Encoding.ASCII.GetString(rx);
            });
        }

        public static Task SetBatteryTestModeParameters(IPAddress device_address, int list_index, double current_range,
            double discharge_current, double cutoff_voltage, double cutoff_capacity, double discharge_time)
        {
            using (UdpClient client = new UdpClient(KEL103Persistance.Configuration.CommandPort))
            {
                KEL103Tools.ConfigureClient(device_address, client);

                return SetBatteryTestModeParameters(client, list_index, current_range, discharge_current, cutoff_voltage, cutoff_capacity, discharge_time);
            }
        }

        public static Task SetBatteryTestModeParameters(UdpClient client, int list_index, double current_range,
            double discharge_current, double cutoff_voltage, double cutoff_capacity, double discharge_time)
        {
            return Task.Run(() => { 
            var tx_bytes = Encoding.ASCII.GetBytes(":BATT " + list_index + "," + KEL103Tools.FormatString(current_range) +
                "A," + KEL103Tools.FormatString(discharge_current) + "A," + KEL103Tools.FormatString(cutoff_voltage) +
                "V," + KEL103Tools.FormatString(cutoff_capacity) + "AH," + KEL103Tools.FormatString(discharge_time) + "S\n");

             client.Send(tx_bytes, tx_bytes.Length);
            });
        }

        public static Task<double> QueryBatteryTestTimer(IPAddress device_address)
        {
            using (UdpClient client = new UdpClient(KEL103Persistance.Configuration.CommandPort))
            {
                KEL103Tools.ConfigureClient(device_address, client);

                return QueryBatteryTestTimer(client);
            }
        }

        public static Task<double> QueryBatteryTestTimer(UdpClient client)
        {
            return Task.Run(() => {
                var endpoint = client.Client.RemoteEndPoint as IPEndPoint;
                var tx_bytes = Encoding.ASCII.GetBytes(":BATT:TIM?\n");

                 client.Send(tx_bytes, tx_bytes.Length);

                var rx = client.Receive(ref endpoint);

                return Convert.ToDouble(Encoding.ASCII.GetString(rx).Split('S')[0]);
            });
        }

        public static Task<double> QueryBatteryTestCapacityCounter(IPAddress device_address)
        {
            using (UdpClient client = new UdpClient(KEL103Persistance.Configuration.CommandPort))
            {
                KEL103Tools.ConfigureClient(device_address, client);

                return QueryBatteryTestCapacityCounter(client);
            }
        }

        public static Task<double> QueryBatteryTestCapacityCounter(UdpClient client)
        {
            return Task.Run(() => {
                var endpoint = client.Client.RemoteEndPoint as IPEndPoint; 
                var tx_bytes = Encoding.ASCII.GetBytes(":BATT:CAP?\n");

                 client.Send(tx_bytes, tx_bytes.Length);

                var rx = client.Receive(ref endpoint);

                return Convert.ToDouble(Encoding.ASCII.GetString(rx).Split('A')[0]);
            });
        }
    }
}
