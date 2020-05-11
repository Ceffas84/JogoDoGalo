using EI.SI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using JogoDoGalo_Server.Models;

namespace JogoDoGalo.Models
{
    class ServerHandler
    {
        TcpClient TcpClient;
        ProtocolSI protocolSI;
        TSCryptography tsCrypto;
        public ServerHandler(TcpClient tcpClient)
        {
            this.TcpClient = tcpClient;
            this.protocolSI = new ProtocolSI();
            this.tsCrypto = new TSCryptography();
        }
        public void Handle()
        {
            Thread thread = new Thread(ServerListener);
            thread.Name = "ServerListener";
            thread.Start(TcpClient);
        }
        public void ServerListener(object obj)
        {
            TcpClient tcpClient = (TcpClient)obj;
            NetworkStream networkStream = TcpClient.GetStream();

            ProtocolSI protocolSI = new ProtocolSI();

            while (protocolSI.GetCmdType() != ProtocolSICmdType.EOT)
            {
                try
                {
                    networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
                    switch (protocolSI.GetCmdType())
                    {
                        case ProtocolSICmdType.SECRET_KEY:
                            //Recebe a Chave Simétrica do ProtocolSI
                            byte[] encryptedSymKey = protocolSI.GetData();

                            //Desencripta e atribui a key ao objecto aes
                            byte[] decryptedSymKey = tsCrypto.RsaDecryption(encryptedSymKey, tsCrypto.GetPrivateKey());
                            tsCrypto.SetAesSymKey(decryptedSymKey);

                            SendAcknowledged(protocolSI, networkStream);
                            break;
                        case ProtocolSICmdType.IV:
                            //Recebe o Vetor de Inicialização do ProtocolSI
                            byte[] ivEncrypted = protocolSI.GetData();

                            //desencripta e atribui o iv ao objecto aes
                            byte[] decryptedIV = tsCrypto.RsaDecryption(ivEncrypted, tsCrypto.GetPrivateKey());
                            tsCrypto.SetAesIV(decryptedIV);

                            SendAcknowledged(protocolSI, networkStream);
                            break;
                        case ProtocolSICmdType.DATA:
                            //recebe a mensagem
                            byte[] encriptedMsg = protocolSI.GetData();

                            //Desencripta a mensagem recebida
                            byte[] decryptedMsg = tsCrypto.SymetricDecryption(encriptedMsg);

                            //O código abaixo serve para utilizar elementos do ClientForm apartir de outra thread.
                            //Explicação: Uma vez que os elementos do form só podem ser chamados pela thread que executa o form.
                            string message = Encoding.UTF8.GetString(decryptedMsg, 0, decryptedMsg.Length);

                            //Invoke(new Action(() =>
                            //{
                            //    rtbMensagens.AppendText(message); rtbMensagens.AppendText(Environment.NewLine);
                            //}));

                            SendAcknowledged(protocolSI, networkStream);
                            break;

                        case ProtocolSICmdType.EOT:
                            SendAcknowledged(protocolSI, networkStream);
                            //Thread.Sleep(500);

                            break;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK);
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.HResult);
                    break;
                }
            }
            //SendAcknowledged(protocolSI, networkStream);
            networkStream.Close();
            tcpClient.Close();
        }
        public void SendEncryptedProtocol(ProtocolSICmdType protocolSICmdType, NetworkStream networkStream, byte[] data)
        {
            byte[] encrypteData = tsCrypto.SymetricEncryption(data);
            byte[] packet = protocolSI.Make(protocolSICmdType, encrypteData);
            networkStream.Write(packet, 0, packet.Length);
            networkStream.Flush();
        }
        public void SendAcknowledged(ProtocolSI propotcolSi, NetworkStream networkStream)
        {
            byte[] ack = protocolSI.Make(ProtocolSICmdType.ACK);
            networkStream.Write(ack, 0, ack.Length);
            networkStream.Flush();
        }
    }
}
