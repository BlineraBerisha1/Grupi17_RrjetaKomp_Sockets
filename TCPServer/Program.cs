using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;

class Program{    
//Serveri TCP qe pranon lidhje nga klientet, i cili ka role të ndryshme (ADMIN dhe USER) dhe lejon komandat per lexim, shkrim, listim dhe ekzekutim të skedareve ne nje dosje te caktuar. ADMIN mund të beje gjithçka, ndersa USER mund vetem të lexoje dhe listoje skedaret.
    static TcpListener? server;
    static Dictionary<TcpClient, string> clientRoles = new Dictionary<TcpClient, string>();
    static List<TcpClient> clients = new List<TcpClient>();

    static string ip = "192.168.1.125"; // IP test
    static int port = 5050;

    static string baseFolder = "ServerFiles";

   static void Main()
    {
        if (!Directory.Exists(baseFolder))
            Directory.CreateDirectory(baseFolder);

        server = new TcpListener(IPAddress.Parse(ip), port);
        server.Start();

        Console.WriteLine($"Server running on {ip}:{port}");

        while (true)
        {
            TcpClient client = server.AcceptTcpClient();
            clients.Add(client);

            string role = (clients.Count == 1) ? "ADMIN" : "USER";
            clientRoles[client] = role;

            Console.WriteLine($"Client connected -> Role: {role}");

            Thread t = new Thread(HandleClient);
            t.Start(client);
        }
    }

    static void HandleClient(object? obj)
    {
        if (obj == null) return;
        TcpClient client = (TcpClient)obj;
        NetworkStream stream = client.GetStream();

        byte[] buffer = new byte[2048];

        Send(client, $"ROLE:{clientRoles[client]}");

        while (true)
        {
            try
            {
                int bytes = stream.Read(buffer, 0, buffer.Length);
                if (bytes == 0) break;

                string request = Encoding.UTF8.GetString(buffer, 0, bytes);
                Console.WriteLine("REQUEST: " + request);

                string response = ProcessRequest(client, request);

                Send(client, response);
            }
            catch
            {
                break;
            }
        }

        clients.Remove(client);
        clientRoles.Remove(client);
        client.Close();
    }

//Procesimi i kerkesave nga klientet
    static string ProcessRequest(TcpClient client, string req)
    {
        string role = clientRoles[client];

        string[] parts = req.Split('|');
        string cmd = parts[0].ToLower();

        switch (cmd)
        {
            case "read":
                if (role == "ADMIN" || role == "USER")
                {
                    string file = Path.Combine(baseFolder, parts[1]);
                    if (File.Exists(file))
                        return File.ReadAllText(file);
                    return "File not found";
                }
                return "No permission";

            case "write":
                if (role == "ADMIN")
                {
                    string file = Path.Combine(baseFolder, parts[1]);
                    File.WriteAllText(file, parts[2]);
                    return "File written successfully";
                }
                return "WRITE denied (USER only read)";

            case "list":
                return string.Join(", ", Directory.GetFiles(baseFolder));

            case "exec":
                if (role == "ADMIN")
                    return "EXECUTED: " + parts[1];
                return "No execute permission";

            default:
                return "Unknown command";
        }
    }

//Dergimi i pergjigjeve tek klientet
    static void Send(TcpClient client, string msg)
    {
        byte[] data = Encoding.UTF8.GetBytes(msg);
        client.GetStream().Write(data, 0, data.Length);
    }
}
