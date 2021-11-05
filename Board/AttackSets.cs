using System;
using System.Numerics;
using static BitBoardBot.Board.BoardUtils;

namespace BitBoardBot.Board 
{
    public static class AttackSets
    {
        public static ulong[][] rays { get; private set; } = new ulong[8][]; //first index is rotation, 0 = up-left, and increases clockwise (2 = up-right, 3 = right)
        public static ulong[] KingAttacks { get; private set;} = new ulong[64];
        public static ulong[] KnightAttacks { get; private set;} = new ulong[64];
        public static ulong[] RookAttacks { get; private set;} = new ulong[64];
        public static ulong[] BishopAttacks { get; private set;} = new ulong[64];
        public static ulong[] QueenAttacks { get; private set;} = new ulong[64];
        public static ulong[][] PawnAttacks { get; private set;} = new ulong[2][];
        public static ulong[][] AttacksByPieceType { get; private set;  } = new ulong[9][];
        public static Func<int, ulong, BitBoard, ulong>[] AttackSetFuncs { get; private set; } = new Func<int, ulong, BitBoard, ulong>[9];

        private static bool _init = false;
        public static void Init() {
            if (_init)
                return;
            _init = true;

            for (int i = 0; i < 8; i++)
            {
                rays[i] = new ulong[64];
            }

            PawnAttacks[0] = new ulong[64];
            PawnAttacks[1] = new ulong[64];

            for (int i = 0; i < 64; i++)
            {
                CalcKingAttack(i);
                CalcKnightAttack(i);
                CalcRays(i);
                CalcLineAttacks(i);
                CalcPawnAttacks(i);
            }
            AttacksByPieceType[(int)PieceCode.wPawn] = PawnAttacks[(int)PieceCode.White];
            AttacksByPieceType[(int)PieceCode.bPawn] = PawnAttacks[(int)PieceCode.Black];
            AttacksByPieceType[(int)PieceCode.Bishop] = BishopAttacks;
            AttacksByPieceType[(int)PieceCode.Rook] = RookAttacks;
            AttacksByPieceType[(int)PieceCode.King] = KingAttacks;
            AttacksByPieceType[(int)PieceCode.Queen] = QueenAttacks;
            AttacksByPieceType[(int)PieceCode.Knight] = KnightAttacks;

            AttackSetFuncs[(int)PieceCode.wPawn] = PawnAttack;
            AttackSetFuncs[(int)PieceCode.bPawn] = PawnAttack;
            AttackSetFuncs[(int)PieceCode.Bishop] = BishopAttack;
            AttackSetFuncs[(int)PieceCode.Rook] = RookAttack;
            AttackSetFuncs[(int)PieceCode.King] = KingAttack;
            AttackSetFuncs[(int)PieceCode.Queen] = QueenAttack;
            AttackSetFuncs[(int)PieceCode.Knight] = KnightAttack;
        }

        private static void CalcKingAttack(int pos)
        {
            ulong _BBPos = BBPos[pos];
            ulong BBAtk = BoardUtils.East(_BBPos) | BoardUtils.West(_BBPos);
            BBAtk |= BoardUtils.North(_BBPos | BBAtk) | BoardUtils.South(_BBPos | BBAtk);
            if (pos == 4 || pos == 60)
                BBAtk |= EaWe(_BBPos, 2, 2);
            KingAttacks[pos] = BBAtk;
        }

        private static void CalcKnightAttack(int pos)
        {
            ulong _BBPos = BBPos[pos];
            ulong BBAtk = NoEa(_BBPos, 2, 1);
            BBAtk |= NoWe(_BBPos, 2, 1);
            BBAtk |= SoEa(_BBPos, 2, 1);
            BBAtk |= SoWe(_BBPos, 2, 1);
            BBAtk |= NoEa(_BBPos, 1, 2);
            BBAtk |= NoWe(_BBPos, 1, 2);
            BBAtk |= SoEa(_BBPos, 1, 2);
            BBAtk |= SoWe(_BBPos, 1, 2);

            KnightAttacks[pos] = BBAtk;
        }

