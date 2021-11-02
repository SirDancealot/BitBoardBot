using System;
using System.Text;
using BitBoardBot.Board;
using static BitBoardBot.Board.BoardUtils;
using static BitBoardBot.Board.AttackSets;
using BitBoardBot.Game;
using System.Collections.Generic;
using static BitBoardBot.Engine.Engine;
using BitBoardBot.Engine;

namespace BitBoardBot
{
    class Program
    {
        static void Main(string[] args)
        {
            BoardUtils.Init();
            AttackSets.Init();

            UIHandler.StartGame(PlayerInput, PlayerInput, 0);
        }
    }
}
