using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ChessEngine
{
    class Killer
    {
        public Move[,] killer_moves;

        public Killer()
        {
            killer_moves = new Move[30, 2];
        }

        public void add_killer_move(int depth, Move move)
        {
            for (int i = 0; i < 2; i++)
            {
                if (killer_moves[depth, i].IsNull)
                {
                    killer_moves[depth, i] = move;
                    return;
                }
                else if (killer_moves[depth, i].Equals(move))
                {

                    return;
                }
            }
            killer_moves[depth, 0] = move;
        }
    }
}
