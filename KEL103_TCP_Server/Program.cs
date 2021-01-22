using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using KEL103Driver;

namespace KEL103_Server
{
    class Program
    {
        static void Main(string[] args)
        {
            new KEL103_Server();
        }
    }

    class KEL103_Server
    {
        public KEL103_Server()
        {
            Task.Run(() => { SynchronousSocketListener.StartListening(); });
            Thread.Sleep(-1);
        }
    }

    public class SynchronousSocketListener
    {
        // Incoming data from the client.  
        public static string data = null;

        public static async void StartListening()
        {
            while (true)
            {
                Console.WriteLine("Searching for load...");
                IPAddress load_address = null;
                while (true)
                {
                    try
                    {
                        load_address = KEL103Tools.FindLoadAddress();
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error searching for load. Retry in 30 seconds.");
                        await Task.Delay(30 * 1000);
                    }
                }

                Console.WriteLine("Load found at " + load_address.ToString() + ".");

                // Data buffer for incoming data.  
                byte[] bytes = new Byte[1024];

                IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
                IPAddress ipAddress = ipHostInfo.AddressList.Where(x => x.AddressFamily == AddressFamily.InterNetwork).ToArray()[0];
                Console.WriteLine("My IP is " + ipAddress.ToString());

                IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 5025);

                // Bind the socket to the local endpoint and
                // listen for incoming connections.  
                try
                {
                    // Create a TCP/IP socket.  
                    using (Socket listener = new Socket(ipAddress.AddressFamily,
                        SocketType.Stream, ProtocolType.Tcp))
                    {
                        listener.Bind(localEndPoint);
                        listener.Listen(10);

                        using (Socket handler = listener.Accept())
                        {

                            // Start listening for connections.  
                            while (true)
                            {
                                // Program is suspended while waiting for an incoming connection.  

                                data = null;

                                // An incoming connection needs to be processed.  
                                int bytesRec = -1;
                                while (bytesRec == -1)
                                {
                                    bytesRec = handler.Receive(bytes);
                                    data += Encoding.ASCII.GetString(bytes, 0, bytesRec);

                                    await Task.Delay(1);
                                }

                                byte[] msg = Encoding.ASCII.GetBytes(data);

                                using (UdpClient client = new UdpClient(KEL103Persistance.Configuration.CommandPort))
                                {
                                    KEL103Tools.ConfigureClient(load_address, client);
                                    client.Send(bytes, bytesRec);

                                    if (data.Contains('?'))
                                    {
                                        var rx = (await client.ReceiveAsync()).Buffer;
                                        handler.Send(rx);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }

                Console.Clear();
            }
        }
    }

}
