
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
        public ulong CastleMask { get; private set; } = 0x4400_0000_0000_0044ul;
        public int MoveCount { get; private set; } = 0;
        public ulong[] pieceBB { get; private set; }
        public BitBoard() {
            pieceBB = (ulong[])BoardUtils.BBStartPos.Clone();
        }

        public BitBoard(ulong[] BBarr, ulong castleMask)
        {
            pieceBB = (ulong[])BBarr.Clone();
            CastleMask = castleMask;
        }

        public BitBoard MakeMove(Move move)
        {
            ulong sourcePos = BBPos[(int)move.Source];
            ulong targetPos = BBPos[(int)move.Target];
            ulong flipMask = sourcePos | targetPos;
            int self = (int)move.Color;
            int opponent = self ^ 1;

            //Casteling (updating mask and moving rook)
            ulong rookMove = sourcePos & (BBStartPos[(int)PieceCode.Rook]);
            CastleMask &= ~(EaWe(rookMove, 2, 1));
            ulong kingMove = sourcePos & (BBStartPos[(int)PieceCode.King]);
            CastleMask &= ~(EaWe(kingMove, 2, 2));

            ulong kingSource = pieceBB[(int)PieceCode.King] & BBPos[(int)move.Source];
            ulong longCastle = West(kingSource, 2) & targetPos;
            ulong shortCastle = East(kingSource, 2) & targetPos;
            ulong rookMask = EaWe(longCastle, 1, 2) | EaWe(shortCastle, 1, 1);
            pieceBB[(int)PieceCode.Rook] ^= rookMask;
            pieceBB[self] ^= rookMask;

            //delete any piece on target square
            for (int i = 0; i < pieceBB.Length; i++)
            {
                pieceBB[i] &= ~(targetPos);
            }

            //own color
            pieceBB[self] ^= flipMask;

            //opponent color
            pieceBB[opponent] &= ~targetPos;

            //move own piece
            pieceBB[(int)move.Piece] ^= flipMask;
            // pieceBB[(int)move.Piece] &= ~BBPos[(int)move.Source];
            // pieceBB[(int)move.Piece] |= BBPos[(int)move.Target];

            //en passant
            ulong passantMask = (
                NoSo(BBPos[(int)LastSource], 2, 2) &
                BBPos[(int)LastTarget] &
                NoSo(targetPos) &
                EaWe(sourcePos) &
                pieceBB[opponent + 2] &
                South(pieceBB[self + 2])
            );

            pieceBB[opponent] ^= passantMask; 
            pieceBB[opponent + 2] ^= passantMask; 

            LastSource = move.Source;
            LastTarget = move.Target;
            MoveCount++;
            return this;
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
                ulong BB = pieceBB[(int)c] & pieceBB[turn];
                for (ulong _BBPos = (BB & (ulong)(-(long)BB)); _BBPos != 0; _BBPos = (BB & (ulong)(-(long)BB)))
                {
                    BB &= ~_BBPos;
                    int pos = Array.IndexOf(BBPos, _BBPos);
                    ulong attackSet = AttackSets.AttackByPieceType(pos, c, /*pieceBB[turn ^ 1], pieceBB[turn]*/ this);

                    for (ulong attack = (attackSet & (ulong)(-(long)attackSet)); attackSet != 0; attack = (attackSet & (ulong)(-(long)attackSet)))
                    {
                        attackSet &= ~attack;
                        int attackPos = Array.IndexOf(BBPos, attack);
                        moves.Add(new Move((PieceCode)turn, c, (SquareEnum)pos, (SquareEnum)attackPos, this));
                    }

                }
            }


            return moves;
        }

        public override string ToString()
        {
            return BoardUtils.BitBoardToString(pieceBB[(int)PieceCode.Black] | pieceBB[(int)PieceCode.White]);
        }

    }
}