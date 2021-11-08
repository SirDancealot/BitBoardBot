using System;
using System.Text;
using BitBoardBot.Board;
using static BitBoardBot.Board.BoardUtils;
using static BitBoardBot.Board.AttackSets;
using BitBoardBot.Game;
using System.Numerics;
using System.Collections.Generic;
using static BitBoardBot.Engine.Engine;
using BitBoardBot.Engine;

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

            BitBoard BB = new BitBoard();
            BB = BB.MakeMove(new Move("a2a4", BB, BB.GetAllLegalMoves()));
            BB = BB.MakeMove(new Move("b7b5", BB, BB.GetAllLegalMoves()));
            BB = BB.MakeMove(new Move("a4b5", BB, BB.GetAllLegalMoves()));
            BB = BB.MakeMove(new Move("c7c5", BB, BB.GetAllLegalMoves()));

            Console.WriteLine("\nNodes searched: " + Ply.PlyCount(BB, 1, true));

            if (FEN != null)
                UIHandler.StartGame(moveGen1, moveGen2, 0, FEN);
            else
                UIHandler.StartGame(moveGen1, moveGen2, 0);
        }
    }
}
