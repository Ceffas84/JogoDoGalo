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
        private static string secret = "abcd";
        private static AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
        
        static void Main(string[] args)
        {
            //declaração das variáveis
            //o servidor vai estar à escuta de todos os endereços IP
            IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Any, PORT);
            tcpListener = new TcpListener(iPEndPoint);

            //iniciar o servidor
            tcpListener.Start();
            Console.WriteLine("SERVER IS READY");

            //Gerar a key e o iv
            aes.Key = generateKey(secret);
            aes.IV = generateIV(secret);

            while (true)
            {
                //aceitar ligações
                Player player = new Player();
                player.TcpClient = tcpListener.AcceptTcpClient();
                playersList.Add(player);
                
                Console.WriteLine("Client {0} connected", playersList.Count());

                Thread thread = new Thread(ClientListener);
                thread.Start(player);
            }
        }

        private static void ClientListener(object obj)
        {
            Player player = (Player)obj;
            TcpClient tcpClient = player.TcpClient;
            NetworkStream networkStream = tcpClient.GetStream();
            ProtocolSI protocolSI = new ProtocolSI();

            //Console.WriteLine("Client connected");
            while (protocolSI.GetCmdType() != ProtocolSICmdType.EOT)
            {
                networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
                byte[] ack;

                switch (protocolSI.GetCmdType())
                {
                    case ProtocolSICmdType.PUBLIC_KEY:
                        player.PublicKey = protocolSI.GetStringFromData();
                        ack = protocolSI.Make(ProtocolSICmdType.ACK);
                        networkStream.Write(ack, 0, ack.Length);
                        Console.WriteLine("Chave pública recebida.");

                        //Constrói e envia para o cliente a secretKey e o iv encriptados com a chave pública
                        byte[] secretKey = protocolSI.Make(ProtocolSICmdType.SECRET_KEY, assymetricCipher(aes.Key, player.PublicKey));
                        networkStream.Write(secretKey, 0, secretKey.Length);

                        byte[] iv = protocolSI.Make(ProtocolSICmdType.IV, assymetricCipher(aes.IV, player.PublicKey));
                        networkStream.Write(iv, 0, iv.Length);
                        break;
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

            foreach (Player player in playersList)
            {
                NetworkStream networkStream = player.TcpClient.GetStream();
                networkStream.Write(packet, 0, packet.Length);
                networkStream.Flush();
            }
        }

        private static byte[] generateKey(string secret)
        {
            byte[] salt = new byte[] { 1, 9, 7, 3, 8, 7, 1, 5 };
            Rfc2898DeriveBytes pwdGen = new Rfc2898DeriveBytes(secret, salt, 1000);

            //gerar a chave
            byte[] key = pwdGen.GetBytes(16);

            return key; ;
        }

        private static byte[] generateIV(string pass)
        {
            byte[] salt = new byte[] { 3, 5, 7, 1, 4, 2, 6, 8 };
            Rfc2898DeriveBytes pwdGen = new Rfc2898DeriveBytes(pass, salt, 1000);

            //gerar a chave
            byte[] iv = pwdGen.GetBytes(16);

            return iv;
        }

        private static byte[] assymetricCipher(byte[] arr, string publicKey)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(publicKey);

            byte[] arrEncriptado = rsa.Encrypt(arr, true);

            return arrEncriptado;
        }
    }
}
