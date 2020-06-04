using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
        public GameBoard(int boardDimension)
        {
            BoardDimension = boardDimension;
            SequenceSize = SequenceSizeCalc(boardDimension);
            MaxNumberPlay = boardDimension * boardDimension;
            PlayList = new List<GamePlay>();
            PlayCounter = 0;
        }
        public void AddGamePlay(int coord_x, int coord_y, int playerId)
        {
            GamePlay gamePlay = new GamePlay(coord_x, coord_y, playerId);
            PlayList.Add(gamePlay);
            PlayCounter++;
        }
        public bool CheckPLayerWins(int playerId)
        {
            for (int offset_row = 0; offset_row <= BoardDimension - SequenceSize; offset_row++)
            {
                for (int offset_col = 0; offset_col <= BoardDimension - SequenceSize; offset_col++)
                {
                    //--------------------------------------------------------------------------------
                    int contarDiagonalDrt = 0;
                    int contarDiagonalEsq = 0;
                    //Verificação de linhas completas e colunas completas
                    for (int Y = offset_row; Y < SequenceSize + offset_row; Y++)
                    {
                        int contarLinha = 0;
                        int contarColuna = 0;
                        for (int X = offset_col; X < SequenceSize + offset_col; X++)
                        {
                            GamePlay newGamePLay = new GamePlay(Y, X, playerId);
                            //conta as marcaçoes das colunas
                            if (GamePlayExist(Y, X) && PlayList[PlayId(Y, X)].playerId == playerId)
                            {
                                contarLinha++;
                            }
                            //conta as marcaçoes das linhas
                            if (GamePlayExist(X, Y) && PlayList[PlayId(X, Y)].playerId == playerId)
                            {
                                contarColuna++;

                            }
                            //conta as marcações das diagonal direita
                            //if (Y - offset_row == X - offset_col)
                            //if (Y - offset_row == SequenceSize - offset_col - X - 1)
                            if (Y - offset_row == X - offset_col)
                            {
                                if (GamePlayExist(X, Y) && PlayList[PlayId(X, Y)].playerId == playerId)
                                {
                                    contarDiagonalDrt++;
                                }
                            }
                            //conta as marcacoes da diagonal esquerda
                            //if (Y - offset_row == X - offset_col)
                            //if (Y - offset_row == SequenceSize - offset_col - X - 1)
                            if (Y - offset_row == (X - offset_col) + (1 - SequenceSize) + ((Y - offset_row)* 2))
                            {
                                if (GamePlayExist(X, Y) && PlayList[PlayId(X, Y)].playerId == playerId)
                                {
                                    contarDiagonalEsq++;
                                }
                            }
                            Console.WriteLine("OffSetRow: {0}, OffSet Col: {1}, Y: {2}, X: {3}  => Col: {4} Ln: {5} DD: {6} DE: {7}", offset_row, offset_col, Y, X, contarColuna, contarLinha, contarDiagonalDrt, contarDiagonalEsq);
                        }
                        //Verifica se foi completada uma linha ou coluna
                        if (contarLinha == SequenceSize || contarColuna == SequenceSize)
                        {
                            return true;
                        }
                        Console.WriteLine();
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
                case 5:
                case 6:
                    sequenceSize = 4;
                    break;
                case 7:
                case 8:
                    sequenceSize = 5;
                    break;
                case 9:
                    sequenceSize = 6;
                    break;
            }
            return sequenceSize;
        }
        public int GetBoardDimension()
        {
            return this.BoardDimension;
        }
        public List<GamePlay> GetListOfPlays()
        {
            return PlayList;
        }

        public void RestartBoard()
        {
            this.PlayList.Clear();
            this.PlayCounter = 0;
        }
        public bool IsNumberPlaysOver()
        {
            if(!(PlayCounter < MaxNumberPlay))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
