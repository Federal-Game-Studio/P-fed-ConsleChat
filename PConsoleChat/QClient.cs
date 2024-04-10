using System.Net.Sockets;
using System.Text;

public class QClient
{
    private TcpClient client;
    private NetworkStream stream;

    public QClient()
    {
        string[] lines = File.ReadAllLines("userconfig.txt");
        string server = lines[0];
        int port = int.Parse(lines[1]);
        string nickname = lines[2];

        try
        {
            client = new TcpClient(server, port);
            stream = client.GetStream();
            byte[] buffer = Encoding.UTF8.GetBytes(nickname);
            stream.Write(buffer, 0, buffer.Length);
            Console.WriteLine("Connected to server successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to connect to server: " + ex.Message);
            return;
        }

        Run();
    }

    public void Run()
    {
        while (true)
        {
            string message = Console.ReadLine();
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            stream.Write(buffer, 0, buffer.Length);
        }
    }
}