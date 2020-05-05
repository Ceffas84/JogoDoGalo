using EI.SI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JogoDoGalo_Server.Models
{
    public class ClientHandler
    {
        private Player Player;
        private int ClientID;
        private List<Player> PlayersList;
        private RSACryptoServiceProvider rsa;
        private AesCryptoServiceProvider aes;
        private ProtocolSI protocolSI;
        
        public ClientHandler(Player player, int clientID, List<Player> playerList)
        {
            this.Player = player;
            this.Player.PlayerID = clientID;
            this.ClientID = clientID;
            this.PlayersList = playerList;
            this.rsa = new RSACryptoServiceProvider();
            this.aes = new AesCryptoServiceProvider();
            this.protocolSI = new ProtocolSI();
        }
        public void Handle()
        {
            Thread thread = new Thread(ClientListener);
            thread.Start(this.Player);
        }
        public void ClientListener(object obj)
        {
            string secret = "abcd";
            Player player = (Player)obj;
            TcpClient tcpClient = player.TcpClient;
            NetworkStream networkStream = tcpClient.GetStream();

            //Gerar a key e o iv para a criptografia simétrica
            aes.Key = generateKey(secret);
            Console.WriteLine("Generated Simetric Key: {0}", Convert.ToBase64String(aes.Key));
            aes.IV = generateIV(secret);
            Console.WriteLine("Generated Initialization Vector: {0}", Convert.ToBase64String(aes.IV));
            byte[] encrypteMsg;

            //Console.WriteLine("Client connected");
            while (protocolSI.GetCmdType() != ProtocolSICmdType.EOT)
            {
                networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);

                switch (protocolSI.GetCmdType())
                {
                    case ProtocolSICmdType.PUBLIC_KEY:
                        //Recebe a public key do client
                        player.PublicKey = protocolSI.GetStringFromData();
                        SendAcknowledged(protocolSI, networkStream);
                        //Imprime na consola a public key recebida
                        Console.WriteLine();
                        Console.WriteLine("Chave pública recebida: {0}", player.PublicKey);

                        //Constrói e envia para o cliente a secretKey encriptada com a chave pública
                        byte[] encryptedKey = RsaEncryption(aes.Key, player.PublicKey);
                        encrypteMsg = protocolSI.Make(ProtocolSICmdType.SECRET_KEY, encryptedKey);
                        networkStream.Write(encrypteMsg, 0, encrypteMsg.Length);
                        while (protocolSI.GetCmdType() != ProtocolSICmdType.ACK)
                        {
                            networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
                        }

                        //Imprime na consola a chave simetrica encriptada enviada
                        Console.WriteLine();
                        Console.WriteLine("Chave simétrica encriptada com a chave publica do cliente enviada!");
                        Console.WriteLine(Convert.ToBase64String(encryptedKey));
                        
                        //Constrói e envia para o cliente o vetor inicialização encriptado com a chave pública
                        byte[] encryptedIV = RsaEncryption(aes.IV, player.PublicKey);
                        encrypteMsg = protocolSI.Make(ProtocolSICmdType.IV, encryptedIV);
                        networkStream.Write(encrypteMsg, 0, encrypteMsg.Length);
                        while (protocolSI.GetCmdType() != ProtocolSICmdType.ACK)
                        {
                            networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
                        }

                        //Imprime na consola a chave simetrica encriptada enviada
                        Console.WriteLine();
                        Console.WriteLine("Vetor de inicialização encriptado com a chave publica do cliente enviado!");
                        Console.WriteLine(Convert.ToBase64String(encryptedIV));
                        break;

                    case ProtocolSICmdType.DATA:
                        encrypteMsg = protocolSI.GetData();
                        byte[] decryptedMsg = symetricDecryption(encrypteMsg);
                        string msg = Encoding.UTF8.GetString(decryptedMsg) + Environment.NewLine;
                        File.AppendAllText(Server.FILEPATH, msg);
                        Console.WriteLine(msg);

                        BroadCast(decryptedMsg, tcpClient);

                        SendAcknowledged(protocolSI, networkStream);
                        break;

                    case ProtocolSICmdType.EOT:
                        Console.WriteLine("Client_{0} disconnected.", ClientID);
                        PlayersList.Remove(this.Player);
                        byte[] eot = protocolSI.Make(ProtocolSICmdType.EOT);
                        
                        //SendAcknowledged(protocolSI, networkStream);
                        break;
                }
            }
            networkStream.Close();
            Player.TcpClient.Close();
        }
        private void BroadCast(byte[] msg, TcpClient excludetcpClient)
        {
            foreach (Player player in PlayersList)
            {
                if (!player.TcpClient.Equals(excludetcpClient))
                {
                    ProtocolSI protocolSI = new ProtocolSI();
                    NetworkStream networkStream = player.TcpClient.GetStream();
                    byte[] player_plus_message = MsgLine(player, msg);
                    string message_str = Encoding.UTF8.GetString(player_plus_message);
                    byte[] encryptedMsg = symetricEncryption(player_plus_message);
                    byte[] packet = protocolSI.Make(ProtocolSICmdType.DATA, encryptedMsg);

                    networkStream.Write(packet, 0, packet.Length);

                    networkStream.Flush();
                }
            }
        }

        private byte[] MsgLine(Player player, byte[] msg)
        {
            string player_plus_message = "Jogador " + player.PlayerID + ": " + Encoding.UTF8.GetString(msg);

            return Encoding.UTF8.GetBytes(player_plus_message);
        }

        private byte[] generateKey(string secret)
        {
            byte[] salt = new byte[] { 1, 9, 7, 3, 8, 7, 1, 5 };
            Rfc2898DeriveBytes pwdGen = new Rfc2898DeriveBytes(secret, salt, 1000);

            //gerar a chave
            byte[] key = pwdGen.GetBytes(16);

            return key; ;
        }
        private byte[] generateIV(string pass)
        {
            byte[] salt = new byte[] { 3, 5, 7, 1, 4, 2, 6, 8 };
            Rfc2898DeriveBytes pwdGen = new Rfc2898DeriveBytes(pass, salt, 1000);

            //gerar a chave
            byte[] iv = pwdGen.GetBytes(16);

            return iv;
        }

        private byte[] RsaEncryption(byte[] arr, string publicKey)
        {
            rsa.FromXmlString(publicKey);

            byte[] arrEncriptado = rsa.Encrypt(arr, true);

            return arrEncriptado;
        }

        private byte[] symetricEncryption(byte[] arr)
        {
            byte[] encryptedArr;

            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(arr, 0, arr.Length);
                    //cs.Close();
                }
                //guardar os dados cifrados que estão em memória
                encryptedArr = ms.ToArray();
                //ms.Close();
            }
            return encryptedArr;
        }

        private byte[] symetricDecryption(byte[] encryptedArr)
        {
            byte[] decryptedArr;

            MemoryStream ms = new MemoryStream(encryptedArr);

            CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);

            decryptedArr = new byte[ms.Length];

            cs.Read(decryptedArr, 0, decryptedArr.Length);

            cs.Close();
            ms.Close();
            return decryptedArr;
        }
        private void SendAcknowledged(ProtocolSI propotcolSi, NetworkStream networkStream)
        {
            byte[] ack = protocolSI.Make(ProtocolSICmdType.ACK);
            networkStream.Write(ack, 0, ack.Length);
            networkStream.Flush();
        }
    }
}
