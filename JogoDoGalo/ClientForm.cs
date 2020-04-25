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
        private string recivedData;
        public ClientForm()
        {
            InitializeComponent();

            ipEndPoint = new IPEndPoint(IPAddress.Loopback, PORT);
            tcpClient = new TcpClient();

            tcpClient.Connect(ipEndPoint);
            networkStream = tcpClient.GetStream();

            //preparação da comunicação utilizando a class ProtocolSI
            protocolSI = new ProtocolSI();

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
            StreamReader reader = new StreamReader(tcpClient.GetStream());

            while (true)
            {
                try
                {
                    string message = reader.ReadLine();
                    //Console.WriteLine(message);

                    //O código abaixo serve para utilizar elementos do ClientForm apartir de outra thread.
                    //Explicação: Uma vez
                    Invoke(new Action(() =>
                    {
                        rtb_Mensagens.AppendText(message + Environment.NewLine);
                    }));
                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    break;
                }
            }
        }

        private void bt_EnviaMensagem_Click(object sender, EventArgs e)
        {
            if (tcpClient.Connected)
            {
                StreamWriter writer = new StreamWriter(tcpClient.GetStream());
                string msg = tb_EscreveMensagem.Text;
                tb_EscreveMensagem.Clear();

                rtb_Mensagens.SelectionAlignment = HorizontalAlignment.Right;
                rtb_Mensagens.AppendText(msg);

                writer.WriteLine(msg);
                writer.Flush();
            }
            //preparar a mensagem para ser enviada
            
            //byte[] packet = protocolSI.Make(ProtocolSICmdType.DATA, msg);

            //enviar a mensagem
        }
    }
}
