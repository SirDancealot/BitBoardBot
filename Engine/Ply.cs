using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using BitBoardBot.Board;

namespace BitBoardBot.Engine
{
    public class Ply
    {
        public static ulong PlyCount(BitBoard BB, int Depth, bool first)
        {
            List<Move> moves = BB.GetAllLegalMoves();
            if (Depth == 1)
                return (ulong)LastLayer(moves, first);
            
            ulong sum = 0;
            foreach (Move move in moves)
            {
                ulong count = PlyCount(BB.MakeMove(move), Depth - 1, false);
                sum += count;
                if (first)
                    Console.WriteLine(move.ToString() + ": " + count);
            }
            return sum;
        }

        private static ConcurrentQueue<Move> moveQueue = new ConcurrentQueue<Move>();
        public static ulong PlyCountThreading(BitBoard BB, int Depth, bool first)
        {
            List<Move> moves = BB.GetAllLegalMoves();
            if (Depth == 1)
                return (ulong)LastLayer(moves, moves.Count > 2*Environment.ProcessorCount);

            if (moves.Count < 2*Environment.ProcessorCount)
            {
                Console.WriteLine("Taking smaller perft steps");
                List<BitBoard> boards = new List<BitBoard>();
                ulong sum = 0;

                foreach (Move move in moves)
                {
                    boards.Add(BB.MakeMove(move));
                    ulong tmpSum = PlyCountThreading(BB.MakeMove(move), Depth - 1, false);
                    if (first)
                        Console.WriteLine(move + ": " + tmpSum);
                    sum += tmpSum;
                }
                return sum;
            }

            foreach (Move move in moves)
            {
                moveQueue.Enqueue(move);
            }

            ulong moveCountSum = 0;

            Action action = () => {
                ulong localMoves = 0;
                Move localMove;
                while (moveQueue.TryDequeue(out localMove))
                {
                    BitBoard localBB = BB.MakeMove(localMove);
                    ulong localBBMoves = PlyCount(localBB, Depth - 1, false);
                    if (first)
                        Console.WriteLine(localMove.ToString() + ": " + localBBMoves);
                    localMoves += localBBMoves;
                }
                Interlocked.Add(ref moveCountSum, localMoves);
            };

            Action[] actions = new Action[Environment.ProcessorCount];
            for (int i = 0; i < Environment.ProcessorCount; i++)
            {
                actions[i] = action;
            }

            Parallel.Invoke(actions);

            return moveCountSum;
        }

        private static int LastLayer(List<Move> moves, bool first)
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
    }
}