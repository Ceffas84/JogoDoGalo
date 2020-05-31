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
        public int AddClient(Client client)
        {
            listClients.Add(client);

            int lastClientID = 0;
            foreach(Client cli in listClients)
            {
                if(cli.ClientID > lastClientID)
                {
                    lastClientID = cli.ClientID;
                }
            }
            return lastClientID + 1;
        }
        public void RemoveClient(Client client)
        {
            listClients.Remove(client);
        }
    }
}
