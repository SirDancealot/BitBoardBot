using System;
using BitBoardBot.Board;
using BitBoardBot.Game;
using BitBoardBot.Engine;
using static BitBoardBot.Engine.Engine;
using static BitBoardBot.Board.BoardUtils;
using static BitBoardBot.Board.AttackSets;

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


            if (FEN != null)
                UIHandler.StartGame(moveGen1, moveGen2, 0, FEN);
            else
                UIHandler.StartGame(moveGen1, moveGen2, 0);
        }
    }
}
