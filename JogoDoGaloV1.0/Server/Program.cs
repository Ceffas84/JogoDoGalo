using EI.SI;
using Server.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    class Program
    {
        private const int PORT = 10000;
        private static TcpListener tcpListener;
        private static Lobby lobby = new Lobby();
        public static string SERVERPUBLICKEY;
        public static string SERVERPRIVATEKEY;
        static void Main(string[] args)
        {
            //declaração das variáveis
            //o servidor vai estar à escuta de todos os endereços IP
            IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Any, PORT);
            tcpListener = new TcpListener(iPEndPoint);

            //iniciar o servidor
            tcpListener.Start();

            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            SERVERPUBLICKEY = rsa.ToXmlString(false);
            SERVERPRIVATEKEY = rsa.ToXmlString(true);

            Console.WriteLine("SERVER IS READY" + Environment.NewLine);
            while (true)
            {
                //Client client = new Client();
                //client.TcpClient = tcpListener.AcceptTcpClient();  // não sei como não aceitar a ligação

                //gameRoom.listUsers.Add(client);
                //Console.WriteLine("Client_{0} added to the game room!" + Environment.NewLine, client.ClientID);
                //client.ClientID = gameRoom.listUsers.Count();
                //Console.WriteLine("Client_{0} connected" + Environment.NewLine, client.ClientID);

                ////Lança uma thread com um listner
                //ClientHandler clientHandler = new ClientHandler(gameRoom);
                //clientHandler.Handle();

                Client client = new Client();

            
                        //aceitar ligações
                        client.TcpClient = tcpListener.AcceptTcpClient();  // não sei como não aceitar a ligação

                        lobby.listClients.Add(client);
                        client.ClientID = lobby.listClients.Count();
                        Console.WriteLine("Client_{0} connected" + Environment.NewLine, client.ClientID);
                        Console.WriteLine("Client_{0} added to the game room!" + Environment.NewLine, client.ClientID);

                        //Lança uma thread com um listner
                        ClientHandler clientHandler = new ClientHandler(lobby);
                        clientHandler.Handle();
                
            }
        }
    }

    class ClientHandler
    {
        Lobby lobby;

        private TSCryptography tsCrypto;
        private Authentication Auth;
        private ProtocolSI protocolSI;
        private NetworkStream networkStream;
        //private string ServerPrivateKey;


        private byte[] digitalSignature;
        private byte[] symDecipherData;

        private byte[] encryptedData;
        private byte[] decryptedData;
        private byte[] packet;
        public ClientHandler(Lobby lobby)
        {
            this.lobby = lobby;
            tsCrypto = new TSCryptography();
            this.Auth = new Authentication();
            this.protocolSI = new ProtocolSI();
        }
        public void Handle()
        {
            //ServerPublicKey = serverPublicKey;
            //ServerPrivateKey = serverPrivateKey;
            Thread thread = new Thread(ClientListener);
            //thread.Start(this.gameRoom.listUsers[gameRoom.listUsers.Count - 1]);
            thread.Start(this.lobby);
        }
        public void ClientListener(object obj)
        {
            //Recebemos o user que efetuou ligação no servidor
            Lobby lobbyThread = (Lobby)obj;
            Client client = lobby.listClients[lobby.listClients.Count-1];

            //user.PrivateKey = tsCrypto.GetPrivateKey();

            //Atribuimos as credencias de criptografia simétrica ao utilizador
            client.SymKey = tsCrypto.GetSymKey();
            client.IV = tsCrypto.GetIV();

            //Abrimos um novo canal de comunicação
            networkStream = client.TcpClient.GetStream();

            //  **** TABELA DE UTILIZAÇÃO DE COMANDOS DO PROTOCOLSI ****
            //
            //  SYM_CIPHER_DATA       => Receção de menssagem encriptada
            //  DIGITAL_SIGNATURE     => Receção de assinatura digital da mensagem enviada
            //
            //  PublickKey            => Receção da PublicKey do client
            //
            //  UserOption1           => Receção de username
            //  UserOption2           => Receção de password
            //  UserOption3           => Receção de pedido de login
            //  UserOption4           => Receção de pedido de registo
            //  UserOption5           => Receção de pedido de logout
            //  UserOption6           => Receção de pedido de StartGame
            //  UserOption7           => Receção de pedido de Jogada
            //  UserOption8           => Receção de mensagens do Chat
            //
            while (protocolSI.GetCmdType() != ProtocolSICmdType.EOT)
            {
                networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);

                switch (protocolSI.GetCmdType())
                {
                    case ProtocolSICmdType.SYM_CIPHER_DATA:                 //RECEÇÃO DE UMA MENSAGEM ENCRYPTADA SIMETRICA
                        encryptedData = protocolSI.GetData();
                        decryptedData = tsCrypto.SymetricDecryption(encryptedData);
                        this.symDecipherData = decryptedData;

                        Console.WriteLine("SymCipherData recebida no servidor: {0}", Encoding.UTF8.GetString(this.symDecipherData));

                        packet = protocolSI.Make(ProtocolSICmdType.ACK);    
                        networkStream.Write(packet, 0, packet.Length);
                        networkStream.Flush();
                        break;
                    case ProtocolSICmdType.DIGITAL_SIGNATURE:               //RECEÇÃO DA ASSINATURA DIGITAL
                        this.digitalSignature = protocolSI.GetData();                     

                        Console.WriteLine("Assinatura digital recebida no servidor: {0}", Convert.ToBase64String(digitalSignature));

                        packet = protocolSI.Make(ProtocolSICmdType.ACK);
                        networkStream.Write(packet, 0, packet.Length);
                        networkStream.Flush();
                        break;
                    case ProtocolSICmdType.USER_OPTION_1:                   //VALIDAÇÃO DO USERNAME ENVIADA VS ASSINATURA DIGITAL

                        if (tsCrypto.VerifyData(symDecipherData, digitalSignature, client.PublicKey))
                        {

                            client.username = Encoding.UTF8.GetString(symDecipherData);
                            //Console.WriteLine(client.username);
                        }
                        else
                        {
                            Console.WriteLine("Assinatura diginal falhou no servidor.");
                            packet = protocolSI.Make(ProtocolSICmdType.USER_OPTION_9, (int)ServerResponse.INVALID_DIGITAL_SIGNATURE);
                            networkStream.Write(packet, 0, packet.Length);
                        }

                        packet = protocolSI.Make(ProtocolSICmdType.ACK);
                        networkStream.Write(packet, 0, packet.Length);
                        networkStream.Flush();
                        break;

                    case ProtocolSICmdType.USER_OPTION_2:                   //VALIDAÇÃO DA PASSWORD ENVIADA VS ASSINATURA DIGITAL

                        if (tsCrypto.VerifyData(symDecipherData, digitalSignature, client.PublicKey))
                        {
                            client.password = symDecipherData;

                            //Gera um slat e guarda-o no user
                            byte[] salt = new byte[8];
                            salt = tsCrypto.GenerateSalt();
                            client.salt = salt;

                            //Gera uma saltedhash da password e guarda-a no user
                            byte[] saltedHash = tsCrypto.GenerateSaltedHash(Encoding.UTF8.GetString(client.password), salt);
                            client.saltedPasswordHash = saltedHash;
                        }
                        else
                        {
                            Console.WriteLine("Assinatura diginal falhou no servidor."); //Subtitur erros
                            packet = protocolSI.Make(ProtocolSICmdType.USER_OPTION_9, (int)ServerResponse.INVALID_DIGITAL_SIGNATURE);
                            networkStream.Write(packet, 0, packet.Length);
                        }
                        packet = protocolSI.Make(ProtocolSICmdType.ACK);
                        networkStream.Write(packet, 0, packet.Length);
                        networkStream.Flush();
                        break;

                    case ProtocolSICmdType.USER_OPTION_3:                   //RECEÇÃO DE UM PEDIDO DE LOGIN
                        packet = protocolSI.Make(ProtocolSICmdType.ACK);
                        networkStream.Write(packet, 0, packet.Length);
                        networkStream.Flush();
                        bool alreadyLogged;

                        if (!(lobby.gameRoom.listPlayers.Count < 2))
                        {
                            Console.WriteLine("Numero máximo de jogadores na sala já atingido");
                            break;
                        }

                        if (!lobby.gameRoom.listPlayers.Contains(client))        //Verificamos se o client já está loggado no gameroom
                        {
                            int id = Auth.LoggedUserId(client.username);
                            if(!lobby.gameRoom.listPlayers.Exists(x => x.GetUserId() == id))        //Verificamo se o user com que o cliente está a tentar aceder
                            {                                                                       //já está logado noutro client
                                if (client.username.Length > 7 && client.password.Length > 7)
                                {
                                    if (Auth.VerifyLogin(client.username, Encoding.UTF8.GetString(client.password)))
                                    {
                                        Console.WriteLine("Client_{0}: {1} => login successfull" + Environment.NewLine, client.ClientID, client.username);
                                        packet = protocolSI.Make(ProtocolSICmdType.USER_OPTION_9, (int)ServerResponse.LOGIN_SUCCESS);
                                        networkStream.Write(packet, 0, packet.Length);
                                       
                                        client.isLogged = true;
                                        client.userId = id;
                                        lobby.gameRoom.listPlayers.Add(client);

                                        //Broadcast dos utilizadores logados
                                        byte[] objectArrayBytes = TSCryptography.ObjectToByteArray(lobby.gameRoom.ListPlayers());
                                        BroadCastData(objectArrayBytes, ProtocolSICmdType.USER_OPTION_5);
                                    }
                                    else
                                    {
                                        Console.WriteLine("Client_{0}: login unsuccessfull" + Environment.NewLine, client.ClientID);
                                        packet = protocolSI.Make(ProtocolSICmdType.USER_OPTION_9, (int)ServerResponse.LOGIN_ERROR);
                                        networkStream.Write(packet, 0, packet.Length);
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Client_{0}: username and password must be at least 8 characters long!" + Environment.NewLine, client.ClientID);
                                    packet = protocolSI.Make(ProtocolSICmdType.USER_OPTION_9, (int)ServerResponse.USERNAME_OR_PASSWORD_INVALID_LENGTH);
                                    networkStream.Write(packet, 0, packet.Length);
                                }
                            }
                            else
                            {
                                Console.WriteLine("Username => {0}: is already logged on another client" + Environment.NewLine, client.username);
                                packet = protocolSI.Make(ProtocolSICmdType.USER_OPTION_9, (int)ServerResponse.LOGGED_IN_ANOTHER_CLIENT);
                                networkStream.Write(packet, 0, packet.Length);
                            }
                            
                        }
                        else
                        {
                            Console.WriteLine("Client_{0}: is already logged" + Environment.NewLine, client.ClientID);
                            packet = protocolSI.Make(ProtocolSICmdType.USER_OPTION_9, (int)ServerResponse.ALREADY_LOGGED);
                            networkStream.Write(packet, 0, packet.Length);
                        }
                        break;

                    case ProtocolSICmdType.USER_OPTION_4:                   //RECEÇÃO DE UM PEDIDO DE REGISTO   
                        //FAZ O REGISTO DE UM NOVO USER
                        if (!lobby.gameRoom.listPlayers.Contains(client))        //verificamos se o cliente está loggado no gameroom
                            {
                            if (client.username.Length > 7 && client.password.Length > 7)
                            {
                                try
                                {
                                    Auth.Register(client.username, client.saltedPasswordHash, client.salt);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("Client_{0}: => {0}: register unsuccessfull", client.ClientID, client.username);
                                    packet = protocolSI.Make(ProtocolSICmdType.USER_OPTION_9, (int)ServerResponse.REGISTER_ERROR);
                                    networkStream.Write(packet, 0, packet.Length);
                                    networkStream.Flush();
                                    break;
                                }
                                Console.WriteLine("Client_{0}: => {0}: register successfull", client.ClientID, client.username);
                                packet = protocolSI.Make(ProtocolSICmdType.USER_OPTION_9, (int)ServerResponse.REGISTER_SUCCESS);
                                networkStream.Write(packet, 0, packet.Length);
                                //packet = protocolSI.Make(ProtocolSICmdType.ACK);
                                //networkStream.Write(packet, 0, packet.Length);
                            }
                            else
                            {
                                Console.WriteLine("Client_{0}: username and password must be at least 8 characters long!" + Environment.NewLine, client.ClientID);
                                packet = protocolSI.Make(ProtocolSICmdType.USER_OPTION_9, (int)ServerResponse.USERNAME_OR_PASSWORD_INVALID_LENGTH);
                                networkStream.Write(packet, 0, packet.Length);
                            }
                        }
                        else
                        {
                            Console.WriteLine("Client_{0}: is already logged, please logout and then register a new user!" + Environment.NewLine, client.ClientID);
                            packet = protocolSI.Make(ProtocolSICmdType.USER_OPTION_9, (int)ServerResponse.ALREADY_LOGGED);
                            networkStream.Write(packet, 0, packet.Length);
                        }

                        break;
                    case ProtocolSICmdType.USER_OPTION_5:
                        //logout
                        break;
                    case ProtocolSICmdType.USER_OPTION_6:                   //RECEÇÃO DE START GAME
                                                                            //falta tamanho tabuleiro
                        byte[] boardDimension = protocolSI.GetData();

                        if (!lobby.gameRoom.listPlayers.Contains(client))        //verificamos se o cliente está loggado no gameroom
                            {
                            if (tsCrypto.VerifyData(symDecipherData, digitalSignature, client.PublicKey))
                            {
                                if(lobby.gameRoom.listPlayers.Count > 1) //---> Esta comparação devia ser o count == 2
                                {
                                    switch (lobby.gameRoom.gameBoard.GetGameState())
                                    {
                                        case GameState.Standby:
                                            //1 - Cria um novo GameBoard com os users que estão na GameRoom
                                            lobby.gameRoom.gameBoard = new GameBoard(3, lobby.gameRoom.ListPlayers());      
                                            
                                            //2 - Broadcast do start game
                                            boardDimension = symDecipherData;
                                            BroadCastData(boardDimension, ProtocolSICmdType.USER_OPTION_1);

                                            //3 - Broadcast do Next Player                                            
                                            byte[] objNextPlayer = TSCryptography.ObjectToByteArray(lobby.gameRoom.GetNextPlayer());
                                            BroadCastData(objNextPlayer, ProtocolSICmdType.USER_OPTION_2);

                                            break;

                                        case GameState.OnGoing:
                                            Console.WriteLine("Game already runnig!");
                                            packet = protocolSI.Make(ProtocolSICmdType.USER_OPTION_9, (int)ServerResponse.GAME_ALREADY_RUNNUNG);
                                            networkStream.Write(packet, 0, packet.Length);
                                            break;

                                        case GameState.GameOver:
                                            //1 - Restart do jogo
                                            lobby.gameRoom.gameBoard.RestartGame();

                                            //2 - Broadcast do start game com a dimensao do tabuleiro atual
                                            boardDimension = new byte[1];
                                            boardDimension[0] = (byte)lobby.gameRoom.gameBoard.GetBoardDimension();
                                            BroadCastData(boardDimension, ProtocolSICmdType.USER_OPTION_1);

                                            //3 - Broadcast do Next Player                                            
                                            objNextPlayer = TSCryptography.ObjectToByteArray(lobby.gameRoom.GetNextPlayer());
                                            BroadCastData(objNextPlayer, ProtocolSICmdType.USER_OPTION_2);
                                            break;
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Not possible start game, wait for another player!"); //Subtitur erros
                                    packet = protocolSI.Make(ProtocolSICmdType.USER_OPTION_9, (int)ServerResponse.GAME_NOT_YET_STARTED);
                                    networkStream.Write(packet, 0, packet.Length);
                                }
                            }
                            else
                            {
                                Console.WriteLine("Assinatura diginal falhou no servidor."); //Subtitur erros
                                packet = protocolSI.Make(ProtocolSICmdType.USER_OPTION_9, (int)ServerResponse.INVALID_DIGITAL_SIGNATURE);
                                networkStream.Write(packet, 0, packet.Length);
                            }
                            //packet = protocolSI.Make(ProtocolSICmdType.ACK);
                            //networkStream.Write(packet, 0, packet.Length);
                            //networkStream.Flush();
                        }
                        else
                        {
                            Console.WriteLine("Client_{0}: is not logged in!" + Environment.NewLine, client.ClientID);
                            packet = protocolSI.Make(ProtocolSICmdType.USER_OPTION_9, (int)ServerResponse.NOT_LOGGED);
                            networkStream.Write(packet, 0, packet.Length);
                        }
                        break;
                    case ProtocolSICmdType.USER_OPTION_7:                   //RECEÇÃO DE JOGADA 
                        GamePlay gamePlay;
                        byte[] objPlayList;
                        byte[] objNextPlayer;

                        if (!lobby.gameRoom.listPlayers.Contains(client))        //verificamos se o cliente está loggado no gameroom
                        {
                            if (tsCrypto.VerifyData(symDecipherData, digitalSignature, client.PublicKey))
                            {
                                switch (lobby.gameRoom.gameBoard.GetGameState())
                                {
                                    //case GameState.Standby:
                                    //    Console.WriteLine("Game as not yeat started!");
                                    //    packet = protocolSI.Make(ProtocolSICmdType.USER_OPTION_9, (int)ServerResponse.GAME_NOT_YET_STARTED);
                                    //    networkStream.Write(packet, 0, packet.Length);
                                    //    break;
                                    case GameState.OnGoing:
                                        
                                        //1 - Verifica se a jogada recebida é válida
                                        gamePlay = (GamePlay)TSCryptography.ByteArrayToObject(symDecipherData);
                                        if(!lobby.gameRoom.gameBoard.GamePlayExist(gamePlay.Coord_x, gamePlay.Coord_y))
                                        {
                                            //2 - Adiciona a jogada
                                            lobby.gameRoom.gameBoard.AddGamePlay(gamePlay);

                                            //3 - Atualiza o próximo jogador a jogar
                                            lobby.gameRoom.SetNextPlayer();

                                            //4 - Broadcast da jogada
                                            objPlayList = TSCryptography.ObjectToByteArray(lobby.gameRoom.gameBoard.GetPlayList());
                                            BroadCastData(objPlayList, ProtocolSICmdType.USER_OPTION_3);

                                            //5 - Broadcast do Next Player                                            
                                            objNextPlayer = TSCryptography.ObjectToByteArray(lobby.gameRoom.GetNextPlayer());
                                            BroadCastData(objNextPlayer, ProtocolSICmdType.USER_OPTION_2);
                                        }
                                        else
                                        {
                                            Console.WriteLine("Jogada enviada incorreta, jogada já existe");
                                        }

                                        break;
                                    case GameState.GameOver:
                                        Console.WriteLine("Please restart game to play");
                                        break;
                                }
                            }
                            else
                            {
                                Console.WriteLine("Assinatura diginal falhou no servidor.");
                                packet = protocolSI.Make(ProtocolSICmdType.USER_OPTION_9, (int)ServerResponse.INVALID_DIGITAL_SIGNATURE);
                                networkStream.Write(packet, 0, packet.Length);
                            }
                        }
                        else
                        {
                            Console.WriteLine("Client_{0}: is not logged in!" + Environment.NewLine, client.ClientID);
                            packet = protocolSI.Make(ProtocolSICmdType.USER_OPTION_9, (int)ServerResponse.NOT_LOGGED);
                            networkStream.Write(packet, 0, packet.Length);
                        }
                        break;
                    case ProtocolSICmdType.USER_OPTION_8:
                        break;
                    case ProtocolSICmdType.PUBLIC_KEY:
                        //Recebe a public key do client
                        client.PublicKey = protocolSI.GetStringFromData();
                        
                        //Envia um acknoledged
                        packet = protocolSI.Make(ProtocolSICmdType.ACK);
                        networkStream.Write(packet, 0, packet.Length);
                        networkStream.Flush();

                        //Envia a public key
                        packet = protocolSI.Make(ProtocolSICmdType.PUBLIC_KEY, Program.SERVERPUBLICKEY);
                        networkStream.Write(packet, 0, packet.Length);
                        while (protocolSI.GetCmdType() != ProtocolSICmdType.ACK)
                        {
                            networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
                        }

                        Console.WriteLine("Client_{0}: Public Key received" + Environment.NewLine, client.ClientID);
                        Console.WriteLine("Publick Key Recebida: {0}" + Environment.NewLine, client.PublicKey);

                        //Constrói e envia para o cliente a secretKey encriptada com a chave pública
                        byte[] encryptedKey = tsCrypto.RsaEncryption(tsCrypto.GetSymKey(), client.PublicKey);
                        encryptedData = protocolSI.Make(ProtocolSICmdType.SECRET_KEY, encryptedKey);
                        networkStream.Write(encryptedData, 0, encryptedData.Length);
                        networkStream.Flush();

                        while (protocolSI.GetCmdType() != ProtocolSICmdType.ACK)
                        {
                            networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
                        }
                        Console.WriteLine("Client_{0}: Generated and sent Symetric Key" + Environment.NewLine, client.ClientID);

                        //Constrói e envia para o cliente o vetor inicialização encriptado com a chave pública
                        byte[] encryptedIV = tsCrypto.RsaEncryption(tsCrypto.GetIV(), client.PublicKey);
                        encryptedData = protocolSI.Make(ProtocolSICmdType.IV, encryptedIV);
                        networkStream.Write(encryptedData, 0, encryptedData.Length);
                        networkStream.Flush();
                        while (protocolSI.GetCmdType() != ProtocolSICmdType.ACK)
                        {
                            networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
                        }
                        Console.WriteLine("Client_{0}: Generated and sent Initialization Vector" + Environment.NewLine, client.ClientID);
                        break;
                }
            }
            packet = protocolSI.Make(ProtocolSICmdType.EOT);
            networkStream.Write(packet, 0, packet.Length);

            client.TcpClient.Close();
            networkStream.Close();

            //Quando o utilizador abandona o game room
            if (lobby.gameRoom.listPlayers.Contains(client))
            {
                lobby.gameRoom.listPlayers.Remove(client);
                Console.WriteLine("Player {0} left the game room" + Environment.NewLine, client.username);
            }
            
            //Quando o utilizador abandona o lobby
            lobby.listClients.Remove(client);
            Console.WriteLine("Client_{0} left lobby" + Environment.NewLine, client.ClientID);

        }

        /**
         * <summary>    Encrypts a sign and send protocol. </summary>
         *
         * <remarks>    Simão Pedro, 27/05/2020. </remarks>
         *
         * <param name="data">      The data. </param>
         * <param name="cmdType">   Type of the command. </param>
         */
        private void EncryptSignAndSendProtocol(byte[] data, ProtocolSICmdType cmdType)
        {
            encryptedData = tsCrypto.SymetricEncryption(data);
            packet = protocolSI.Make(ProtocolSICmdType.SYM_CIPHER_DATA, encryptedData);
            networkStream.Write(packet, 0, packet.Length);
            networkStream.Flush();

            while (protocolSI.GetCmdType() != ProtocolSICmdType.ACK)
            {
                networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
            }

            //Cria e envia a assinatura digital da menssagem
            digitalSignature = tsCrypto.SignData(data, Program.SERVERPRIVATEKEY);
            packet = protocolSI.Make(ProtocolSICmdType.DIGITAL_SIGNATURE, digitalSignature);
            networkStream.Write(packet, 0, packet.Length);
            networkStream.Flush();

            while (protocolSI.GetCmdType() != ProtocolSICmdType.ACK)
            {
                networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
            }

            //Envia o Protocol de comando
            packet = protocolSI.Make(cmdType);
            networkStream.Write(packet, 0, packet.Length);
            networkStream.Flush();

            while (protocolSI.GetCmdType() != ProtocolSICmdType.ACK)
            {
                networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
            }

            //Envia o Protocol de comando
            Console.WriteLine("Assinatura digital confirmada");
        }


        private void EncryptSignAndSendProtocol(byte[] data, ProtocolSICmdType cmdType, NetworkStream stream, TSCryptography crypto)
        {
            encryptedData = crypto.SymetricEncryption(data);
            packet = protocolSI.Make(ProtocolSICmdType.SYM_CIPHER_DATA, encryptedData);
            stream.Write(packet, 0, packet.Length);
            stream.Flush();

            //Cria e envia a assinatura digital da menssagem
            digitalSignature = crypto.SignData(data, Program.SERVERPRIVATEKEY);
            packet = protocolSI.Make(ProtocolSICmdType.DIGITAL_SIGNATURE, digitalSignature);
            stream.Write(packet, 0, packet.Length);
            stream.Flush();


            //Envia o Protocol de comando
            packet = protocolSI.Make(cmdType);
            stream.Write(packet, 0, packet.Length);
            stream.Flush();

            //Envia o Protocol de comando
            Console.WriteLine("Assinatura digital confirmada");
        }

        private void BroadCastChat(byte[] data, Client clientWhoSentMsg)
        {
            foreach (Client player in lobby.gameRoom.listPlayers)
            {
                TSCryptography tsCryptoBroadCast = new TSCryptography(player.IV, player.SymKey);
                NetworkStream streamBroadCast = player.TcpClient.GetStream();

                string str_player_plus_message;
                byte[] player_plus_message;

                if (player.ClientID == clientWhoSentMsg.ClientID){
                    str_player_plus_message = "Eu: " + Encoding.UTF8.GetString(data);
                    player_plus_message = Encoding.UTF8.GetBytes(str_player_plus_message);
                }
                else
                {
                    str_player_plus_message = "Jogador " + clientWhoSentMsg.ClientID + ": " + Encoding.UTF8.GetString(data);
                    player_plus_message = Encoding.UTF8.GetBytes(str_player_plus_message);
                }

                EncryptSignAndSendProtocol(player_plus_message, ProtocolSICmdType., streamBroadCast, tsCryptoBroadCast);
            }
        }
        private void BroadCastData(byte[] data, ProtocolSICmdType protocolSICmdType)
        {
            foreach (Client player in lobby.gameRoom.listPlayers)
            {
                TSCryptography tsCryptoBroadCast = new TSCryptography(player.IV, player.SymKey);
                NetworkStream streamBroadCast = player.TcpClient.GetStream();
                
                EncryptSignAndSendProtocol(data, protocolSICmdType, streamBroadCast, tsCryptoBroadCast);
            }
        }
        private List<Client> ListClientsGameRoom()
        {
            List<Client> listPlayers = new List<Client>();
            foreach (Client player in lobby.gameRoom.listPlayers)
            {
                listPlayers.Add(player);
                    
            }
            return listPlayers;
        }
        
    }
}
