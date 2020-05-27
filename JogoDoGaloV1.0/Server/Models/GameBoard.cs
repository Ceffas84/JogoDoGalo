using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models
{
    public class GameBoard
    {
        private int BoardDimension;
        private int SequenceSize;
        private int MaxNumberPlay;
        private List<GamePlay> PlayList;
        private int PlayCounter;
        private List<GamePlayer> PlayersList;
        private GamePlayer GameTurn;
        private GameState gameState;
        public GameBoard()
        {

        }
        public GameBoard(int boardDimension, List<GamePlayer> gamePlayersList)
        {
            BoardDimension = boardDimension;
            SequenceSize = SequenceSizeCalc(boardDimension);
            MaxNumberPlay = SequenceSize * SequenceSize;
            PlayList = new List<GamePlay>();
            PlayCounter = 0;
            PlayersList = gamePlayersList;
            GameTurn = PlayersList[0];
        }
        public void AddGamePLay(int coord_x, int coord_y)
        {
            GamePlay gamePlay = new GamePlay(coord_x, coord_y, GameTurn);
            PlayList.Add(gamePlay);
            GameTurn.IncNumberGamePLay();
            PlayCounter++;
        }
        public bool CheckPLayerWins(GamePlayer player)
        {
            for (int offset_row = 0; offset_row <= BoardDimension - SequenceSize; offset_row++)
            {
                for (int offset_col = 0; offset_col <= BoardDimension - SequenceSize; offset_col++)
                {
                    //--------------------------------------------------------------------------------
                    int contarDiagonalDrt = 0;
                    int contarDiagonalEsq = 0;
                    //Verificação de linhas completas e colunas completas
                    for (int I = offset_row; I < SequenceSize + offset_row; I++)
                    {
                        int contarLinha = 0;
                        int contarColuna = 0;
                        for (int J = offset_col; J < SequenceSize + offset_col; J++)
                        {
                            GamePlay newGamePLay = new GamePlay(I, J, player);
                            //conta as marcaçoes das linhas
                            if (GamePlayExist(I, J) && PlayList[PlayId(I, J)].Player.PlayerId == player.PlayerId)
                            {
                                contarLinha++;
                            }
                            //conta as marcaçoes das colunas
                            if (GamePlayExist(J, I) && PlayList[PlayId(J, I)].Player.PlayerId == player.PlayerId)
                            {
                                contarColuna++;

                            }
                            //conta as marcações das diagonal direita
                            if (I - offset_row == J - offset_row)
                            {
                                if (GamePlayExist(I, J) && PlayList[PlayId(I, J)].Player.PlayerId == player.PlayerId)
                                {
                                    contarDiagonalDrt++;
                                }
                            }
                            //conta as marcacoes da diagonal esquerda
                            if (J - offset_col == SequenceSize + offset_row - I - 1)
                            {
                                if (GamePlayExist(I, J) && PlayList[PlayId(I, J)].Player.PlayerId == player.PlayerId)
                                {
                                    contarDiagonalEsq++;
                                }
                            }
                        }
                        //Verifica se foi completada uma linha ou coluna
                        if (contarLinha == SequenceSize || contarColuna == SequenceSize)
                        {
                            return true;
                        }
                    }
                    //Verifica se foi completada uma diagonal à esquerda ou à direita
                    if (contarDiagonalDrt == SequenceSize || contarDiagonalEsq == SequenceSize)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public bool GamePlayExist(int coord_x, int coord_y)
        {
            foreach (GamePlay gamePLay in PlayList)
            {
                if (coord_x == gamePLay.Coord_x && coord_y == gamePLay.Coord_y)
                {
                    return true;
                }
            }
            return false;
        }
        private int PlayId(int coord_x, int coord_y)
        {
            for (int I = 0; I < PlayList.Count(); I++)
            {
                if (coord_x == PlayList[I].Coord_x && coord_y == PlayList[I].Coord_y)
                {
                    return I;
                }
            }
            return -1;
        }
        private int SequenceSizeCalc(int boardSize)
        {
            int sequenceSize = new int();
            switch (boardSize)
            {
                case 3:
                    sequenceSize = 3;
                    break;
                case 4:
                    sequenceSize = 4;
                    break;
                case 5:
                    sequenceSize = 4;
                    break;
                case 6:
                    sequenceSize = 4;
                    break;
                case 7:
                    sequenceSize = 5;
                    break;
                case 8:
                    sequenceSize = 5;
                    break;
                case 9:
                    sequenceSize = 6;
                    break;
                default:
                    sequenceSize = -1;
                    break;
            }
            return sequenceSize;
        }
        public GamePlayer PlayerTurn()
        {
            return GameTurn;
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
        public bool isPlayerTurn(int idGamePlayer)
        {
            if (idGamePlayer == this.GameTurn.PlayerId)
            {
                return true;
            }
            return false;
        }
    }
}
