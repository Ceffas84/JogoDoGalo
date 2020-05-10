using EI.SI;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
        private List<User> UsersList;
        private TSCryptography tsCrypto;
        private ProtocolSI protocolSI;
        Authentication Auth;
        private byte[] encryptedData;
        private byte[] decryptedData;

        public ClientHandler(List<User> usersList)
        {
            this.UsersList = usersList;
            this.tsCrypto = new TSCryptography();
            this.Auth = new Authentication();
            this.protocolSI = new ProtocolSI();
        }
        public void Handle()
        {
            Thread thread = new Thread(ClientListener);
            thread.Start(this.UsersList);
        }
        public void ClientListener(object obj)
        {
            List<User> usersList = (List<User>)obj;
            User user = usersList[usersList.Count() - 1];
            NetworkStream networkStream = user.TcpClient.GetStream();
            while (protocolSI.GetCmdType() != ProtocolSICmdType.EOT)
            {
                networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);

                switch (protocolSI.GetCmdType())
                {
                    case ProtocolSICmdType.PUBLIC_KEY:
                        //Recebe a public key do client
                        user.PublicKey = protocolSI.GetStringFromData();
                        SendAcknowledged(protocolSI, networkStream);

                        //Imprime na consola a public key recebida
                        Console.WriteLine("Chave pública recebida: {0}", user.PublicKey);   // *** Para apagar ***
                        Console.WriteLine();
                        Console.WriteLine("Generated Simetric Key: {0}", Convert.ToBase64String(tsCrypto.GetSymKey()));

                        //Constrói e envia para o cliente a secretKey encriptada com a chave pública
                        byte[] encryptedKey = tsCrypto.RsaEncryption(tsCrypto.GetSymKey(), user.PublicKey);
                        encryptedData = protocolSI.Make(ProtocolSICmdType.SECRET_KEY, encryptedKey);
                        networkStream.Write(encryptedData, 0, encryptedData.Length);
                        while (protocolSI.GetCmdType() != ProtocolSICmdType.ACK)
                        {
                            networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
                        }

                        string str = Convert.ToBase64String(encryptedData);   // *** Para apagar ***

                        //Imprime na consola a chave simetrica encriptada enviada
                        Console.WriteLine("Chave simétrica encriptada com a chave publica do cliente enviada!");
                        Console.WriteLine(Convert.ToBase64String(encryptedKey));
                        Console.WriteLine();

                        //Constrói e envia para o cliente o vetor inicialização encriptado com a chave pública
                        //byte[] encryptedIV = RsaEncryption(Aes.IV, player.PublicKey);
                        Console.WriteLine("Generated Initialization Vector: {0}", Convert.ToBase64String(tsCrypto.GetIV()));
                        byte[] encryptedIV = tsCrypto.RsaEncryption(tsCrypto.GetIV(), user.PublicKey);

                        encryptedData = protocolSI.Make(ProtocolSICmdType.IV, encryptedIV);
                        str = Convert.ToBase64String(encryptedData);        //para apagar

                        networkStream.Write(encryptedData, 0, encryptedData.Length);
                        while (protocolSI.GetCmdType() != ProtocolSICmdType.ACK)
                        {
                            networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
                        }

                        //Imprime na consola a chave simetrica encriptada enviada
                        Console.WriteLine("Vetor de inicialização encriptado com a chave publica do cliente enviado!");
                        Console.WriteLine(Convert.ToBase64String(encryptedIV));
                        Console.WriteLine();
                        break;

                    case ProtocolSICmdType.DATA:
                        encryptedData = protocolSI.GetData();

                        decryptedData = tsCrypto.SymetricDecryption(encryptedData);

                        string msg = Encoding.UTF8.GetString(decryptedData) + Environment.NewLine;
                        File.AppendAllText(Server.FILEPATH, msg);
                        Console.WriteLine(msg);
                        SendAcknowledged(protocolSI, networkStream);

                        BroadCast(decryptedData, user, usersList);

                        break;

                    case ProtocolSICmdType.USER_OPTION_1:
                        //Recebe o username do user guarda-o no user
                        encryptedData = protocolSI.GetData();
                        byte[] username = tsCrypto.SymetricDecryption(encryptedData);
                        string str_username = Convert.ToBase64String(username);
                        user.Username = username;

                        SendAcknowledged(protocolSI, networkStream);
                        break;

                    case ProtocolSICmdType.USER_OPTION_2:
                        //Recebe a password do user e guarda a sua Hash no user
                        encryptedData = protocolSI.GetData();
                        byte[] password = tsCrypto.SymetricDecryption(encryptedData);
                        string str_password = Convert.ToBase64String(password);

                        //Gera um slat e guarda-o no user
                        byte[] salt = new byte[8];
                        salt = tsCrypto.GenerateSalt();
                        user.Salt = salt;

                        //Gera uma saltedhash da password e guarda-a no user
                        byte[] saltedHash = tsCrypto.GenerateSaltedHash(str_password, salt);
                        user.HashPassword = saltedHash;

                        SendAcknowledged(protocolSI, networkStream);

                        Authentication auth = new Authentication();
                        if(auth.VerifyLogin(Convert.ToBase64String(user.Username), str_password))
                        {
                            Console.WriteLine("Cliente registado");
                        }
                        break;

                    case ProtocolSICmdType.EOT:
                        Console.WriteLine("Ending Thread from Client " + user.UserID);
                        //SendAcknowledged(protocolSI, networkStream);
                        byte[] packet = protocolSI.Make(ProtocolSICmdType.ACK);
                        networkStream.Write(packet, 0, packet.Length);

                        break;
                }
            }

            user.TcpClient.Close();
        
            networkStream.Close();

            usersList.Remove(user);


        }
        private void BroadCast(byte[] msg, User playerWhoSentMsg, List<User> usersList)
        {
            foreach (User user in usersList)
            {
                if (!user.Equals(playerWhoSentMsg))
                {
                    ProtocolSI protocolSI = new ProtocolSI();
                    NetworkStream networkStream = user.TcpClient.GetStream();
                    byte[] player_plus_message = MsgLine(playerWhoSentMsg, msg);

                    string message_str = Encoding.UTF8.GetString(player_plus_message);   // *** Para apagar ***

                    //byte[] encryptedMsg = symetricEncryption(player_plus_message);
                    byte[] encryptedMsg = tsCrypto.SymetricEncryption(player_plus_message);
                    byte[] packet = protocolSI.Make(ProtocolSICmdType.DATA, encryptedMsg);
                    networkStream.Write(packet, 0, packet.Length);
                    //while (protocolSI.GetCmdType() != ProtocolSICmdType.ACK)
                    //{
                    //    networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
                    //}

                    networkStream.Flush();
                }
            }
        }
        private byte[] MsgLine(User player, byte[] msg)
        {
            string player_plus_message = "Jogador " + player.UserID + ": " + Encoding.UTF8.GetString(msg);

            return Encoding.UTF8.GetBytes(player_plus_message);
        }
        private void SendAcknowledged(ProtocolSI propotcolSi, NetworkStream networkStream)
        {
            byte[] ack = protocolSI.Make(ProtocolSICmdType.ACK);
            networkStream.Write(ack, 0, ack.Length);
            networkStream.Flush();
        }
    }
}
