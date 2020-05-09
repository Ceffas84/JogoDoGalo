using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JogoDoGalo_Server.Models
{
    class GameEngine
    {
        private GameBoard gameBoard;
        private GameState gameState;
        private List<GamePlayer> GamePlayersList;
        public GameEngine(int boardDimension, List<GamePlayer> gamePLayersList)
        {
            gameBoard = new GameBoard(boardDimension, gamePLayersList);
            gameState = GameState.Standby;
        }
        public void GameStart()
        {
            this.gameState = GameState.OnGoing;
        }
        public GameState GetGameState()
        {
            return this.gameState;
        }
        public void UpdateGameState(GameState newState)
        {
            this.gameState = newState;
        }
        
    }
}
