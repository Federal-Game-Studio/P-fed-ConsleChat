// See https://aka.ms/new-console-template for more information
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;

public class Server
{
    private TcpListener listener;
    private Dictionary<TcpClient, string> clients = new Dictionary<TcpClient, string>();

    public Server(int port)
    {
        listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        Console.WriteLine("Server started on port " + port);
    }

    public void Run()
    {
        while (true)
        {
            TcpClient client = listener.AcceptTcpClient();
            Thread thread = new Thread(() => HandleClient(client));
            thread.Start();
        }
    }

    private void HandleClient(TcpClient client)
    {
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[1024];
        int bytes = stream.Read(buffer, 0, buffer.Length);
        string nickname = Encoding.UTF8.GetString(buffer, 0, bytes);
        clients.Add(client, nickname);

        try
        {
            while (true)
            {
                bytes = stream.Read(buffer, 0, buffer.Length);
                string message = Encoding.UTF8.GetString(buffer, 0, bytes);
                BroadcastMessage(nickname + ": " + message);
            }
        }
        catch (IOException)
        {
            Console.WriteLine("Client disconnected: " + nickname);
            clients.Remove(client);
        }
    }

    private void BroadcastMessage(string message)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(message);
        foreach (TcpClient client in clients.Keys)
        {
            try
            {
                client.GetStream().Write(buffer, 0, buffer.Length);
            }
            catch (IOException)
            {
                Console.WriteLine("Failed to send message to client: " + clients[client]);
                clients.Remove(client);
            }
        }
    }
}

public class Client
{
    private TcpClient client;
    private NetworkStream stream;

    public Client(string server, int port, string nickname)
    {
        client = new TcpClient(server, port);
        stream = client.GetStream();
        byte[] buffer = Encoding.UTF8.GetBytes(nickname);
        stream.Write(buffer, 0, buffer.Length);
    }

    public void Run()
    {
        Thread thread = new Thread(() => ReceiveMessages());
        thread.Start();

        try
        {
            while (true)
            {
                string message = Console.ReadLine();
                byte[] buffer = Encoding.UTF8.GetBytes(message);
                stream.Write(buffer, 0, buffer.Length);
            }
        }
        catch (IOException)
        {
            Console.WriteLine("Disconnected from server");
        }
    }

    private void ReceiveMessages()
    {
        byte[] buffer = new byte[1024];
        try
        {
            while (true)
            {
                int bytes = stream.Read(buffer, 0, buffer.Length);
                string message = Encoding.UTF8.GetString(buffer, 0, bytes);
                Console.WriteLine(message);
            }
        }
        catch (IOException)
        {
            Console.WriteLine("Disconnected from server");
        }
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Enter 1 to start server, 0 to start client:");
        string input = Console.ReadLine();

        if (input == "1")
        {
            RunServer();
        }
        else if (input == "0")
        {
            Console.WriteLine("Enter nickname:");
            string nickname = Console.ReadLine();

            Console.WriteLine("Enter server IP:");
            string serverIp = Console.ReadLine();

            Console.WriteLine("Enter server port:");
            int serverPort = int.Parse(Console.ReadLine());

            RunClient(nickname, serverIp, serverPort);
        }
    }

    private static void RunServer()
    {
        Server server = new Server(1234);
        server.Run();
    }

    private static void RunClient(string nickname, string serverIp, int serverPort)
    {
        Client client = new Client(serverIp, serverPort, nickname);
        client.Run();
    }
}
