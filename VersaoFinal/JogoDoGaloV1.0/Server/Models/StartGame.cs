using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models
{
    /**
     * <summary>    (Serializable) a start game. 
     *              Class que representa o pedido de início do jogo por um cliente. 
     *              Recebe o tamanho do tabuleiro escolhido pelo cliente e
     *              a identificação do jogador que fez o pedido. </summary>
     *
     * <remarks>    Simão Pedro, 04/06/2020. </remarks>
     */

    [Serializable]
    public class StartGame
    {
        public int BoardDimension { get; }
        public int PlayerId { get; }
        public StartGame(int boardDimension, int playerId)
        {
            this.BoardDimension = boardDimension;
            this.PlayerId = playerId;
        }
    }
}
