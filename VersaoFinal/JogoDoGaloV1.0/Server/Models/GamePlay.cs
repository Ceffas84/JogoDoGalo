using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models
{
    [Serializable]
    public class GamePlay
    {
        public int Coord_x;
        public int Coord_y;
        public int playerId;
        //public Client Player;
        public GamePlay(int coord_x, int coord_y, int playerId)
        {
            Coord_x = coord_x;
            Coord_y = coord_y;
            this.playerId = playerId;
        }
    }
}
