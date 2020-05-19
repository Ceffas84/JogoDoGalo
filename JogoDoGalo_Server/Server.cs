using EI.SI;
using JogoDoGalo_Server.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace JogoDoGalo_Server
{
    class Server
    {
        private const int PORT = 10000;
        private static TcpListener tcpListener;
        private static List<User> gameRoom = new List<User>();
        public const string FILEPATH = "chatLog.txt";
        static void Main(string[] args)
        {
            //declaração das variáveis
            //o servidor vai estar à escuta de todos os endereços IP
            IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Any, PORT);
            tcpListener = new TcpListener(iPEndPoint);

            //iniciar o servidor
            tcpListener.Start();
            File.AppendAllText(FILEPATH, "SERVER IS READY" + Environment.NewLine);
            Console.WriteLine("SERVER IS READY" + Environment.NewLine);

            while (true)
            {
                //aceitar ligações
                User user = new User();
                user.TcpClient = tcpListener.AcceptTcpClient();
                gameRoom.Add(user);
                user.UserID = gameRoom.Count();
                
                string connectedClient = "Client "+ user.UserID + " connected." + Environment.NewLine;
                File.AppendAllText(FILEPATH, connectedClient);
                Console.WriteLine("Client {0} connected" + Environment.NewLine, user.UserID);

                //Lança uma thread com um listner
                ClientHandler clienHandler = new ClientHandler(gameRoom);
                clienHandler.Handle();
            }
        }
    }
}
