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
        private const int ERROR = 0;
        private const int OK = 1;
        private const int NOTLOGGED = 2;
        private const int NOT_ENOUGH_PLAYERS_LOGGED = 9;


        private List<User> gameRoom;
        private GameBoard gameBoard;

        private TSCryptography tsCrypto;
        private Authentication Auth;
        private ProtocolSI protocolSI;
        private NetworkStream networkStream;
        

        private byte[] digitalSignature;
        private byte[] data;

        private byte[] encryptedData;
        private byte[] decryptedData;
        private byte[] packet;
        public ClientHandler(List<User> gameroom, GameBoard gameboard)
        {
            this.gameRoom = gameroom;
            this.gameBoard = gameboard;
            this.tsCrypto = new TSCryptography();
            this.Auth = new Authentication();
            this.protocolSI = new ProtocolSI();
        }
        public void Handle()
        {
            Thread thread = new Thread(ClientListener);
            thread.Name = "ClientListener_" + gameRoom[gameRoom.Count() - 1].UserID;
            thread.Start(this.gameRoom[gameRoom.Count() -1]);
        }
        public void ClientListener(object obj)
        {
            //Recebemos o user que efetuou ligação no servidor
            User user = (User)obj;

            //Atribuimos as credencias de criptografia simétrica ao utilizador
            user.SymKey = tsCrypto.GetSymKey();
            user.IV = tsCrypto.GetIV();

            //Abrimos um novo canal de comunicação
            //NetworkStream networkStream = user.TcpClient.GetStream();
            networkStream = user.TcpClient.GetStream();

            //ProtocolSI protocolSI = new ProtocolSI();

            //Lançamos a thread num loop
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
                        Console.WriteLine("Publick Key Recebida: {0}" + Environment.NewLine, user.PublicKey);

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
                        SendAcknowledged(protocolSI, networkStream);

                        if (user.isLogged)
                        {
                            if (!VerifiedDataSignature(user))
                            {
                                //Constrói e envia para o cliente o vetor inicialização encriptado com a chave pública
                                encryptedData = protocolSI.Make(ProtocolSICmdType.USER_OPTION_4);
                                networkStream.Write(encryptedData, 0, encryptedData.Length);
                                return;
                            }

                            string msg = Encoding.UTF8.GetString(this.data);
                            msg = msg.Trim('\0');
                            string message = "Client_" + user.UserID + ": " + user.username + "escreveu em " + DateTime.Now.ToString("g") + " => " + msg + Environment.NewLine;
                            File.AppendAllText(Server.FILEPATH, message);

                            BroadCastMsg(ProtocolSICmdType.DATA, decryptedData, user, gameRoom);
                        }
                        else
                        {
                            packet = protocolSI.Make(ProtocolSICmdType.USER_OPTION_1, NOTLOGGED);
                            networkStream.Write(packet, 0, packet.Length);
                        }

                        break;


                    case ProtocolSICmdType.SYM_CIPHER_DATA:
                        encryptedData = protocolSI.GetData();
                        decryptedData = tsCrypto.SymetricDecryption(encryptedData);
                        this.data = decryptedData;
                        SendAcknowledged(protocolSI, networkStream);
                        Console.WriteLine("Dados recebidos encriptados: {0}", Convert.ToBase64String(encryptedData));
                        Console.WriteLine("Dados recebidos dencriptados: {0}", Encoding.UTF8.GetString(decryptedData));
                        break;

                    case ProtocolSICmdType.DIGITAL_SIGNATURE:
                        digitalSignature = protocolSI.GetData();
                        SendAcknowledged(protocolSI, networkStream);
                        Console.WriteLine("Assinatura digital recebeida: {0}", Convert.ToBase64String(digitalSignature));
                        break;

                    case ProtocolSICmdType.USER_OPTION_1:
                        //Recebe o username do user guarda-o no user
                        encryptedData = protocolSI.GetData();
                        byte[] username = tsCrypto.SymetricDecryption(encryptedData);
                        user.username = Encoding.UTF8.GetString(username);
                        user.username = user.username.Trim('\0');
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
                        //FAZ A AUTENTICAÇÃO DO LOGGIN
                        SendAcknowledged(protocolSI, networkStream);
                        if (user.username.Length > 0 && user.saltedPasswordHash.Length > 0)
                        {
                            if (Auth.VerifyLogin(user.username, Encoding.UTF8.GetString(user.password)))
                            {
                                Console.WriteLine("Client_{0}: {1} => login successfull" + Environment.NewLine, user.UserID, user.username);
                                packet = protocolSI.Make(ProtocolSICmdType.USER_OPTION_1, OK);
                                networkStream.Write(packet, 0, packet.Length);
                                user.isLogged = true;

                                BroadLoggedUsers(ProtocolSICmdType.USER_OPTION_3, user.username, user, gameRoom);
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
                        //FAZ O REGISTO DE UM NOVO USER
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
                        SendAcknowledged(protocolSI, networkStream);
                        Console.WriteLine("Client_{0}: {1} => register successfull", user.UserID, user.username);
                        packet = protocolSI.Make(ProtocolSICmdType.USER_OPTION_2, OK);
                        networkStream.Write(packet, 0, packet.Length);
                        break;
                    case ProtocolSICmdType.USER_OPTION_7:
                        //Recebe a jogada do user e desencripta
                        encryptedData = protocolSI.GetData();
                        byte[] jogada = tsCrypto.SymetricDecryption(encryptedData);
                        int coord_x = jogada[0];
                        int coord_y = jogada[1];

                        //Verifica se a jogada exite
                        if(!gameBoard.GamePlayExist(coord_x, coord_y))
                        {
                            decryptedData[0] = (int)ServerResponse.INVALID_PLAY;
                            SendEncryptedProtocol(ProtocolSICmdType.USER_OPTION_1, decryptedData);
                            break;
                        }

                        //Verifica se o jogador que enviou a jogada é o jogador a jogar
                        if (!gameBoard.isPlayerTurn(user.playerID))
                        {
                            decryptedData[0] = (int)ServerResponse.NOT_YOUR_TURN;
                            SendEncryptedProtocol(ProtocolSICmdType.USER_OPTION_1, decryptedData);
                            break;
                        }



                        break;
                    case ProtocolSICmdType.EOT:
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
                    case ProtocolSICmdType.USER_OPTION_9:
                        encryptedData = protocolSI.GetData();
                        decryptedData = tsCrypto.SymetricDecryption(encryptedData);
                        
                        string data = Encoding.UTF8.GetString(decryptedData);
                        data.Trim('\0');
                        int boardDimension = int.Parse(data);

                        if (user.isLogged)
                        {
                            if (LoggedUsersCount() == 2)
                            {
                                List<GamePlayer> gamePlayersList = new List<GamePlayer>();
                                UpdateGamePLayersList(gamePlayersList);

                                gameBoard = new GameBoard(boardDimension, gamePlayersList);
                                gameBoard.GameStart();

                                List<object> gameStart = new List<object>();
                                gameStart.Add(boardDimension);
                                gameStart.Add(gameBoard.PlayerTurn());

                                BroadCastObject(ProtocolSICmdType.USER_OPTION_9, gameStart);
                            }
                            else
                            {
                                packet = protocolSI.Make(ProtocolSICmdType.USER_OPTION_1, NOT_ENOUGH_PLAYERS_LOGGED);
                                networkStream.Write(packet, 0, packet.Length);

                                //decryptedData[0] = (int)ServerResponse.NOT_ENOUGH_PLAYERS;
                                //SendEncryptedProtocol(ProtocolSICmdType.USER_OPTION_1, decryptedData);
                            }
                        }
                        else
                        {
                            packet = protocolSI.Make(ProtocolSICmdType.USER_OPTION_1, NOTLOGGED);
                            networkStream.Write(packet, 0, packet.Length);
                        }
                        break;
                }
            }

            //user.TcpClient.Close();
        
            //networkStream.Close();

            gameRoom.Remove(user);


        }
        private bool VerifiedDataSignature(User user)
        {
            return tsCrypto.VerifyData(this.data, this.digitalSignature, user.PublicKey);

        }

        private void BroadCastMsg(ProtocolSICmdType protocolSICmdType, byte[] data, User userWhoSentMsg, List<User> gameRoom)
        {
            foreach (User user in gameRoom)
            {  
                TSCryptography tsCryptoBroadCast = new TSCryptography(user.IV, user.SymKey);
                //ProtocolSI protocolSI = new ProtocolSI();
                NetworkStream streamBroadCast = user.TcpClient.GetStream();
                string str_player_plus_message;
                byte[] player_plus_message;

                //Se o user for o mesmo que enviou a mensagem, antes da mensagem é colocadao "Eu:, caso contrário é colocada a identificação do utilizador
                if (user.Equals(userWhoSentMsg))
                {
                    str_player_plus_message = "Eu: " + Encoding.UTF8.GetString(data).Trim('/');
                    player_plus_message = Encoding.UTF8.GetBytes(str_player_plus_message);
                }
                else
                {
                    str_player_plus_message = "Jogador " + userWhoSentMsg.UserID + ": " + Encoding.UTF8.GetString(data);
                    player_plus_message = Encoding.UTF8.GetBytes(str_player_plus_message);
                }
                                
                //Encripta a mensagem trabalhada e envia para o stream
                byte[] encryptedMsg = tsCryptoBroadCast.SymetricEncryption(player_plus_message);
                byte[] packet = protocolSI.Make(protocolSICmdType, encryptedMsg);
                
                streamBroadCast.Write(packet, 0, packet.Length);
                //while (protocolSI.GetCmdType() != ProtocolSICmdType.ACK)
                //{
                //    networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
                //}
                streamBroadCast.Flush();
            }
        }
        private int LoggedUsersCount()
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
        private void UpdateGamePLayersList(List<GamePlayer> gamePlayers)
        {
            foreach(User user in gameRoom)
            {
                if (user.isLogged)
                {
                    gamePlayers.Add(new GamePlayer(gamePlayers.Count+1, user.username, gamePlayers.Count == 0 ? 'O' : 'X'));
                    user.playerID = gamePlayers.Count;
                }

                if(gamePlayers.Count > 1)
                {
                    break;
                }
            }
        }
        private void BroadLoggedUsers(ProtocolSICmdType protocolSICmdType, string data, User playerWhoSentMsg, List<User> gameRoom)
        {
            List<string> usersLogged = new List<string>();
            for (int i = 0; i < gameRoom.Count(); i++)
            {
                if (gameRoom[i].isLogged)
                {
                    usersLogged.Add(gameRoom[i].username);
                }
            }
            BroadCastObject(protocolSICmdType, usersLogged);
        }
        private void BroadCastObject(ProtocolSICmdType protocolSICmdType, object obj)
        { 
            decryptedData = TSCryptography.ObjectToByteArray(obj);

            foreach (User user in gameRoom)
            {
                if (user.isLogged)
                {
                    TSCryptography tsCryptoBroadCast = new TSCryptography(user.IV, user.SymKey);
                    NetworkStream streamBroadCast = user.TcpClient.GetStream();
                    StreamCryptoSendEncryptedProtocol(streamBroadCast, tsCryptoBroadCast, protocolSICmdType, decryptedData);
                }
            }
        }

        private void SendAcknowledged(ProtocolSI protocolSI, NetworkStream networkStream)
        {
            byte[] ack = protocolSI.Make(ProtocolSICmdType.ACK);
            networkStream.Write(ack, 0, ack.Length);
            networkStream.Flush();
        }
        private void SendEncryptedProtocol(ProtocolSICmdType protocolSICmdType, byte[] data)
        {
            byte[] encrypteData = tsCrypto.SymetricEncryption(data);
            byte[] packet = protocolSI.Make(protocolSICmdType, encrypteData);
            networkStream.Write(packet, 0, packet.Length);
            networkStream.Flush();
        }
        private void StreamCryptoSendEncryptedProtocol(NetworkStream stream, TSCryptography tsCryptography, ProtocolSICmdType protocolSICmdType, byte[] data)
        {
            byte[] encrypteData = tsCryptography.SymetricEncryption(data);
            byte[] packet = protocolSI.Make(protocolSICmdType, encrypteData);
            stream.Write(packet, 0, packet.Length);
            stream.Flush();
        }
    }
}