        private static void CalcRays(int pos)
        {
            for (int i = 0; i < 8; i++)
            {
                int shiftAmt;
                if ((i & 0b11) == 3)
                    shiftAmt = 1;
                else
                    shiftAmt = 7 + (i & 0b11);

                ulong lrMask = 0xFFFFFFFFFFFFFFFFul;
                if (i == 0 || i == 6 || i == 7)
                    lrMask = ~BoardUtils.AFile;
                if (i == 4 || i == 3 || i == 2)
                    lrMask = ~BoardUtils.HFile;

                ulong ray = (BBPos[pos] & lrMask) << shiftAmt;
                if ((i & 0b100)  != 0)
                    ray = (BBPos[pos] & lrMask) >> shiftAmt;

                for (int j = 0; j < 6; j++)
                {
                    if ((i & 0b100)  != 0)
                        ray |= (ray & lrMask) >> shiftAmt;
                    else
                        ray |= (ray & lrMask) << shiftAmt;

                }
                rays[i][pos] = ray;
            }
        }

        private static void CalcLineAttacks(int pos)
        {
            RookAttacks[pos] = rays[1][pos] | rays[3][pos] | rays[5][pos] | rays[7][pos];
            BishopAttacks[pos] = rays[0][pos] | rays[2][pos] | rays[4][pos] | rays[6][pos];
            QueenAttacks[pos] = RookAttacks[pos] | BishopAttacks[pos];
        }

        private static void CalcPawnAttacks(int pos)
        {
            ulong _BBPos = BBPos[pos];
            ulong wAttack = North(_BBPos);
            ulong bAttack = South(_BBPos);

            wAttack |= East(wAttack) | West(wAttack) | (pos < 16 ? North(wAttack) : 0);
            bAttack |= East(bAttack) | West(bAttack) | (pos > 47 ? South(bAttack) : 0);

            PawnAttacks[(int)PieceCode.Black][pos] = bAttack;
            PawnAttacks[(int)PieceCode.White][pos] = wAttack;
        }

        public static ulong SlidingAttack(int pos, ulong emptyBoardAttackSet, BitBoard BB) 
        {
            /*pieceBB[turn ^ 1], pieceBB[turn]*/
            ulong opponents = BB.pieceBB[(BB.MoveCount & 0b1) ^ 1];
            ulong ownPieces = BB.pieceBB[BB.MoveCount & 0b1];
            ulong blockers = opponents | ownPieces;
            ulong final = 0;

            for (int i = 0; i < 4; i++)
            {
                ulong blockedBy = (rays[i][pos] & blockers);
                //Positive directions
                if (blockedBy != 0) 
                {
                    ulong lowest = blockedBy & (ulong)-((long)blockedBy);
                    final |= rays[i][pos] & (lowest | (lowest - 1));
                }
                else
                    final |= rays[i][pos];
            }

            for (int i = 4; i < 8; i++)
            {
                //Negative directions
                ulong blockedBy = (rays[i][pos] & blockers);
                if (blockedBy != 0)
                {
                    ulong highest = BBPos[63 - BitOperations.LeadingZeroCount(blockedBy)];
                    final |= rays[i][pos] & ~(highest - 1);
                } else
                    final |= rays[i][pos];
            }
            return final & emptyBoardAttackSet & (~ownPieces);
        }

