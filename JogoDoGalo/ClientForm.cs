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

namespace JogoDoGalo
{
    public partial class ClientForm : Form
    {
        private static int dimensaoTabuleiro = 3;
        private const int PORT = 10000;
        TcpClient tcpClient;
        IPEndPoint ipEndPoint;
        NetworkStream networkStream;
        ProtocolSI protocolSI;
        RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
        private string publicKey;
        AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
        private byte[] decryptedSecretKey;
        private byte[] decryptedIV;
        public ClientForm()
        {
            InitializeComponent();

            ipEndPoint = new IPEndPoint(IPAddress.Parse(tb_Servidor.Text), PORT);
            tcpClient = new TcpClient();

            tcpClient.Connect(ipEndPoint);
            networkStream = tcpClient.GetStream();

            //preparação da comunicação utilizando a class ProtocolSI
            protocolSI = new ProtocolSI();

        }
        private void ServerLisneter(object obj)
        {
            TcpClient tcpClient = (TcpClient) obj;
            networkStream = tcpClient.GetStream();
            while (protocolSI.GetCmdType() != ProtocolSICmdType.EOT)
            {
                try
                {
                    networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
                    switch (protocolSI.GetCmdType())
                    {
                        case ProtocolSICmdType.SECRET_KEY:
                            //Recebe a Chave Simétrica do ProtocolSI
                            byte[] secretKeyEncrypted = protocolSI.GetData();
                            string str_secret = Convert.ToBase64String(secretKeyEncrypted);

                            //Desencripta e atribui a key ao objecto aes
                            decryptedSecretKey = rsa.Decrypt(secretKeyEncrypted, true);
                            string str_symkey = Convert.ToBase64String(decryptedSecretKey);
                            aes.Key = decryptedSecretKey;

                            //Imprime na consola do Form o Vetor de Inicialização
                            Invoke(new Action(() => {
                                Console.WriteLine("Chave simetrica desencriptada: {0}", Convert.ToBase64String(aes.Key));
                            }));

                            SendAcknowledged(protocolSI, networkStream);
                            break;
                        case ProtocolSICmdType.IV:
                            //Recebe o Vetor de Inicialização do ProtocolSI
                            byte[] ivEncrypted = protocolSI.GetData();

                            //atribui o iv ao objecto aes
                            decryptedIV = rsa.Decrypt(ivEncrypted, true);
                            string str_iv = Convert.ToBase64String(decryptedIV);
                            aes.IV = decryptedIV;

                            //Imprime na consola do Form o Vetor de Inicialização
                            Invoke(new Action(() => { 
                                Console.WriteLine("Vetor de inicialização desencriptado: {0}", Convert.ToBase64String(aes.IV)); 
                            }));

                            SendAcknowledged(protocolSI, networkStream);
                            break;
                        case ProtocolSICmdType.DATA:
                            byte[] encriptedMsg = protocolSI.GetData();
                            SendAcknowledged(protocolSI, networkStream);

                            byte[] decryptedMsg = symetricDecryption(encriptedMsg);

                            string message = Encoding.UTF8.GetString(decryptedMsg, 0, decryptedMsg.Length);
                            //O código abaixo serve para utilizar elementos do ClientForm apartir de outra thread.
                            //Explicação: Uma vez que os elementos do form só podem ser chamados pela thread
                            //que executa o form.
                            Invoke(new Action(() => {
                                rtbMensagens.AppendText(message); rtbMensagens.AppendText(Environment.NewLine);
                            }));


                            break;

                        case ProtocolSICmdType.EOT:
                            SendAcknowledged(protocolSI, networkStream);
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
            SendAcknowledged(protocolSI, networkStream);
            networkStream.Close();
            tcpClient.Close();
        }
        private byte[] symetricEncryption(byte[] arr)
        {
            byte[] encryptedArr;

            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(arr, 0, arr.Length);
                    //cs.Close();
                }
                //guardar os dados cifrados que estão em memória
                encryptedArr = ms.ToArray();
                //ms.Close();
            }
            return encryptedArr;
        }
        private byte[] symetricDecryption(byte[] encryptedArr)
        {
            byte[] decryptedArr;

            MemoryStream ms = new MemoryStream(encryptedArr);

            CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);

            decryptedArr = new byte[ms.Length];

            cs.Read(decryptedArr, 0, decryptedArr.Length);

            cs.Close();
            ms.Close();
            return decryptedArr;
        }
        private void bt_EnviaMensagem_Click(object sender, EventArgs e)
        {
            if (tcpClient.Connected)
            {
                networkStream = tcpClient.GetStream();
                string msg = tb_EscreveMensagem.Text;
                tb_EscreveMensagem.Clear();

                //byte[] encryptedMsg = symetricEncryption(Encoding.UTF8.GetBytes(msg));
                //byte[] packet = protocolSI.Make(ProtocolSICmdType.DATA, encryptedMsg);
                //networkStream.Write(packet, 0, packet.Length);

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
            byte[] encrypteData = symetricEncryption(data);
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

        private void ClientForm_Load(object sender, EventArgs e)
        {
            //Exporta e envia a public key para o servidor
            publicKey = rsa.ToXmlString(false);
            byte[] publicKeyPacket = protocolSI.Make(ProtocolSICmdType.PUBLIC_KEY, publicKey);
            networkStream.Write(publicKeyPacket, 0, publicKeyPacket.Length);
            while (protocolSI.GetCmdType() != ProtocolSICmdType.ACK)
            {
                networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
            }

            Thread thread = new Thread(ServerLisneter);
            thread.Start(tcpClient);
        }

        private void ClientForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            //Preparar o envio da mensagem para desligar as ligações
            byte[] eot = protocolSI.Make(ProtocolSICmdType.EOT);
            networkStream.Write(eot, 0, eot.Length);

            networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);


            this.networkStream.Close();
            this.tcpClient.Close();
        }
        private void bt_Autenticar_Click(object sender, EventArgs e)
        {
            byte[] packet;

         
            //Enviamos encriptado os dados do username
            byte[] username = Encoding.UTF8.GetBytes(tb_Jogador.Text);
            byte[] encryptedData = symetricEncryption(username);
            packet = protocolSI.Make(ProtocolSICmdType.USER_OPTION_1, encryptedData);
            networkStream.Write(packet, 0, packet.Length);

            while (protocolSI.GetCmdType() != ProtocolSICmdType.ACK)
            {
                networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
            }

            Console.WriteLine("Username encriptado: " + Convert.ToBase64String(packet));

            //Enviamos encriptado os dados da password
            byte[] password = Encoding.UTF8.GetBytes(tb_Password.Text);
            encryptedData = symetricEncryption(password);
            packet = protocolSI.Make(ProtocolSICmdType.USER_OPTION_2, encryptedData);
            networkStream.Write(packet, 0, packet.Length);
            
            while (protocolSI.GetCmdType() != ProtocolSICmdType.ACK)
            {
                networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
            }

            Console.WriteLine("Password encriptado: " + Convert.ToBase64String(packet));
        }
    }
}
