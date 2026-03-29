using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ChessEngine
{
    public readonly struct Eval
    {
        public readonly Move move;
        public readonly double eval;
        public readonly bool ismate;
        public Eval(Move move, double eval, bool ismate)
        {
            this.move = move;
            this.eval = eval;
            this.ismate = ismate;
        }
    }
}
