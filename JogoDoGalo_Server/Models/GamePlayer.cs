using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JogoDoGalo_Server.Models
{
    [Serializable]
    public class GamePlayer
    {
        private int PlayerId;
        private string Name;
        private char GameSymbol;
        private int NumberGamePlay;
        public GamePlayer()
        {
        }
        public GamePlayer(int playerId, string name, char gameSymbol)
        {
            this.PlayerId = playerId;
            this.Name = name;
            this.GameSymbol = gameSymbol;
            this.NumberGamePlay = 0;
        }
        public int GetPlayerId()
        {
            return PlayerId;
        }
        public string GetPlayerUsername()
        {
            return Name;
        }
        public void IncNumberGamePLay()
        {
            NumberGamePlay++;
        }
    }
}