        public static ulong PawnAttack(int pos, ulong emptyBoardAttackSet, BitBoard BB)
        {
            ulong opponents = BB.pieceBB[(BB.MoveCount & 0b1) ^ 1];
            ulong ownPieces = BB.pieceBB[BB.MoveCount & 0b1];
            ulong final = 0;
            ulong blockers = opponents | ownPieces;

            ulong _BBPos = BBPos[pos];
            ulong ns = North(_BBPos) | South(_BBPos);
            ulong ew = East(ns) | West(ns);

            final = ns & (~blockers);
            final |= (North(final) | South(final)) & ((~blockers) & (~BBPos[pos]));

            final |= ew & opponents;


            ulong enemyPawns = NoSo(BB.pieceBB[((BB.MoveCount & 0b1) ^ 1) + 2]);
            ulong passantMask = enemyPawns & NoSo(BBPos[(int)BB.LastSource]) & NoSo(BBPos[(int)BB.LastTarget]);

            final |= passantMask;

            return final & emptyBoardAttackSet;
        }
        public static ulong KingAttack(int pos, ulong emptyBoardAttackSet, BitBoard BB)
        {
            ulong ownPieces = BB.pieceBB[BB.MoveCount & 0b1];

            ulong notCastleSet = emptyBoardAttackSet & ~ownPieces & ~(StaticCastleMask & EaWe(BBPos[pos], 2, 2));

            //not blocked by own pieces for castling
            ulong castleSet = emptyBoardAttackSet & BB.CastleMask & EaWe(notCastleSet) & ~ownPieces & East(~ownPieces);

            //not in check for casteling
            ulong opponentAttackSet = BB.AttackSet((PieceCode)((BB.MoveCount & 0b1) ^ 1));
            ulong _BBPos = BBPos[pos];
            
            ulong longCastle = _BBPos | West(_BBPos);
            longCastle |= West(longCastle);
            
            ulong shortCastle = _BBPos | East(_BBPos);
            shortCastle |= East(shortCastle);

            longCastle &= opponentAttackSet;
            shortCastle &= opponentAttackSet;

            longCastle |= West(longCastle);
            longCastle |= West(longCastle);
            shortCastle |= East(shortCastle);
            shortCastle |= East(shortCastle);

            ulong castleInCheckMask = ~(longCastle | shortCastle);

            return notCastleSet | (castleSet & castleInCheckMask);
        }
        public static ulong KnightAttack(int pos, ulong emptyBoardAttackSet, BitBoard BB)
        {
            ulong ownPieces = BB.pieceBB[BB.MoveCount & 0b1];
            return emptyBoardAttackSet & ~ownPieces;
        }
        public static ulong RookAttack(int pos, ulong emptyBoardAttackSet, BitBoard BB)
        {
            return SlidingAttack(pos, emptyBoardAttackSet, BB);
        }
        public static ulong BishopAttack(int pos, ulong emptyBoardAttackSet, BitBoard BB)
        {
            return SlidingAttack(pos, emptyBoardAttackSet, BB);
        }
        public static ulong QueenAttack(int pos, ulong emptyBoardAttackSet, BitBoard BB)
        {
            return SlidingAttack(pos, emptyBoardAttackSet, BB);
        }

        public static ulong AttackByPieceType(int pos, PieceCode code, BitBoard BB)
        {
            ulong emptyAttackSet = AttacksByPieceType[(int)code][pos];
            return AttackSetFuncs[(int)code].Invoke(pos, emptyAttackSet, BB);
        }

        public static ulong CheckedByBitmask(BitBoard BB, PieceCode color)
        {
            int moveCount = BB.MoveCount;

            BB.MoveCount = (int)color;

            ulong ownColorMask = BB.pieceBB[(int)color];
            ulong opponentColorMask = BB.pieceBB[(int)color ^ 1];

            ulong kingBBPos = BB.pieceBB[(int)PieceCode.King] & BB.pieceBB[(int)color];
            int kingPos = BitOperations.Log2(kingBBPos);

            ulong opPawn, opKnight, opRQ, opBQ, opKing;

            opPawn = BB.pieceBB[(int)PieceCode.wPawn + ((int)color ^ 1)];
            opKnight = BB.pieceBB[(int)PieceCode.Knight] & opponentColorMask;
            opKing = BB.pieceBB[(int)PieceCode.King] & opponentColorMask;
            opRQ = BB.pieceBB[(int)PieceCode.Queen] & opponentColorMask;
            opBQ = BB.pieceBB[(int)PieceCode.Queen] & opponentColorMask;
            opRQ |= BB.pieceBB[(int)PieceCode.Rook] & opponentColorMask;
            opBQ |= BB.pieceBB[(int)PieceCode.Bishop] & opponentColorMask;

            ulong attackedBy = 0;

            ulong pawnAttackMask = EaWe(NoSo(kingBBPos));

            attackedBy |= AttackByPieceType(kingPos, (PieceCode)((int)PieceCode.wPawn | (int)color), BB) & opPawn & pawnAttackMask;
            attackedBy |= AttackByPieceType(kingPos, PieceCode.Knight, BB) & opKnight;
            attackedBy |= AttackByPieceType(kingPos, PieceCode.Bishop, BB) & opBQ;
            attackedBy |= AttackByPieceType(kingPos, PieceCode.Rook, BB) & opRQ;
            attackedBy |= AttackByPieceType(kingPos, PieceCode.King, BB) & opKing;

            BB.MoveCount = moveCount;
            return attackedBy;
        }
    }
}