
using System;
using System.Numerics;
using System.Text;
using BitBoardBot.Engine;
using PieceCode = BitBoardBot.Board.BoardUtils.PieceCode;
using static BitBoardBot.Board.BoardUtils;
using System.Collections.Generic;

namespace BitBoardBot.Board
{


    public class BitBoard {
        public SquareEnum LastSource { get; private set; } = SquareEnum.a1;
        public SquareEnum LastTarget { get; private set; } = SquareEnum.a1;
        public ulong HashValue = 0;
        public ulong CastleMask { get; private set; } = 0x4400_0000_0000_0044ul;
        public int MoveCount { get; private set; } = 0;
        public ulong[] pieceBB { get; private set; }
        public BitBoard() {
            pieceBB = (ulong[])BoardUtils.BBStartPos.Clone();
        }

        public BitBoard(string FEN)
        {
            pieceBB = new ulong[9];

            CastleMask = 0ul;

            string[] FENParts = FEN.Split(' ');

            string[] rows = FENParts[0].Split("/");
            for (int i = 0; i < rows.Length; i++)
            {
                string row = rows[i];
                ulong head = 0x1ul << (8 * (7-i));
                foreach (char c in row)
                {
                    char lowerCase = c;
                    bool black = (int)c >= 97;
                    if (black)
                        lowerCase = (char)((int)c - 32);

                    switch (lowerCase)
                    {
                        case 'R':
                            pieceBB[(int)PieceCode.Rook] ^= head;
                            pieceBB[black ? 1 : 0] ^= head;
                            head <<= 1;
                            break;
                        case 'N':
                            pieceBB[(int)PieceCode.Knight] ^= head;
                            pieceBB[black ? 1 : 0] ^= head;
                            head <<= 1;
                            break;
                        case 'B':
                            pieceBB[(int)PieceCode.Bishop] ^= head;
                            pieceBB[black ? 1 : 0] ^= head;
                            head <<= 1;
                            break;
                        case 'Q':
                            pieceBB[(int)PieceCode.Queen] ^= head;
                            pieceBB[black ? 1 : 0] ^= head;
                            head <<= 1;
                            break;
                        case 'K':
                            pieceBB[(int)PieceCode.King] ^= head;
                            pieceBB[black ? 1 : 0] ^= head;
                            head <<= 1;
                            break;
                        case 'P':
                            pieceBB[(int)PieceCode.wPawn + (black ? 1 : 0)] ^= head;
                            pieceBB[black ? 1 : 0] ^= head;
                            head <<= 1;
                            break;
                        default:
                            int shift = int.Parse(lowerCase.ToString());
                            head <<= shift;
                            break;
                    }
                }
            }

            if (FENParts[1].ToLower().Equals("b"))
                MoveCount++;
            
            if (!FENParts[2].Equals("-"))
            {
                foreach (char c in FENParts[2])
                {
                    switch (c)
                    {
                        case 'K':
                            CastleMask |= (0x1ul << 6);
                            break;
                        case 'Q':
                            CastleMask |= (0x1ul << 2);
                            break;
                        case 'k':
                            CastleMask |= (0x1ul << 62);
                            break;
                        case 'q':
                            CastleMask |= (0x1ul << 58);
                            break;
                    }
                }
            } else
            {
                CastleMask = 0x4400_0000_0000_0044ul;
            }

            if (FENParts[3].Equals("-"))
            {
                LastSource = SquareEnum.a1;
                LastTarget = SquareEnum.a1;
            } else 
            {
                SquareEnum passantSquare = (SquareEnum)Enum.Parse(typeof(SquareEnum), FENParts[3]);
                if ((int)passantSquare < 24)
                {
                    LastSource = (SquareEnum)((int)passantSquare - 8);
                    LastTarget = (SquareEnum)((int)passantSquare + 8);
                } else
                {
                    LastTarget = (SquareEnum)((int)passantSquare - 8);
                    LastSource = (SquareEnum)((int)passantSquare + 8);
                }
            }

            MoveCount += 2*(int.Parse(FENParts[5]) - 1);

            Hasher.Hash(this);
        }

        public BitBoard(ulong[] BBarr, ulong castleMask)
        {
            pieceBB = (ulong[])BBarr.Clone();
            CastleMask = castleMask;
        }

        public BitBoard MakeMove(Move move)
        {
            BitBoard moveBB = Clone();
            moveBB.HashValue = Hasher.Hash(this, move);
            return MakeMoveOn(moveBB, move);
        }

