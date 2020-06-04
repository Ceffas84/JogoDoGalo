using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models
{
    [Serializable]

    /**
     * <summary>    (Serializable) Class que representa uma jogada no tabuleiro. </summary>
     *
     * <remarks>    Ricardo Lopes, 04/06/2020. </remarks>
     */

    public class GamePlay
    {
        public int Coord_x;
        public int Coord_y;
        public int playerId;

        /**
         * <summary>    Constructor da Class GamePlay que constroi uma jogada
         *              a ser armazenada na list de jogadas. </summary>
         *
         * <remarks>    Ricardo Lopes, 04/06/2020. </remarks>
         *
         * <param name="coord_x">   Recebe a coordenada X. </param>
         * <param name="coord_y">   Recebe a coordenada Y. </param>
         * <param name="playerId">  Recebe o Id do jogador que fez a jogada. </param>
         */

        public GamePlay(int coord_x, int coord_y, int playerId)
        {
            Coord_x = coord_x;
            Coord_y = coord_y;
            this.playerId = playerId;
        }
    }
}
