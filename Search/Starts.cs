using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessEngine
{
    public readonly struct Starts
    {
        public readonly int piece;
        public readonly int start;
        public Starts(int piece, int start)
        {
            this.piece = piece;
            this.start = start;
        }
    }
}
