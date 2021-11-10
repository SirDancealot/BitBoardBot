using System;
using System.Collections.Generic;
using BitBoardBot.Board;
using BitBoardBot.Engine;

namespace BitBoardBot.Graph.MinMax
{
    public class Node
    {
        private Vertex Parent;
        private List<Vertex> Children;
        private BitBoard State;
        public int BoardValue { get; private set; } = 0;
        private int init = 0;
        private Vertex[] MiniMaxMoves = new Vertex[2];

        public Node(BitBoard state)
        {
            State = state;
            BoardValue = state.GetBoardValue();
            Children = new List<Vertex>();
        }

        public Node(BitBoard state, Vertex pVertex)
        {
            Parent = pVertex;
            State = state;
            BoardValue = state.GetBoardValue();
            Children = new List<Vertex>();
        }

        public int Max(int Depth)
        {
            if (Depth == 0)
                return BoardValue;
            int _value = -100000;
            if (init == 0)
            {
                init ^= 1;
                List<Move> moves = State.GetAllLegalMoves();
                foreach (Move move in moves)
                {
                    Vertex _vertex = new Vertex(this, move);
                    Children.Add(_vertex);
                    (Move, int) vertexData = _vertex.Min(Depth - 1);
                    if (vertexData.Item2 - _value > 0){
                        _value = vertexData.Item2;
                        MiniMaxMoves[0] = _vertex;
                    }
                }
                return _value;
            }
            foreach (Vertex vertex in Children)
            {
                (Move, int) vertexData = vertex.Min(Depth - 1);
                if (vertexData.Item2 - _value > 0)
                {
                    _value = vertexData.Item2;
                    MiniMaxMoves[0] = vertex;
                }
            }
            return _value;
        }
        public int Min(int Depth)
        {
            if (Depth == 0)
                return BoardValue;
            int _value = 100000;
            if (init == 0)
            {
                init ^= 1;
                List<Move> moves = State.GetAllLegalMoves();
                foreach (Move move in moves)
                {
                    Vertex _vertex = new Vertex(this, move);
                    Children.Add(_vertex);
                    (Move, int) vertexData = _vertex.Max(Depth - 1);
                    if (_value - vertexData.Item2 > 0){
                        _value = vertexData.Item2;
                        MiniMaxMoves[1] = _vertex;
                    }
                }
                return _value;
            }
            foreach (Vertex vertex in Children)
            {
                (Move, int) vertexData = vertex.Max(Depth - 1);
                if (_value - vertexData.Item2 > 0){
                    _value = vertexData.Item2;
                    MiniMaxMoves[1] = vertex;
                }
            }
            return _value;
        }

        public (Node, Move) MiniMax(int Depth)
        {
            int colorOffset = State.MoveCount & 0b1;
            Func<int, int>[] methods = new Func<int, int>[] { Max, Min };
            methods[colorOffset].Invoke(Depth);

            Node retNode = MiniMaxMoves[colorOffset].Child;
            retNode.Parent = null;

            return (retNode, MiniMaxMoves[colorOffset].Move);
        }

        public BitBoard MakeMove(Move move)
        {
            return State.MakeMove(move);
        }

        public Node updatePosition(Move move)
        {
            foreach (Vertex vertex in Children)
            {
                if (vertex.Move.Equals(move))
                {
                    Node returnNode = vertex.Child;
                    returnNode.Parent = null;
                    return returnNode;
                }
            }
            return null;
        }
    }
}