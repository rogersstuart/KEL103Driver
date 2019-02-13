using System;
using System.Threading;
using System.Threading.Tasks;


namespace KEL103Driver
{
    class Program
    {
        static void Main(string[] args)
        {
            new Program();

            Thread.Sleep(-1);
        }

        
        public Program()
        {
            Task.Run(async () =>
            {
                var address = await KEL103Tools.FindLoadAddress();

                var result = await KEL103Command.GetCommandFunc(KEL103Command.IDENTIFY)(address);

                Console.WriteLine(result);

                //Console.WriteLine(KEL103Command.mode_strings[ await KEL103Command.GetSystemMode(address)]);

                await KEL103Command.SetDynamicMode_Pulse(address, 0.1, 0.1, .01, 0.1, 1);
                Console.WriteLine(await KEL103Command.QueryDynamicMode(address));

                //await KEL103Command.SetBatteryTestModeParameters(address, 2, 30, 7, 35, 11, 50);
               
                //await KEL103Command.RecallBatteryTestModeParameters(address, 2);
                //await KEL103Command.QueryBatteryTestModeParameters(address, 2);

                //Console.WriteLine(await KEL103Command.QueryBatteryTestTimer(address));
                //Console.WriteLine(await KEL103Command.QueryBatteryTestCapacityCounter(address));

                //await KEL103Command.SetSystemMode(address, KEL103Command.SHORT_MODE);

                //await KEL103Command.Beep(address);

                //for (double d = 0; d < 120.0; d += 0.0001)
                //    await KEL103Command.SetConstantVoltageTarget(address, d);
            });
        }
    }
}
