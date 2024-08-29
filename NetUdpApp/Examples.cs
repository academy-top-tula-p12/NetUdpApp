using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NetUdpApp
{
    static class Examples
    {
        public static void UdpSocketReceiveSend()
        {
            Console.Write("Input our number: ");
            int port = 5000 + Int32.Parse(Console.ReadLine());

            using Socket socket = new(AddressFamily.InterNetwork,
                                      SocketType.Dgram,
                                      ProtocolType.Udp);

            IPEndPoint endPoint = new IPEndPoint(IPAddress.Loopback, port);
            socket.Bind(endPoint);

            UdpSocketSend();
            UdpSocketReceive();

            Console.WriteLine("Press any key");
            Console.ReadKey();

            async Task UdpSocketReceive()
            {
                byte[] buffer = new byte[65535];

                IPEndPoint remoteEndPoint = new(IPAddress.Any, 0);

                var result = await socket.ReceiveFromAsync(buffer, remoteEndPoint);
                var message = Encoding.UTF8.GetString(buffer);

                Console.WriteLine($"Receive {result.ReceivedBytes} bytes");
                Console.WriteLine($"Remote address: {result.RemoteEndPoint}");
                Console.WriteLine(message);
            }

            async Task UdpSocketSend()
            {
                Console.Write("Input number remote client: ");
                int port = 5000 + Int32.Parse(Console.ReadLine());

                Console.Write("Input message: ");
                string message = Console.ReadLine();

                byte[] buffer = Encoding.UTF8.GetBytes(message);
                IPEndPoint remoteEndPoint = new(IPAddress.Loopback, port);

                int bytes = await socket.SendToAsync(buffer, remoteEndPoint);
                Console.WriteLine($"Send {bytes} bytes");
            }
        }
        public static void UdpClientReceiveSend()
        {
            //Console.WriteLine("1 - Send\n2 - Receive");
            //int choise = Int32.Parse(Console.ReadLine());

            UdpClientReceiverAsync();
            UdpClientSenderAsync();



            Console.WriteLine("Press any key");
            Console.ReadKey();

            async Task UdpClientReceiverAsync()
            {
                Console.Write("Input our number: ");
                int port = 5000 + Int32.Parse(Console.ReadLine());

                //using UdpClient receiver = new UdpClient(new IPEndPoint(IPAddress.Loopback, port));
                using UdpClient receiver = new UdpClient(port);
                Console.WriteLine("UDP receiver start...");

                UdpReceiveResult result = await receiver.ReceiveAsync();

                string message = Encoding.UTF8.GetString(result.Buffer);

                Console.WriteLine($"Receive {result.Buffer.Length} bytes");
                Console.WriteLine($"Remote address: {result.RemoteEndPoint}");
                Console.WriteLine(message);
            }

            async Task UdpClientSenderAsync()
            {
                using UdpClient sender = new();

                Console.Write("Input number of receiver: ");
                int portReceiver = 5000 + Int32.Parse(Console.ReadLine());

                Console.Write("Input message: ");
                string message = Console.ReadLine();

                byte[] buffer = Encoding.UTF8.GetBytes(message);

                //IPEndPoint remoteEndPoint = new(IPAddress.Loopback, portReceiver);
                //int bytes = await sender.SendAsync(buffer, remoteEndPoint);

                sender.Connect(IPAddress.Loopback, portReceiver);
                int bytes = await sender.SendAsync(buffer);

                Console.WriteLine($"Send {bytes} bytes");
            }
        }

        public static async Task UdpChatSockets()
        {
            IPAddress localAddress = IPAddress.Loopback;
            int localPort;
            int remotePort;
            string name;

            Console.Write("Input name: ");
            name = Console.ReadLine();

            Console.Write("Input local port: ");
            if (!Int32.TryParse(Console.ReadLine(), out localPort)) return;

            Console.Write("Input remote port: ");
            if (!Int32.TryParse(Console.ReadLine(), out remotePort)) return;

            Task.Run(ReceiveMessageAsync);
            await SendMessageAsync();


            async Task ReceiveMessageAsync()
            {
                using Socket receiver = new(AddressFamily.InterNetwork,
                                            SocketType.Dgram,
                                            ProtocolType.Udp);
                int bufferSize = 65535;
                byte[] buffer = new byte[bufferSize];
                string message;

                receiver.Bind(new IPEndPoint(localAddress, localPort));

                while (true)
                {
                    var result = await receiver.ReceiveFromAsync(buffer, new IPEndPoint(IPAddress.Any, 0));
                    message = Encoding.UTF8.GetString(buffer, 0, result.ReceivedBytes);

                    PrintMessage(message);
                }
            }

            async Task SendMessageAsync()
            {
                using Socket sender = new(AddressFamily.InterNetwork,
                                            SocketType.Dgram,
                                            ProtocolType.Udp);
                sender.Connect(new IPEndPoint(IPAddress.Loopback, remotePort));

                string? message;
                byte[] buffer;

                while (true)
                {
                    Console.Write("Input message: ");
                    message = Console.ReadLine();

                    if (String.IsNullOrWhiteSpace(message)) break;

                    message = $"{name}: {message}";
                    buffer = Encoding.UTF8.GetBytes(message);

                    await sender.SendAsync(buffer);
                }
            }

            void PrintMessage(string message)
            {
                if (OperatingSystem.IsWindows())
                {
                    var position = Console.GetCursorPosition();
                    int row = position.Top;
                    int column = position.Left;

                    Console.MoveBufferArea(0, row, column, 1, 0, row + 1);
                    Console.SetCursorPosition(0, row);
                    Console.WriteLine(message);
                    Console.SetCursorPosition(column, row + 1);
                }
            }
        }
        public static async Task UdpChatClients()
        {
            IPAddress localAddress = IPAddress.Loopback;
            int localPort;
            int remotePort;
            string name;

            Console.Write("Input name: ");
            name = Console.ReadLine();

            Console.Write("Input local port: ");
            if (!Int32.TryParse(Console.ReadLine(), out localPort)) return;

            Console.Write("Input remote port: ");
            if (!Int32.TryParse(Console.ReadLine(), out remotePort)) return;


            Task.Run(ReceiveMessageAsync);
            await SendMessageAsync();


            async Task ReceiveMessageAsync()
            {
                using UdpClient receiver = new(localPort);

                while (true)
                {
                    var result = await receiver.ReceiveAsync();
                    var message = Encoding.UTF8.GetString(result.Buffer);
                    PrintMessage(message);
                }
            }

            async Task SendMessageAsync()
            {
                string message;
                using UdpClient sender = new();
                while (true)
                {
                    Console.Write("Input message: ");
                    message = Console.ReadLine();

                    if (String.IsNullOrEmpty(message)) return;

                    message = $"{name}: {message}";
                    byte[] buffer = Encoding.UTF8.GetBytes(message);

                    await sender.SendAsync(buffer, new IPEndPoint(localAddress, remotePort));
                }
            }

            void PrintMessage(string message)
            {
                if (OperatingSystem.IsWindows())
                {
                    var position = Console.GetCursorPosition();
                    int row = position.Top;
                    int column = position.Left;

                    Console.MoveBufferArea(0, row, column, 1, 0, row + 1);
                    Console.SetCursorPosition(0, row);
                    Console.WriteLine(message);
                    Console.SetCursorPosition(column, row + 1);
                }
            }
        }
    }
}
