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
            this.Promoted = promoted;
            this.Source = source;
            this.Target = target;
            value = BB.MoveValue(this);
        }

        public Move(string moveString, BitBoard BB)
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

            value = BB.MoveValue(this);
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