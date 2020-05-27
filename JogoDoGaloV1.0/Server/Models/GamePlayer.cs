using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models
{
    public class GamePlayer
    {
        public int PlayerId { get; }
        public string Name { get; }
        public char GameSymbol { get; }
        private int NumberPlay { get; set; }
        public GamePlayer()
        {
        }
        public GamePlayer(int playerId, string name, char gameSymbol)
        {
            this.PlayerId = playerId;
            this.Name = name;
            this.GameSymbol = gameSymbol;
            this.NumberPlay = 0;
        }
        public void IncNumberGamePLay()
        {
            NumberPlay++;
        }
    }
}
