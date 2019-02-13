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

                await KEL103Command.SetSystemMode(address, KEL103Command.SHORT_MODE);

                //for (double d = 0; d < 120.0; d += 0.0001)
                //    await KEL103Command.SetConstantVoltageTarget(address, d);
            });
        }
    }
}
