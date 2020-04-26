using EI.SI;
using JogoDoGalo_Server.Models;
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
        private JogoGalo Galo;

        private const int PORT = 10000;
        TcpClient tcpClient;
        IPEndPoint ipEndPoint;
        NetworkStream networkStream;
        ProtocolSI protocolSI;
        RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
        AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
        private byte[] decryptedSecretKey;
        private byte[] decryptedIV;
        public ClientForm()
        {
            InitializeComponent();

            ipEndPoint = new IPEndPoint(IPAddress.Loopback, PORT);
            tcpClient = new TcpClient();

            tcpClient.Connect(ipEndPoint);
            networkStream = tcpClient.GetStream();

            //preparação da comunicação utilizando a class ProtocolSI
            protocolSI = new ProtocolSI();

            string publicKey = rsa.ToXmlString(false);
            byte[] publicKeyPacket = protocolSI.Make(ProtocolSICmdType.PUBLIC_KEY, publicKey);
            networkStream.Write(publicKeyPacket, 0, publicKeyPacket.Length);
            
            Thread thread = new Thread(Read);
            thread.Start(tcpClient);
        }
        public void IniciarJogo()
        {
            List<Jogador> listaJogadores = new List<Jogador>();
            listaJogadores.Add(new Jogador("Ricardo", 'X'));
            listaJogadores.Add(new Jogador("Nuno", 'O'));

            int dimensaoTabuleiro = 4;

            Galo = new JogoGalo(dimensaoTabuleiro, listaJogadores);

            DesenharTabuleiro(100, 50, 400, Galo.DimensaoTabuleiro);
        }
        private void DesenharTabuleiro(int offset_x, int offset_y, int size, int numButtons)
        {
            int padding = Convert.ToInt32(Math.Round(size * 0.02, MidpointRounding.AwayFromZero));
            int buttonSize = (size - (padding * (numButtons +1))) / numButtons;
            for (int I = 0; I < Galo.DimensaoTabuleiro; I++)
            {
                for (int J = 0; J < Galo.DimensaoTabuleiro; J++)
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
            if (Galo.Estado == Estado.GameOver)
            {
                return;
            }

            Button clickedButton = sender as Button;
            List<int> coord = GetClickedButton(clickedButton);

            if (Galo.JogadaExiste(coord[0], coord[1]))
            {
                System.Media.SystemSounds.Beep.Play();
                return;
            }

            //Cria-se e introduz-se a jogada
            Jogada jogadaIntroduzida = Galo.AdicionarJogada(coord[0], coord[1]);

            //Atualiza-se o botão com o simbolo do jogador que jogou
            clickedButton.Text = jogadaIntroduzida.Jogador.Simbolo.ToString();

            //Verifica-se se o jogador ganhou o jogo
            if (Galo.Ganhou(jogadaIntroduzida.Jogador))
            {
                Galo.Estado = Estado.GameOver;
                MessageBox.Show("Ganhou: " + jogadaIntroduzida.Jogador.Nome + " em " + jogadaIntroduzida.Jogador.NumJogadas + " jogadas!", "GameOver -");
                return;
            }
            //Verifica se foi a ultima jogada
            if (Galo.ListaJogadas.Count() == Galo.MaxJogadas)
            {
                Galo.Estado = Estado.GameOver;
                System.Media.SystemSounds.Beep.Play();
                MessageBox.Show("Jogo terminou empatado", "GameOver");
            }
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
        private void Read(object obj)
        {
            TcpClient tcpClient = (TcpClient)obj;
            networkStream = tcpClient.GetStream();

            while (protocolSI.GetCmdType() != ProtocolSICmdType.EOT)
            {
                try
                {
                    networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
                    byte[] secretKeyEncrypted;
                    byte[] ivEncrypted;
                    byte[] ack;
                    switch (protocolSI.GetCmdType())
                    {
                        case ProtocolSICmdType.SECRET_KEY:
                            secretKeyEncrypted = protocolSI.GetBuffer();
                            ack = protocolSI.Make(ProtocolSICmdType.ACK);
                            networkStream.Write(ack, 0, ack.Length);

                            //atribui a key ao objecto aes
                            aes.Key = rsa.Decrypt(secretKeyEncrypted, true);
                            break;
                        case ProtocolSICmdType.IV:
                            ivEncrypted = protocolSI.GetBuffer();
                            ack = protocolSI.Make(ProtocolSICmdType.ACK);
                            networkStream.Write(ack, 0, ack.Length);

                            //atribui o iv ao objecto aes
                            aes.IV = rsa.Decrypt(ivEncrypted, true);
                            break;
                        case ProtocolSICmdType.DATA:
                            string message = Encoding.UTF8.GetString(symetricDecryption(protocolSI.GetData()));

                            //O código abaixo serve para utilizar elementos do ClientForm apartir de outra thread.
                            //Explicação: Uma vez que os elementos do form só podem ser chamados pela thread
                            //que executa o form.
                            Invoke(new Action(() =>
                            {
                                rtb_Mensagens.AppendText(message);
                                rtb_Mensagens.AppendText(Environment.NewLine);
                            }));

                            ack = protocolSI.Make(ProtocolSICmdType.ACK);
                            networkStream.Write(ack, 0, ack.Length);
                            networkStream.Flush();
                            break;
                        case ProtocolSICmdType.EOT:
                            ack = protocolSI.Make(ProtocolSICmdType.ACK);
                            networkStream.Write(ack, 0, ack.Length);
                            networkStream.Flush();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK);
                    //Console.WriteLine(ex.Message);
                    break;
                }
            }
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
                byte[] encryptedMsg = symetricEncryption(Encoding.UTF8.GetBytes(msg));
                byte[] packet = protocolSI.Make(ProtocolSICmdType.DATA, encryptedMsg);
                networkStream.Write(packet, 0, packet.Length);
                networkStream.Flush();
            }
        }
    }
}
