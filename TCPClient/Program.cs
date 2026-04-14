using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class Program
{
    static TcpClient? client;
    static NetworkStream? stream;
    static string? role;

    static void Main()
    {
        Console.WriteLine("- TCP Client -");
        Console.Write("Server IP: ");
        string ip = Console.ReadLine()!;

        Console.WriteLine("Connecting to " + ip + ":5050...");
        client = new TcpClient();
        client.Connect(ip, 5050);

        stream = client.GetStream();

        Thread t = new Thread(Receive);
        t.Start();

        Console.WriteLine("Connected to server!");

        while (true)
        {
            Console.WriteLine("\n--- Available Commands ---");
            Console.WriteLine("1. read|file.txt");
            Console.WriteLine("2. write|file.txt|text");
            Console.WriteLine("3. list");
            Console.WriteLine("4. exec|something");
            Console.WriteLine("--------------------------");

            string msg = Console.ReadLine()!;
            Send(msg);
        }
    }
    static void Send(string msg)
    {
        if (stream == null) return;
        
        byte[] data = Encoding.UTF8.GetBytes(msg);
        stream.Write(data, 0, data.Length);
    }
    static void Receive()
    {
        byte[] buffer = new byte[2048];

        while (true)
        {
            try
            {
                if (stream == null) break;
                int bytes = stream.Read(buffer, 0, buffer.Length);
                string msg = Encoding.UTF8.GetString(buffer, 0, bytes);

                if (msg.StartsWith("ROLE:"))
                {
                    role = msg.Split(':')[1];
                    Console.WriteLine("Your role: " + role);
                }
                else
                {
                    Console.WriteLine("SERVER: " + msg);
                }
            }
            catch
            {
                break;
            }
        }
    }


}


