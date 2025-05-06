using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
class TcpChatServer
{
    private static TcpListener listener;
    private static List<StreamWriter> clients = new List<StreamWriter>();
    private static object lockObj = new object();
    static void Main(string[] args)
    {
        int port = 5000;
        listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        Console.WriteLine($"Chat server har startat på port {port}...");

        while (true)
        {
            TcpClient client = listener.AcceptTcpClient();
            Console.WriteLine("Ny person har anslutit");
            Thread t = new Thread(HandleClient);
            t.Start();
        }
    }
    private static void HandleClient(object obj)
    {
        TcpClient client = (TcpClient)obj;
        using NetworkStream ns = client.GetStream();
        using StreamReader reader =new StreamReader(ns);
        using StreamWriter writer = new StreamWriter(ns) { AutoFlush = true };

        lock (lockObj) clients.Add(writer);
        writer.WriteLine("Välkommen till PoE Chatten.");
        string? message;
        try
        {
            while ((message = reader.ReadLine()) != null)
            {
                Console.WriteLine("Mottaget: " + message);
                Broadcast(message, writer);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Klient error: " + ex.Message);
        }
        finally
        {
            lock(lockObj) clients.Remove(writer);
            client.Close();
            Console.WriteLine("Klienten har kopplat ifrån.");
        }
    }
    private static void Broadcast(string message, StreamWriter sender)
    {
       lock (lockObj)
        {
            foreach (var client in clients)
            {
                if (client != sender)
                {
                    try
                    {
                        client.WriteLine(message);
                    }
                    catch
                    {
                        // strunta i klienter som misslyckas
                    }
                }
            }

        }

    }
}