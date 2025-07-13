using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using KEL103Driver;
using System.Threading;


namespace KEL103_Server
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await new KEL103_Server().RunAsync();
        }
    }

    /// <summary>
    /// A TCP server that acts as a bridge between TCP clients and a KEL103 electronic load device.
    /// The server communicates with clients via TCP and translates their commands to UDP for the KEL103 device.
    /// </summary>
    class KEL103_Server
    {
        /// <summary>
        /// Maximum size of the buffer used for receiving TCP client data.
        /// </summary>
        private const int MAX_BUFFER_SIZE = 1024;

        /// <summary>
        /// The TCP port number on which the server listens for incoming client connections.
        /// </summary>
        private const int SERVER_PORT = 5025;

        /// <summary>
        /// Flag indicating whether the server is currently running.
        /// </summary>
        private volatile bool _isRunning = true;

        /// <summary>
        /// Source for cancellation tokens used to coordinate async operations shutdown.
        /// </summary>
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        /// <summary>
        /// Starts the server and handles the main server lifecycle.
        /// </summary>
        /// <remarks>
        /// Sets up console cancellation handling and manages the server's main loop.
        /// The server will continue running until explicitly stopped or an unrecoverable error occurs.
        /// </remarks>
        /// <returns>A task representing the asynchronous server operation.</returns>
        public async Task RunAsync()
        {
            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                _isRunning = false;
                _cts.Cancel();
            };

            try
            {
                await StartServerAsync(_cts.Token);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Server shutdown requested.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fatal error: {ex.Message}");
            }
            finally
            {
                _cts.Dispose();
            }
        }

        /// <summary>
        /// Manages the main server loop, including load device discovery and client handling.
        /// </summary>
        /// <param name="ct">Cancellation token for stopping the server operation.</param>
        /// <returns>A task representing the asynchronous server operation.</returns>
        private async Task StartServerAsync(CancellationToken ct)
        {
            while (_isRunning)
            {
                IPAddress loadAddress = null;
                try
                {
                    Console.WriteLine("Searching for load...");
                    loadAddress = await FindLoadWithRetryAsync(ct);
                    Console.WriteLine($"Load found at {loadAddress}");

                    await RunServerLoopAsync(loadAddress, ct);
                }
                catch (Exception ex) when (!(ex is OperationCanceledException))
                {
                    Console.WriteLine($"Server error: {ex.Message}");
                    await Task.Delay(5000, ct); // Wait before retry
                }
            }
        }

        /// <summary>
        /// Attempts to locate the KEL103 load device on the network with retry capability.
        /// </summary>
        /// <param name="ct">Cancellation token for stopping the search operation.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains the IP address
        /// of the discovered load device.
        /// </returns>
        /// <exception cref="OperationCanceledException">Thrown when the search operation is cancelled.</exception>
        private async Task<IPAddress> FindLoadWithRetryAsync(CancellationToken ct)
        {
            while (_isRunning)
            {
                try
                {
                    return KEL103Tools.FindLoadAddress();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error searching for load: {ex.Message}");
                    await Task.Delay(30000, ct); // 30 second retry
                }
            }
            throw new OperationCanceledException();
        }

        /// <summary>
        /// Runs the TCP server loop that accepts and handles client connections.
        /// </summary>
        /// <param name="loadAddress">The IP address of the KEL103 load device.</param>
        /// <param name="ct">Cancellation token for stopping the server loop.</param>
        /// <returns>A task representing the asynchronous server operation.</returns>
        private async Task RunServerLoopAsync(IPAddress loadAddress, CancellationToken ct)
        {
            var localEndPoint = new IPEndPoint(IPAddress.Any, SERVER_PORT);
            var listener = new TcpListener(localEndPoint);
            
            try
            {
                listener.Start();
                Console.WriteLine($"Server listening on port {SERVER_PORT}");

                while (_isRunning)
                {
                    using (var client = await listener.AcceptTcpClientAsync())
                    {
                        await HandleClientAsync(client, loadAddress, ct);
                    }
                }
            }
            finally
            {
                listener.Stop();
            }
        }

        /// <summary>
        /// Handles communication with a connected TCP client.
        /// </summary>
        /// <param name="client">The connected TCP client.</param>
        /// <param name="loadAddress">The IP address of the KEL103 load device.</param>
        /// <param name="ct">Cancellation token for stopping the client handler.</param>
        /// <returns>A task representing the asynchronous client handling operation.</returns>
        /// <remarks>
        /// This method bridges TCP client communication to UDP communication with the KEL103 device.
        /// It forwards commands from the client to the load device and returns responses for query commands.
        /// </remarks>
        private async Task HandleClientAsync(TcpClient client, IPAddress loadAddress, CancellationToken ct)
        {
            Console.WriteLine($"Client connected from {client.Client.RemoteEndPoint}");
            
            try
            {
                using (var networkStream = client.GetStream())
                {
                    var buffer = new byte[MAX_BUFFER_SIZE];

                    while (_isRunning && !ct.IsCancellationRequested)
                    {
                        int bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length, ct);
                        if (bytesRead == 0) break; // Client disconnected

                        string data = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                        
                        using (var udpClient = new UdpClient(KEL103Persistance.Configuration.CommandPort))
                        {
                            KEL103Tools.ConfigureClient(loadAddress, udpClient);
                            await udpClient.SendAsync(buffer, bytesRead);

                            if (data.Contains('?'))
                            {
                                var response = await udpClient.ReceiveAsync();
                                await networkStream.WriteAsync(response.Buffer, 0, response.Buffer.Length, ct);
                            }
                        }
                    }
                }
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                Console.WriteLine($"Client error: {ex.Message}");
            }
            finally
            {
                client.Close();
                Console.WriteLine($"Client disconnected from {client.Client.RemoteEndPoint}");
            }
        }
    }
}
