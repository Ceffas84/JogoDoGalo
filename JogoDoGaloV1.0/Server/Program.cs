using EI.SI;
using Server.Models;
using Server.Models.JogoDoGalo_Server.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
        private static GameRoom gameRoom = new GameRoom();
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
            
            while (true)
            {
                //aceitar ligações
                Client user = new Client();
                user.TcpClient = tcpListener.AcceptTcpClient();
                gameRoom.listUsers.Add(user);
                user.ClientID = gameRoom.listUsers.Count();

                Console.WriteLine("Client_{0} connected" + Environment.NewLine, user.ClientID);


                //Lança uma thread com um listner
                ClientHandler clientHandler = new ClientHandler(gameRoom);
                clientHandler.Handle();
            }
        }
    }

    class ClientHandler
    {
        GameRoom gameRoom;

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
        public ClientHandler(GameRoom gameroom)
        {
            gameRoom = gameroom;
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
            thread.Start(this.gameRoom);
        }
        public void ClientListener(object obj)
        {
            //Recebemos o user que efetuou ligação no servidor
            GameRoom gameRoomThread = (GameRoom)obj;
            Client client = gameRoom.listUsers[gameRoom.listUsers.Count-1];

            //user.PrivateKey = tsCrypto.GetPrivateKey();

            //Atribuimos as credencias de criptografia simétrica ao utilizador
            client.SymKey = tsCrypto.GetSymKey();
            client.IV = tsCrypto.GetIV();

            //Abrimos um novo canal de comunicação
            networkStream = client.TcpClient.GetStream();
            while (protocolSI.GetCmdType() != ProtocolSICmdType.EOT)
            {
                networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);

                switch (protocolSI.GetCmdType())
                {
                    case ProtocolSICmdType.SYM_CIPHER_DATA:
                        encryptedData = protocolSI.GetData();
                        decryptedData = tsCrypto.SymetricDecryption(encryptedData);
                        this.symDecipherData = decryptedData;



                        Console.WriteLine("SymCipherData recebida no servidor: {0}", Encoding.UTF8.GetString(this.symDecipherData));

                        packet = protocolSI.Make(ProtocolSICmdType.ACK);
                        networkStream.Write(packet, 0, packet.Length);
                        networkStream.Flush();

                        break;
                    case ProtocolSICmdType.DIGITAL_SIGNATURE:
                        this.digitalSignature = protocolSI.GetData();                     

                        Console.WriteLine("Assinatura digital recebida no servidor: {0}", Convert.ToBase64String(digitalSignature));

                        packet = protocolSI.Make(ProtocolSICmdType.ACK);
                        networkStream.Write(packet, 0, packet.Length);
                        networkStream.Flush();
                        break;
                    case ProtocolSICmdType.USER_OPTION_1:
                        
                        if (tsCrypto.VerifyData(symDecipherData, digitalSignature, client.PublicKey))
                        {

                            client.username = Encoding.UTF8.GetString(symDecipherData);
                            Console.WriteLine(client.username);
                        }
                        else
                        {
                            Console.WriteLine("Assinatura diginal falhou no servidor.");
                        }
                        
                        packet = protocolSI.Make(ProtocolSICmdType.ACK);
                        networkStream.Write(packet, 0, packet.Length);
                        networkStream.Flush();
                        break;

                    case ProtocolSICmdType.USER_OPTION_2:

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
                        }
                        packet = protocolSI.Make(ProtocolSICmdType.ACK);
                        networkStream.Write(packet, 0, packet.Length);
                        networkStream.Flush();
                        break;

                    case ProtocolSICmdType.USER_OPTION_3:
                        packet = protocolSI.Make(ProtocolSICmdType.ACK);
                        networkStream.Write(packet, 0, packet.Length);
                        networkStream.Flush();

                        if (!client.isLogged)
                        {
                            if (client.username.Length > 0 && client.saltedPasswordHash.Length > 0)
                            {
                                if (Auth.VerifyLogin(client.username, Encoding.UTF8.GetString(client.password)))
                                {
                                    Console.WriteLine("Client_{0}: {1} => login successfull" + Environment.NewLine, client.ClientID, client.username);
                                    //packet = protocolSI.Make(ProtocolSICmdType.USER_OPTION_1, (int)ServerResponse.LOGIN_SUCCESS);
                                    //networkStream.Write(packet, 0, packet.Length);
                                    client.isLogged = true;

                                    //BroadLoggedUsers(ProtocolSICmdType.USER_OPTION_3, user.username, user, gameRoom);
                                }
                                else
                                {
                                    Console.WriteLine("Client_{0}: login unsuccessfull" + Environment.NewLine, client.ClientID);

                                    //packet = protocolSI.Make(ProtocolSICmdType.USER_OPTION_1, (int)ServerResponse.LOGIN_ERROR);
                                    //networkStream.Write(packet, 0, packet.Length);
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("Client_{0}: is already logged" + Environment.NewLine, client.ClientID);
                        }
                       

                        break;

                    case ProtocolSICmdType.USER_OPTION_4:
                        //FAZ O REGISTO DE UM NOVO USER
                        if (!client.isLogged)
                        {
                            try
                            {
                                Auth.Register(client.username, client.saltedPasswordHash, client.salt);
                            }
                            catch (Exception ex)
                            {
                                //packet = protocolSI.Make(ProtocolSICmdType.USER_OPTION_1, (int)ServerResponse.REGISTER_ERROR);
                                //networkStream.Write(packet, 0, packet.Length);

                                Console.WriteLine("Client_{0}: register unsuccessfull", client.ClientID);
                                packet = protocolSI.Make(ProtocolSICmdType.ACK);
                                networkStream.Write(packet, 0, packet.Length);
                                networkStream.Flush();
                                break;
                            }
                            Console.WriteLine("Client_{0}: register successfull", client.ClientID);
                            packet = protocolSI.Make(ProtocolSICmdType.ACK);
                            networkStream.Write(packet, 0, packet.Length);
                        }
                        else
                        {
                            Console.WriteLine("Client_{0}: is already logged, please logout and then register a new user!" + Environment.NewLine, client.ClientID);
                        }

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
            gameRoom.listUsers.Remove(client);
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
            foreach (Client client in gameRoom.listUsers)
            {
                TSCryptography tsCryptoBroadCast = new TSCryptography(client.IV, client.SymKey);
                //ProtocolSI protocolSI = new ProtocolSI();
                NetworkStream streamBroadCast = client.TcpClient.GetStream();
                string str_player_plus_message;
                byte[] player_plus_message;

                if (client.ClientID == clientWhoSentMsg.ClientID){
                    str_player_plus_message = "Eu: " + Encoding.UTF8.GetString(data);
                    player_plus_message = Encoding.UTF8.GetBytes(str_player_plus_message);
                }
                else
                {
                    str_player_plus_message = "Jogador " + clientWhoSentMsg.ClientID + ": " + Encoding.UTF8.GetString(data);
                    player_plus_message = Encoding.UTF8.GetBytes(str_player_plus_message);
                }

                EncryptSignAndSendProtocol(player_plus_message, ProtocolSICmdType.USER_OPTION_1, streamBroadCast, tsCryptoBroadCast);
            }
        }
    }
}
