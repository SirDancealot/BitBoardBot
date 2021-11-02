
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
        public bool[] canCastle { get; set; } = new bool[] { true, true };
        public int MoveCount { get; private set; } = 0;
        public ulong[] pieceBB { get; private set; }
        public BitBoard() {
            pieceBB = (ulong[])BoardUtils.BBStartPos.Clone();
        }

        public BitBoard(ulong[] BBarr)
        {
            pieceBB = (ulong[])BBarr.Clone();
        }

        public BitBoard MakeMove(Move move)
        {
            LastSource = move.Source;
            LastTarget = move.Target;
            ulong flipMask = BBPos[(int)move.Source] | BBPos[(int)move.Target];

            for (int i = 0; i < pieceBB.Length; i++)
            {
                pieceBB[i] &= ~(BBPos[(int)move.Target]);
            }

            //Fix own color
            pieceBB[(int)move.Color] ^= flipMask;

            //Fix other color
            pieceBB[((int)move.Color) ^ 1] &= ~BBPos[(int)move.Target];

            //Fix piece
            pieceBB[(int)move.Piece] &= ~BBPos[(int)move.Source];
            pieceBB[(int)move.Piece] |= BBPos[(int)move.Target];

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
            value += wKing * 1000;

            return value;
        }

        public int MoveValue(Move move)
        {
            BitBoard moveBB = new BitBoard(pieceBB);
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