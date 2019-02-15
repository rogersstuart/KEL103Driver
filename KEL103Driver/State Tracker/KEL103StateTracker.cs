using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KEL103Driver
{
    public static class KEL103StateTracker
    {
        private static Object task_locker = new Object();
        private static Task state_tracker;

        private static bool tracker_active = false;

        public static void Start()
        {

        }

        public static void Stop()
        {

        }

        private static Task GenerateTrackerTask()
        {
            return new Task(async () =>
            {
                while(tracker_active)
                {
                    try
                    {
                        //do work

                        while(tracker_active)
                        {

                        }
                    }
                    catch(Exception ex)
                    {

                    }

                    var address = KEL103Tools.FindLoadAddress();
                }
            });
        }
    }
}
