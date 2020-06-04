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
        public void AddClient(Client client)
        {
            int newId;
            if (listClients.Count > 0)
            {
                newId = listClients[listClients.Count - 1].ClientID + 1;
            }
            else
            {
                newId = 1;
            }
            client.ClientID = newId;

            listClients.Add(client);
        }
    }
}
