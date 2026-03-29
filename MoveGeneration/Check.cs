using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ChessEngine
{
    public struct Check
    {
        public readonly ulong illegal;
        public readonly Starts start;
        public readonly int check;
        public ulong mask;
        public ulong pawn_mask;
        public Check(ulong illegal, Starts start, int check, ulong mask, ulong pawn_mask)
        {
            this.illegal = illegal;
            this.start = start;
            this.check = check;
            this.mask = mask;
            this.pawn_mask = pawn_mask;
        }
    }
}
