using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ChessEngine
{
    public sealed class GameState
    {
        public bool[] get_gamestate(Board board)
        {

            bool[] states = new bool[2];

            int material_count = 0;

            int[] material = new int[12] { 10, 10, 1000, 1000, 300, 300, 300, 300, 2000, 2000, 1, 1 };

            for (int i = 1; i < 12; i += 2)
            {

                ulong bitboard = board.bitboards[i];

                for (; bitboard > 0;)
                {
                    material_count += material[i];

                    bitboard &= bitboard - 1;
                }

            }

            if (material_count > 2500)
            {
                states[0] = false;
            }
            else
            {
                states[0] = true;
            }


            return states;
        }
    }
}
