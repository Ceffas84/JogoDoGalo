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
            public List<User> listUsers;
            public GameBoard gameBoard;
            public GameRoom()
            {
                listUsers = new List<User>();
                gameBoard = new GameBoard();
            }
        }
    }
}
