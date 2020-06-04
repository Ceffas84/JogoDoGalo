using EI.SI;
using Server.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Remoting.Channels;
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
        public static string CHATLOGFILENAME = "chatLog.txt";
        static void Main(string[] args)
        {
            //O servidor vai estar à escuta de todos os endereços IP
            IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Any, PORT);
            tcpListener = new TcpListener(iPEndPoint);

            //Iniciamos o listener
            tcpListener.Start();

            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            SERVERPUBLICKEY = rsa.ToXmlString(false);
            SERVERPRIVATEKEY = rsa.ToXmlString(true);

            Console.WriteLine("SERVER IS READY" + Environment.NewLine);
            while (true)
            {
                try
                {
                    //Aguardamos a entrada de um cliente
                    Client client = new Client();
                    client.TcpClient = tcpListener.AcceptTcpClient();

                    //Adicionamos o cliente ao lobby
                    lobby.AddClient(client);

                    //Lança uma thread com um listner
                    ClientHandler clientHandler = new ClientHandler(lobby);
                    clientHandler.Handle();

                    Console.WriteLine("Client_{0} connected" + Environment.NewLine, client.ClientID);
                    Console.WriteLine("Client_{0} added to the lobby!" + Environment.NewLine, client.ClientID);
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Erro => " + ex.Message);
                }
            }
        }
    }

    /**
     * <summary>    A client handler.
     *              Classe que isrá lidar com os clientes. </summary>
     *
     * <remarks>    Simão Pedro, 04/06/2020. </remarks>
     */
    class ClientHandler
    {
        Lobby lobby;

        //private TSCryptography tsCrypto;
        private Authentication Auth;
        private ProtocolSI protocolSI;
        private NetworkStream networkStream;
        private TSProtocol tsProtocol;

        private byte[] digitalSignature;
        private byte[] decryptedData;

        private const string LOGOUT_STRING = "Deixem-me sair!";
        
        public ClientHandler(Lobby lobby)
        {
            this.lobby = lobby;
            //tsCrypto = new TSCryptography();
            this.Auth = new Authentication();
            this.protocolSI = new ProtocolSI();
        }

        /**
         * <summary>    Handles this object. 
         *              Método que lança threads para lidar com cada cliente. </summary>
         *
         * <remarks>    Simão Pedro, 04/06/2020. </remarks>
         */
        public void Handle()
        {
            Thread thread = new Thread(ClientListener);
            thread.Start(this.lobby);
        }

        /**
         * <summary>    Client listener.
         *              Método invocado pela thread de cada cliente
         *              responsável por tratar dos pedidos de cada cliente. </summary>
         *
         * <remarks>    Simão Pedro, 04/06/2020. </remarks>
         *
         * <param name="obj">   The object. </param>
         */
        public void ClientListener(object obj)
        {
            //Recebemos o user que efetuou ligação no servidor
            Client client = lobby.listClients[lobby.listClients.Count-1];

            //Iniciamos um novo objeto de criptografia
            TSCryptography tsCrypto = new TSCryptography();

            //Atribuimos as credencias de criptografia simétrica ao client
            client.SymKey = tsCrypto.GetSymKey();
            //Console.WriteLine(Convert.ToBase64String(tsCrypto.GetSymKey()));
            
            client.IV = tsCrypto.GetIV();
            //Console.WriteLine(Convert.ToBase64String(tsCrypto.GetIV()));

            //Abrimos um novo canal de comunicação
            networkStream = client.TcpClient.GetStream();

            //Geramos o objeto TSProtocol para auxiliar as comunicações
            tsProtocol = new TSProtocol(networkStream);

            Packet packet;
            byte[] packetByteArray;
            byte[] objByteArray;

            byte[] salt;
            byte[] saltedHash;

            GamePlayer gamePlayer;

            //
            //  **** TABELA DE UTILIZAÇÃO DE COMANDOS DO PROTOCOLSI ****
            //
            //  USER_OPTION_3           => Receção de pedido de login
            //  USER_OPTION_4           => Receção de pedido de registo
            //  USER_OPTION_5           => Receção de pedido de logout
            //  USER_OPTION_6           => Receção de pedido de StartGame
            //  USER_OPTION_7           => Receção de pedido de Jogada
            //  USER_OPTION_8           => Receção de mensagens do Chat
            //  
            //  PUBLIC_KEY           => Receção da PublicKey do client
            //

            while (protocolSI.GetCmdType() != ProtocolSICmdType.EOT)
            {
                try
                {
                    networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
                    switch (protocolSI.GetCmdType())
                    {
                        case ProtocolSICmdType.USER_OPTION_3:
                            //Receção de pedido de login
                            tsProtocol.SendAck();

                            packetByteArray = protocolSI.GetData();
                            packet = (Packet)TSCryptography.ByteArrayToObject(packetByteArray);
                            decryptedData = tsCrypto.SymetricDecryption(packet.EncryptedData);
                            digitalSignature = packet.DigitalSignature;

                            if(tsCrypto.VerifyData(decryptedData, digitalSignature, client.PublicKey))
                            {
                                Credentials credentials = (Credentials)TSCryptography.ByteArrayToObject(decryptedData);
                                client.username = credentials.Username;
                                client.password = Encoding.UTF8.GetBytes(credentials.Password);

                                //Gera um slat e guarda-o no client
                                salt = new byte[8];
                                salt = tsCrypto.GenerateSalt();
                                client.salt = salt;

                                //Gera uma saltedhash da password e guarda-a no client
                                saltedHash = tsCrypto.GenerateSaltedHash(Encoding.UTF8.GetString(client.password), salt);
                                client.saltedPasswordHash = saltedHash;

                                //Verificamos se a sala já tem 2 clientes
                                if (!(lobby.gameRoom.listPlayers.Count < 2))
                                {
                                    Console.WriteLine("Numero máximo de jogadores na sala já atingido");
                                    break;
                                }
                                //Verificamos se o client não está loggado no gameroom
                                if (!lobby.gameRoom.listPlayers.Contains(client))
                                {
                                    //Verificamo se o user com que o cliente está a tentar aceder já está logado noutro cliente
                                    int id = Auth.UserId(client.username);
                                    if (!lobby.gameRoom.listPlayers.Exists(x => x.GetPlayerId() == id))
                                    {
                                        if (client.username.Length > 7 && client.password.Length > 7)
                                        {
                                            if (Auth.VerifyLogin(client.username, Encoding.UTF8.GetString(client.password)))
                                            {

                                                objByteArray = TSCryptography.ObjectToByteArray(new Response((int)ServerResponse.LOGIN_SUCCESS));
                                                tsProtocol.SendPacket(ProtocolSICmdType.USER_OPTION_9, tsCrypto, objByteArray, Program.SERVERPRIVATEKEY);
                                                tsProtocol.WaitForAck();

                                                //Adiciona o cliente ao game room
                                                client.playerID = id;
                                                lobby.gameRoom.listPlayers.Add(client);

                                                //Broadcast dos utilizadores logados
                                                byte[] objectArrayBytes = TSCryptography.ObjectToByteArray(lobby.gameRoom.GetPlayersList());
                                                BroadCastData(objectArrayBytes, ProtocolSICmdType.USER_OPTION_5);

                                                Console.WriteLine("Client_{0}: {1} => login successfull" + Environment.NewLine, client.ClientID, client.username);
                                            }
                                            else
                                            {
                                                objByteArray = TSCryptography.ObjectToByteArray(new Response((int)ServerResponse.LOGIN_ERROR));
                                                tsProtocol.SendPacket(ProtocolSICmdType.USER_OPTION_9, tsCrypto, objByteArray, Program.SERVERPRIVATEKEY);
                                                tsProtocol.WaitForAck();
                                                Console.WriteLine("Client_{0}: login unsuccessfull" + Environment.NewLine, client.ClientID);
                                            }
                                        }
                                        else
                                        {
                                            objByteArray = TSCryptography.ObjectToByteArray(new Response((int)ServerResponse.USERNAME_OR_PASSWORD_INVALID_LENGTH));
                                            tsProtocol.SendPacket(ProtocolSICmdType.USER_OPTION_9, tsCrypto, objByteArray, Program.SERVERPRIVATEKEY);
                                            tsProtocol.WaitForAck();
                                            Console.WriteLine("Client_{0}: username and password must be at least 8 characters long!" + Environment.NewLine, client.ClientID);
                                        }
                                    }
                                    else
                                    {
                                        objByteArray = TSCryptography.ObjectToByteArray(new Response((int)ServerResponse.LOGGED_IN_ANOTHER_CLIENT));
                                        tsProtocol.SendPacket(ProtocolSICmdType.USER_OPTION_9, tsCrypto, objByteArray, Program.SERVERPRIVATEKEY);
                                        tsProtocol.WaitForAck();
                                        Console.WriteLine("Username => {0}: is already logged on another client" + Environment.NewLine, client.username);
                                    }
                                }
                                else
                                {
                                    objByteArray = TSCryptography.ObjectToByteArray(new Response((int)ServerResponse.ALREADY_LOGGED));
                                    tsProtocol.SendPacket(ProtocolSICmdType.USER_OPTION_9, tsCrypto, objByteArray, Program.SERVERPRIVATEKEY);
                                    tsProtocol.WaitForAck();
                                    Console.WriteLine("Client_{0}: is already logged" + Environment.NewLine, client.ClientID);
                                }
                            }
                            else
                            {
                                objByteArray = TSCryptography.ObjectToByteArray(new Response((int)ServerResponse.INVALID_DIGITAL_SIGNATURE));
                                tsProtocol.SendPacket(ProtocolSICmdType.USER_OPTION_9, tsCrypto, objByteArray, Program.SERVERPRIVATEKEY);
                                tsProtocol.WaitForAck();
                                Console.WriteLine("Assinatura diginal falhou no servidor.");
                            }
                            break;

                        case ProtocolSICmdType.USER_OPTION_4:                   //RECEÇÃO DE UM PEDIDO DE REGISTO   
                            //Receção de pedido de registo
                            tsProtocol.SendAck();

                            packetByteArray = protocolSI.GetData();
                            packet = (Packet)TSCryptography.ByteArrayToObject(packetByteArray);
                            decryptedData = tsCrypto.SymetricDecryption(packet.EncryptedData);
                            digitalSignature = packet.DigitalSignature;

                            if(tsCrypto.VerifyData(decryptedData, digitalSignature, client.PublicKey))
                            {
                                Credentials credentials = (Credentials)TSCryptography.ByteArrayToObject(decryptedData);

                                //Gera um slat e guarda-o no client
                                salt = new byte[8];
                                salt = tsCrypto.GenerateSalt();

                                //Gera uma saltedhash da password e guarda-a no client
                                saltedHash = tsCrypto.GenerateSaltedHash(Encoding.UTF8.GetString(client.password), salt);

                                //Verificamos se o cliente está loggado no gameroom
                                if (!lobby.gameRoom.listPlayers.Contains(client))
                                {
                                    //Validamos o tamanho minimo do username e da password
                                    if (credentials.Username.Length > 7 && credentials.Password.Length > 7)
                                    {
                                        try
                                        {
                                            Auth.Register(credentials.Username, saltedHash, salt);
                                        }
                                        catch (Exception ex)
                                        {
                                            //tsProtocol.SendProtocol(ProtocolSICmdType.USER_OPTION_9, (int)ServerResponse.REGISTER_ERROR);
                                            objByteArray = TSCryptography.ObjectToByteArray(new Response((int)ServerResponse.REGISTER_ERROR));
                                            tsProtocol.SendPacket(ProtocolSICmdType.USER_OPTION_9, tsCrypto, objByteArray, Program.SERVERPRIVATEKEY);
                                            tsProtocol.WaitForAck();
                                            Console.WriteLine("Client_{0}: => {0}: register unsuccessfull", client.ClientID, client.username);
                                            break;
                                        }
                                        objByteArray = TSCryptography.ObjectToByteArray(new Response((int)ServerResponse.REGISTER_SUCCESS));
                                        tsProtocol.SendPacket(ProtocolSICmdType.USER_OPTION_9, tsCrypto, objByteArray, Program.SERVERPRIVATEKEY);
                                        tsProtocol.WaitForAck();
                                        Console.WriteLine("Client_{0}: => {0}: register successfull", client.ClientID, client.username);
                                    }
                                    else
                                    {
                                        objByteArray = TSCryptography.ObjectToByteArray(new Response((int)ServerResponse.USERNAME_OR_PASSWORD_INVALID_LENGTH));
                                        tsProtocol.SendPacket(ProtocolSICmdType.USER_OPTION_9, tsCrypto, objByteArray, Program.SERVERPRIVATEKEY);
                                        tsProtocol.WaitForAck();
                                        Console.WriteLine("Client_{0}: username and password must be at least 8 characters long!" + Environment.NewLine, client.ClientID);

                                    }
                                }
                                else
                                {
                                    objByteArray = TSCryptography.ObjectToByteArray(new Response((int)ServerResponse.ALREADY_LOGGED));
                                    tsProtocol.SendPacket(ProtocolSICmdType.USER_OPTION_9, tsCrypto, objByteArray, Program.SERVERPRIVATEKEY);
                                    tsProtocol.WaitForAck();
                                    Console.WriteLine("Client_{0}: is already logged, please logout and then register a new user!" + Environment.NewLine, client.ClientID);
                                }
                            }
                            else
                            {
                                objByteArray = TSCryptography.ObjectToByteArray(new Response((int)ServerResponse.INVALID_DIGITAL_SIGNATURE));
                                tsProtocol.SendPacket(ProtocolSICmdType.USER_OPTION_9, tsCrypto, objByteArray, Program.SERVERPRIVATEKEY);
                                tsProtocol.WaitForAck();
                                Console.WriteLine("Assinatura diginal falhou no servidor.");
                            }
                            break;
                        case ProtocolSICmdType.USER_OPTION_5:
                            //Receção de pedido de logout
                            
                            packetByteArray = protocolSI.GetData();
                            packet = (Packet)TSCryptography.ByteArrayToObject(packetByteArray);
                            decryptedData = tsCrypto.SymetricDecryption(packet.EncryptedData);
                            digitalSignature = packet.DigitalSignature;

                            string logoutRequest = Encoding.UTF8.GetString(decryptedData);
                            Console.WriteLine(logoutRequest);
                            if (lobby.gameRoom.listPlayers.Contains(client))
                            {
                                if(tsCrypto.VerifyData(decryptedData, digitalSignature, client.PublicKey))
                                {
                                    if(logoutRequest == LOGOUT_STRING)
                                    {
                                        //Remove o cliente do game room
                                        lobby.gameRoom.listPlayers.Remove(client);
                                        Console.WriteLine("Player {0} left the game room" + Environment.NewLine, client.username);

                                        //Broadcast dos utilizadores logados
                                        byte[] objectArrayBytes = TSCryptography.ObjectToByteArray(lobby.gameRoom.GetPlayersList());
                                        BroadCastData(objectArrayBytes, ProtocolSICmdType.USER_OPTION_5);


                                        objByteArray = TSCryptography.ObjectToByteArray(new Response((int)ServerResponse.LOGOUT_SUCCESS));
                                        tsProtocol.SendPacket(ProtocolSICmdType.USER_OPTION_9, tsCrypto, objByteArray, Program.SERVERPRIVATEKEY);
                                        tsProtocol.WaitForAck();
                                        Console.WriteLine("Client_{0}: as logged out" + Environment.NewLine, client.ClientID);

                                        if (lobby.gameRoom.GetGameState() == GameState.OnGoing)
                                        {
                                            //Broadcast de GameOver by abandon
                                            GameOver gameOver = new GameOver(TypeGameOver.Abandon, client.playerID, client.username);
                                            objByteArray = TSCryptography.ObjectToByteArray(gameOver);
                                            BroadCastData(objByteArray, ProtocolSICmdType.USER_OPTION_4);

                                            lobby.gameRoom.SetGameState(GameState.GameOver);
                                        }
                                    }
                                    else
                                    {
                                        objByteArray = TSCryptography.ObjectToByteArray(new Response((int)ServerResponse.LOGOUT_ERROR));
                                        tsProtocol.SendPacket(ProtocolSICmdType.USER_OPTION_9, tsCrypto, objByteArray, Program.SERVERPRIVATEKEY);
                                        tsProtocol.WaitForAck();
                                        Console.WriteLine("Erro ao tentar fazer logout no servidor.");
                                    }
                                }
                                else
                                {
                                    objByteArray = TSCryptography.ObjectToByteArray(new Response((int)ServerResponse.INVALID_DIGITAL_SIGNATURE));
                                    tsProtocol.SendPacket(ProtocolSICmdType.USER_OPTION_9, tsCrypto, objByteArray, Program.SERVERPRIVATEKEY);
                                    tsProtocol.WaitForAck();
                                    Console.WriteLine("Assinatura diginal falhou no servidor.");
                                }
                            }
                            else
                            {
                                objByteArray = TSCryptography.ObjectToByteArray(new Response((int)ServerResponse.NOT_LOGGED_TO_START_GAME));
                                tsProtocol.SendPacket(ProtocolSICmdType.USER_OPTION_9, tsCrypto, objByteArray, Program.SERVERPRIVATEKEY);
                                tsProtocol.WaitForAck();
                                Console.WriteLine("Client_{0}: is not logged in!" + Environment.NewLine, client.ClientID);
                            }
                            
                            break;
                        case ProtocolSICmdType.USER_OPTION_6:                   //RECEÇÃO DE START GAME
                            //Receção de pedido de StartGame                                                                       
                            tsProtocol.SendAck();

                            packetByteArray = protocolSI.GetData();
                            packet = (Packet)TSCryptography.ByteArrayToObject(packetByteArray);
                            decryptedData = tsCrypto.SymetricDecryption(packet.EncryptedData);
                            digitalSignature = packet.DigitalSignature;
                            
                            //verificamos se o cliente está loggado no gameroom
                            if (lobby.gameRoom.listPlayers.Contains(client))
                            {
                                //Verificamos de a assinatura digital confirma os dados enviados
                                if (tsCrypto.VerifyData(decryptedData, digitalSignature, client.PublicKey))
                                {
                                    //Verificamos se estão 2 jogadores na sala
                                    if (lobby.gameRoom.listPlayers.Count > 1) //---> Esta comparação devia ser o count == 2
                                    {
                                        StartGame startGame = (StartGame)TSCryptography.ByteArrayToObject(decryptedData);

                                        //Conforme o estado do jogo:
                                        switch (lobby.gameRoom.GetGameState())
                                        {
                                            case GameState.Standby:
                                                //Cria um novo GameBoard com os users que estão na GameRoom
                                                lobby.gameRoom.StartGame(startGame.BoardDimension);

                                                //Broadcast do start game
                                                BroadCastStarGame(startGame.BoardDimension);

                                                //Broadcast do Active Player    
                                                gamePlayer = new GamePlayer(lobby.gameRoom.listPlayers[0].playerID, lobby.gameRoom.listPlayers[0].username);
                                                objByteArray = TSCryptography.ObjectToByteArray(gamePlayer);
                                                BroadCastData(objByteArray, ProtocolSICmdType.USER_OPTION_2);

                                                break;

                                            case GameState.OnGoing:
                                                objByteArray = TSCryptography.ObjectToByteArray(new Response((int)ServerResponse.GAME_ALREADY_RUNNING));
                                                tsProtocol.SendPacket(ProtocolSICmdType.USER_OPTION_9, tsCrypto, objByteArray, Program.SERVERPRIVATEKEY);
                                                tsProtocol.WaitForAck();
                                                Console.WriteLine("Game already runnig!");
                                                break;

                                            case GameState.GameOver:
                                                //Cria um novo GameBoard com os users que estão na GameRoom
                                                lobby.gameRoom.StartGame(startGame.BoardDimension);

                                                //Broadcast do start game
                                                BroadCastStarGame(startGame.BoardDimension);

                                                //Broadcast do Active Player    
                                                gamePlayer = new GamePlayer(lobby.gameRoom.listPlayers[0].playerID, lobby.gameRoom.listPlayers[0].username);
                                                objByteArray = TSCryptography.ObjectToByteArray(gamePlayer);
                                                BroadCastData(objByteArray, ProtocolSICmdType.USER_OPTION_2);
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        objByteArray = TSCryptography.ObjectToByteArray(new Response((int)ServerResponse.NOT_ENOUGH_PLAYERS));
                                        tsProtocol.SendPacket(ProtocolSICmdType.USER_OPTION_9, tsCrypto, objByteArray, Program.SERVERPRIVATEKEY);
                                        tsProtocol.WaitForAck();
                                        Console.WriteLine("Not possible start game, wait for another player!");
                                    }
                                }
                                else
                                {
                                    objByteArray = TSCryptography.ObjectToByteArray(new Response((int)ServerResponse.INVALID_DIGITAL_SIGNATURE));
                                    tsProtocol.SendPacket(ProtocolSICmdType.USER_OPTION_9, tsCrypto, objByteArray, Program.SERVERPRIVATEKEY);
                                    tsProtocol.WaitForAck();
                                    Console.WriteLine("Assinatura diginal falhou no servidor.");
                                }
                            }
                            else
                            {
                                objByteArray = TSCryptography.ObjectToByteArray(new Response((int)ServerResponse.NOT_LOGGED_TO_START_GAME));
                                tsProtocol.SendPacket(ProtocolSICmdType.USER_OPTION_9, tsCrypto, objByteArray, Program.SERVERPRIVATEKEY);
                                tsProtocol.WaitForAck();
                                Console.WriteLine("Client_{0}: is not logged in!" + Environment.NewLine, client.ClientID);
                            }
                            break;
                        case ProtocolSICmdType.USER_OPTION_7:
                            //Receção de pedido de Jogada
                            tsProtocol.SendAck();

                            packetByteArray = protocolSI.GetData();
                            packet = (Packet)TSCryptography.ByteArrayToObject(packetByteArray);
                            decryptedData = tsCrypto.SymetricDecryption(packet.EncryptedData);
                            digitalSignature = packet.DigitalSignature;

                            //Verificamos se o cliente está loggado no gameroom
                            if (lobby.gameRoom.listPlayers.Contains(client))
                            {
                                //Verificamos de a assinatura digital confirma os dados enviados
                                if (tsCrypto.VerifyData(decryptedData, digitalSignature, client.PublicKey))
                                {
                                    //Conforme o estado do jogo:
                                    switch (lobby.gameRoom.GetGameState())
                                    {
                                        case GameState.OnGoing:
                                            GamePlay gamePlay = (GamePlay)TSCryptography.ByteArrayToObject(decryptedData);

                                            //Verifica se a jogada recebida é válida
                                            if (!lobby.gameRoom.gameBoard.GamePlayExist(gamePlay.Coord_x, gamePlay.Coord_y))
                                            {
                                                //Adiciona a jogada
                                                lobby.gameRoom.gameBoard.AddGamePlay(gamePlay.Coord_x, gamePlay.Coord_y, client.playerID);

                                                //Broadcast da jogada
                                                objByteArray = TSCryptography.ObjectToByteArray(lobby.gameRoom.gameBoard.GetListOfPlays());
                                                BroadCastData(objByteArray, ProtocolSICmdType.USER_OPTION_3);

                                                //Verifica se o jogador Ganhou
                                                if (!lobby.gameRoom.gameBoard.CheckPLayerWins(client.playerID))
                                                {
                                                    //Verifica se o numero de jogadas terminou
                                                    if (lobby.gameRoom.gameBoard.IsNumberPlaysOver())
                                                    {
                                                        //Faz BroadCast do GameOver por empate
                                                        GameOver gameOver = new GameOver(TypeGameOver.Draw, 0, "");
                                                        objByteArray = TSCryptography.ObjectToByteArray(gameOver);
                                                        BroadCastData(objByteArray, ProtocolSICmdType.USER_OPTION_4);

                                                        //Atualiza o estado do jogo para GameOver
                                                        lobby.gameRoom.SetGameState(GameState.GameOver);
                                                    }
                                                    else
                                                    {
                                                        //Atualiza o próximo jogador a jogar
                                                        lobby.gameRoom.SetNextPlayer();

                                                        //Broadcast do Next Player    
                                                        gamePlayer = new GamePlayer(lobby.gameRoom.GetCurrentPlayerId(), lobby.gameRoom.GetCurrentPlayerUsername());
                                                        objByteArray = TSCryptography.ObjectToByteArray(gamePlayer);
                                                        BroadCastData(objByteArray, ProtocolSICmdType.USER_OPTION_2);
                                                    }
                                                }
                                                else
                                                {
                                                    //Faz BroadCast do GameOver e do jogador que ganhou
                                                    GameOver gameOver = new GameOver(TypeGameOver.Winner, lobby.gameRoom.GetCurrentPlayerId(), lobby.gameRoom.GetCurrentPlayerUsername());
                                                    objByteArray = TSCryptography.ObjectToByteArray(gameOver);
                                                    BroadCastData(objByteArray, ProtocolSICmdType.USER_OPTION_4);

                                                    //Atualiza o estado do jogo para GameOver
                                                    lobby.gameRoom.SetGameState(GameState.GameOver);
                                                }
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
                                    objByteArray = TSCryptography.ObjectToByteArray(new Response((int)ServerResponse.INVALID_DIGITAL_SIGNATURE));
                                    tsProtocol.SendPacket(ProtocolSICmdType.USER_OPTION_9, tsCrypto, objByteArray, Program.SERVERPRIVATEKEY);
                                    tsProtocol.WaitForAck();
                                    Console.WriteLine("Assinatura diginal falhou no servidor.");
                                }
                            }
                            else
                            {
                                objByteArray = TSCryptography.ObjectToByteArray(new Response((int)ServerResponse.NOT_LOGGED_TO_START_GAME));
                                tsProtocol.SendPacket(ProtocolSICmdType.USER_OPTION_9, tsCrypto, objByteArray, Program.SERVERPRIVATEKEY);
                                tsProtocol.WaitForAck();
                                Console.WriteLine("Client_{0}: is not logged in!" + Environment.NewLine, client.ClientID);
                            }
                            break;
                        case ProtocolSICmdType.USER_OPTION_8:
                            //Receção de mensagens do Chat
                            packetByteArray = protocolSI.GetData();
                            packet = (Packet)TSCryptography.ByteArrayToObject(packetByteArray);

                            decryptedData = tsCrypto.SymetricDecryption(packet.EncryptedData);
                            digitalSignature = packet.DigitalSignature;

                            if (lobby.gameRoom.listPlayers.Contains(client))        //verificamos se o cliente está loggado no gameroom
                            {
                                if (tsCrypto.VerifyData(decryptedData, digitalSignature, client.PublicKey))
                                {
                                    BroadCastChat(decryptedData, client);
                                }
                                else
                                {
                                    objByteArray = TSCryptography.ObjectToByteArray(new Response((int)ServerResponse.INVALID_DIGITAL_SIGNATURE));
                                    tsProtocol.SendPacket(ProtocolSICmdType.USER_OPTION_9, tsCrypto, objByteArray, Program.SERVERPRIVATEKEY);
                                    tsProtocol.WaitForAck();
                                    Console.WriteLine("Assinatura diginal falhou no servidor.");
                                }
                            }
                            else
                            {
                                objByteArray = TSCryptography.ObjectToByteArray(new Response((int)ServerResponse.NOT_LOGGED_TO_START_GAME));
                                tsProtocol.SendPacket(ProtocolSICmdType.USER_OPTION_9, tsCrypto, objByteArray, Program.SERVERPRIVATEKEY);
                                tsProtocol.WaitForAck();
                                Console.WriteLine("Client_{0}: is not logged in!" + Environment.NewLine, client.ClientID);
                            }
                            break;
                        case ProtocolSICmdType.PUBLIC_KEY:
                            //Receção da PublicKey do client
                            client.PublicKey = protocolSI.GetStringFromData();
                            tsProtocol.SendAck();

                            //Envia a public key
                            tsProtocol.SendProtocol(ProtocolSICmdType.PUBLIC_KEY, Program.SERVERPUBLICKEY);
                            tsProtocol.WaitForAck();

                            Console.WriteLine("Client_{0}: Public Key received" + Environment.NewLine, client.ClientID);
                            Console.WriteLine("Publick Key Recebida: {0}" + Environment.NewLine, client.PublicKey);

                            //Constrói e envia para o cliente a secretKey encriptada com a chave pública
                            byte[] encryptedKey = tsCrypto.RsaEncryption(tsCrypto.GetSymKey(), client.PublicKey);
                            tsProtocol.SendProtocol(ProtocolSICmdType.SECRET_KEY, encryptedKey);
                            tsProtocol.WaitForAck();

                            Console.WriteLine("Client_{0}: Generated and sent Symetric Key: {1}" + Environment.NewLine, client.ClientID, Convert.ToBase64String(tsCrypto.GetSymKey()));

                            //Constrói e envia para o cliente o vetor inicialização encriptado com a chave pública
                            byte[] encryptedIV = tsCrypto.RsaEncryption(tsCrypto.GetIV(), client.PublicKey);
                            tsProtocol.SendProtocol(ProtocolSICmdType.IV, encryptedIV);
                            tsProtocol.WaitForAck();

                            Console.WriteLine("Client_{0}: Generated and sent Initialization Vector: {1}" + Environment.NewLine, client.ClientID, Convert.ToBase64String(tsCrypto.GetIV()));
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            //Quando o utilizador abandona o game room
            if (lobby.gameRoom.listPlayers.Contains(client))
            {
                //Remove o cliente do game room
                lobby.gameRoom.listPlayers.Remove(client);
                Console.WriteLine("Player {0} left the game room" + Environment.NewLine, client.username);

                //Broadcast dos utilizadores logados
                byte[] objectArrayBytes = TSCryptography.ObjectToByteArray(lobby.gameRoom.GetPlayersList());
                BroadCastData(objectArrayBytes, ProtocolSICmdType.USER_OPTION_5);

                if(lobby.gameRoom.GetGameState() == GameState.OnGoing)
                {
                    //Broadcast de GameOver by abandon
                    GameOver gameOver = new GameOver(TypeGameOver.Abandon, client.playerID, client.username);
                    objByteArray = TSCryptography.ObjectToByteArray(gameOver);
                    BroadCastData(objByteArray, ProtocolSICmdType.USER_OPTION_4);

                    lobby.gameRoom.SetGameState(GameState.GameOver);
                }
            }
            //Quando o utilizador abandona o lobby
            lobby.listClients.Remove(client);
            Console.WriteLine("Client_{0} left lobby" + Environment.NewLine, client.ClientID);

            tsProtocol.SendProtocol(ProtocolSICmdType.EOT);
            tsProtocol.WaitForAck();

            client.TcpClient.Close();
            networkStream.Close();
        }

        /**
         * <summary>    Broad cast chat. 
         *              Método resposável por fazer o broadcast das mensagens de chat enviadas por cada cliente. </summary>
         *
         * <remarks>    Simão Pedro, 04/06/2020. </remarks>
         *
         * <param name="data">              The data. </param>
         * <param name="clientWhoSentMsg">  Message describing the client who sent. </param>
         */
        private void BroadCastChat(byte[] data, Client clientWhoSentMsg)
        {
            string logEntry = string.Format(("{0} -> {1}: {2} " + Environment.NewLine), DateTime.Now.ToString(), clientWhoSentMsg.username, Encoding.UTF8.GetString(data));
            File.AppendAllText(Program.CHATLOGFILENAME, logEntry);

            foreach (Client player in lobby.gameRoom.listPlayers)
            {
                TSCryptography tsCryptoBroadCast = new TSCryptography(player.IV, player.SymKey);
                NetworkStream streamBroadCast = player.TcpClient.GetStream();
                TSProtocol tsProt = new TSProtocol(streamBroadCast);

                string str_player_plus_message;
                byte[] player_plus_message;

                if (player.ClientID == clientWhoSentMsg.ClientID){
                    str_player_plus_message = "Eu: " + Encoding.UTF8.GetString(data);
                    player_plus_message = Encoding.UTF8.GetBytes(str_player_plus_message);
                }
                else
                {
                    str_player_plus_message = clientWhoSentMsg.username + ": " + Encoding.UTF8.GetString(data);
                    player_plus_message = Encoding.UTF8.GetBytes(str_player_plus_message);
                }
                tsProtocol.SendPacket(ProtocolSICmdType.USER_OPTION_8, tsCryptoBroadCast, streamBroadCast, player_plus_message, Program.SERVERPRIVATEKEY);
                tsProtocol.WaitForAck();
                Console.WriteLine("Player {0}: Enviado Procolo => 8", player.username);
            }
        }

        /**
         * <summary>    Broad cast data. 
         *              Método responsável por fazer o broadcast de dados da aplicação.
         *              Ex: Mensagens de erro ou de sucesso. </summary>
         *
         * <remarks>    Simão Pedro, 04/06/2020. </remarks>
         *
         * <param name="data">              The data. </param>
         * <param name="protocolSICmdType"> Type of the protocol SI command. </param>
         */
        private void BroadCastData(byte[] data, ProtocolSICmdType protocolSICmdType)
        {
            foreach (Client player in lobby.gameRoom.listPlayers)
            {
                TSCryptography tsCryptoBroadCast = new TSCryptography(player.IV, player.SymKey);
                NetworkStream streamBroadCast = player.TcpClient.GetStream();
                TSProtocol tsProt = new TSProtocol(streamBroadCast);
                
                tsProtocol.SendPacket(protocolSICmdType, tsCryptoBroadCast, streamBroadCast, data, Program.SERVERPRIVATEKEY);
                tsProtocol.WaitForAck();
                Console.WriteLine("PLayer {0}: Enviado Procolo => {1}", player.username, protocolSICmdType);
            }
        }

        /**
         * <summary>    Broad cast star game. 
         *              Método responsável por fazer o broadcast do ínicio de jogo. </summary>
         *
         * <remarks>    Simão Pedro, 04/06/2020. </remarks>
         *
         * <param name="boardDimension">    The board dimension. </param>
         */
        private void BroadCastStarGame(int boardDimension)
        {
            foreach (Client player in lobby.gameRoom.listPlayers)
            {
                TSCryptography tsCryptoBroadCast = new TSCryptography(player.IV, player.SymKey);
                NetworkStream streamBroadCast = player.TcpClient.GetStream();
                TSProtocol tsProt = new TSProtocol(streamBroadCast);

                StartGame start = new StartGame(boardDimension, player.playerID);
                decryptedData = TSCryptography.ObjectToByteArray(start);

                tsProtocol.SendPacket(ProtocolSICmdType.USER_OPTION_1, tsCryptoBroadCast, streamBroadCast, decryptedData, Program.SERVERPRIVATEKEY);
                tsProtocol.WaitForAck();
                Console.WriteLine("Player: {0} : Enviado Procolo => USER_OPTION_1", player.username);
            }
        }
    }
}
