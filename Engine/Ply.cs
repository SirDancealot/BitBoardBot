using System;
using System.Collections.Generic;
using BitBoardBot.Board;

namespace BitBoardBot.Engine
{
    public class Ply
    {
        public static int PlyCount(BitBoard BB, int Depth, bool first)
        {
            List<Move> moves = BB.GetAllLegalMoves();
            if (Depth == 1)
            {
                if (first)
                {
                    foreach (Move move in moves)
                    {
                        Console.WriteLine(move.ToString() + ": 1");
                    }
                }

                return moves.Count;
            }
            
            int sum = 0;
            foreach (Move move in moves)
            {
                int count = PlyCount(BB.MakeMove(move), Depth - 1, false);
                sum += count;
                if (first)
                    Console.WriteLine(move.ToString() + ": " + count);
            }
            return sum;
        }
    }
}