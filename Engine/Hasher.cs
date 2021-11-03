
using System;
using System.Numerics;
using BitBoardBot.Board;
using static BitBoardBot.Board.BoardUtils;

namespace BitBoardBot.Engine
{
    public static class Hasher
    {
        private static ulong[][] HashTable;

        public static void Init()
        {
            Random r = new Random();
            HashTable = new ulong[13][];
            for (int i = 0; i < 13; i++)
            {
                HashTable[i] = new ulong[64];
                for (int j = 0; j < 64; j++)
                {
                    ulong randNum = 0ul;
                    byte[] bytes = new byte[8];
                    r.NextBytes(bytes);
                    foreach (byte b in bytes)
                    {
                        randNum = (randNum << 8) | b;
                    }
                    HashTable[i][j] = randNum;
                }
            }
        }

        public static BitBoard Hash(BitBoard BB)
        {
            ulong hashValue = 0;

            for (PieceCode c = PieceCode.wPawn; c <= PieceCode.King; c++)
            {
                ulong pieceBits = BB.pieceBB[(int)c];

                for (ulong pos = pieceBits & (ulong)(-(long)pieceBits); pos != 0; pos = pieceBits & (ulong)(-(long)pieceBits))
                {
                    pieceBits ^= pos;
                    int log2 = BitOperations.Log2(pos);
                    hashValue ^= HashTable[HashIndex((PieceCode)((pos & BB.pieceBB[(int)PieceCode.Black]) >> log2), c)][log2];
                }
            }
            ulong nothingBits = ~(BB.pieceBB[(int)PieceCode.Black] | BB.pieceBB[(int)PieceCode.White]);
            for (ulong pos = nothingBits & (ulong)(-(long)nothingBits); pos != 0; pos = nothingBits & (ulong)(-(long)nothingBits))
            {
                nothingBits ^= pos;
                int log2 = BitOperations.Log2(pos);
                hashValue ^= HashTable[1][log2];
            }
            BB.HashValue = hashValue;
            return BB;
        }

        public static ulong Hash(BitBoard BB, Move move)
        {
            ulong hashValue = BB.HashValue;

            hashValue ^= HashTable[HashIndex(move.Color, move.Piece)][(int)move.Source];
            hashValue ^= HashTable[HashIndex(move.Color, move.Promoted)][(int)move.Target];
            hashValue ^= HashTable[1][(int)move.Source];

            int opponent = (int)move.Color ^ 1;
            PieceCode targetColor = PieceCode.White;
            PieceCode targetType = PieceCode.bPawn;

            for (PieceCode c = PieceCode.wPawn; c <= PieceCode.King ; c++)
            {
                ulong posTypeBit = (BB.pieceBB[(int)c] & BBPos[(int)move.Target]);
                int isThisType = (int)(posTypeBit >> BitOperations.Log2(posTypeBit)); //0 if not this type, 1 if it is
                targetType += (int)c * isThisType;
                targetColor = (PieceCode)(isThisType * opponent);
            }

            hashValue ^= HashTable[HashIndex(targetColor, (PieceCode)targetType)][(int)move.Target];

            return hashValue;
        }

        private static int HashIndex(PieceCode color, PieceCode type)
        {
            int colorOffset = ((int)color << 2 | (int)color << 1);
            return ((int)type - 2) + colorOffset;
        }
    }
}