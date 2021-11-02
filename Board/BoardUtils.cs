
using System.Text;

namespace BitBoardBot.Board
{
    public static class BoardUtils
    {
        public static readonly ulong AFile = FileMask(0);
        public static readonly ulong HFile = FileMask(7);
        public static readonly ulong Rank8 = 0xFFul << 56;
        public static readonly ulong Rank1 = 0xFFul;
        public static readonly ulong Diagonal = 0x8040201008040201ul;
        public static readonly ulong AntiDiagonal = 0x0102040810204080ul;
        public static readonly ulong LightSquares = 0x55AA55AA55AA55AA;
        public static readonly ulong DarkSquares = 0xAA55AA55AA55AA55;
        public static readonly ulong[] BBStartPos = new ulong[9] {
                0xFFFFul,
                0xFFFFul << 48,
                0xFFul << 8,
                0xFFul << 48,
                0x42ul << 56 | 0x42ul,
                0x24ul << 56 | 0x24ul,
                0x81ul << 56 | 0x81ul,
                0x1ul << 59 | 0x1ul << 3,
                0x1ul << 60 | 0x1ul << 4
        };
        public enum PieceCode
        {
            White, Black,
            wPawn, bPawn, Knight, Bishop, Rook, Queen, King
        }
        public enum SquareEnum
        {
            a1, b1, c1, d1, e1, f1, g1, h1,
            a2, b2, c2, d2, e2, f2, g2, h2,
            a3, b3, c3, d3, e3, f3, g3, h3,
            a4, b4, c4, d4, e4, f4, g4, h4,
            a5, b5, c5, d5, e5, f5, g5, h5,
            a6, b6, c6, d6, e6, f6, g6, h6,
            a7, b7, c7, d7, e7, f7, g7, h7,
            a8, b8, c8, d8, e8, f8, g8, h8
        }
        public static ulong[] BBPos { get; private set; } = new ulong[64];
        public static void Init() 
        {
            for (int i = 0; i < 64; i++)
            {
                BBPos[i] = 0x1ul << i;
            }
        }
        public static ulong North(ulong set) 
        {
            return set << 8;
        }
        public static ulong North(ulong set, int count)
        {
            for (int i = 0; i < count; i++)
            {
                set = North(set);
            }
            return set;
        }

        public static ulong South(ulong set)
        {
            return set >> 8;
        }
        public static ulong South(ulong set, int count)
        {
            for (int i = 0; i < count; i++)
            {
                set = South(set);
            }
            return set;
        }
        public static ulong East(ulong set)
        {
            return (set & ~HFile) << 1;
        }
        public static ulong East(ulong set, int count)
        {
            for (int i = 0; i < count; i++)
            {
                set = East(set);
            }
            return set;
        }
        public static ulong West(ulong set)
        {
            return (set & ~AFile) >> 1;
        }
        public static ulong West(ulong set, int count)
        {
            for (int i = 0; i < count; i++)
            {
                set = West(set);
            }
            return set;
        }
        public static ulong NoSo(ulong set)
        {
            return North(set) | South(set);
        }
        public static ulong NoSo(ulong set, int nCount, int sCount)
        {
            return North(set, nCount) | South(set, sCount);
        }
        public static ulong EaWe(ulong set)
        {
            return East(set) | West(set);
        }
        public static ulong EaWe(ulong set, int eCount, int wCount)
        {
            return East(set, eCount) | West(set, wCount);
        }
        public static ulong NoEa(ulong set, int nCount, int eCount)
        {
            return North(East(set, eCount), nCount);
        }
        public static ulong NoWe(ulong set, int nCount, int WCount)
        {
            return North(West(set, WCount), nCount);
        }
        public static ulong SoEa(ulong set, int sCount, int eCount)
        {
            return South(East(set, eCount), sCount);
        }
        public static ulong SoWe(ulong set, int sCount, int WCount)
        {
            return South(West(set, WCount), sCount);
        }

        public static ulong FileMask(int file) // 0 = a-file, 7 = h-file
        {
            return 0x0101010101010101ul << file;
        }

        public static ulong RankMask(int rank) // 0 = rank 1, 7 = rank 8
        {
            return 0xFFul << (rank << 3);
        }
        public static string BitBoardToString(ulong board) {
            StringBuilder sb = new StringBuilder();
            for (int y = 7; y >= 0; y--)
            {
                for(int x = 0; x < 8; x++) {
                    int bitPos = y * 8 + x;
                    sb.Append((board & (0b1ul << bitPos)) != 0 ? '1' : '0');
                }
                sb.Append("\n");
            }
            return sb.ToString();
        }
    }
}