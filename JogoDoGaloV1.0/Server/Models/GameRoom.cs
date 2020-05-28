using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models
{
    class GameRoom
    {
        public List<Client> listPlayers;
        public GameBoard gameBoard;
        public GameRoom()
        {
            listPlayers = new List<Client>();
            gameBoard = new GameBoard();
        }
        
    }
}
