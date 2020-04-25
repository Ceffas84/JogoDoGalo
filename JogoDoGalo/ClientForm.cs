using JogoDoGalo_Server.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JogoDoGalo
{
    public partial class ClientForm : Form
    {
        private JogoGalo Galo;
        private List<Button> Tabuleiro;
        public ClientForm()
        {
            InitializeComponent();
           
        }
        public void IniciarJogo()
        {
            List<Jogador> listaJogadores = new List<Jogador>();
            listaJogadores.Add(new Jogador("Ricardo", 'X'));
            listaJogadores.Add(new Jogador("Nuno", 'O'));

            int dimensaoTabuleiro = 3;

            Galo = new JogoGalo(dimensaoTabuleiro, listaJogadores);

            DesenharTabuleiro(50, 50, 80, 80);
        }
        private void DesenharTabuleiro(int offset_x, int offset_y, int size_x, int size_y)
        {
            for (int I = 0; I < Galo.DimensaoTabuleiro; I++)
            {
                for (int J = 0; J < Galo.DimensaoTabuleiro; J++)
                {
                    Button newButton = new Button();
                    newButton.Text = "";
                    newButton.Name = I + "_" + J;
                    newButton.Location = new Point(offset_x + I * size_x, offset_y + J * size_y);
                    newButton.Width = size_x;
                    newButton.Height = size_y;
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
    }
}
