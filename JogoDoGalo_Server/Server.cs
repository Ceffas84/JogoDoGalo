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
        private static List<Player> playersList = new List<Player>();
        static void Main(string[] args)
        {
            //declaração das variáveis
            //o servidor vai estar à escuta de todos os endereços IP
            IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Any, PORT);
            tcpListener = new TcpListener(iPEndPoint);

            //iniciar o servidor
            tcpListener.Start();
            Console.WriteLine("SERVER IS READY");
            Console.WriteLine();

            while (true)
            {
                //aceitar ligações
                Player player = new Player();
                player.TcpClient = tcpListener.AcceptTcpClient();
                playersList.Add(player);

                Console.WriteLine("Client {0} connected", playersList.Count());

                //Lança uma thread com um listner
                ClientHandler clienHandler = new ClientHandler(player, playersList.Count(), playersList);
                clienHandler.Handle();
            }
        }
    }
}
