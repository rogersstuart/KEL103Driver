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
    /// <summary>
    /// Delegate for handling state updates from the KEL103 device
    /// </summary>
    /// <param name="state">The new state of the KEL103 device</param>
    public delegate void StateAvailable(KEL103State state);

    public delegate void ConnectionStateChanged(bool isConnected, string message);

    public static class KEL103StateTracker
    {
        // Add connection state event
        public static event ConnectionStateChanged ConnectionStateChanged;
        public static event StateAvailable NewKEL103StateAvailable;

        private static Object task_locker = new Object();
        private static Task state_tracker;

        private static bool tracker_active = false;
        private static bool tracker_init_complete = false;
        private static bool is_connected = false;

        private static Object state_locker = new Object();
        private static KEL103State state = null;

        private static IPAddress address;

        private static Object client_locker = new Object();
        private static UdpClient client = null;
        private static bool is_client_checked_out = false;

        // Add public property for connection state
        public static bool IsConnected 
        {
            get { return is_connected; }
            private set 
            {
                if (is_connected != value)
                {
                    is_connected = value;
                    ConnectionStateChanged?.Invoke(value, 
                        value ? "Connected to KEL103" : "Disconnected from KEL103");
                }
            }
        }

        private static void UpdateConnectionState(bool connected, string message = null)
        {
            lock (client_locker)
            {
                IsConnected = connected;
                Debug.WriteLine($"KEL103 Connection State: {(connected ? "Connected" : "Disconnected")} - {message ?? "No message"}");
            }
        }

        public static void Start()
        {
            lock(task_locker)
            {
                UpdateConnectionState(false, "Starting connection...");
                state_tracker = GenerateTrackerTask();
                state_tracker.Start();
            }
        }

        public static void Stop()
        {
            lock(task_locker)
            {
                tracker_active = false;
                if (client != null)
                {
                    client.Close();
                    client = null;
                }
                if (state_tracker != null && !state_tracker.IsCompleted)
                {
                    state_tracker.Wait(1000);
                }
                tracker_init_complete = false;
                UpdateConnectionState(false, "Connection stopped");
            }
        }

        private static async Task<UdpClient> CreateAndConfigureClientAsync()
        {
            var newClient = new UdpClient(KEL103Persistance.Configuration.CommandPort);
            KEL103Tools.ConfigureClient(address, newClient);
            return newClient;
        }

        public static async Task<UdpClient> CheckoutClientAsync(int timeoutMs = 1000)
        {
            using (CancellationTokenSource cts = new CancellationTokenSource(timeoutMs))
            {
                while (is_client_checked_out)
                {
                    if (cts.Token.IsCancellationRequested)
                        throw new TimeoutException("Client checkout timeout");
                    await Task.Delay(10, cts.Token);
                }

                lock (client_locker)
                {
                    if (client == null || client.Client == null)
                    {
                        client = CreateAndConfigureClientAsync().Result;
                    }
                    is_client_checked_out = true;
                    return client;
                }
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

                try 
                {
                    address = KEL103Tools.FindLoadAddress();
                    if (address == null)
                    {
                        UpdateConnectionState(false, "Failed to find KEL103 device address");
                        return;
                    }

                    tracker_init_complete = true;

                    while (tracker_active)
                    {
                        try
                        {
                            lock (client_locker)
                            {
                                if (client != null)
                                {
                                    client.Close();
                                    client = null;
                                }
                                client = CreateAndConfigureClientAsync().Result;
                            }
                            
                            UpdateConnectionState(true);

                            while (tracker_active)
                            {
                                Stopwatch q = new Stopwatch();
                                q.Start();

                                await CheckoutClientAsync();

                                var kel_state = new KEL103State();

                                try
                                {
                                    var voltage = await KEL103Command.MeasureVoltage(client);
                                    var current = await KEL103Command.MeasureCurrent(client);
                                    var power = await KEL103Command.MeasurePower(client);
                                    var input_state = await KEL103Command.GetLoadInputSwitchState(client);

                                    var time_stamp = DateTime.Now;

                                    q.Stop();

                                    var retrieval_span = TimeSpan.FromTicks(q.ElapsedTicks);

                                    kel_state.Voltage = voltage;
                                    kel_state.Current = current;
                                    kel_state.Power = power;
                                    kel_state.TimeStamp = time_stamp;
                                    kel_state.InputState = input_state;
                                    kel_state.ValueAquisitionTimespan = retrieval_span;

                                    lock (state_locker)
                                    {
                                        state = kel_state;
                                    }

                                    Task.Run(() => { NewKEL103StateAvailable?.Invoke(kel_state); });
                                }
                                catch (Exception ex)
                                {
                                    UpdateConnectionState(false, $"Communication error: {ex.Message}");
                                    throw;
                                }
                                finally
                                {
                                    CheckinClient();
                                }

                                await Task.Delay(100);
                            }
                        }
                        catch(Exception ex)
                        {
                            UpdateConnectionState(false, $"Connection error: {ex.Message}");
                            await Task.Delay(1000); // Delay before retry
                        }
                    }
                }
                catch (Exception ex)
                {
                    UpdateConnectionState(false, $"Fatal error: {ex.Message}");
                    Debug.WriteLine($"KEL103StateTracker fatal error: {ex}");
                }
            });
        }

        public static bool TryGetLatestState(out KEL103State state)
        {
            lock (state_locker)
            {
                state = KEL103StateTracker.state;
                return state != null;
            }
        }
    }
}
