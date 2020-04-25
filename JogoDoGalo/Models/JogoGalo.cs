using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JogoDoGalo_Server.Models
{
    class JogoGalo
    {
        public int DimensaoTabuleiro;
        private int Sequencia;
        public int MaxJogadas;
        public List<Jogada> ListaJogadas;
        private int ContadorJogadas;
        private List<Jogador> ListaJogadores;
        private Jogador VezJogador;
        public Estado Estado;
        public JogoGalo(int dimensaoTabuleiro, List<Jogador> listaJogadores)
        {
            this.DimensaoTabuleiro = dimensaoTabuleiro;

            switch (dimensaoTabuleiro)
            {
                case 3:
                    this.Sequencia = 3;
                    break;
                case 4:
                    this.Sequencia = 4;
                    break;
                case 5:
                    this.Sequencia = 4;
                    break;
                case 6:
                    this.Sequencia = 4;
                    break;
                case 7:
                    this.Sequencia = 5;
                    break;
                case 8:
                    this.Sequencia = 5;
                    break;
                case 9:
                    this.Sequencia = 6;
                    break;
            }

            this.MaxJogadas = dimensaoTabuleiro * dimensaoTabuleiro;
            this.ContadorJogadas = 0;

            this.ListaJogadores = new List<Jogador>(listaJogadores);
            for (int I = 0; I < listaJogadores.Count(); I++)
            {
                listaJogadores[I].IdJogador = I;
            }
            this.VezJogador = ListaJogadores[0];

            this.ListaJogadas = new List<Jogada>();

            this.Estado = Estado.EmCurso;
        }
        public void AdicionarJogador(Jogador jogador)
        {
            jogador.IdJogador = ListaJogadores.Count();
            this.ListaJogadores.Add(jogador);
        }
        public Jogada AdicionarJogada(int coord_x, int coord_y)
        {
            //Criar e adiciona a jogada
            Jogada jogada = new Jogada(coord_x, coord_y, this.VezJogador);
            ListaJogadas.Add(jogada);

            //Atualiza-se o contador de jogadas do jogador e do jogo
            this.ListaJogadores[VezJogador.IdJogador].NumJogadas++;
            this.ContadorJogadas++;

            //Coloca na VezJogador o próximo jogador
            this.AlternarVez();

            return jogada;
        }
        public bool Ganhou(Jogador jogador)
        {
            for (int OFFSET_LN = 0; OFFSET_LN <= this.DimensaoTabuleiro - this.Sequencia; OFFSET_LN++)
            {
                for (int OFFSET_COL = 0; OFFSET_COL <= this.DimensaoTabuleiro - this.Sequencia; OFFSET_COL++)
                {
                    int contarDiagonalDrt = 0;
                    int contarDiagonalEsq = 0;
                    //Verificação de linhas completas e colunas completas
                    for (int I = OFFSET_LN; I < this.Sequencia + OFFSET_LN; I++)
                    {
                        int contarLinha = 0;
                        int contarColuna = 0;
                        for (int J = OFFSET_COL; J < this.Sequencia + OFFSET_COL; J++)
                        {
                            Jogada novaJogada = new Jogada(I, J, jogador);
                            //conta as marcaçoes das linhas
                            if (JogadaExiste(I, J) && this.ListaJogadas[IdJogada(I, J)].Jogador.IdJogador == jogador.IdJogador)
                            {
                                contarLinha++;
                            }
                            //conta as marcaçoes das colunas
                            if (JogadaExiste(J, I) && this.ListaJogadas[IdJogada(J, I)].Jogador.IdJogador == jogador.IdJogador)
                            {
                                contarColuna++;

                            }
                            //conta as marcações das diagonal direita
                            if (I - OFFSET_LN == J - OFFSET_LN)
                            {
                                if (JogadaExiste(I, J) && this.ListaJogadas[IdJogada(I, J)].Jogador.IdJogador == jogador.IdJogador)
                                {
                                    contarDiagonalDrt++;
                                }
                            }
                            //conta as marcacoes da diagonal esquerda
                            if (J - OFFSET_COL == this.Sequencia + OFFSET_LN - I - 1)
                            {
                                if (JogadaExiste(I, J) && this.ListaJogadas[IdJogada(I, J)].Jogador.IdJogador == jogador.IdJogador)
                                {
                                    contarDiagonalEsq++;
                                }
                            }
                        }
                        //Verifica se foi completada uma linha ou coluna
                        if (contarLinha == this.Sequencia || contarColuna == this.Sequencia)
                        {
                            return true;
                        }
                    }
                    //Verifica se foi completada uma diagonal à esquerda ou à direita
                    if (contarDiagonalDrt == this.Sequencia || contarDiagonalEsq == this.Sequencia)
                    {
                        Console.ReadLine();
                        return true;
                    }
                }
            }
            return false;
        }
        public bool JogadaExiste(int coord_x, int coord_y)
        {
            foreach (Jogada jogada in this.ListaJogadas)
            {
                if (coord_x == jogada.Coord_x && coord_y == jogada.Coord_y)
                {
                    return true;
                }
            }
            return false;
        }
        public Jogador JogadorJogar ()
        {
            return this.VezJogador;
        }
        public void AlternarVez()
        {
            int proximoJogador = (this.VezJogador.IdJogador < this.ListaJogadores.Count() - 1) ? this.VezJogador.IdJogador + 1 : 0;
            this.VezJogador = this.ListaJogadores[proximoJogador];
        }
        public int IdJogada(int coord_x, int coord_y)
        {
            for (int I = 0; I < this.ListaJogadas.Count(); I++)
            {
                if (coord_x == ListaJogadas[I].Coord_x && coord_y == ListaJogadas[I].Coord_y)
                {
                    return I;
                }
            }
            return -1;
        }
        public int IdJogador(int coord_x, int coord_y)
        {
            for (int I = 0; I < this.ListaJogadas.Count(); I++)
            {
                if (coord_x == ListaJogadas[I].Coord_x && coord_y == ListaJogadas[I].Coord_y)
                {
                    return this.ListaJogadas[IdJogada(coord_x, coord_y)].Jogador.IdJogador;
                }
            }
            return -1;
        }
    }
}