        private BitBoard MakeMoveOn(BitBoard BB, Move move)
        {
            ulong sourcePos = BBPos[(int)move.Source];
            ulong targetPos = BBPos[(int)move.Target];
            ulong flipMask = sourcePos | targetPos;
            int self = (int)move.Color;
            int opponent = self ^ 1;

            //Casteling (updating mask and moving rook)
            ulong rookMove = (sourcePos | targetPos) & (BBStartPos[(int)PieceCode.Rook]);
            BB.CastleMask &= ~(EaWe(rookMove, 2, 1));
            ulong kingMove = sourcePos & (BBStartPos[(int)PieceCode.King]);
            BB.CastleMask &= ~(EaWe(kingMove, 2, 2));

            ulong kingSource = BB.pieceBB[(int)PieceCode.King] & BBPos[(int)move.Source];
            ulong longCastle = West(kingSource, 2) & targetPos;
            ulong shortCastle = East(kingSource, 2) & targetPos;
            ulong rookMask = EaWe(longCastle, 1, 2) | EaWe(shortCastle, 1, 1);
            BB.pieceBB[(int)PieceCode.Rook] ^= rookMask;
            BB.pieceBB[self] ^= rookMask;

            //delete any piece on target square
            for (int i = 0; i < BB.pieceBB.Length; i++)
            {
                BB.pieceBB[i] &= ~(targetPos);
            }

            //own color
            BB.pieceBB[self] ^= flipMask;

            //opponent color
            BB.pieceBB[opponent] &= ~targetPos;

            //move own piece
            BB.pieceBB[(int)move.Piece] ^= flipMask;
            // pieceBB[(int)move.Piece] &= ~BBPos[(int)move.Source];
            // pieceBB[(int)move.Piece] |= BBPos[(int)move.Target];

            //en passant
            ulong passantMask = (
                NoSo(BBPos[(int)LastSource], 2, 2) &
                BBPos[(int)LastTarget] &
                NoSo(targetPos) &
                EaWe(sourcePos) &
                BB.pieceBB[opponent + 2] &
                South(BB.pieceBB[self + 2])
            );

            BB.pieceBB[opponent] ^= passantMask; 
            BB.pieceBB[opponent + 2] ^= passantMask; 

            //Promote piece
            BB.pieceBB[(int)move.Piece] ^= targetPos;
            BB.pieceBB[(int)move.Promoted] ^= targetPos;

            LastSource = move.Source;
            LastTarget = move.Target;
            BB.MoveCount++;
            return BB;
        }

        ///White ahead = positive value, Black ahead = negative value
        public int GetBoardValue() 
        {
            int value = 0;

            int wPawns = BitOperations.PopCount(pieceBB[(int)PieceCode.wPawn]) - BitOperations.PopCount(pieceBB[(int)PieceCode.bPawn]);
            value += wPawns * 1;
            int wKnights = BitOperations.PopCount(pieceBB[(int)PieceCode.Knight] & pieceBB[(int)PieceCode.White]) - BitOperations.PopCount(pieceBB[(int)PieceCode.Knight] & pieceBB[(int)PieceCode.Black]);
            value += wKnights * 3;
            int wBishops = BitOperations.PopCount(pieceBB[(int)PieceCode.Bishop] & pieceBB[(int)PieceCode.White]) - BitOperations.PopCount(pieceBB[(int)PieceCode.Bishop] & pieceBB[(int)PieceCode.Black]);
            value += wBishops * 3;
            int wRook = BitOperations.PopCount(pieceBB[(int)PieceCode.Rook] & pieceBB[(int)PieceCode.White]) - BitOperations.PopCount(pieceBB[(int)PieceCode.Rook] & pieceBB[(int)PieceCode.Black]);
            value += wRook * 5;
            int wQueen = BitOperations.PopCount(pieceBB[(int)PieceCode.Queen] & pieceBB[(int)PieceCode.White]) - BitOperations.PopCount(pieceBB[(int)PieceCode.Queen] & pieceBB[(int)PieceCode.Black]);
            value += wQueen * 9;
            int wKing = BitOperations.PopCount(pieceBB[(int)PieceCode.King] & pieceBB[(int)PieceCode.White]) - BitOperations.PopCount(pieceBB[(int)PieceCode.King] & pieceBB[(int)PieceCode.Black]);
            value += wKing * 10000;

            return value;
        }

        public int MoveValue(Move move)
        {
            BitBoard moveBB = new BitBoard(pieceBB, CastleMask);
            return moveBB.MakeMove(move).GetBoardValue();
        }

        public List<Move> GetAllLegalMoves() 
        {
            List<Move> moves = new List<Move>();
            int turn = MoveCount & 0b1;
            for (PieceCode c = PieceCode.wPawn; c <= PieceCode.King; c++)
            {
                ulong pawnMask = Convert.ToUInt64(c != PieceCode.wPawn && c != PieceCode.bPawn) - 1;
                ulong BB = pieceBB[(int)c] & pieceBB[turn];
                for (ulong _BBPos = (BB & (ulong)(-(long)BB)); _BBPos != 0; _BBPos = (BB & (ulong)(-(long)BB)))
                {
                    BB &= ~_BBPos;
                    int pos = Array.IndexOf(BBPos, _BBPos);
                    ulong attackSet = AttackSets.AttackByPieceType(pos, c, this);

                    for (ulong attack = (attackSet & (ulong)(-(long)attackSet)); attackSet != 0; attack = (attackSet & (ulong)(-(long)attackSet)))
                    {
                        attackSet &= ~attack;
                        int attackPos = BitOperations.Log2(attack);

                        if ((attack & (Rank1 | Rank8) & pawnMask) != 0)
                        {
                            moves.Add(new Move((PieceCode)turn, c, (SquareEnum)pos, (SquareEnum)attackPos, this, PieceCode.Knight));
                            moves.Add(new Move((PieceCode)turn, c, (SquareEnum)pos, (SquareEnum)attackPos, this, PieceCode.Bishop));
                            moves.Add(new Move((PieceCode)turn, c, (SquareEnum)pos, (SquareEnum)attackPos, this, PieceCode.Rook));
                            moves.Add(new Move((PieceCode)turn, c, (SquareEnum)pos, (SquareEnum)attackPos, this, PieceCode.Queen));
                            continue;
                        }

                        moves.Add(new Move((PieceCode)turn, c, (SquareEnum)pos, (SquareEnum)attackPos, this, c));
                    }

                }
            }


            return moves;
        }

        public override string ToString()
        {
            return BoardUtils.BitBoardToString(pieceBB[(int)PieceCode.Black] | pieceBB[(int)PieceCode.White]);
        }

        public BitBoard Clone() 
        {
            BitBoard BB = new BitBoard(pieceBB, CastleMask);
            BB.MoveCount = MoveCount;
            return BB;
        }

    }
}