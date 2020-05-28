using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models
{

    class Lobby
    {
        public List<Client> listClients;
        public GameRoom gameRoom;
        public Lobby()
        {
            listClients = new List<Client>();
            gameRoom = new GameRoom();
        }
    }
}
