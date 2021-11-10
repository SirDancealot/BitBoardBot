using System;
using System.Collections.Generic;
using BitBoardBot.Board;
using BitBoardBot.Graph.MinMax;

namespace BitBoardBot.Engine
{
    public static class Engine
    {
        private static Random R = new Random((int)(DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond));
        private static bool init = false;
        public static void Init(BitBoard BB)
        {
            if (init)
                return;
            init = true;
            RootNode = new Node(BB);
        }

        public static Move GreedyAI(BitBoard BB)
        {
            List<Move> moves = BB.GetAllLegalMoves();
            if (moves.Count == 0)
                return null;
            moves.Sort();
            if (moves[0].value == moves[moves.Count - 1].value)
                return RandomAI(BB);
            Move move = moves[(BB.MoveCount & 0b1) == 0 ? moves.Count - 1 : 0];
            RootNode = RootNode.updatePosition(move);
            return move;
        }

        public static Move RandomAI(BitBoard BB)
        {
            List<Move> moves = BB.GetAllLegalMoves();
            if (moves.Count == 0)
                return null;

            int random = R.Next(moves.Count);
            Move move = moves[random];
            RootNode = RootNode.updatePosition(move);
            return move;
        }

        public static Move PlayerInput(BitBoard BB)
        {
            List<Move> moves = BB.GetAllLegalMoves();
            if (moves.Count == 0)
                return null;
            bool whitesTurn = (BB.MoveCount % 2 == 0);
            Console.WriteLine("Player " + (whitesTurn ? "white " : "black ") + "enter your move in the format:\n(Source Square)(Target Square)<Promoted To q|r|b|n>");
            Move move;
            do {
                string moveString = Console.ReadLine();
                move = new Move(moveString, BB, moves);
            } while (move.Illegal);
            RootNode = RootNode.updatePosition(move);
            return move;
        }

        private static Node RootNode = null;
        public static Move MiniMaxAI(BitBoard BB)
        {
            if (RootNode == null)
                RootNode = new Node(BB);
            (Node, Move) minmaxReturn = RootNode.MiniMax(4);
            RootNode = minmaxReturn.Item1;
            return minmaxReturn.Item2;
            
        }
    }
}