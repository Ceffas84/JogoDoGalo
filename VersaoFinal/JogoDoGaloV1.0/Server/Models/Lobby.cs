using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models
{
    /**
     * <summary>    Class que representa a sala de espera onde os
     *              jogadores estão até fazerem login. </summary>
     *
     * <remarks>    Ricardo Lopes, 04/06/2020. </remarks>
     */

    class Lobby
    {
        public List<Client> listClients;
        public GameRoom gameRoom;

        /**
         * <summary>    Construtor da Class Lobby que cria uma nova
         *              sala de espera com uma lista de clients e uma sala de jogo. </summary>
         *
         * <remarks>    Ricardo Lopes, 04/06/2020. </remarks>
         */

        public Lobby()
        {
            listClients = new List<Client>();
            gameRoom = new GameRoom();
        }

        /**
         * <summary>    Função que adiciona um client à lista de clients
         *              na sala de espera, e atribui o seu id de cliente. </summary>
         *
         * <remarks>    Ricardo Lopes, 04/06/2020. </remarks>
         *
         * <param name="client">    Recebe o client a adicionar à lista. </param>
         */

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
