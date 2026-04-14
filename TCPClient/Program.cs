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
        Console.Write("Server IP: ");
        string ip = Console.ReadLine()!;

        client = new TcpClient();
        client.Connect(ip, 5050);

        stream = client.GetStream();

        Thread t = new Thread(Receive);
        t.Start();

        Console.WriteLine("Connected to server!");

        while (true)
        {
            Console.WriteLine("\nCommands:");
            Console.WriteLine("1. read|file.txt");
            Console.WriteLine("2. write|file.txt|text");
            Console.WriteLine("3. list");
            Console.WriteLine("4. exec|something");

            string msg = Console.ReadLine()!;
            Send(msg);
        }
    }