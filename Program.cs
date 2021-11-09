using System;
using BitBoardBot.Board;
using BitBoardBot.Game;
using BitBoardBot.Engine;
using static BitBoardBot.Engine.Engine;
using static BitBoardBot.Board.BoardUtils;
using static BitBoardBot.Board.AttackSets;
using System.Diagnostics;

namespace BitBoardBot
{
    class Program
    {
        static void Main(string[] args)
        {
            string FEN = null;

            Func<BitBoard, Move> moveGen1 = PlayerInput;
            Func<BitBoard, Move> moveGen2 = PlayerInput;

            foreach (string s in args)
            {
                if (s.Split('/').Length == 8)
                    FEN = s;
            }

            BoardUtils.Init();
            AttackSets.Init();
            Hasher.Init();

            // BitBoard BB = new BitBoard("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1");

            // Stopwatch sw = new Stopwatch();

            // sw.Start();
            // Console.WriteLine("Moves: " + Ply.PlyCountThreading(BB, 5, true));
            // sw.Stop();
            // Console.WriteLine("Took " + sw.ElapsedMilliseconds + " ms");

            if (FEN != null)
                UIHandler.StartGame(moveGen1, moveGen2, 0, FEN);
            else
                UIHandler.StartGame(moveGen1, moveGen2, 0);
        }
    }
}
