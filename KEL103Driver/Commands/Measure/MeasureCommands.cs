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
        public static async Task<double> MeasureVoltage(IPAddress device_address)
        {
            using (UdpClient client = new UdpClient(KEL103Persistance.Configuration.CommandPort))
            {
                KEL103Tools.ConfigureClient(device_address, client);

                return await MeasureVoltage(client);
            }
        }

        public static async Task<double> MeasureVoltage(UdpClient client)
        {
            var tx_bytes = Encoding.ASCII.GetBytes(":MEAS:VOLT?\n");

            await client.SendAsync(tx_bytes, tx_bytes.Length);

            var rx = (await client.ReceiveAsync()).Buffer;

            return Convert.ToDouble(Encoding.ASCII.GetString(rx).Split('V')[0]);
        }

        public static async Task<double> MeasureCurrent(IPAddress device_address)
        {
            using (UdpClient client = new UdpClient(KEL103Persistance.Configuration.CommandPort))
            {
                KEL103Tools.ConfigureClient(device_address, client);

                return await MeasureCurrent(client);
            }
        }

        public static async Task<double> MeasureCurrent(UdpClient client)
        {
            var tx_bytes = Encoding.ASCII.GetBytes(":MEAS:CURR?\n");

            await client.SendAsync(tx_bytes, tx_bytes.Length);

            var rx = (await client.ReceiveAsync()).Buffer;

            return Convert.ToDouble(Encoding.ASCII.GetString(rx).Split('A')[0]);
        }

        public static async Task<double> MeasurePower(IPAddress device_address)
        {
            using (UdpClient client = new UdpClient(KEL103Persistance.Configuration.CommandPort))
            {
                KEL103Tools.ConfigureClient(device_address, client);

                return await MeasurePower(client);
            }
        }

        public static async Task<double> MeasurePower(UdpClient client)
        {
            var tx_bytes = Encoding.ASCII.GetBytes(":MEAS:POW?\n");

            await client.SendAsync(tx_bytes, tx_bytes.Length);

            var rx = (await client.ReceiveAsync()).Buffer;

            return Convert.ToDouble(Encoding.ASCII.GetString(rx).Split('W')[0]);
        }

        public static async Task<double> MeasureResistance(IPAddress device_address)
        {
            using (UdpClient client = new UdpClient(KEL103Persistance.Configuration.CommandPort))
            {
                KEL103Tools.ConfigureClient(device_address, client);

                return await MeasureResistance(client);
            }
        }

        public static async Task<double> MeasureResistance(UdpClient client)
        {
            var voltage = await MeasureVoltage(client);
            var current = await MeasureCurrent(client);

            //i'm not sure what the theory on this is. if the voltage drop is 0 then the resistor is perfect and the resistance is 0.
            //with current, we're supposed to know how much is flowing through the resistor. 0 current is invalid.
            //nan is annoying and fake though, and in real life I doubt that the value would truely be zero it would just be really small.
            //so I'm assuming we'd have a massive voltage across the however many terraohms of resitance with no detectable drop or flow so I'm just going to return positive infinity.

            if (current == 0.0)
                return Double.PositiveInfinity;
            else
                return voltage / current;
        }
    }
}
