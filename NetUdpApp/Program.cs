using System.Net;
using System.Net.Sockets;
using System.Text;

int localPort = 5000;
IPAddress brodcastAddress = IPAddress.Parse("235.0.0.1"); //IPAddress.Broadcast; // //IPAddress.Loopback;
IPEndPoint brodcastEndPoint = new(brodcastAddress, localPort);

Console.Write("Input name: ");
string name = Console.ReadLine();

Task.Run(ReceiveMessageAsync);
await SendMessageAsync();


async Task SendMessageAsync()
{
    using UdpClient sender = new();
    string message;

    while(true)
    {
        Console.Write("Input message: ");
        message = Console.ReadLine();

        if (String.IsNullOrEmpty(message)) break;

        message = $"{name}: {message}";
        byte[] buffer = Encoding.UTF8.GetBytes(message);

        await sender.SendAsync(buffer, brodcastEndPoint);
    }
}

async Task ReceiveMessageAsync()
{
    using UdpClient receiver = new(localPort);
    receiver.JoinMulticastGroup(brodcastAddress);
    receiver.MulticastLoopback = false;

    string message;

    while(true)
    {
        var result = await receiver.ReceiveAsync();
        message = Encoding.UTF8.GetString(result.Buffer);

        if(message == "exit")
            receiver.DropMulticastGroup(brodcastAddress);

        PrintMessage(message);
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
