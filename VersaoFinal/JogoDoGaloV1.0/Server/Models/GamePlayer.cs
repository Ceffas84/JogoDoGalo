using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models
{
    [Serializable]

    /**
     * <summary>    (Serializable) Classe que representa um jogador do jogo, sendo
     *              que é utilizada para envio de identificação de jogadores do servidor
     *              para o cliente. </summary>
     *
     * <remarks>    Ricardo Lopes, 04/06/2020. </remarks>
     */

    public class GamePlayer
    {
        public int PlayerId { get; }
        public string Username { get; }
        public GamePlayer()
        {
        }

        /**
         * <summary>    Constructor da class game player. </summary>
         *
         * <remarks>    Ricardo Lopes, 04/06/2020. </remarks>
         *
         * <param name="playerId">  Recebe o Id do jogador. </param>
         * <param name="username">  Recebe o username do jogador. </param>
         */

        public GamePlayer(int playerId, string username)
        {
            this.PlayerId = playerId;
            this.Username = username;
        }
    }
}
