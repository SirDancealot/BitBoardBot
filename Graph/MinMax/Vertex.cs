using System;
using BitBoardBot.Board;
using BitBoardBot.Engine;

namespace BitBoardBot.Graph.MinMax
{
    public class Vertex
    {
        public Node Parent;
        public Node Child;
        public Move Move { get; private set; }
        public int Value;

        public Vertex(Node parent, Move move)
        {
            Parent = parent;
            Child = new Node(Parent.MakeMove(move), this);
            Move = move;
            Value = Child.BoardValue;
        }

        public (Move, int) Max(int Depth)
        {
            Value = Child.Max(Depth);
            return (Move, Value);
        }
        public (Move, int) Min(int Depth)
        {
            Value = Child.Min(Depth);
            return (Move, Value);
        }
    }
}