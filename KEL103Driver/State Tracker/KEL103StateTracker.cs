using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KEL103Driver
{
    public delegate void StateAvailable(KEL103State state);

    public static class KEL103StateTracker
    {
        public static event StateAvailable NewKEL103StateAvailable;

        private static Object task_locker = new Object();
        private static Task state_tracker;

        private static bool tracker_active = false;
        private static bool tracker_init_complete = false;

        private static Object state_locker = new Object();
        private static KEL103State state = null;

        private static IPAddress address;

        private static Object client_locker = new Object();
        private static UdpClient client;
        private static bool is_client_checked_out = false;


        public static void Start()
        {
            lock(task_locker)
            {
                state_tracker = GenerateTrackerTask();
                state_tracker.Start();
            }
        }

        public static void Stop()
        {

        }

        public static UdpClient CheckoutClient()
        {
            lock (client_locker)
            {
                while (is_client_checked_out)
                    Thread.Sleep(1);

                is_client_checked_out = true;
                return client;
            }
        }

        public static void CheckinClient()
        {
            is_client_checked_out = false;
        }

        public static bool IsInitComplete
        {
            get { return tracker_init_complete; }
        }

        private static Task GenerateTrackerTask()
        {
            return new Task(async () =>
            {
                tracker_active = true;

                while (tracker_active)
                {
                    try
                    {
                        address = await KEL103Tools.FindLoadAddress();

                        tracker_init_complete = true;

                        //do work
                        using (UdpClient client = new UdpClient(KEL103Persistance.Configuration.CommandPort))
                        {
                            KEL103Tools.ConfigureClient(address, client);

                            KEL103StateTracker.client = client;

                            while (tracker_active)
                            {
                                Stopwatch q = new Stopwatch();
                                q.Start();

                                CheckoutClient();

                                var kel_state = new KEL103State();

                                var voltage = await KEL103Command.MeasureVoltage(client);
                                var current = await KEL103Command.MeasureCurrent(client);
                                var power = await KEL103Command.MeasurePower(client);

                                var input_state = await KEL103Command.GetLoadInputSwitchState(client);

                                var time_stame = DateTime.Now;

                                CheckinClient();

                                q.Stop();

                                var retreval_span = TimeSpan.FromTicks(q.ElapsedTicks);

                                kel_state.Voltage = voltage;
                                kel_state.Current = current;
                                kel_state.Power = power;
                                kel_state.TimeStamp = time_stame;
                                kel_state.InputState = input_state;
                                kel_state.ValueAquisitionTimespan = retreval_span;

                                NewKEL103StateAvailable(kel_state);

                                //await Task.Delay(10);
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        
                    }
                }
            });
        }
    }
}
