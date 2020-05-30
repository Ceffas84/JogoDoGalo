using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models
{
    class GameRoom
    {
        public List<Client> listPlayers;
        public GameBoard gameBoard;
        public Client activePlayer;
        private GameState gameState;
        public GameRoom()
        {
            this.listPlayers = new List<Client>();
            //this.gameBoard = new GameBoard(boardDimension);
            this.activePlayer = new Client();
            this.gameState = GameState.Standby;
        }
        public bool isPlayerTurn(int idGamePlayer)
        {
            if (idGamePlayer == this.activePlayer.playerID)
            {
                return true;
            }
            return false;
        }
        public byte[] GetCurrentPlayer()
        {
            byte[] id = new byte[1];
            id[0] = (byte) activePlayer.playerID;
            return id;
        }
        public byte[] GetWinner()
        {
            byte[] winner = new byte[activePlayer.username.Length + 1];
            winner[0] = GetCurrentPlayer()[0];
            byte[] winnerUsername = new byte[activePlayer.username.Length];
            winnerUsername = Encoding.UTF8.GetBytes(activePlayer.username);
            winnerUsername.CopyTo(winner, 1);
            return winner;
        }
        public void SetNextPlayer()
        {
            int playerid = listPlayers.IndexOf(activePlayer) == 0 ? 1 : 0;
            activePlayer = listPlayers[playerid];
        }
        public List<string> GetPlayersList()
        {
            List<string> list = new List<string>();
            foreach(Client client in listPlayers)
            {
                list.Add(client.username);
            }
            return list;
        }
        public void StartGame(int boardDimension)
        {
            this.gameBoard = new GameBoard(boardDimension);
            this.activePlayer = listPlayers[0];
            this.gameState = GameState.OnGoing;
        }
        public GameState GetGameState()
        {
            return this.gameState;
        }
        public void SetGameState(GameState newState)
        {
            this.gameState = newState;
        }
    }

}
