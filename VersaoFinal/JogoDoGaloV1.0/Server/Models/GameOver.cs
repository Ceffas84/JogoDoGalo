using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models
{
    [Serializable]

    /**
     * <summary>    (Serializable) Classe que representa o GameOver do jogo. </summary>
     *
     * <remarks>    Ricardo Lopes, 04/06/2020. </remarks>
     */

    public class GameOver
    {
        public TypeGameOver TypeGameOver { get; set; }
        public int PlayerId { get; set; }
        public string PlayerUsername { get; set; }
        public GameOver(TypeGameOver typeGameOver, int winnerId, string winnerUsername)
        {
            this.TypeGameOver = typeGameOver;
            this.PlayerId = winnerId;
            this.PlayerUsername = winnerUsername;

        }
    }
}
