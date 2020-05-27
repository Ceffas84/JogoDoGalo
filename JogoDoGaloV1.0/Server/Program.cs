using EI.SI;
using Server.Models;
using Server.Models.JogoDoGalo_Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
        static void Main(string[] args)
        {
            //declaração das variáveis
            //o servidor vai estar à escuta de todos os endereços IP
            IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Any, PORT);
            tcpListener = new TcpListener(iPEndPoint);

            //iniciar o servidor
            tcpListener.Start();
            
            while (true)
            {
                //aceitar ligações
                User user = new User();
                user.TcpClient = tcpListener.AcceptTcpClient();
                gameRoom.listUsers.Add(user);
                user.UserID = gameRoom.listUsers.Count();

                Console.WriteLine("Client_{0} connected" + Environment.NewLine, user.UserID);


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
            Thread thread = new Thread(ClientListener);
            thread.Start(this.gameRoom.listUsers[gameRoom.listUsers.Count - 1]);
        }
        public void ClientListener(object obj)
        {
            //Recebemos o user que efetuou ligação no servidor
            User user = (User)obj;
            user.PrivateKey = tsCrypto.GetPrivateKey();

            //Atribuimos as credencias de criptografia simétrica ao utilizador
            user.SymKey = tsCrypto.GetSymKey();
            user.IV = tsCrypto.GetIV();

            //Abrimos um novo canal de comunicação
            networkStream = user.TcpClient.GetStream();
            while (protocolSI.GetCmdType() != ProtocolSICmdType.EOT)
            {
                networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);

                switch (protocolSI.GetCmdType())
                {
                    case ProtocolSICmdType.SYM_CIPHER_DATA:
                        encryptedData = protocolSI.GetData();
                        decryptedData = tsCrypto.SymetricDecryption(encryptedData);
                        this.symDecipherData = decryptedData;

                        packet = protocolSI.Make(ProtocolSICmdType.ACK);
                        networkStream.Write(packet, 0, packet.Length);

                        Console.WriteLine("Recebido o SymCipherData: {0}", Encoding.UTF8.GetString(this.symDecipherData));
                        break;
                    case ProtocolSICmdType.DIGITAL_SIGNATURE:
                        this.digitalSignature = protocolSI.GetData();

                        packet = protocolSI.Make(ProtocolSICmdType.ACK);
                        networkStream.Write(packet, 0, packet.Length);


                        Console.WriteLine("Assinatura digital recebeida: {0}", Convert.ToBase64String(digitalSignature));
                        break;
                    case ProtocolSICmdType.USER_OPTION_1:
                        //string trimming = Encoding.UTF8.GetString(symDecipherData);
                        //trimming = trimming.Trim('\0');
                        //decryptedData = Encoding.UTF8.GetBytes(trimming);
                        decryptedData = symDecipherData;
                        
                        //tsCrypto.SetRsaPrivateKeyCryptography(user.PrivateKey);

                        packet = protocolSI.Make(ProtocolSICmdType.ACK);
                        networkStream.Write(packet, 0, packet.Length);

                        if (tsCrypto.VerifyData(decryptedData, digitalSignature, user.PublicKey))
                        {

                            decryptedData = symDecipherData;

                            //Envia um Protocol com a mensagem encriptada
                            encryptedData = tsCrypto.SymetricEncryption(decryptedData);
                            packet = protocolSI.Make(ProtocolSICmdType.SYM_CIPHER_DATA, encryptedData);
                            networkStream.Write(packet, 0, packet.Length);
                            while (protocolSI.GetCmdType() != ProtocolSICmdType.ACK)
                            {
                                networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
                            }

                            //Cria e envia a assinatura digital da menssagem
                            digitalSignature = tsCrypto.SignData(decryptedData, user.PrivateKey);
                            packet = protocolSI.Make(ProtocolSICmdType.DIGITAL_SIGNATURE, digitalSignature);
                            networkStream.Write(packet, 0, packet.Length);
                            while (protocolSI.GetCmdType() != ProtocolSICmdType.ACK)
                            {
                                networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
                            }

                            //Envia o Protocol de comando
                            packet = protocolSI.Make(ProtocolSICmdType.USER_OPTION_1, encryptedData);
                            networkStream.Write(packet, 0, packet.Length);
                            while (protocolSI.GetCmdType() != ProtocolSICmdType.ACK)
                            {
                                networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
                            }

                            //Envia o Protocol de comando
                            Console.WriteLine("Assinatura digital confirmada");
                        }

                        break;
                    case ProtocolSICmdType.PUBLIC_KEY:
                        //Recebe a public key do client
                        user.PublicKey = protocolSI.GetStringFromData();
                        
                        //Envia um acknoledged
                        packet = protocolSI.Make(ProtocolSICmdType.ACK);
                        networkStream.Write(packet, 0, packet.Length);

                        //Envia a public key
                        packet = protocolSI.Make(ProtocolSICmdType.PUBLIC_KEY, tsCrypto.GetPublicKey());
                        networkStream.Write(packet, 0, packet.Length);
                        while (protocolSI.GetCmdType() != ProtocolSICmdType.ACK)
                        {
                            networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
                        }

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
                    case ProtocolSICmdType.EOT:
                        break;
                }
            }
            user.TcpClient.Close();
            networkStream.Close();
            gameRoom.listUsers.Remove(user);
        }
    }
}
