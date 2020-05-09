using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JogoDoGalo_Server.Models
{
    class GamePlayer
    {
        private int PlayerId;
        private string Name;
        private char GameSymbol;
        private int NumberGamePlay;
        public GamePlayer()
        {
        }
        public GamePlayer(string name, char gameSymbol)
        {
            this.PlayerId = new int();
            this.Name = name;
            this.NumberGamePlay = 0;
            this.GameSymbol = gameSymbol;
        }
        public int GetPlayerId()
        {
            return PlayerId;
        }
    }
}
