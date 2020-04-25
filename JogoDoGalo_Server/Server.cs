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
            NetworkStream networkStream = tcpClient.GetStream();
            ProtocolSI protocolSI = new ProtocolSI();


            //Console.WriteLine("Client connected");
            while (protocolSI.GetCmdType() != ProtocolSICmdType.EOT)
            {
                networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
                byte[] ack;

                switch (protocolSI.GetCmdType())
                {
                    case ProtocolSICmdType.DATA:
                        string message = protocolSI.GetStringFromData();
                        ack = protocolSI.Make(ProtocolSICmdType.ACK);
                        networkStream.Write(ack, 0, ack.Length);
                        BroadCast(message, tcpClient);
                        Console.WriteLine(message);
                        break;
                    case ProtocolSICmdType.EOT:
                        Console.WriteLine("Client_ disconnected.");
                        ack = protocolSI.Make(ProtocolSICmdType.ACK);
                        networkStream.Write(ack, 0, ack.Length);
                        break;
                }
  
            }
        }

        private static void BroadCast(string msg, TcpClient excludetcpClient)
        {
            ProtocolSI protocolSI = new ProtocolSI();
            byte[] packet = protocolSI.Make(ProtocolSICmdType.DATA, msg);

            foreach (TcpClient client in tcpClientsList)
            {
                NetworkStream networkStream = client.GetStream();
                networkStream.Write(packet, 0, packet.Length);
                networkStream.Flush();
            }
        }
    }
}
