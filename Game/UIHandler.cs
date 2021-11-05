using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using BitBoardBot.Board;
using BitBoardBot.Engine;
using static BitBoardBot.Board.BoardUtils;

namespace BitBoardBot.Game
{
    public static class UIHandler
    {
        private static BitBoard BB;
        private static bool GameRunning = true;

        public static void StartGame(Func<BitBoard, Move> MoveGen1, Func<BitBoard, Move> MoveGen2, int roundDelay, string FEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1") 
        {
            BB = new BitBoard(FEN);
            Func<BitBoard, Move>[] MoveGens = new Func<BitBoard, Move>[] {MoveGen1, MoveGen2};
            Console.WriteLine(FormatBB());
            bool whiteInCheck = false, blackInCheck = false, staleMate = false;
            while (GameRunning)
            {
                Move moveToMake = MoveGens[BB.MoveCount & 0b1].Invoke(BB);
                if (moveToMake == null)
                {
                    whiteInCheck = AttackSets.CheckedByBitmask(BB, PieceCode.White) != 0;
                    blackInCheck = AttackSets.CheckedByBitmask(BB, PieceCode.Black) != 0;
                    staleMate = !(whiteInCheck || blackInCheck);
                    break;
                }
                if (BB.FiftyMoveRule >= 100)
                {
                    staleMate = !(whiteInCheck || blackInCheck);
                    break;
                }
                BB = BB.MakeMove(moveToMake);
                Thread.Sleep(roundDelay);
                Console.WriteLine(FormatBB());
                if (Math.Abs(BB.GetBoardValue()) > 1000)
                    GameRunning = false;
            }
            if (staleMate)
            {
                Console.WriteLine("Stalemate at move " + BB.MoveCount);
            } else
            {
                Console.WriteLine("Game over\n" + (blackInCheck ? "White" : "Black") + " Won in " + BB.MoveCount + " moves");
            }

        }

        private static string FormatBB() 
        {
            StringBuilder sb = new StringBuilder();
            ulong pieces = BB.pieceBB[(int)PieceCode.White] | BB.pieceBB[(int)PieceCode.Black];

            sb.Append("  +---+---+---+---+---+---+---+---+\n");
            for (int y = 7; y >= 0; y--)
            {
                sb.Append((1+y) + " | ");
                for(int x = 0; x < 8; x++) {
                    int bitPos = y * 8 + x;
                    ulong _BBpos = BBPos[bitPos];
                    // if ((pieces & _BBpos) != 0)
                    // {
                        char pieceChar = ' ';
                        if ((_BBpos & BB.pieceBB[(int)PieceCode.King]) != 0)
                            pieceChar = 'k';
                        if (((_BBpos & BB.pieceBB[(int)PieceCode.wPawn]) | (_BBpos & BB.pieceBB[(int)PieceCode.bPawn])) != 0)
                            pieceChar = 'p';
                        if ((_BBpos & BB.pieceBB[(int)PieceCode.Queen]) != 0)
                            pieceChar = 'q';
                        if ((_BBpos & BB.pieceBB[(int)PieceCode.Rook]) != 0)
                            pieceChar = 'r';
                        if ((_BBpos & BB.pieceBB[(int)PieceCode.Bishop]) != 0)
                            pieceChar = 'b';
                        if ((_BBpos & BB.pieceBB[(int)PieceCode.Knight]) != 0)
                            pieceChar = 'n';

                        sb.Append((_BBpos & BB.pieceBB[(int)PieceCode.White]) != 0 ? (char)(((byte)pieceChar) - 32)  : pieceChar);
                        sb.Append(" | ");
                    // } else 
                    // {
                    //     sb.Append(" |");
                    // }
                }
                sb.Append("\n  +---+---+---+---+---+---+---+---+\n");
            }
            sb.Append("    A   B   C   D   E   F   G   H  \n");
            return sb.ToString();
        }
    }
}