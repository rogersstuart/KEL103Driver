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

                await KEL103Command.GetCommandFunc(KEL103Command.STORE)(address, 100);

                await Task.Delay(1000);

                var result3 = await KEL103Command.GetCommandFunc(KEL103Command.MEASURE_VOLTAGE)(address);

                Console.WriteLine("{0:R}", result3);

                //await KEL103Command.GetCommandFunc(KEL103Command.SET_CV_VOLTAGE)(address, 5.0001);

                var result4 = await KEL103Command.GetCommandFunc(KEL103Command.GET_CV_VOLTAGE)(address);

                Console.WriteLine("{0:R}", result4);


            });
        }
    }
}
