﻿using EI.SI;
using Server.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JogoDoGaloV1._0
{
    public partial class Form1 : Form
    {
        private TcpClient tcpClient;
        private IPEndPoint ipEndPoint;
        private NetworkStream networkStream;

        private TSCryptography tsCrypto;
        private ProtocolSI protocolSI;
        TSProtocol tsProtocol;

        private string publicKey;
        private string privateKey;
        private string serverPublicKey;

        private byte[] digitalSignature;
        private byte[] symDecipherData;

        private byte[] encryptedData;
        private byte[] decryptedData;
        private byte[] packet;

        private Thread thread;

        private bool Acknoledged = false;
        public Form1()
        {
            InitializeComponent();

            ipEndPoint = new IPEndPoint(IPAddress.Loopback, 10000);
            tcpClient = new TcpClient();

            tcpClient.Connect(ipEndPoint);
            networkStream = tcpClient.GetStream();

            
            tsCrypto = new TSCryptography();
            protocolSI = new ProtocolSI();
            tsProtocol = new TSProtocol(networkStream);

            thread = new Thread(ServerLisneter);
            thread.Name = "ServerLisneter";
            thread.Start(tcpClient);

            publicKey = tsCrypto.GetPublicKey();
            privateKey = tsCrypto.GetPrivateKey();

            byte[] packet = protocolSI.Make(ProtocolSICmdType.PUBLIC_KEY, publicKey);
            networkStream.Write(packet, 0, packet.Length);
        }
        private void ServerLisneter(object obj)
        {
            TcpClient tcpClient = (TcpClient)obj;
            NetworkStream stream = tcpClient.GetStream();
            TSProtocol tsProtocol = new TSProtocol(stream);

            while (protocolSI.GetCmdType() != ProtocolSICmdType.EOT)
            {
                try
                {
                    stream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
                    switch (protocolSI.GetCmdType())
                    {
                        case ProtocolSICmdType.SYM_CIPHER_DATA:
                            encryptedData = protocolSI.GetData();
                            decryptedData = tsCrypto.SymetricDecryption(encryptedData);
                            this.symDecipherData = decryptedData;
                            
                            Invoke(new Action(() => { Console.WriteLine("Recebido o SymCipherData: {0}", Encoding.UTF8.GetString(this.symDecipherData)); }));

                            packet = protocolSI.Make(ProtocolSICmdType.ACK);
                            stream.Write(packet, 0, packet.Length);
                            break;
                        case ProtocolSICmdType.DIGITAL_SIGNATURE:
                            this.digitalSignature = protocolSI.GetData();

                            Invoke(new Action(() => { Console.WriteLine("Assinatura digital recebida: {0}", Convert.ToBase64String(this.digitalSignature)); }));

                            packet = protocolSI.Make(ProtocolSICmdType.ACK);
                            stream.Write(packet, 0, packet.Length);
                            break;
                        case ProtocolSICmdType.USER_OPTION_1:

                            if (tsCrypto.VerifyData(decryptedData, digitalSignature, serverPublicKey))
                            {
                                packet = protocolSI.Make(ProtocolSICmdType.ACK);
                                stream.Write(packet, 0, packet.Length);

                                Invoke(new Action(() => { Console.WriteLine("Assinatura digital confirmada"); }));
                            }
                               
                            
                            break;
                        //******************   ABRETURA DE COMUNICAÇÃO ENCRIPTADA SIMÉTRICA   ******************
                        case ProtocolSICmdType.PUBLIC_KEY:
                            //Recebe a public key do client
                            serverPublicKey = protocolSI.GetStringFromData();

                            //Envia um acknoledged
                            packet = protocolSI.Make(ProtocolSICmdType.ACK);
                            networkStream.Write(packet, 0, packet.Length);
                            break;
                        case ProtocolSICmdType.SECRET_KEY:
                            //Recebe a Chave Simétrica do ProtocolSI
                            byte[] encryptedSymKey = protocolSI.GetData();

                            //Desencripta e atribui a key ao objecto aes
                            byte[] decryptedSymKey = tsCrypto.RsaDecryption(encryptedSymKey, privateKey);
                            tsCrypto.SetAesSymKey(decryptedSymKey);

                            SendAcknowledged(protocolSI, stream);
                            Invoke(new Action(() => { Console.WriteLine("Recebido SymKey: {0}", Convert.ToBase64String(decryptedSymKey)); }));
                            break;
                        case ProtocolSICmdType.IV:
                            //Recebe o Vetor de Inicialização do ProtocolSI
                            byte[] ivEncrypted = protocolSI.GetData();

                            //Desencripta e atribui o iv ao objecto aes
                            byte[] decryptedIV = tsCrypto.RsaDecryption(ivEncrypted, tsCrypto.GetPrivateKey());
                            tsCrypto.SetAesIV(decryptedIV);

                            SendAcknowledged(protocolSI, stream);
                            Invoke(new Action(() => { Console.WriteLine("Recebido IV: {0}", Convert.ToBase64String(decryptedIV)); }));
                            
                            break;
                        case ProtocolSICmdType.ACK:
                            Acknoledged = true;
                            break;
                    }
                }
                catch
                {

                }
            }
        }
        public void SendProtocol(NetworkStream stream, ProtocolSICmdType protocolSICmdType, byte[] data)
        {
            byte[] packet = protocolSI.Make(protocolSICmdType, data);
            stream.Write(packet, 0, packet.Length);
        }
        public void SendProtocol(NetworkStream stream, ProtocolSICmdType protocolSICmdType)
        {
            byte[] packet = protocolSI.Make(protocolSICmdType);
            stream.Write(packet, 0, packet.Length);
        }
        public void SendEncrDigSignProtocol(NetworkStream stream, TSCryptography tsCryptoObj, ProtocolSICmdType protocolSICmdType, User activeUser, byte[] data)
        {
            SendProtocol(stream, ProtocolSICmdType.SYM_CIPHER_DATA, tsCryptoObj.SymetricEncryption(data));
            WaitForAck();

            //tsCryptoObj.SetRsaPrivateKeyCryptography(privateKey);
            byte[] digitalSignature = tsCryptoObj.SignData(data, activeUser.PrivateKey);

            SendProtocol(stream, ProtocolSICmdType.DIGITAL_SIGNATURE, digitalSignature);
            WaitForAck();

            SendProtocol(stream, protocolSICmdType);
            WaitForAck();
        }
        public void SendAcknowledged(ProtocolSI protocolSI, NetworkStream networkStream)
        {
            byte[] ack = protocolSI.Make(ProtocolSICmdType.ACK);
            networkStream.Write(ack, 0, ack.Length);
        }
        public void WaitForAck()
        {
            while (protocolSI.GetCmdType() != ProtocolSICmdType.ACK)
            {
                networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            byte[] decryptedData = Encoding.UTF8.GetBytes(tbChat.Text);
            
            //Envia um Protocol com a mensagem encriptada
            byte[] encryptedData = tsCrypto.SymetricEncryption(decryptedData);
            byte[] packet = protocolSI.Make(ProtocolSICmdType.SYM_CIPHER_DATA, encryptedData);
            networkStream.Write(packet, 0, packet.Length);
            while (!Acknoledged) { }
            Acknoledged = false;


            //Cria e envia a assinatura digital da menssagem
            byte[] digitalSignature = tsCrypto.SignData(decryptedData, privateKey);
            packet = protocolSI.Make(ProtocolSICmdType.DIGITAL_SIGNATURE, digitalSignature);
            networkStream.Write(packet, 0, packet.Length);
            while (!Acknoledged) { }
            Acknoledged = false;

            //Envia o Protocol de comando
            packet = protocolSI.Make(ProtocolSICmdType.USER_OPTION_1);
            networkStream.Write(packet, 0, packet.Length);
            while (!Acknoledged) { }
            Acknoledged = false;

        }
    }
}
