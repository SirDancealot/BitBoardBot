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
        private static BitBoard BB = new BitBoard();
        private static bool GameRunning = true;

        public static void StartGame(Func<BitBoard, Move> MoveGen1, Func<BitBoard, Move> MoveGen2, int roundDelay) 
        {
            Func<BitBoard, Move>[] MoveGens = new Func<BitBoard, Move>[] {MoveGen1, MoveGen2};
            Console.WriteLine(FormatBB());
            while (GameRunning)
            {
                 BB.MakeMove(MoveGens[BB.MoveCount & 0b1].Invoke(BB));
                 Thread.Sleep(roundDelay);
                 Console.WriteLine(FormatBB());
                 if (Math.Abs(BB.GetBoardValue()) > 200)
                    GameRunning = false;
            }
            Console.WriteLine("Game over\n" + ((BB.MoveCount & 0b1) != 0 ? "White" : "Black") + " Won");
        }

        private static void twoPlayers()
        {
            while (GameRunning)
            {
                Console.WriteLine(FormatBB());
                Move move = Engine.Engine.PlayerInput(BB);
                BB.MakeMove(move);
            }
            Console.WriteLine(FormatBB());
        }

        private static void twoAIs()
        {
            Console.WriteLine("using two random AIs");
            Thread.Sleep(5000);
            Console.WriteLine(FormatBB());
            try
            {
                while (true)
                {
                    Thread.Sleep(5);
                    Move move = Engine.Engine.RandomAI(BB);
                    Console.WriteLine("Making move: " + move.ToString());
                    BB.MakeMove(move);
                    Console.WriteLine(FormatBB());
                }
            }
            catch (System.Exception)
            {
                Console.WriteLine("Game over\n" + ((BB.MoveCount & 0b1) != 0 ? "White" : "Black") + " won in " + BB.MoveCount + " moves");
            }
        }

        private static void playerWhite()
        {
            throw new NotImplementedException("AIs not implemented yet");
        }

        private static void playerBlack()
        {
            throw new NotImplementedException("AIs not implemented yet");
        }

        private static string FormatBB() 
        {
            StringBuilder sb = new StringBuilder();
            ulong pieces = BB.pieceBB[(int)PieceCode.White] | BB.pieceBB[(int)PieceCode.Black];

            sb.Append("  +-+-+-+-+-+-+-+-+\n");
            for (int y = 7; y >= 0; y--)
            {
                sb.Append((1+y) + " |");
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
                        sb.Append("|");
                    // } else 
                    // {
                    //     sb.Append(" |");
                    // }
                }
                sb.Append("\n  +-+-+-+-+-+-+-+-+\n");
            }
            sb.Append("   A B C D E F G H \n");
            return sb.ToString();
        }
    }
}