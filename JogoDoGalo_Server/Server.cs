using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JogoDoGalo_Server
{
    class Server
    {
        private const int PORT = 10000;
        private static TcpListener tcpListener;
        private static List<TcpClient> tcpClientsList = new List<TcpClient>();
        static void Main(string[] args)
        {
            //declaração das variáveis
            //o servidor vai estar à escuta de todos os endereços IP
            IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Any, PORT);
            tcpListener = new TcpListener(iPEndPoint);

            //iniciar o servidor
            tcpListener.Start();
            Console.WriteLine("SERVER IS READY");
            int clientCounter = 0;

            while (true)
            {
                //aceitar ligações
                TcpClient client = tcpListener.AcceptTcpClient();
                tcpClientsList.Add(client);
                clientCounter = tcpClientsList.Count();
                Console.WriteLine("Client {0} connected", clientCounter);

                Thread thread = new Thread(ClientListener);
                thread.Start(client);
            }
        }

        private static void ClientListener(object obj)
        {
            TcpClient tcpClient = (TcpClient)obj;
            StreamReader reader = new StreamReader(tcpClient.GetStream());

            //Console.WriteLine("Client connected");
            while (true)
            {
                string message = reader.ReadLine();
                BroadCast(message, tcpClient);
                Console.WriteLine(message);
            }
        }

        private static void BroadCast(string msg, TcpClient excludetcpClient)
        {
            foreach (TcpClient client in tcpClientsList)
            {
                StreamWriter writer = new StreamWriter(client.GetStream());
                writer.WriteLine(msg);
                writer.Flush();
            }
        }
    }
}
