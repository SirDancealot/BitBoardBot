using System;
using BitBoardBot.Board;
using static BitBoardBot.Board.BoardUtils;

namespace BitBoardBot.Engine
{
    public class Move : IComparable
    {
        public int value { get; private set; }
        public PieceCode Color { get; private set; }
        public PieceCode Piece { get; private set; }
        public PieceCode Promoted { get; private set; }
        private bool isPromoted = false;
        public SquareEnum Source { get; private set; }
        public SquareEnum Target { get; private set; }
        public bool Illegal { get; private set; }

        public Move(PieceCode Color, PieceCode piece, SquareEnum source, SquareEnum target, BitBoard BB, PieceCode promoted = 0)
        {
            this.Color = Color;
            this.Piece = piece;
            isPromoted = promoted == PieceCode.Knight || promoted == PieceCode.Bishop || promoted == PieceCode.Rook || promoted == PieceCode.Queen;
            this.Promoted = isPromoted ? promoted : piece;
            this.Source = source;
            this.Target = target;
            value = BB.MoveValue(this);
        }

        public Move(string moveString, BitBoard BB)
        {
            Color = ((PieceCode)(BB.MoveCount & 0b1));
            moveString = moveString.ToLower();
            try
            {
                string _source = moveString.Substring(0, 2);
                string _target = moveString.Substring(2, 2);
                string promotedPiece = "q";
                if (moveString.Length > 4)
                {
                    promotedPiece = moveString.Substring(4, 1);
                }

                Source = (SquareEnum)Enum.Parse(typeof(SquareEnum),
                                                _source);
                if (!((BBPos[(int)Source] & BB.pieceBB[(int)Color]) != 0))
                {
                    Illegal = true;
                    throw new Exception("Source Square is either not a piece, or not a pice owned by the current player");
                }
                Target = (SquareEnum)Enum.Parse(typeof(SquareEnum), _target);

                foreach (PieceCode code in Enum.GetValues(typeof(PieceCode)))
                {
                    if (code == PieceCode.White || code == PieceCode.Black)
                        continue;
                    if ((BBPos[(int)Source] & BB.pieceBB[(int)code]) != 0)
                    {
                        Piece = code;
                        break;
                    }    
                }

                ulong ownPieces = BB.pieceBB[(int)Color];
                ulong opponents = BB.pieceBB[((int)Color) ^ 1];

                ulong sourceAttackSet = AttackSets.AttackByPieceType((int)Source, Piece, BB);
                if (!((sourceAttackSet & BBPos[(int)Target]) != 0)) {
                    Illegal = true;
                    throw new Exception("Moving the source square to the target square would result in an illegal move");
                }

                if (isPromoted)
                {
                    Promoted = (PieceCode)Enum.Parse(typeof(PieceCode), promotedPiece);
                }

                value = BB.MoveValue(this);
            }
            catch (System.Exception)
            {
                Illegal = true;
                Console.WriteLine("The moved you entered is not in the correct format");
                return;
            }
        }

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;
            Move otherMove = obj as Move;
            return this.value.CompareTo(otherMove.value);
        }

        public override string ToString()
        {
            return Source.ToString() + Target.ToString() + (isPromoted ? Promoted.ToString() : "") + '\n';
        }
    }
}