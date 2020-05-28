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
        public Client playerTurn;
        public GameRoom()
        {
            listPlayers = new List<Client>();
            gameBoard = new GameBoard();
        }
        public List<GamePlayer> ListPlayers()
        {
            List<GamePlayer> listGamePlayers = new List<GamePlayer>();
            foreach (Client client in listPlayers)
            {
                listGamePlayers.Add(new GamePlayer(listGamePlayers.Count + 1, client.username, GameBoard.Symbol[listGamePlayers.Count + 1]));
            }
            return listGamePlayers;
        }
        public bool isPlayerTurn(int idGamePlayer)
        {
            if (idGamePlayer == this.playerTurn.playerID)
            {
                return true;
            }
            return false;
        }
        public Client GetNextPlayer()
        {
            return playerTurn;
        }
        public void SetNextPlayer()
        {
            playerTurn = listPlayers[playerTurn.playerID];
        }

    }

}
