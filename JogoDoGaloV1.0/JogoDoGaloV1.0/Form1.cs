using EI.SI;
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
        private const int BREAK = 100; 

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

            //  **** TABELA DE UTILIZAÇÃO DE COMANDOS DO PROTOCOLSI ****
            //
            //  SYM_CIPHER_DATA       => Receção de menssagem encriptada
            //  DIGITAL_SIGNATURE     => Receção de assinatura digital da mensagem enviada
            //
            //  PUBLIC_KEY            => Receção da PublicKey do servidor
            //  SECRET_KEY            => Receção da SecretKey
            //  IV                    => Receção do vetor de inicialização
            //
            //  USER_OPTION_1         => Receção do Start Game
            //  USER_OPTION_2         => Receção do Próximo jogador
            //  USER_OPTION_3         => Receção de Jogadas
            //  USER_OPTION_4         => Receção de Game Over
            //  USER_OPTION_5         => Receção de Jogadores Logados
            //  USER_OPTION_6         =>
            //  USER_OPTION_7         =>
            //  USER_OPTION_8         => Receção de mensagens no chat
            //  USER_OPTION_9         => Receção de mensagens de Erro ou Sucesso
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
                            
                            Invoke(new Action(() => { Console.WriteLine("SymCipherData recebida no cliente: {0}", Encoding.UTF8.GetString(this.symDecipherData)); }));

                            packet = protocolSI.Make(ProtocolSICmdType.ACK);
                            stream.Write(packet, 0, packet.Length);
                            break;
                        case ProtocolSICmdType.DIGITAL_SIGNATURE:
                            this.digitalSignature = protocolSI.GetData();

                            Invoke(new Action(() => { Console.WriteLine("Assinatura digital recebida no cliente: {0}", Convert.ToBase64String(this.digitalSignature)); }));

                            packet = protocolSI.Make(ProtocolSICmdType.ACK);
                            stream.Write(packet, 0, packet.Length);
                            break;
                        case ProtocolSICmdType.USER_OPTION_1:

                            if (tsCrypto.VerifyData(decryptedData, digitalSignature, serverPublicKey))
                            {
                                packet = protocolSI.Make(ProtocolSICmdType.ACK);
                                stream.Write(packet, 0, packet.Length);

                                Invoke(new Action(() => { Console.WriteLine("Assinatura digital confirmada no cliente"); }));
                            }

                            packet = protocolSI.Make(ProtocolSICmdType.ACK);
                            stream.Write(packet, 0, packet.Length);
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

                        case ProtocolSICmdType.USER_OPTION_9:
                            int resultCode = protocolSI.GetIntFromData();
                            Invoke(new Action(() => { checkServerResponse(resultCode); }));
                            break;
                    }
                }
                catch
                {

                }
            }
            tcpClient.Close();
            stream.Close();
        }

        private void checkServerResponse(int resultCode)
        {
            switch (resultCode)
            {
                case 00:
                    MessageBox.Show("Não foi possível fazer o seu registo.", "Erro de Registo");
                    break;
                case 01:
                    MessageBox.Show("Login inválido.", "Erro de Login");
                    break;
                case 02:
                    MessageBox.Show("Para jogar tem de estar logado.", "Erro de Login");
                    break;
                case 03:
                    MessageBox.Show("Já está logado. Para fazer novo registo tem de fazer logout e registar um novo utilizador.", "Erro de Login");
                    break;
                case 04:
                    MessageBox.Show("O username e a password têm de ter pelo menos 8 caractéres.", "Erro de introdução de dados");
                    break;
                case 05:
                    MessageBox.Show("Assinatura diginal inválida.", "Erro de Assinatura digital.");
                    break;
                case 06:
                    MessageBox.Show("O utilizador já se encontra logado noutra aplicaçáo cliente.", "Erro de utilizador.");
                    break;
                case 10:
                    MessageBox.Show("Registado com sucesso! Faça login para se juntar a uma sala de jogo.", "Sucesso de Registo");
                    break;
                case 11:
                    MessageBox.Show("Logado com sucesso. Espere até que estejam dois jogadores na sala para começar o jogo.", "Sucesso de Login");
                    break;
                case 20:
                    MessageBox.Show("O jogo ainda não começou! Espere que estejam duas pessoas na sala para começar o jogo.", "Erro de utilizador");
                    break;
                case 21:
                    MessageBox.Show("O jogo já começou! Não é possível iniciar um novo jogo enquanto outro está em curso.", "Erro de utilizador");
                    break;
                case 22:
                    MessageBox.Show("Jogada inválida. Jogue novamente.", "Erro de utilizador");
                    break;
                case 23:
                    MessageBox.Show("Não é a sua vez de jogar. Espere que o seu adversário jogue.", "Erro de utilizador");
                    break;
            }
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

        private void button1_Click(object sender, EventArgs e)  //BUTÃO DE TESTE!!!! PARA APAGAR!!!!
        {
            byte[] decryptedData = Encoding.UTF8.GetBytes(tbChat.Text);

            //Envia um Protocol com a mensagem encriptada
            EncryptSignAndSendProtocol(decryptedData, ProtocolSICmdType.USER_OPTION_1);
        }

        private void EncryptSignAndSendProtocol(byte[] data, ProtocolSICmdType protocolCmd)
        {
            byte[] encryptedData = tsCrypto.SymetricEncryption(data);
            byte[] packet = protocolSI.Make(ProtocolSICmdType.SYM_CIPHER_DATA, encryptedData);
            networkStream.Write(packet, 0, packet.Length);
            networkStream.Flush();
            Console.WriteLine("Data encryptada enviada do cliente: {0}", Encoding.UTF8.GetString(data));
            Thread.Sleep(BREAK);

            //Cria e envia a assinatura digital da menssagem
            byte[] digitalSignature = tsCrypto.SignData(data, privateKey);
            packet = protocolSI.Make(ProtocolSICmdType.DIGITAL_SIGNATURE, digitalSignature);
            networkStream.Write(packet, 0, packet.Length);
            networkStream.Flush();
            Console.WriteLine("Assinatura digital enviada do cliente: {0}", Convert.ToBase64String(digitalSignature));
            Thread.Sleep(BREAK);

            //Envia o Protocol de comando
            packet = protocolSI.Make(protocolCmd);
            networkStream.Write(packet, 0, packet.Length);
            networkStream.Flush();
            Console.WriteLine("Comando enviado do cliente: {0}", protocolCmd);
            Thread.Sleep(BREAK);

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            CloseClient();
        }

        private void CloseClient()
        {
            //Preparar o envio da mensagem para desligar as ligações
            byte[] eot = protocolSI.Make(ProtocolSICmdType.EOT);
            networkStream.Write(eot, 0, eot.Length);

            //Aguarda confirmação da receção do username
            Thread.Sleep(2000);

            networkStream.Close();
            tcpClient.Close();
        }

        private void button2_Click(object sender, EventArgs e) //BUTÃO DE TESTE!!!! PARA APAGAR!!!!
        {
            Form1 newcliente = new Form1();
            newcliente.Show();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            EncryptSignAndSendProtocol(Encoding.UTF8.GetBytes(tbUsername.Text), ProtocolSICmdType.USER_OPTION_1);

            EncryptSignAndSendProtocol(Encoding.UTF8.GetBytes(tbPassword.Text), ProtocolSICmdType.USER_OPTION_2);

            packet = protocolSI.Make(ProtocolSICmdType.USER_OPTION_3);
            networkStream.Write(packet, 0, packet.Length);
            Thread.Sleep(BREAK);  //???
        }

        private void btnSignup_Click(object sender, EventArgs e)
        {
            EncryptSignAndSendProtocol(Encoding.UTF8.GetBytes(tbUsername.Text), ProtocolSICmdType.USER_OPTION_1);

            EncryptSignAndSendProtocol(Encoding.UTF8.GetBytes(tbPassword.Text), ProtocolSICmdType.USER_OPTION_2);

            packet = protocolSI.Make(ProtocolSICmdType.USER_OPTION_4);
            networkStream.Write(packet, 0, packet.Length);
            Thread.Sleep(BREAK);
        }

        private void bt_EnviaMensagem_Click(object sender, EventArgs e)
        {
            byte[] decryptedData = Encoding.UTF8.GetBytes(tbEscreverMensagem.Text);

            //Envia a mensagem encriptada, a assinatura digital e o Protocol a usar para lidar com a informação recebida
            EncryptSignAndSendProtocol(decryptedData, ProtocolSICmdType.USER_OPTION_8);
        }
    }
}
