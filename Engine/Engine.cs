using System;
using System.Collections.Generic;
using BitBoardBot.Board;

namespace BitBoardBot.Engine
{
    public static class Engine
    {
        private static Random R = new Random((int)(DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond));

        public static Move GreedyAI(BitBoard BB)
        {
            List<Move> moves = BB.GetAllLegalMoves();
            moves.Sort();
            if (moves[0].value == moves[moves.Count - 1].value)
                return RandomAI(BB);
            return moves[(BB.MoveCount & 0b1) == 0 ? moves.Count - 1 : 0];
        }

        public static Move RandomAI(BitBoard BB)
        {
            List<Move> moves = BB.GetAllLegalMoves();
            int random = R.Next(moves.Count);
            return moves[random];
        }

        public static Move PlayerInput(BitBoard BB)
        {
            bool whitesTurn = (BB.MoveCount % 2 == 0);
            Console.WriteLine("Player " + (whitesTurn ? "white " : "black ") + "enter your move in the format:\n(Source Square)(Target Square)<Promoted To q|r|b|n>");
            Move move;
            do {
                string moveString = Console.ReadLine();
                move = new Move(moveString, BB);
            } while (move.Illegal);
            return move;
        }
    }
}