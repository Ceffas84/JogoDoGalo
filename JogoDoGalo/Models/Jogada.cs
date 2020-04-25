using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JogoDoGalo_Server.Models
{
    class Jogada
    {
        public int Coord_x;
        public int Coord_y;
        public Jogador Jogador;
        public Jogada(int coord_x, int coord_y, Jogador jogador)
        {
            this.Coord_x = coord_x;
            this.Coord_y = coord_y;
            this.Jogador = jogador;
        }
    }
}
