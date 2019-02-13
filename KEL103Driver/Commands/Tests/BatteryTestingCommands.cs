﻿using System;
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
            using (UdpClient client = new UdpClient(KEL103Configuration.command_port))
            {
                client.Client.ReceiveTimeout = 2000;
                client.Client.SendTimeout = 2000;
                client.DontFragment = false;

                client.Connect(device_address, KEL103Configuration.command_port);

                var tx_bytes = Encoding.ASCII.GetBytes(":RCL:BATT " + list_index + "\n");

                await client.SendAsync(tx_bytes, tx_bytes.Length);
            }
        }

        public static async Task QueryBatteryTestModeParameters(IPAddress device_address, int list_index)
        {
            using (UdpClient client = new UdpClient(KEL103Configuration.command_port))
            {
                client.Client.ReceiveTimeout = 2000;
                client.Client.SendTimeout = 2000;
                client.DontFragment = false;

                client.Connect(device_address, KEL103Configuration.command_port);

                var tx_bytes = Encoding.ASCII.GetBytes(":RCL:BATT?\n");

                await client.SendAsync(tx_bytes, tx_bytes.Length);

                var rx = (await client.ReceiveAsync()).Buffer;

                Console.WriteLine(Encoding.ASCII.GetString(rx));
            }
        }


        public static async Task SetBatteryTestModeParameters(IPAddress device_address, int list_index, double current_range,
            double discharge_current, double cutoff_voltage, double cutoff_capacity, double discharge_time)
        {
            using (UdpClient client = new UdpClient(KEL103Configuration.command_port))
            {
                client.Client.ReceiveTimeout = 2000;
                client.Client.SendTimeout = 2000;
                client.DontFragment = false;

                client.Connect(device_address, KEL103Configuration.command_port);

                var tx_bytes = Encoding.ASCII.GetBytes(":BATT " + list_index + "," + KEL103Tools.FormatString(current_range) +
                    "A," + KEL103Tools.FormatString(discharge_current) + "A," + KEL103Tools.FormatString(cutoff_voltage) + 
                    "V," + KEL103Tools.FormatString(cutoff_capacity) + "AH," + KEL103Tools.FormatString(discharge_time) + "S\n");

                Console.WriteLine(Encoding.ASCII.GetString(tx_bytes));

                await client.SendAsync(tx_bytes, tx_bytes.Length);
            }
        }
    }
}