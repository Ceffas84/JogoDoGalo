using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JogoDoGalo_Server.Models
{
    class GamePlay
    {
        public int Coord_x;
        public int Coord_y;
        public GamePlayer Player;
        public GamePlay(int coord_x, int coord_y, GamePlayer player)
        {
            Coord_x = coord_x;
            Coord_y = coord_y;
            Player = player;
        }
    }
}
