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
    public delegate void ConnectionStateChanged(bool isConnected, string message);

    public static class KEL103StateTracker
    {
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

        // Performance monitoring for adaptive polling
        private static readonly Queue<TimeSpan> recentMeasurementTimes = new Queue<TimeSpan>();
        private static TimeSpan targetPollingInterval = TimeSpan.FromMilliseconds(100);
        private static readonly int measurementHistorySize = 20;

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

        private static void UpdatePollingRate(TimeSpan measurementTime)
        {
            lock (state_locker)
            {
                recentMeasurementTimes.Enqueue(measurementTime);
                
                if (recentMeasurementTimes.Count > measurementHistorySize)
                {
                    recentMeasurementTimes.Dequeue();
                }

                if (recentMeasurementTimes.Count >= 10)
                {
                    // Calculate the 90th percentile to exclude outliers
                    var sortedTimes = recentMeasurementTimes.OrderBy(t => t.TotalMilliseconds).ToList();
                    var index90th = (int)(sortedTimes.Count * 0.9);
                    var typical90thPercentile = sortedTimes[index90th];

                    // Set polling interval to be slightly slower than the 90th percentile measurement time
                    // Add 20ms buffer to avoid overwhelming the device
                    targetPollingInterval = typical90thPercentile.Add(TimeSpan.FromMilliseconds(20));
                    
                    // Clamp between reasonable bounds
                    if (targetPollingInterval < TimeSpan.FromMilliseconds(50))
                        targetPollingInterval = TimeSpan.FromMilliseconds(50);
                    else if (targetPollingInterval > TimeSpan.FromMilliseconds(500))
                        targetPollingInterval = TimeSpan.FromMilliseconds(500);
                }
            }
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
                                var measurementStart = DateTime.Now;
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

                                    // Update polling rate based on measurement time
                                    UpdatePollingRate(retrieval_span);

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

                                // Wait for the adaptive polling interval
                                var elapsed = DateTime.Now - measurementStart;
                                var delayTime = targetPollingInterval - elapsed;
                                
                                if (delayTime > TimeSpan.Zero)
                                {
                                    await Task.Delay(delayTime);
                                }
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

        public static TimeSpan GetCurrentPollingInterval()
        {
            lock (state_locker)
            {
                return targetPollingInterval;
            }
        }
    }
}
