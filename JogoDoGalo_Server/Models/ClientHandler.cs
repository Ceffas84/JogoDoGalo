using EI.SI;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JogoDoGalo_Server.Models
{
    public class ClientHandler
    {
        private List<User> gameRoom;
        private TSCryptography tsCrypto;
        Authentication Auth;
        private byte[] encryptedData;
        private byte[] decryptedData;
        private byte[] packet;
        
        private const int ERROR = 0;
        private const int OK = 1;
        private const int NOTLOGGED = 2;


        public ClientHandler(List<User> gameroom)
        {
            this.gameRoom = gameroom;
            this.tsCrypto = new TSCryptography();
            this.Auth = new Authentication();
        }
        public void Handle()
        {
            Thread thread = new Thread(ClientListener);
            thread.Name = "ClientListener_" + gameRoom[gameRoom.Count() - 1].UserID;
            thread.Start(this.gameRoom[gameRoom.Count() -1]);
        }
        public void ClientListener(object obj)
        {
            //List<User> gameRoom = (List<User>)obj;
            //User user = gameRoom[gameRoom.Count() - 1];
            User user = (User)obj;
            user.SymKey = tsCrypto.GetSymKey();
            user.IV = tsCrypto.GetIV();

            //Abrimos um novo canal de comunicação
            NetworkStream networkStream = user.TcpClient.GetStream();
            ProtocolSI protocolSI = new ProtocolSI();

            while (protocolSI.GetCmdType() != ProtocolSICmdType.EOT)
            {
                networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
                switch (protocolSI.GetCmdType())
                {
                    case ProtocolSICmdType.PUBLIC_KEY:
                        //Recebe a public key do client
                        user.PublicKey = protocolSI.GetStringFromData();
                        SendAcknowledged(protocolSI, networkStream);
                        Console.WriteLine("Client_{0}: Public Key received" + Environment.NewLine, user.UserID);

                        //Constrói e envia para o cliente a secretKey encriptada com a chave pública
                        byte[] encryptedKey = tsCrypto.RsaEncryption(tsCrypto.GetSymKey(), user.PublicKey);
                        encryptedData = protocolSI.Make(ProtocolSICmdType.SECRET_KEY, encryptedKey);
                        networkStream.Write(encryptedData, 0, encryptedData.Length);
                        while (protocolSI.GetCmdType() != ProtocolSICmdType.ACK)
                        {
                            networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
                        }
                        Console.WriteLine("Client_{0}: Generated and sent Symetric Key" + Environment.NewLine, user.UserID);

                        //Constrói e envia para o cliente o vetor inicialização encriptado com a chave pública
                        byte[] encryptedIV = tsCrypto.RsaEncryption(tsCrypto.GetIV(), user.PublicKey);
                        encryptedData = protocolSI.Make(ProtocolSICmdType.IV, encryptedIV);
                        networkStream.Write(encryptedData, 0, encryptedData.Length);
                        while (protocolSI.GetCmdType() != ProtocolSICmdType.ACK)
                        {
                            networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
                        }
                        Console.WriteLine("Client_{0}: Generated and sent Initialization Vector" + Environment.NewLine, user.UserID);
                        break;

                    case ProtocolSICmdType.DATA:
                        //Recebe uma mensagem do chat
                        if (user.isLogged)
                        {
                            encryptedData = protocolSI.GetData();
                            decryptedData = tsCrypto.SymetricDecryption(encryptedData);

                            string msg = Encoding.UTF8.GetString(decryptedData) + Environment.NewLine;

                            string message = "Client_" + user.UserID + ": " + Encoding.UTF8.GetString(user.username) + "escreveu em " + DateTime.Now.ToString("g") + " => " + msg;
                            File.AppendAllText(Server.FILEPATH, message);

                            SendAcknowledged(protocolSI, networkStream);

                            BroadCastMsg(ProtocolSICmdType.DATA, decryptedData, user, gameRoom);
                        }
                        else
                        {
                            packet = protocolSI.Make(ProtocolSICmdType.USER_OPTION_1, NOTLOGGED);
                            networkStream.Write(packet, 0, packet.Length);
                        }

                        break;

                    case ProtocolSICmdType.USER_OPTION_1:
                        //Recebe o username do user guarda-o no user
                        encryptedData = protocolSI.GetData();
                        byte[] username = tsCrypto.SymetricDecryption(encryptedData);
                        user.username = username;

                        SendAcknowledged(protocolSI, networkStream);
                        break;

                    case ProtocolSICmdType.USER_OPTION_2:
                        //Recebe a password do user e guarda a sua Hash no user
                        encryptedData = protocolSI.GetData();
                        byte[] password = tsCrypto.SymetricDecryption(encryptedData);
                        user.password = password;

                        //Gera um slat e guarda-o no user
                        byte[] salt = new byte[8];
                        salt = tsCrypto.GenerateSalt();
                        user.salt = salt;

                        //Gera uma saltedhash da password e guarda-a no user
                        byte[] saltedHash = tsCrypto.GenerateSaltedHash(Encoding.UTF8.GetString(password), salt);
                        user.saltedPasswordHash = saltedHash;

                        SendAcknowledged(protocolSI, networkStream);
                        break;

                    case ProtocolSICmdType.USER_OPTION_3:
                        //Autenticação
                        if (user.username.Length > 0 && user.saltedPasswordHash.Length > 0)
                        {
                            //Authentication auth = new Authentication();
                            if (Auth.VerifyLogin(Encoding.UTF8.GetString(user.username), Encoding.UTF8.GetString(user.password)))
                            {
                                Console.WriteLine("Client_{0}: {1} => login successfull" + Environment.NewLine, user.UserID, Encoding.UTF8.GetString(user.username));
                                packet = protocolSI.Make(ProtocolSICmdType.USER_OPTION_1, OK);
                                networkStream.Write(packet, 0, packet.Length);
                                user.isLogged = true;
                                BroadCastData(ProtocolSICmdType.USER_OPTION_3, user.username, user, gameRoom);
                            }
                            else
                            {
                                Console.WriteLine("Client_{0}: login unsuccessfull" + Environment.NewLine, user.UserID);
                                packet = protocolSI.Make(ProtocolSICmdType.USER_OPTION_1, ERROR);
                                networkStream.Write(packet, 0, packet.Length);
                            }
                        }
                        break;
                    case ProtocolSICmdType.USER_OPTION_4:
                        //Registo
                        try
                        {
                            Auth.Register(user.username, user.saltedPasswordHash, user.salt);
                        }
                        catch (Exception ex)
                        {
                            packet = protocolSI.Make(ProtocolSICmdType.USER_OPTION_2, ERROR);
                            networkStream.Write(packet, 0, packet.Length);
                            Console.WriteLine("Client_{0}: register unsuccessfull", user.UserID);
                        }
                        Console.WriteLine("Client_{0}: {1} => register successfull", user.UserID, Encoding.UTF8.GetString(user.username));
                        packet = protocolSI.Make(ProtocolSICmdType.USER_OPTION_2, OK);
                        networkStream.Write(packet, 0, packet.Length);
                        break;

                    case ProtocolSICmdType.EOT:
<<<<<<< Updated upstream
                        Console.WriteLine("Ending Thread from Client " + user.UserID);

                        //Comunica o Server Listener um EOT
                        packet = protocolSI.Make(ProtocolSICmdType.EOT);
                        networkStream.Write(packet, 0, packet.Length);
                        while (protocolSI.GetCmdType() != ProtocolSICmdType.ACK)
                        {
                            networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
                        }
                        //Thread.Sleep(1000);
                        break;
                }
            }

            //user.TcpClient.Close();
        
            //networkStream.Close();

            gameRoom.Remove(user);


        }
        private void BroadCastMsg(ProtocolSICmdType protocolSICmdType, byte[] data, User userWhoSentMsg, List<User> gameRoom)
        {
            foreach (User user in gameRoom)
            {
<<<<<<< Updated upstream
               
                TSCryptography tsCryptoBroadCast = new TSCryptography(user.IV, user.SymKey);
                ProtocolSI protocolSI = new ProtocolSI();
                NetworkStream networkStream = user.TcpClient.GetStream();
                string str_player_plus_message;
                byte[] player_plus_message;

                //Se o user for o mesmo que enviou a mensagem, antes da mensagem é colocadao "Eu:, caso contrário é colocada a identificação do utilizador
                if (user.Equals(userWhoSentMsg))
                {
                    str_player_plus_message = "Eu: " + Encoding.UTF8.GetString(data);
                    player_plus_message = Encoding.UTF8.GetBytes(str_player_plus_message);
                }
                else
                {
                    str_player_plus_message = "Jogador " + userWhoSentMsg.UserID + ": " + Encoding.UTF8.GetString(data);
                    player_plus_message = Encoding.UTF8.GetBytes(str_player_plus_message);
=======
                TSCryptography tSCryptBroadcast = new TSCryptography(user.iv, user.symKey);
                if (!user.Equals(playerWhoSentMsg))
                {
                    ProtocolSI protocolSI = new ProtocolSI();
                    NetworkStream networkStream = user.TcpClient.GetStream();
                    byte[] player_plus_message = MsgLine(playerWhoSentMsg, msg);

                    string message_str = Encoding.UTF8.GetString(player_plus_message);   // *** Para apagar ***

                    //byte[] encryptedMsg = symetricEncryption(player_plus_message);
                    byte[] encryptedMsg = tSCryptBroadcast.SymetricEncryption(player_plus_message);
                    byte[] packet = protocolSI.Make(ProtocolSICmdType.DATA, encryptedMsg);
                    networkStream.Write(packet, 0, packet.Length);
                    //while (protocolSI.GetCmdType() != ProtocolSICmdType.ACK)
                    //{
                    //    networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
                    //}

                    networkStream.Flush();
>>>>>>> Stashed changes
                }

                //Encripta a mensagem trabalhada e envia para o stream
                byte[] encryptedMsg = tsCryptoBroadCast.SymetricEncryption(player_plus_message);
                byte[] packet = protocolSI.Make(protocolSICmdType, encryptedMsg);
                
                networkStream.Write(packet, 0, packet.Length);
                //while (protocolSI.GetCmdType() != ProtocolSICmdType.ACK)
                //{
                //    networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
                //}
                networkStream.Flush();
            }
        }
        private int loggedUsersCount()
        {
            int count = 0;
            foreach (User user in gameRoom)
            {
                if (user.isLogged)
                {
                    count++;
                }
            }
            return count;
        }
        private void BroadCastData(ProtocolSICmdType protocolSICmdType, byte[] data, User playerWhoSentMsg, List<User> gameRoom)
        {
            List<string> usersLogged = new List<string>();
            byte[] loggedUsers;
            for (int i = 0; i < gameRoom.Count(); i++)
            {
                if (gameRoom[i].isLogged)
                {
                    usersLogged.Add(Encoding.UTF8.GetString(gameRoom[i].username));
                }
            }
            loggedUsers = TSCryptography.ObjectToByteArray(usersLogged);


            foreach (User user in gameRoom)
            {
                if (user.isLogged)
                {
                    TSCryptography tsCryptoBroadCast = new TSCryptography(user.IV, user.SymKey);
                    ProtocolSI protocolSI = new ProtocolSI();
                    NetworkStream networkStream = user.TcpClient.GetStream();

                    //byte[] encryptedMsg = tsCryptoBroadCast.SymetricEncryption(data);
                    byte[] encryptedMsg = tsCryptoBroadCast.SymetricEncryption(loggedUsers);

                    byte[] packet = protocolSI.Make(ProtocolSICmdType.USER_OPTION_3, encryptedMsg);

                    networkStream.Write(packet, 0, packet.Length);
                    networkStream.Flush();
                }
            }
        }
        private void SendAcknowledged(ProtocolSI protocolSI, NetworkStream networkStream)
        {
            byte[] ack = protocolSI.Make(ProtocolSICmdType.ACK);
            networkStream.Write(ack, 0, ack.Length);
            networkStream.Flush();
        }
    }
}
