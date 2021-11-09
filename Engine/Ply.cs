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

        private static ConcurrentQueue<(Move, BitBoard, int)> moveQueue = new ConcurrentQueue<(Move, BitBoard, int)>();
        public static ulong PlyCountThreading(BitBoard BB, int Depth, bool first)
        {
            List<Move> moves = BB.GetAllLegalMoves();
            if (Depth == 1)
                return (ulong)LastLayer(moves, moves.Count > 2*Environment.ProcessorCount);

            if (moves.Count < 2*Environment.ProcessorCount)
            {
                Console.WriteLine("Taking smaller perft steps");
                List<BitBoard> boards = new List<BitBoard>();

                foreach (Move move in moves)
                {
                    boards.Add(BB.MakeMove(move));
                }

                foreach (BitBoard board in boards)
                {
                    List<Move> localMoves = localMoves = board.GetAllLegalMoves();
                    foreach (Move move in localMoves)
                    {
                        moveQueue.Enqueue((move, board, Depth - 1));
                    }
                }
            } else
            {
                foreach (Move move in moves)
                {
                    moveQueue.Enqueue((move, BB, Depth));
                }
            }


            ulong moveCountSum = 0;

            Action action = () => {
                ulong localMoves = 0;
                (Move, BitBoard, int) localData;
                while (moveQueue.TryDequeue(out localData))
                {
                    BitBoard localBB = localData.Item2.MakeMove(localData.Item1);
                    ulong localBBMoves = PlyCount(localBB, localData.Item3 - 1, false);
                    if (first)
                        Console.WriteLine(localData.Item1.ToString() + ": " + localBBMoves);
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