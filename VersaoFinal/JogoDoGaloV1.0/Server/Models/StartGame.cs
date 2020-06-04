using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models
{
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
