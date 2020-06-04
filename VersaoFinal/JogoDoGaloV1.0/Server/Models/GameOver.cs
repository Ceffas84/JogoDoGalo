using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models
{
    [Serializable]
    public class GameOver
    {
        public TypeGameOver TypeGameOver { get; set; }
        public int WinnerId { get; set; }
        public string WinnerUsername { get; set; }
        public GameOver(TypeGameOver typeGameOver, int winnerId, string winnerUsername)
        {
            this.TypeGameOver = typeGameOver;
            this.WinnerId = winnerId;
            this.WinnerUsername = winnerUsername;

        }
    }
}
