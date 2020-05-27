using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models
{
    namespace JogoDoGalo_Server.Models
    {
        class GameRoom
        {
            public List<Client> listUsers;
            public GameBoard gameBoard;
            public GameRoom()
            {
                listUsers = new List<Client>();
                gameBoard = new GameBoard();
            }
        }
    }
}
