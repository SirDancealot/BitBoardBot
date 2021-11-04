using System;
using System.Collections.Generic;
using BitBoardBot.Board;
using static BitBoardBot.Board.BoardUtils;

namespace BitBoardBot.Engine
{
    public class Move : IComparable, IEquatable<Move>
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
            this.Promoted = promoted;
            this.Source = source;
            this.Target = target;
            value = BB.MoveValue(this);
        }

        public Move(string moveString, BitBoard BB, List<Move> legalMoves)
        {
            Color = ((PieceCode)(BB.MoveCount & 0b1));
            moveString = moveString.ToLower();

            string _source = moveString.Substring(0, 2);
            string _target = moveString.Substring(2, 2);
            char promotedPiece = 'q';
            if (moveString.Length > 4)
            {
                isPromoted = true;
                promotedPiece = moveString.Substring(4, 1)[0];
            }

            Source = (SquareEnum)Enum.Parse(typeof(SquareEnum),
                                            _source);
            if (!((BBPos[(int)Source] & BB.pieceBB[(int)Color]) != 0))
            {
                Illegal = true;
                Console.WriteLine("Source Square is either not a piece, or not a pice owned by the current player");
                return;
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
            if (Piece == PieceCode.White)
            {
                Illegal = true;
                Console.WriteLine("Cannot move an empty square");
                return;
            }

            ulong ownPieces = BB.pieceBB[(int)Color];
            ulong opponents = BB.pieceBB[((int)Color) ^ 1];

            ulong sourceAttackSet = AttackSets.AttackByPieceType((int)Source, Piece, BB);
            if (!((sourceAttackSet & BBPos[(int)Target]) != 0)) {
                Illegal = true;
                Console.WriteLine("Moving the source square to the target square would result in an illegal move");
            }

            bool pieceIsPawn = Piece == PieceCode.wPawn || Piece == PieceCode.bPawn;
            bool moveToRank1or8 = Target > SquareEnum.h7 || Target < SquareEnum.a2;
            if (isPromoted)
            {
                if (!pieceIsPawn || !(moveToRank1or8))
                {
                    Console.WriteLine("You cannot promote a piece with this move");
                    Illegal = true;
                    return;
                }
                switch (promotedPiece)
                {
                    case 'r':
                        Promoted = PieceCode.Rook;
                        break;
                    case 'b':
                        Promoted = PieceCode.Bishop;
                        break;
                    case 'n':
                        Promoted = PieceCode.Knight;
                        break;
                    case 'q':
                        Promoted = PieceCode.Queen;
                        break;
                    default:
                        Promoted = Piece;
                        break;
                }
            } else
            {
                Promoted = Piece;
            }
            if (pieceIsPawn && moveToRank1or8 && !isPromoted)
            {
                Illegal = true;
                Console.WriteLine("You must choose a piece to promote to for this move");
                return;
            }
            if (!legalMoves.Contains(this))
            {
                Illegal = true;
                Console.WriteLine("That is not a legal move");
                return;
            }

            value = BB.MoveValue(this);
        }

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;
            Move otherMove = obj as Move;
            return this.value.CompareTo(otherMove.value);
        }

        public override bool Equals(object obj)
        {
            Move oMove = obj as Move;
            if (oMove == null)
                return false;
            return Color == oMove.Color && Piece == oMove.Piece && Promoted == oMove.Promoted && Source == oMove.Source && Target == oMove.Target;
        }

        public override string ToString()
        {
            return Source.ToString() + Target.ToString() + (isPromoted ? Promoted.ToString() : "") + '\n';
        }

        public bool Equals(Move other)
        {
            return Equals((object)other);
        }

        public override int GetHashCode()
        {
            return (((((int)Color << 5) | (int)Piece) << 5 | (int)Promoted)) ^ ((int)Source ^ (int)Target);
        }
    }
}