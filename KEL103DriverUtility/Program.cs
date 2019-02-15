using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KEL103DriverUtility
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var address = KEL103Driver.KEL103Tools.FindLoadAddress();
            address.Wait();
            var res = KEL103Driver.KEL103Command.MeasureResistance(address.Result);
            res.Wait();
            Console.WriteLine(res.Result);
            
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new MainForm());
        }
    }
}
