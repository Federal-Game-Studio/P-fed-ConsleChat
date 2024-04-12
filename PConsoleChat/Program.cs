// See https://aka.ms/new-console-template for more information
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;

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
        try
        {
            client = new TcpClient();
            var result = client.BeginConnect(server, port, null, null);
            var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(5));

            if (success)
            {
            client.EndConnect(result);
            stream = client.GetStream();
            byte[] buffer = Encoding.UTF8.GetBytes(nickname);
            stream.Write(buffer, 0, buffer.Length);
            Console.WriteLine("Connected to server");
            }
            else
            {
            client.Close();
            Console.WriteLine("Connection timed out");
            Environment.Exit(0); // Exit the entire program
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to connect to server: " + ex.Message);
            Environment.Exit(0); // Exit the entire program
        }
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

public class Listener
{
    private TcpClient client;
    private NetworkStream stream;

    public Listener()
    {
        string[] lines = File.ReadAllLines("ip.txt");
        string server = lines[0];
        int port = int.Parse(lines[1]);
        string nickname = "listener";

        try
        {
            client = new TcpClient();
            var result = client.BeginConnect(server, port, null, null);
            var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(5));

            if (success)
            {
            client.EndConnect(result);
            stream = client.GetStream();
            byte[] buffer = Encoding.UTF8.GetBytes(nickname);
            stream.Write(buffer, 0, buffer.Length);
            Console.WriteLine("Connected to server");
            }
            else
            {
            client.Close();
            Console.WriteLine("Connection timed out");
            Environment.Exit(0); // Exit the entire program
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to connect to server: " + ex.Message);
            Environment.Exit(0); // Exit the entire program
        }
    }

    public void Run()
    {
        Thread thread = new Thread(() => ReceiveMessages());
        thread.Start();
        
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
    [DllImport("kernel32.dll")]
    static extern IntPtr GetConsoleWindow();

    [DllImport("user32.dll")]
    static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    const int SW_HIDE = 0;
    private static bool isBackground = false;
    private static bool isQC = false;
    private static string[] lines = File.ReadAllLines("ip.txt");
    private static string server = lines[0];
    private static int port = int.Parse(lines[1]);

    public static void Main(string[] args)
    {
        bool isServer = false;
        bool isListener = false;

        foreach (var arg in args)
        {
            if (arg == "--server")
            {
                isServer = true;
            }
            else if (arg == "--background")
            {
                isBackground = true;
            }
            else if (arg == "--listen")
            {
                isListener = true;
            }
            else if (arg == "--QC")
            {
                isQC = true;
                
            }
        }

        if (isBackground)
        {
            var handle = GetConsoleWindow();
            ShowWindow(handle, SW_HIDE);
        }

        if (isListener)
        {
            Listener listener = new Listener();
            listener.Run();
        }
        
        if (isQC)
        {
            QClient qClient = new QClient();
        }
        if (isServer)
        {
            RunServer();
        }

        else
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
        Server server = new Server(Program.port);
        server.Run();
    }

    private static void RunClient(string nickname, string serverIp, int serverPort)
    {
        Client client = new Client(serverIp, serverPort, nickname);
        client.Run();
    }
}
