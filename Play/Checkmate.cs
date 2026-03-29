using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ChessEngine
{
    public class Checkmate
    {
        public int mate(MoveGeneration move, Board board, PieceCall cache, int mod)
        {
            if (move.all_moves(board, cache, mod, out int move_count, out ulong pawn_mask, out ulong capture_mask).moves[0].IsNull)
            {
                if (move.all_moves(board, cache, mod, out move_count, out pawn_mask, out capture_mask).check > 0)
                {
                    return 2;
                }
                return 1;
            }
            return 0;
        }


    }
}
