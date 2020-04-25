using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JogoDoGalo_Server.Models
{
    class Jogador
    {
        public int IdJogador;
        public string Nome;
        public char Simbolo;
        public int NumJogadas;
        public Jogador()
        {
        }
        public Jogador(string nome, char simbolo)
        {
            this.IdJogador = new int();
            this.Nome = nome;
            this.NumJogadas = 0;
            this.Simbolo = simbolo;
        }
    }
}
