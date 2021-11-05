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

            Func<BitBoard, Move> moveGen1 = RandomAI;
            Func<BitBoard, Move> moveGen2 = RandomAI;

            foreach (string s in args)
            {
                if (s.Split('/').Length == 8)
                    FEN = s;
            }

            BoardUtils.Init();
            AttackSets.Init();
            Hasher.Init();

            if (FEN != null)
                UIHandler.StartGame(moveGen1, moveGen2, 0, FEN);
            else
                UIHandler.StartGame(moveGen1, moveGen2, 0);
        }
    }
}
