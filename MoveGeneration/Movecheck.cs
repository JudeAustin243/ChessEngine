using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessEngine
{
    public readonly struct Movecheck
    {
        public readonly Move[] moves;
        public readonly int check;

        public Movecheck(Move[] moves, int check)
        {
            this.moves = moves;
            this.check = check;
        }
    }
}
