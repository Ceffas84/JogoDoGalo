using EI.SI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography;
using JogoDoGalo_Server.Models;

namespace JogoDoGalo
{
    public partial class ClientForm : Form
    {
        private static int dimensaoTabuleiro = 3;
        private const int PORT = 10000;
        private TcpClient tcpClient;
        private IPEndPoint ipEndPoint;
        private NetworkStream networkStream;
        private ProtocolSI protocolSI;
        private TSCryptography tsCrypto;
        private string publicKey;
        //private AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
        private Thread thread;
        private byte[] decryptedSymKey;
        private byte[] decryptedIV;
        public ClientForm()
        {
            InitializeComponent();

            ipEndPoint = new IPEndPoint(IPAddress.Parse(tb_Servidor.Text), PORT);
            tcpClient = new TcpClient();

            tcpClient.Connect(ipEndPoint);
            networkStream = tcpClient.GetStream();

            protocolSI = new ProtocolSI();
            tsCrypto = new TSCryptography();

            thread = new Thread(ServerLisneter);
            thread.Start(tcpClient);
        }
        private void ServerLisneter(object obj)
        {
            TcpClient tcpClient = (TcpClient) obj;
            NetworkStream networkStream = tcpClient.GetStream();

            publicKey = tsCrypto.GetPublicKey();

            Invoke(new Action(() => {
                Console.WriteLine("Chave publica: {0}", publicKey);
            }));

            byte[] packet = protocolSI.Make(ProtocolSICmdType.PUBLIC_KEY, publicKey);
            networkStream.Write(packet, 0, packet.Length);
            while (protocolSI.GetCmdType() != ProtocolSICmdType.ACK)
            {
                networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
            }

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
                            decryptedSymKey = tsCrypto.RsaDecryption(encryptedSymKey, tsCrypto.GetPrivateKey());
                            tsCrypto.SetAesSymKey(decryptedSymKey);

                            SendAcknowledged(protocolSI, networkStream);
                            break;
                        case ProtocolSICmdType.IV:
                            //Recebe o Vetor de Inicialização do ProtocolSI
                            byte[] ivEncrypted = protocolSI.GetData();

                            //Desencripta e atribui o iv ao objecto aes
                            decryptedIV = tsCrypto.RsaDecryption(ivEncrypted, tsCrypto.GetPrivateKey());
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
                            Invoke(new Action(() => {
                                rtbMensagens.AppendText(message); rtbMensagens.AppendText(Environment.NewLine);
                            }));

                            SendAcknowledged(protocolSI, networkStream);
                            break;

                        //case ProtocolSICmdType.EOT:                                   o cliente nunca vai receber um EOT
                        //    //SendAcknowledged(protocolSI, networkStream);
                        //    packet = protocolSI.Make(ProtocolSICmdType.ACK);
                        //    networkStream.Write(packet, 0, packet.Length);
                        //    break;
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
        private void bt_EnviaMensagem_Click(object sender, EventArgs e)
        {
            if (tcpClient.Connected)
            {
                networkStream = tcpClient.GetStream();
                string msg = tb_EscreveMensagem.Text;
                tb_EscreveMensagem.Clear();

                SendEncryptedProtocol(ProtocolSICmdType.DATA, Encoding.UTF8.GetBytes(msg));

                //while (protocolSI.GetCmdType() != ProtocolSICmdType.ACK)
                //{
                //    networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
                //}

                networkStream.Flush();
                rtbMensagens.AppendText("Eu: " + msg + Environment.NewLine);
            }
        }
        public void SendEncryptedProtocol(ProtocolSICmdType protocolSICmdType, byte[] data)
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
        //
        //Código para teste do jogo no tabuleiro
        //
        private void btnAddClient_Click(object sender, EventArgs e)
        {
            ClientForm newcliente = new ClientForm();
            newcliente.Show();
        }
        public void IniciarJogo()
        {
            DesenharTabuleiro(100, 50, 400, 3);
        }
        private void DesenharTabuleiro(int offset_x, int offset_y, int size, int numButtons)
        {
            int padding = Convert.ToInt32(Math.Round(size * 0.02, MidpointRounding.AwayFromZero));
            int buttonSize = (size - (padding * (numButtons + 1))) / numButtons;
            for (int I = 0; I < dimensaoTabuleiro; I++)
            {
                for (int J = 0; J < dimensaoTabuleiro; J++)
                {
                    Button newButton = new Button();
                    newButton.Text = "";
                    newButton.Name = I + "_" + J;
                    newButton.Location = new Point(offset_x + padding + (I * (padding + buttonSize)), offset_y + padding + (J * (padding + buttonSize)));
                    newButton.Width = buttonSize;
                    newButton.Height = buttonSize;
                    newButton.BackColor = System.Drawing.Color.LightGray;
                    newButton.Click += Tabuleiro_Click;
                    this.Controls.Add(newButton);
                }
            }
        }
        private void Tabuleiro_Click(object sender, EventArgs e)
        {

            Button clickedButton = sender as Button;
            List<int> coord = GetClickedButton(clickedButton);



            //Cria-se e introduz-se a jogada

            //Atualiza-se o botão com o simbolo do jogador que jogou

            //clickedButton.Text = jogadaIntroduzida.Jogador.Simbolo.ToString();

            //Verifica-se se o jogador ganhou o jogo

            //Verifica se foi a ultima jogada

        }
        private List<int> GetClickedButton(Button clickedButton)
        {
            string[] a = clickedButton.Name.Split('_');
            List<int> coord = new List<int>();
            coord.Add(int.Parse(a[1]));
            coord.Add(int.Parse(a[0]));
            return coord;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            IniciarJogo();
        }
        private void ClientForm_FormClosing(object sender, FormClosingEventArgs e)
        {

        }
        private void CloseClient()
        {
            //Preparar o envio da mensagem para desligar as ligações
            
            byte[] eot = protocolSI.Make(ProtocolSICmdType.EOT);
            networkStream.Write(eot, 0, eot.Length);
            networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);

            networkStream.Close();
            tcpClient.Close();
        }
        private void bt_Autenticar_Click(object sender, EventArgs e)
        {
            byte[] packet;

         
            //Enviamos encriptado os dados do username
            byte[] username = Encoding.UTF8.GetBytes(tb_Jogador.Text);
            byte[] encryptedData = tsCrypto.SymetricEncryption(username);
            packet = protocolSI.Make(ProtocolSICmdType.USER_OPTION_1, encryptedData);
            networkStream.Write(packet, 0, packet.Length);

            while (protocolSI.GetCmdType() != ProtocolSICmdType.ACK)
            {
                networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
            }

            Console.WriteLine("Username encriptado: " + Convert.ToBase64String(packet));

            //Enviamos encriptado os dados da password
            byte[] password = Encoding.UTF8.GetBytes(tb_Password.Text);
            encryptedData = tsCrypto.SymetricEncryption(password);
            packet = protocolSI.Make(ProtocolSICmdType.USER_OPTION_2, encryptedData);
            networkStream.Write(packet, 0, packet.Length);
            
            while (protocolSI.GetCmdType() != ProtocolSICmdType.ACK)
            {
                networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
            }

            Console.WriteLine("Password encriptado: " + Convert.ToBase64String(packet));
        }

        private void sairToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CloseClient();
        }
    }
}
