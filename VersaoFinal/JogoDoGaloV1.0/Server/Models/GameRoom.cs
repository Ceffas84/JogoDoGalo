using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models
{
    /**
     * <summary>    Classe que representa o game room, o qual alberga
     *              a lista de clientes logados na sala, . </summary>
     *
     * <remarks>    Ricardo Lopes, 04/06/2020. </remarks>
     */

    class GameRoom
    {
        public List<Client> listPlayers;
        public GameBoard gameBoard;
        public Client activePlayer;
        private GameState gameState;

        /**
         * <summary>    Construtor da Class Game Room. </summary>
         *
         * <remarks>    Ricardo Lopes, 04/06/2020. </remarks>
         */

        public GameRoom()
        {
            this.listPlayers = new List<Client>();
            this.activePlayer = new Client();
            this.gameState = GameState.Standby;
        }

        /**
         * <summary>    Função que retorna o jogador atual. </summary>
         *
         * <remarks>    Ricardo Lopes, 04/06/2020. </remarks>
         *
         * <returns>    The current player identifier. </returns>
         */

        public int GetCurrentPlayerId()
        {
            return activePlayer.playerID;
        }

        /**
         * <summary>    Função que retorna o jogador atual. </summary>
         *
         * <remarks>    Ricardo Lopes, 04/06/2020. </remarks>
         *
         * <returns>    The current player username. </returns>
         */

        public string GetCurrentPlayerUsername()
        {
            return activePlayer.username;
        }

        /**
         * <summary>    Função que atualiza o jogador ativo para o seguinte jogador. </summary>
         *
         * <remarks>    Ricardo Lopes, 04/06/2020. </remarks>
         */

        public void SetNextPlayer()
        {
            int playerid = listPlayers.IndexOf(activePlayer) == 0 ? 1 : 0;
            activePlayer = listPlayers[playerid];
        }

        /**
         * <summary>    Função que retorna a lista de jogadores na sala de jogo </summary>
         *
         * <remarks>    Ricardo Lopes, 04/06/2020. </remarks>
         *
         * <returns>    The players list. </returns>
         */

        public List<string> GetPlayersList()
        {
            List<string> list = new List<string>();
            foreach(Client client in listPlayers)
            {
                list.Add(client.username);
            }
            return list;
        }

        /**
         * <summary>    Função que inicia o jogo com uma dimensão de tabuleiro
         *              recebida como parametep </summary>
         *
         * <remarks>    Ricardo Lopes, 04/06/2020. </remarks>
         *
         * <param name="boardDimension">   A dimensão do tabuleiro. </param>
         */

        public void StartGame(int boardDimension)
        {
            this.gameBoard = new GameBoard(boardDimension);
            this.activePlayer = listPlayers[0];
            this.gameState = GameState.OnGoing;
        }

        /**
         * <summary>    Função que retorna o estado do jogo. </summary>
         *
         * <remarks>    Ricardo Lopes, 04/06/2020. </remarks>
         *
         * <returns>    The game state. </returns>
         */

        public GameState GetGameState()
        {
            return this.gameState;
        }

        /**
         * <summary>    Função que faz o set do estado do jogo. </summary>
         *
         * <remarks>    Ricardo Lopes, 04/06/2020. </remarks>
         *
         * <param name="newState">  State of the new. </param>
         */

        public void SetGameState(GameState newState)
        {
            this.gameState = newState;
        }
    }
}
